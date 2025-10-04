using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;
using COMP9034.Backend.Services;
using COMP9034.Backend.Services.Interfaces;

namespace COMP9034.Backend.Controllers
{
    /// <summary>
    /// Staff management API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StaffsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IStaffService _staffService;
        private readonly ILogger<StaffsController> _logger;
        private readonly IAuditService _auditService;

        public StaffsController(ApplicationDbContext context, IStaffService staffService, ILogger<StaffsController> logger, IAuditService auditService)
        {
            _context = context;
            _staffService = staffService;
            _logger = logger;
            _auditService = auditService;
        }

        /// <summary>
        /// Get all staff list with filtering
        /// </summary>
        /// <param name="limit">Limit return count</param>
        /// <param name="offset">Offset</param>
        /// <param name="search">Search keyword</param>
        /// <param name="role">Role filter</param>
        /// <returns>Staff list</returns>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Staff>>> GetStaffs(
            [FromQuery] int? limit = null,
            [FromQuery] int offset = 0,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null)
        {
            try
            {
                // Accept alias: query -> search (frontend compatibility)
                if (string.IsNullOrWhiteSpace(search))
                {
                    var alias = Request.Query["query"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(alias)) search = alias;
                }
                // Convert offset-based pagination to page-based
                int pageNumber = (offset / (limit ?? 50)) + 1;
                int pageSize = limit ?? 50;

                var result = await _staffService.GetStaffPagedAsync(pageNumber, pageSize, search, role, null);

                if (result.Success)
                {
                    var (staff, totalCount) = result.Data;
                    _logger.LogInformation($"Returning {staff?.Count()} staff records out of {totalCount} total");
                    return Ok(staff);
                }
                else
                {
                    _logger.LogWarning($"Failed to get staffs: {result.Message}");
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting staffs");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get inactive (soft-deleted) staff members
        /// </summary>
        [HttpGet("inactive")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Staff>>> GetInactiveStaffs(
            [FromQuery] int? limit = null,
            [FromQuery] int offset = 0,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null)
        {
            try
            {
                // Check if user is admin
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var currentUser = await _context.Staff.FindAsync(currentUserId.Value);
                if (currentUser?.GetRoleFromId()?.ToLower() != "admin")
                {
                    return Forbid("Only administrators can view inactive staff");
                }

                IQueryable<Staff> query = _context.Staff;

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(s => (s.FirstName + " " + s.LastName).Contains(search) || 
                                           s.Email.Contains(search) ||
                                           s.StaffId.ToString().Contains(search));
                }

                // Apply role filter
                if (!string.IsNullOrEmpty(role) && role != "all")
                {
                    query = query.Where(s => s.Role == role);
                }

                // Only return INACTIVE staff
                query = query.Where(s => !s.IsActive);

                // Sort by ID
                query = query.OrderBy(s => s.StaffId);

                // Apply pagination
                if (offset > 0)
                {
                    query = query.Skip(offset);
                }

                if (limit.HasValue)
                {
                    query = query.Take(limit.Value);
                }

                var inactiveStaffs = await query.ToListAsync();

                // Set roles for each staff member
                foreach (var staff in inactiveStaffs)
                {
                    staff.Role = staff.GetRoleFromId();
                }

                _logger.LogInformation($"Returning {inactiveStaffs.Count} inactive staff records");
                return Ok(inactiveStaffs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting inactive staffs");
                return StatusCode(500, new { message = "Failed to get inactive staff", error = ex.Message });
            }
        }

        /// <summary>
        /// Restore a soft-deleted staff member
        /// </summary>
        [HttpPut("{id}/restore")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RestoreStaff(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Check if user is admin
                var currentUser = await _context.Staff.FindAsync(currentUserId.Value);
                if (currentUser?.GetRoleFromId()?.ToLower() != "admin")
                {
                    return StatusCode(403, new { message = "Only administrators can restore staff accounts" });
                }

                var staff = await _context.Staff
                    .FirstOrDefaultAsync(s => s.StaffId == id && !s.IsActive);

                if (staff == null)
                {
                    return NotFound(new { message = "Deleted staff record not found" });
                }

                // Restore staff
                staff.IsActive = true;
                staff.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Log audit trail
                var ipAddress = GetClientIpAddress();
                await _auditService.LogAsync("Staffs", "RESTORE", staff.StaffId.ToString(),
                    currentUserId.Value, ipAddress, $"Restored staff: {staff.FirstName} {staff.LastName}");

                _logger.LogInformation($"Staff restored by user {currentUserId}: ID={id}, Name={staff.FirstName} {staff.LastName}");
                return Ok(new { message = "Staff account restored successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while restoring staff ID:{id}");
                return StatusCode(500, new { message = "Failed to restore staff", error = ex.Message });
            }
        }

        /// <summary>
        /// Get specific staff by ID
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <returns>Staff information</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Staff>> GetStaff(int id)
        {
            try
            {
                var staff = await _context.Staff
                    .Include(s => s.Events.OrderByDescending(e => e.OccurredAt).Take(10))
                    .Include(s => s.BiometricData)
                    .FirstOrDefaultAsync(s => s.StaffId == id && s.IsActive);

                if (staff == null)
                {
                    return NotFound(new { message = $"Staff with ID {id} not found" });
                }

                // Set role
                staff.Role = staff.GetRoleFromId();

                return Ok(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting staff ID:{id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new staff member with auto-generated ID based on role
        /// </summary>
        /// <param name="request">Staff creation request with role</param>
        /// <returns>Created staff information</returns>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Staff>> PostStaff([FromBody] StaffCreateRequest request)
        {
            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.FirstName))
                {
                    return BadRequest(new { message = "First name is required" });
                }
                if (string.IsNullOrWhiteSpace(request.LastName))
                {
                    return BadRequest(new { message = "Last name is required" });
                }
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { message = "Email is required" });
                }
                if (string.IsNullOrWhiteSpace(request.Role))
                {
                    return BadRequest(new { message = "Role is required" });
                }

                // Validate role
                var validRoles = new[] { "staff", "manager", "admin" };
                if (!validRoles.Contains(request.Role.ToLower()))
                {
                    return BadRequest(new { message = "Role must be one of: staff, manager, admin" });
                }

                // Create staff object and auto-generate ID
                var staff = new Staff
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Role = CapitalizeRole(request.Role.ToLower()),
                    Phone = request.Phone,
                    Address = request.Address,
                    ContractType = request.ContractType ?? "Casual",
                    StandardHoursPerWeek = request.StandardHoursPerWeek ?? GetDefaultHoursForRole(request.Role),
                    IsActive = request.IsActive ?? true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Pin = "0000" // Default PIN, will be updated by user
                };

                // Auto-generate StaffId based on role with retry logic
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    staff.StaffId = await GenerateStaffIdAsync(request.Role.ToLower());
                    
                    // Set default pay rates if not provided
                    if (request.StandardPayRate.HasValue)
                    {
                        staff.StandardPayRate = request.StandardPayRate.Value;
                    }
                    else
                    {
                        staff.StandardPayRate = GetDefaultPayRateForRole(staff.Role);
                    }

                    if (request.OvertimePayRate.HasValue)
                    {
                        staff.OvertimePayRate = request.OvertimePayRate.Value;
                    }
                    else
                    {
                        staff.OvertimePayRate = staff.StandardPayRate * 1.5m;
                    }

                    _context.Staff.Add(staff);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }

                // Log audit trail
                var currentUserId = GetCurrentUserId();
                var ipAddress = GetClientIpAddress();
                if (currentUserId.HasValue)
                {
                    await _auditService.LogAsync("Staffs", "CREATE", staff.StaffId.ToString(), 
                        currentUserId.Value, ipAddress, null, staff);
                }

                _logger.LogInformation($"Created new staff: ID={staff.StaffId}, Name={staff.FirstName} {staff.LastName}, Role={staff.Role}");
                return CreatedAtAction(nameof(GetStaff), new { id = staff.StaffId }, staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating staff");
                return StatusCode(500, new { message = "Failed to create staff", error = ex.Message });
            }
        }

        /// <summary>
        /// Get next available staff ID for a role (preview only)
        /// </summary>
        /// <param name="role">Role: staff, manager, admin</param>
        /// <returns>Next available ID</returns>
        [HttpGet("next-id")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<object>> GetNextStaffId([FromQuery] string role)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(role))
                {
                    return BadRequest(new { message = "Role parameter is required" });
                }

                var validRoles = new[] { "staff", "manager", "admin" };
                if (!validRoles.Contains(role.ToLower()))
                {
                    return BadRequest(new { message = "Role must be one of: staff, manager, admin" });
                }

                var nextId = await GenerateStaffIdAsync(role.ToLower());
                return Ok(new { nextId = nextId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting next ID for role: {role}");
                return StatusCode(500, new { message = "Failed to get next ID", error = ex.Message });
            }
        }

        /// <summary>
        /// Update staff information
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <param name="staff">Updated staff information</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> PutStaff(int id, Staff staff)
        {
            if (id != staff.StaffId)
            {
                return BadRequest(new { message = "ID in path does not match ID in request body" });
            }

            try
            {
                var existingStaff = await _context.Staff.FindAsync(id);
                if (existingStaff == null)
                {
                    return NotFound(new { message = $"Staff with ID {id} not found" });
                }

                // Update fields
                existingStaff.FirstName = staff.FirstName;
                existingStaff.LastName = staff.LastName;
                existingStaff.Pin = staff.Pin;
                existingStaff.Email = staff.Email;
                existingStaff.Phone = staff.Phone;
                existingStaff.Address = staff.Address;
                existingStaff.StandardPayRate = staff.StandardPayRate;
                existingStaff.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated staff: ID={id}");
                return Ok(new { message = "Staff information updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating staff ID:{id}");
                return StatusCode(500, new { message = "Failed to update staff", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete staff (soft delete)
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            try
            {
                var staff = await _context.Staff.FindAsync(id);
                if (staff == null)
                {
                    return NotFound(new { message = $"Staff with ID {id} not found" });
                }

                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Key security checks
                // 1) Prevent user from deleting themselves
                if (currentUserId.Value == id)
                {
                    return BadRequest(new { message = "You cannot delete your own account" });
                }

                // 2) Prevent deleting the last administrator
                var targetStaffRole = staff.GetRoleFromId();
                if (targetStaffRole == "admin")
                {
                    var adminCount = await _context.Staff
                        .Where(s => s.IsActive && s.StaffId >= 9000)  // Admin ID range
                        .CountAsync();
                    
                    if (adminCount <= 1)
                    {
                        return BadRequest(new { message = "Cannot delete the last system administrator" });
                    }
                }

                // 3) Permission check: only administrators can delete other staff
                var currentUser = await _context.Staff.FindAsync(currentUserId.Value);
                if (currentUser?.GetRoleFromId()?.ToLower() != "admin")
                {
                    return StatusCode(403, new { message = "Only administrators can delete staff accounts" });
                }

                // Soft delete
                staff.IsActive = false;
                staff.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Log audit trail
                var ipAddress = GetClientIpAddress();
                await _auditService.LogAsync("Staffs", "DELETE", staff.StaffId.ToString(), 
                    currentUserId.Value, ipAddress, $"Deleted staff: {staff.FirstName} {staff.LastName}");

                _logger.LogInformation($"Staff deleted by user {currentUserId}: ID={id}, Name={staff.FirstName} {staff.LastName}");
                return Ok(new { message = "Staff deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting staff ID:{id}");
                return StatusCode(500, new { message = "Failed to delete staff", error = ex.Message });
            }
        }

        // PIN verification removed - use unified Email+Password authentication via /api/auth/login

        /// <summary>
        /// Quick clock in/out for staff
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <param name="eventType">Event type (IN/OUT)</param>
        /// <returns>Clock operation result</returns>
        [HttpPost("{id}/clock")]
        public async Task<ActionResult> QuickClock(int id, [FromBody] string eventType)
        {
            try
            {
                var staff = await _context.Staff.FirstOrDefaultAsync(s => s.StaffId == id && s.IsActive);
                if (staff == null)
                {
                    return NotFound(new { message = "Staff not found or disabled" });
                }

                // Validate event type
                if (eventType != "IN" && eventType != "OUT")
                {
                    return BadRequest(new { message = "Event type must be 'IN' or 'OUT'" });
                }

                // Check for recent duplicate entries (within 1 minute)
                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
                var recentEvent = await _context.Events
                    .Where(e => e.StaffId == id && e.EventType == eventType)
                    .Where(e => e.OccurredAt > oneMinuteAgo)
                    .FirstOrDefaultAsync();

                if (recentEvent != null)
                {
                    return BadRequest(new { message = "Duplicate clock entry detected. Please wait before clocking again." });
                }

                // Create new event
                var newEvent = new Event
                {
                    StaffId = id,
                    DeviceId = 1, // Default device
                    EventType = eventType,
                    OccurredAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    Notes = $"Quick {eventType.ToLower()} via API"
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Quick clock {eventType}: Staff ID={id}");
                return Ok(new { 
                    message = $"Clock {eventType.ToLower()} successful",
                    eventId = newEvent.EventId,
                    timeStamp = newEvent.OccurredAt,
                    staffName = staff.FirstName + " " + staff.LastName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during quick clock: ID={id}, Type={eventType}");
                return StatusCode(500, new { message = "Clock operation failed", error = ex.Message });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        /// <returns>Staff ID if authenticated, null otherwise</returns>
        private int? GetCurrentUserId()
        {
            var staffIdClaim = User.FindFirst(TokenClaims.StaffId)?.Value;
            if (int.TryParse(staffIdClaim, out int staffId))
            {
                return staffId;
            }
            return null;
        }

        /// <summary>
        /// Get client IP address
        /// </summary>
        /// <returns>Client IP address</returns>
        private string GetClientIpAddress()
        {
            // Check for forwarded IP first (load balancer/proxy)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        /// <summary>
        /// Generate staff ID based on role with range allocation
        /// </summary>
        /// <param name="role">Role: staff, manager, admin</param>
        /// <returns>Generated staff ID</returns>
        private async Task<int> GenerateStaffIdAsync(string role)
        {
            int minId, maxId;

            // Define ID ranges based on role
            switch (role.ToLower())
            {
                case "staff":
                    minId = 1000;
                    maxId = 7999;
                    break;
                case "manager":
                    minId = 8000;
                    maxId = 8999;
                    break;
                case "admin":
                    minId = 9000;
                    maxId = 9999;
                    break;
                default:
                    throw new ArgumentException($"Invalid role: {role}");
            }

            // Retry logic for concurrent conflicts
            for (int retry = 0; retry < 3; retry++)
            {
                try
                {
                    // Find the next available ID in the range
                    var maxExistingId = (await _context.Staff
                        .Where(s => s.StaffId >= minId && s.StaffId <= maxId)
                        .Select(s => (int?)s.StaffId)
                        .MaxAsync()) ?? (minId - 1);

                    var nextId = Math.Max(maxExistingId + 1, minId);

                    if (nextId > maxId)
                    {
                        throw new InvalidOperationException($"No available IDs in {role} range ({minId}-{maxId})");
                    }

                    return nextId;
                }
                catch (Exception ex) when (retry < 2)
                {
                    _logger.LogWarning($"ID generation retry {retry + 1} for role {role}: {ex.Message}");
                    await Task.Delay(100); // Brief delay before retry
                }
            }

            throw new InvalidOperationException($"Failed to generate ID for role {role} after 3 retries");
        }

        /// <summary>
        /// Get default pay rate based on role
        /// </summary>
        /// <param name="role">Staff role</param>
        /// <returns>Default pay rate</returns>
        private static decimal GetDefaultPayRateForRole(string role)
        {
            return role switch
            {
                "Admin" => 50.00m,
                "Manager" => 35.00m,
                "Staff" => 25.00m,
                _ => 25.00m
            };
        }

        /// <summary>
        /// Get default hours per week based on role
        /// </summary>
        /// <param name="role">Staff role</param>
        /// <returns>Default hours per week</returns>
        private static int GetDefaultHoursForRole(string role)
        {
            return role.ToLower() switch
            {
                "admin" => 40,
                "manager" => 40,
                _ => 20 // Casual staff default to part-time
            };
        }

        /// <summary>
        /// Capitalize role name for consistency
        /// </summary>
        /// <param name="role">Role in lowercase</param>
        /// <returns>Capitalized role</returns>
        private static string CapitalizeRole(string role)
        {
            return role.ToLower() switch
            {
                "admin" => "Admin",
                "manager" => "Manager",
                _ => "Staff"
            };
        }

        #endregion
    }
}

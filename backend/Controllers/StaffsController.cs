using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;
using COMP9034.Backend.Services;

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
        private readonly ILogger<StaffsController> _logger;
        private readonly IAuditService _auditService;

        public StaffsController(ApplicationDbContext context, ILogger<StaffsController> logger, IAuditService auditService)
        {
            _context = context;
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
        public async Task<ActionResult<IEnumerable<Staff>>> GetStaffs(
            [FromQuery] int? limit = null,
            [FromQuery] int offset = 0,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null)
        {
            try
            {
                var query = _context.Staffs.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(s => s.Name.Contains(search) || 
                                           s.Email!.Contains(search) ||
                                           s.Id.ToString().Contains(search));
                }

                // Apply role filter
                if (!string.IsNullOrWhiteSpace(role))
                {
                    query = query.Where(s => s.Role == role);
                }

                // Only return active staff
                query = query.Where(s => s.IsActive);

                // Sort by ID
                query = query.OrderBy(s => s.Id);

                // Apply pagination
                if (offset > 0)
                {
                    query = query.Skip(offset);
                }

                if (limit.HasValue)
                {
                    query = query.Take(limit.Value);
                }

                var staffs = await query.ToListAsync();

                // Set roles for each staff member
                foreach (var staff in staffs)
                {
                    staff.Role = staff.GetRoleFromId();
                }

                _logger.LogInformation($"Returning {staffs.Count} staff records");
                return Ok(staffs);
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
        [Authorize]
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

                var currentUser = await _context.Staffs.FindAsync(currentUserId.Value);
                if (currentUser?.GetRoleFromId() != "admin")
                {
                    return Forbid("Only administrators can view inactive staff");
                }

                IQueryable<Staff> query = _context.Staffs;

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(s => s.Name.Contains(search) || 
                                           s.Email.Contains(search) ||
                                           s.Id.ToString().Contains(search));
                }

                // Apply role filter
                if (!string.IsNullOrEmpty(role) && role != "all")
                {
                    query = query.Where(s => s.Role == role);
                }

                // Only return INACTIVE staff
                query = query.Where(s => !s.IsActive);

                // Sort by ID
                query = query.OrderBy(s => s.Id);

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
                return StatusCode(500, new { message = "获取已删除员工失败", error = ex.Message });
            }
        }

        /// <summary>
        /// Restore a soft-deleted staff member
        /// </summary>
        [HttpPut("{id}/restore")]
        [Authorize]
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
                var currentUser = await _context.Staffs.FindAsync(currentUserId.Value);
                if (currentUser?.GetRoleFromId() != "admin")
                {
                    return Forbid("只有管理员可以恢复员工账户");
                }

                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsActive);

                if (staff == null)
                {
                    return NotFound(new { message = "找不到已删除的员工记录" });
                }

                // Restore staff
                staff.IsActive = true;
                staff.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Log audit trail
                var ipAddress = GetClientIpAddress();
                await _auditService.LogAsync("Staffs", "RESTORE", staff.Id.ToString(),
                    currentUserId.Value, ipAddress, $"Restored staff: {staff.Name}");

                _logger.LogInformation($"Staff restored by user {currentUserId}: ID={id}, Name={staff.Name}");
                return Ok(new { message = "员工账户已成功恢复" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while restoring staff ID:{id}");
                return StatusCode(500, new { message = "恢复员工失败", error = ex.Message });
            }
        }

        /// <summary>
        /// Get specific staff by ID
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <returns>Staff information</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Staff>> GetStaff(int id)
        {
            try
            {
                var staff = await _context.Staffs
                    .Include(s => s.Events.OrderByDescending(e => e.TimeStamp).Take(10))
                    .Include(s => s.BiometricData)
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

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
        /// Create new staff member
        /// </summary>
        /// <param name="staff">Staff information</param>
        /// <returns>Created staff information</returns>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Staff>> PostStaff(Staff staff)
        {
            try
            {
                // Validate staff ID format
                if (staff.Id <= 0)
                {
                    return BadRequest(new { message = "Staff ID must be a positive number" });
                }

                // Check if staff ID already exists
                if (await _context.Staffs.AnyAsync(s => s.Id == staff.Id))
                {
                    return BadRequest(new { message = $"Staff ID {staff.Id} already exists" });
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(staff.Name))
                {
                    return BadRequest(new { message = "Staff name is required" });
                }

                if (string.IsNullOrWhiteSpace(staff.Pin) || staff.Pin.Length < 4)
                {
                    return BadRequest(new { message = "PIN must be at least 4 characters" });
                }

                // Set default values
                // 🎯 ID优先级：始终根据ID规则设置角色，确保数据一致性
                staff.Role = staff.GetRoleFromId();
                staff.CreatedAt = DateTime.UtcNow;
                staff.UpdatedAt = DateTime.UtcNow;
                staff.IsActive = true;

                // Set default hourly rate if not provided
                if (staff.HourlyRate <= 0)
                {
                    staff.HourlyRate = staff.Role switch
                    {
                        "admin" => 50,
                        "manager" => 35,
                        _ => 25
                    };
                }

                _context.Staffs.Add(staff);
                await _context.SaveChangesAsync();

                // Log audit trail
                var currentUserId = GetCurrentUserId();
                var ipAddress = GetClientIpAddress();
                if (currentUserId.HasValue)
                {
                    await _auditService.LogAsync("Staffs", "CREATE", staff.Id.ToString(), 
                        currentUserId.Value, ipAddress, null, staff);
                }

                _logger.LogInformation($"Created new staff: ID={staff.Id}, Name={staff.Name}, Role={staff.Role}");
                return CreatedAtAction(nameof(GetStaff), new { id = staff.Id }, staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating staff");
                return StatusCode(500, new { message = "Failed to create staff", error = ex.Message });
            }
        }

        /// <summary>
        /// Update staff information
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <param name="staff">Updated staff information</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStaff(int id, Staff staff)
        {
            if (id != staff.Id)
            {
                return BadRequest(new { message = "ID in path does not match ID in request body" });
            }

            try
            {
                var existingStaff = await _context.Staffs.FindAsync(id);
                if (existingStaff == null)
                {
                    return NotFound(new { message = $"Staff with ID {id} not found" });
                }

                // Update fields
                existingStaff.Name = staff.Name;
                existingStaff.Pin = staff.Pin;
                existingStaff.Email = staff.Email;
                existingStaff.Phone = staff.Phone;
                existingStaff.Address = staff.Address;
                existingStaff.HourlyRate = staff.HourlyRate;
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
        [Authorize]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            try
            {
                var staff = await _context.Staffs.FindAsync(id);
                if (staff == null)
                {
                    return NotFound(new { message = $"Staff with ID {id} not found" });
                }

                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // 🛡️ 关键安全检查
                // 1. 防止用户删除自己
                if (currentUserId.Value == id)
                {
                    return BadRequest(new { message = "您不能删除自己的账户" });
                }

                // 2. 防止删除最后一个管理员
                var targetStaffRole = staff.GetRoleFromId();
                if (targetStaffRole == "admin")
                {
                    var adminCount = await _context.Staffs
                        .Where(s => s.IsActive && s.Id >= 9000)  // Admin ID range
                        .CountAsync();
                    
                    if (adminCount <= 1)
                    {
                        return BadRequest(new { message = "不能删除最后一个系统管理员" });
                    }
                }

                // 3. 权限检查：只有管理员可以删除其他员工
                var currentUser = await _context.Staffs.FindAsync(currentUserId.Value);
                if (currentUser?.GetRoleFromId() != "admin")
                {
                    return Forbid("只有管理员可以删除员工账户");
                }

                // Soft delete
                staff.IsActive = false;
                staff.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Log audit trail
                var ipAddress = GetClientIpAddress();
                await _auditService.LogAsync("Staffs", "DELETE", staff.Id.ToString(), 
                    currentUserId.Value, ipAddress, $"Deleted staff: {staff.Name}");

                _logger.LogInformation($"Staff deleted by user {currentUserId}: ID={id}, Name={staff.Name}");
                return Ok(new { message = "员工已成功删除" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting staff ID:{id}");
                return StatusCode(500, new { message = "删除员工失败", error = ex.Message });
            }
        }

        /// <summary>
        /// Verify staff PIN for authentication
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <param name="pin">PIN code</param>
        /// <returns>Verification result with staff information</returns>
        [HttpPost("{id}/verify")]
        public async Task<ActionResult> VerifyPin(int id, [FromBody] string pin)
        {
            try
            {
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
                
                if (staff == null)
                {
                    return NotFound(new { message = "Staff does not exist or has been disabled" });
                }

                if (staff.Pin != pin)
                {
                    _logger.LogWarning($"Staff ID:{id} PIN verification failed");
                    return Unauthorized(new { message = "Incorrect PIN" });
                }

                // Set role
                staff.Role = staff.GetRoleFromId();

                _logger.LogInformation($"Staff ID:{id} PIN verification successful");
                return Ok(new { 
                    message = "Verification successful", 
                    staff = new {
                        id = staff.Id,
                        name = staff.Name,
                        role = staff.Role,
                        email = staff.Email
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred during PIN verification: ID={id}");
                return StatusCode(500, new { message = "Verification failed", error = ex.Message });
            }
        }

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
                var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
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
                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
                var recentEvent = await _context.Events
                    .Where(e => e.StaffId == id && e.EventType == eventType)
                    .Where(e => e.TimeStamp.CompareTo(oneMinuteAgo) > 0)
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
                    TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    CreatedAt = DateTime.UtcNow,
                    Reason = $"Quick {eventType.ToLower()} via API"
                };

                _context.Events.Add(newEvent);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Quick clock {eventType}: Staff ID={id}");
                return Ok(new { 
                    message = $"Clock {eventType.ToLower()} successful",
                    eventId = newEvent.EventId,
                    timeStamp = newEvent.TimeStamp,
                    staffName = staff.Name
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

        #endregion
    }
}
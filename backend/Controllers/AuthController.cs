using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;
using COMP9034.Backend.Services;

namespace COMP9034.Backend.Controllers
{
    /// <summary>
    /// Authentication API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;
        private readonly IAuditService _auditService;

        public AuthController(ApplicationDbContext context, IJwtService jwtService, ILogger<AuthController> logger, IAuditService auditService)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
            _auditService = auditService;
        }

        /// <summary>
        /// User login with username and password
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";

            try
            {
                // Find staff by email only
                var staff = await _context.Staff
                    .FirstOrDefaultAsync(s => 
                        s.Email == request.Email && 
                        s.IsActive);

                if (staff == null)
                {
                    await LogLoginAttempt(request.Email, ipAddress, userAgent, false, "User not found", null);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Verify password
                if (string.IsNullOrEmpty(staff.PasswordHash) || !VerifyPassword(request.Password, staff.PasswordHash))
                {
                    await LogLoginAttempt(request.Email, ipAddress, userAgent, false, "Invalid password", staff.StaffId);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Generate tokens
                var token = _jwtService.GenerateToken(staff);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Log successful login
                await LogLoginAttempt(request.Email, ipAddress, userAgent, true, null, staff.StaffId);

                var response = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(480), // 8 hours
                    Staff = new StaffInfo
                    {
                        Id = staff.Id,
                        Name = staff.Name,
                        Role = staff.GetRoleFromId(),
                        Email = staff.Email
                    }
                };

                _logger.LogInformation($"Successful login for staff ID: {staff.StaffId} from IP: {ipAddress}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login error for email: {request.Email} from IP: {ipAddress}");
                await LogLoginAttempt(request.Email, ipAddress, userAgent, false, "Internal server error", null);
                return StatusCode(500, new { message = "Login failed due to server error" });
            }
        }

        /// <summary>
        /// User registration with custom password
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>Registration result</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
        {
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";

            try
            {
                // Validate password confirmation
                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest(new { message = "Password and confirmation password do not match" });
                }

                // Check if email already exists
                if (await _context.Staff.AnyAsync(s => s.Email == request.Email))
                {
                    return BadRequest(new { message = "Email address is already registered" });
                }

                // Generate unique staff ID (starting from 2000 for self-registered users)
                var lastStaffId = await _context.Staff
                    .Where(s => s.StaffId >= 2000 && s.StaffId < 8000)
                    .OrderByDescending(s => s.StaffId)
                    .Select(s => s.StaffId)
                    .FirstOrDefaultAsync();
                
                var newStaffId = Math.Max(lastStaffId + 1, 2000);

                // Create new staff member
                var staff = new Staff
                {
                    StaffId = newStaffId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Phone = request.Phone,
                    Address = request.Address,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                    Pin = newStaffId.ToString(), // Temporary - will be removed later
                    StandardPayRate = 25.0m, // Default rate for self-registered users
                    OvertimePayRate = 37.5m,
                    ContractType = "Casual",
                    StandardHoursPerWeek = 20,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Staff.Add(staff);
                await _context.SaveChangesAsync();

                // Generate tokens for immediate login
                var token = _jwtService.GenerateToken(staff);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Log successful registration
                await LogLoginAttempt(request.Email, ipAddress, userAgent, true, "User registration", staff.StaffId);

                var response = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(480), // 8 hours
                    Staff = new StaffInfo
                    {
                        Id = staff.StaffId,
                        Name = staff.FirstName + " " + staff.LastName,
                        Role = staff.GetRoleFromId(),
                        Email = staff.Email
                    }
                };

                _logger.LogInformation($"Successful user registration: ID={staff.StaffId}, Email={request.Email}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Registration error for email: {request.Email}");
                return StatusCode(500, new { message = "Registration failed due to server error" });
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="request">Password change details</param>
        /// <returns>Change result</returns>
        [HttpPut("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                // Validate password confirmation
                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    return BadRequest(new { message = "New password and confirmation do not match" });
                }

                var staff = await _context.Staff.FindAsync(currentUserId.Value);
                if (staff == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Verify current password
                if (string.IsNullOrEmpty(staff.PasswordHash) || !VerifyPassword(request.CurrentPassword, staff.PasswordHash))
                {
                    return BadRequest(new { message = "Current password is incorrect" });
                }

                // Update password
                staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                staff.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Log password change
                var ipAddress = GetClientIpAddress();
                await _auditService.LogAsync("Staff", "PASSWORD_CHANGE", staff.StaffId.ToString(), 
                    currentUserId.Value, ipAddress, "Password changed successfully");

                _logger.LogInformation($"Password changed for user ID: {currentUserId}");
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "Failed to change password" });
            }
        }

        /// <summary>
        /// Get current user information
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<StaffInfo>> GetCurrentUser()
        {
            try
            {
                var staffIdClaim = User.FindFirst(TokenClaims.StaffId)?.Value;
                if (!int.TryParse(staffIdClaim, out int staffId))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                var staff = await _context.Staff
                    .FirstOrDefaultAsync(s => s.Id == staffId && s.IsActive);

                if (staff == null)
                {
                    return NotFound(new { message = "Staff not found" });
                }

                var staffInfo = new StaffInfo
                {
                    Id = staff.Id,
                    Name = staff.Name,
                    Role = staff.GetRoleFromId(),
                    Email = staff.Email
                };

                return Ok(staffInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user information");
                return StatusCode(500, new { message = "Failed to get user information" });
            }
        }

        /// <summary>
        /// Logout (token blacklisting would be implemented here in production)
        /// </summary>
        /// <returns>Logout confirmation</returns>
        [HttpPost("logout")]
        [Authorize]
        public ActionResult Logout()
        {
            // In a production system, you would typically:
            // 1. Add the token to a blacklist
            // 2. Remove the refresh token from storage
            // 3. Log the logout event
            
            _logger.LogInformation($"User logged out: {User.FindFirst(TokenClaims.StaffId)?.Value}");
            return Ok(new { message = "Successfully logged out" });
        }

        /// <summary>
        /// Get login logs (admin only)
        /// </summary>
        /// <param name="limit">Maximum number of records</param>
        /// <param name="offset">Offset for pagination</param>
        /// <returns>List of login logs</returns>
        [HttpGet("login-logs")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<object>>> GetLoginLogs(
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0)
        {
            try
            {
                // Debug: Log user information
                var userId = User.FindFirst(TokenClaims.StaffId)?.Value;
                var userName = User.FindFirst(TokenClaims.Name)?.Value;
                var userRole = User.FindFirst(TokenClaims.Role)?.Value;
                var userClaims = User.Claims.Select(c => $"{c.Type}: {c.Value}").ToList();
                
                _logger.LogInformation($"GetLoginLogs - User ID: {userId}, Name: {userName}, Role: {userRole}");
                _logger.LogInformation($"GetLoginLogs - All claims: {string.Join(", ", userClaims)}");
                var logs = await _context.LoginLogs
                    .Include(l => l.Staff)
                    .OrderByDescending(l => l.Timestamp)
                    .Skip(offset)
                    .Take(limit)
                    .Select(l => new
                    {
                        id = l.Id,
                        username = l.Username,
                        ipAddress = l.IpAddress,
                        success = l.Success,
                        failureReason = l.FailureReason,
                        timestamp = l.Timestamp,
                        staffId = l.StaffId,
                        staffName = l.Staff != null ? l.Staff.Name : null
                    })
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving login logs");
                return StatusCode(500, new { message = "Failed to retrieve login logs" });
            }
        }

        /// <summary>
        /// Delete a specific login log record
        /// </summary>
        /// <param name="id">Login log ID to delete</param>
        /// <returns>Success message</returns>
        [HttpDelete("login-logs/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> DeleteLoginLog(int id)
        {
            try
            {
                var loginLog = await _context.LoginLogs.FindAsync(id);
                if (loginLog == null)
                {
                    return NotFound(new { message = "Login log not found" });
                }

                _context.LoginLogs.Remove(loginLog);
                await _context.SaveChangesAsync();

                // Log the deletion
                var currentUserId = GetCurrentUserId();
                var ipAddress = GetClientIpAddress();
                await _auditService.LogAsync("LoginLogs", "DELETE", id.ToString(),
                    currentUserId ?? 0, ipAddress, $"Deleted login log: {loginLog.Username}");

                _logger.LogInformation($"Login log deleted by user {currentUserId}: ID={id}, Username={loginLog.Username}");

                return Ok(new { message = "Login log deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting login log");
                return StatusCode(500, new { message = "Failed to delete login log" });
            }
        }

        #region Private Methods

        private async Task LogLoginAttempt(string username, string ipAddress, string userAgent, 
            bool success, string? failureReason, int? staffId)
        {
            try
            {
                var loginLog = new LoginLog
                {
                    Username = username,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Success = success,
                    FailureReason = failureReason,
                    Timestamp = DateTime.UtcNow,
                    StaffId = staffId
                };

                _context.LoginLogs.Add(loginLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to log login attempt for username: {username}");
                // Don't throw - logging failure shouldn't break login flow
            }
        }

        private string GetClientIpAddress()
        {
            try
            {
                // Log all available headers for debugging
                var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
                var remoteIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
                
                _logger.LogDebug($"IP Detection - X-Forwarded-For: {forwardedFor ?? "null"}, X-Real-IP: {realIp ?? "null"}, RemoteIP: {remoteIp ?? "null"}");
                
                // Check for forwarded IP first (load balancer/proxy)
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    var clientIp = forwardedFor.Split(',')[0].Trim();
                    _logger.LogDebug($"Using X-Forwarded-For IP: {clientIp}");
                    return clientIp;
                }

                if (!string.IsNullOrEmpty(realIp))
                {
                    _logger.LogDebug($"Using X-Real-IP: {realIp}");
                    return realIp;
                }

                // For development, try to get the actual client IP from connection
                var finalIp = remoteIp ?? "Unknown";
                _logger.LogDebug($"Using RemoteIpAddress: {finalIp}");
                return finalIp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client IP address");
                return "Unknown";
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

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

        #endregion
    }
}
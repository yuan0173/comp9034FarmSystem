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

        public AuthController(ApplicationDbContext context, IJwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
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
                // Find staff by username or email
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => 
                        (s.Username == request.Username || s.Email == request.Username) && 
                        s.IsActive);

                if (staff == null)
                {
                    await LogLoginAttempt(request.Username, ipAddress, userAgent, false, "User not found", null);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Verify password
                if (string.IsNullOrEmpty(staff.PasswordHash) || !VerifyPassword(request.Password, staff.PasswordHash))
                {
                    await LogLoginAttempt(request.Username, ipAddress, userAgent, false, "Invalid password", staff.Id);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Generate tokens
                var token = _jwtService.GenerateToken(staff);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Log successful login
                await LogLoginAttempt(request.Username, ipAddress, userAgent, true, null, staff.Id);

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

                _logger.LogInformation($"Successful login for staff ID: {staff.Id} from IP: {ipAddress}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login error for username: {request.Username} from IP: {ipAddress}");
                await LogLoginAttempt(request.Username, ipAddress, userAgent, false, "Internal server error", null);
                return StatusCode(500, new { message = "Login failed due to server error" });
            }
        }

        /// <summary>
        /// PIN-based login (for terminal devices)
        /// </summary>
        /// <param name="request">PIN login request</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login-pin")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> LoginWithPin([FromBody] PinLoginRequest request)
        {
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";

            try
            {
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.Id == request.StaffId && s.IsActive);

                if (staff == null)
                {
                    await LogLoginAttempt(request.StaffId.ToString(), ipAddress, userAgent, false, "Staff not found", null);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                if (staff.Pin != request.Pin)
                {
                    await LogLoginAttempt(request.StaffId.ToString(), ipAddress, userAgent, false, "Invalid PIN", request.StaffId);
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Generate tokens
                var token = _jwtService.GenerateToken(staff);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // Log successful login
                await LogLoginAttempt(request.StaffId.ToString(), ipAddress, userAgent, true, null, request.StaffId);

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

                _logger.LogInformation($"Successful PIN login for staff ID: {request.StaffId} from IP: {ipAddress}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"PIN login error from IP: {ipAddress}");
                return StatusCode(500, new { message = "Login failed due to server error" });
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

                var staff = await _context.Staffs
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
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetLoginLogs(
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0)
        {
            try
            {
                // Check if user is admin
                var role = User.FindFirst(TokenClaims.Role)?.Value;
                if (role != "admin")
                {
                    return StatusCode(403, new { message = "Access denied. Admin privileges required." });
                }

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

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        #endregion
    }
}
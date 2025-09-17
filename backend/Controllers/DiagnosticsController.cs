using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP9034.Backend.Data;

namespace COMP9034.Backend.Controllers
{
    /// <summary>
    /// Diagnostics controller for debugging production issues
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiagnosticsController> _logger;

        public DiagnosticsController(ApplicationDbContext context, ILogger<DiagnosticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get database connection and data status
        /// </summary>
        [HttpGet("database-status")]
        public async Task<IActionResult> GetDatabaseStatus()
        {
            try
            {
                var result = new
                {
                    CanConnect = false,
                    StaffCount = 0,
                    DeviceCount = 0,
                    EventCount = 0,
                    ConnectionString = "Hidden for security",
                    DatabaseProvider = _context.Database.ProviderName,
                    PendingMigrations = new string[0],
                    AppliedMigrations = new string[0],
                    Error = (string?)null
                };

                // Test database connection
                await _context.Database.OpenConnectionAsync();
                result = result with { CanConnect = true };
                await _context.Database.CloseConnectionAsync();

                // Get data counts
                var staffCount = await _context.Staff.CountAsync();
                var deviceCount = await _context.Device.CountAsync();
                var eventCount = await _context.Events.CountAsync();

                // Get migration status
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();

                result = result with 
                { 
                    StaffCount = staffCount,
                    DeviceCount = deviceCount,
                    EventCount = eventCount,
                    PendingMigrations = pendingMigrations.ToArray(),
                    AppliedMigrations = appliedMigrations.ToArray()
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database diagnostics failed");
                return Ok(new
                {
                    CanConnect = false,
                    StaffCount = 0,
                    DeviceCount = 0,
                    EventCount = 0,
                    ConnectionString = "Hidden for security",
                    DatabaseProvider = _context.Database.ProviderName,
                    PendingMigrations = new string[0],
                    AppliedMigrations = new string[0],
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get sample staff data for debugging
        /// </summary>
        [HttpGet("staff-sample")]
        public async Task<IActionResult> GetStaffSample()
        {
            try
            {
                var staff = await _context.Staff
                    .Select(s => new 
                    {
                        s.StaffId,
                        s.FirstName,
                        s.LastName,
                        s.Email,
                        s.Role,
                        s.Username,
                        HasPassword = !string.IsNullOrEmpty(s.PasswordHash),
                        s.IsActive
                    })
                    .Take(10)
                    .ToListAsync();

                return Ok(new { Count = staff.Count, Staff = staff });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Staff sample query failed");
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Force database initialization
        /// </summary>
        [HttpPost("force-init")]
        public async Task<IActionResult> ForceInitialization()
        {
            try
            {
                // Apply pending migrations
                await _context.Database.MigrateAsync();

                // Check if staff exists
                var staffCount = await _context.Staff.CountAsync();
                if (staffCount == 0)
                {
                    // Create default admin user
                    var admin = new Models.Staff
                    {
                        FirstName = "System",
                        LastName = "Administrator",
                        Email = "admin@farmtimems.com",
                        Role = "Admin",
                        StandardPayRate = 50.00m,
                        OvertimePayRate = 75.00m,
                        ContractType = "FullTime",
                        StandardHoursPerWeek = 40,
                        Pin = "1234",
                        Username = "admin",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Staff.Add(admin);
                    await _context.SaveChangesAsync();
                }

                // Check if devices exist
                var deviceCount = await _context.Device.CountAsync();
                if (deviceCount == 0)
                {
                    var devices = new[]
                    {
                        new Models.Device
                        {
                            DeviceName = "Main Terminal",
                            DeviceType = "terminal",
                            Location = "Main Entrance",
                            Status = "Active",
                            IpAddress = "192.168.1.100",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Models.Device
                        {
                            DeviceName = "Biometric Scanner",
                            DeviceType = "biometric",
                            Location = "Security Office",
                            Status = "Active",
                            IpAddress = "192.168.1.101",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    _context.Device.AddRange(devices);
                    await _context.SaveChangesAsync();
                }

                return Ok(new 
                { 
                    Message = "Database initialization completed",
                    StaffCount = await _context.Staff.CountAsync(),
                    DeviceCount = await _context.Device.CountAsync()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Force initialization failed");
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}

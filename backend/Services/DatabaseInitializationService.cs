using COMP9034.Backend.Data;
using COMP9034.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace COMP9034.Backend.Services
{
    /// <summary>
    /// Service to ensure database is properly initialized with seed data
    /// </summary>
    public class DatabaseInitializationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseInitializationService> _logger;

        public DatabaseInitializationService(ApplicationDbContext context, ILogger<DatabaseInitializationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Initialize database with required seed data
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                // Ensure database is created and migrations are applied
                await _context.Database.MigrateAsync();
                _logger.LogInformation("✅ Database migrations applied successfully");

                // Check and create default users
                await EnsureDefaultUsersExist();

                // Check and create default devices
                await EnsureDefaultDevicesExist();

                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Database initialization failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task EnsureDefaultUsersExist()
        {
            // Check if any staff exists
            if (await _context.Staff.AnyAsync())
            {
                _logger.LogInformation("Staff data already exists, skipping user creation");
                return;
            }

            _logger.LogInformation("Creating default users...");

            var defaultUsers = new[]
            {
                new Staff
                {
                    StaffId = 9001,
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
                },
                new Staff
                {
                    StaffId = 8001,
                    FirstName = "Farm",
                    LastName = "Manager",
                    Email = "manager@farmtimems.com",
                    Role = "Manager",
                    StandardPayRate = 35.00m,
                    OvertimePayRate = 52.50m,
                    ContractType = "FullTime",
                    StandardHoursPerWeek = 40,
                    Pin = "8001",
                    Username = "manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Staff
                {
                    StaffId = 1001,
                    FirstName = "Farm",
                    LastName = "Worker",
                    Email = "worker@farmtimems.com",
                    Role = "Staff",
                    StandardPayRate = 25.00m,
                    OvertimePayRate = 37.50m,
                    ContractType = "Casual",
                    StandardHoursPerWeek = 20,
                    Pin = "1001",
                    Username = "worker",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("worker123"),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            await _context.Staff.AddRangeAsync(defaultUsers);
            _logger.LogInformation("✅ Created {Count} default users", defaultUsers.Length);
        }

        private async Task EnsureDefaultDevicesExist()
        {
            // Check if any devices exist
            if (await _context.Device.AnyAsync())
            {
                _logger.LogInformation("Device data already exists, skipping device creation");
                return;
            }

            _logger.LogInformation("Creating default devices...");

            var defaultDevices = new[]
            {
                new Device
                {
                    DeviceId = 1,
                    DeviceName = "Main Terminal",
                    DeviceType = "terminal",
                    Location = "Main Entrance",
                    Status = "Active",
                    IpAddress = "192.168.1.100",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Device
                {
                    DeviceId = 2,
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

            await _context.Device.AddRangeAsync(defaultDevices);
            _logger.LogInformation("✅ Created {Count} default devices", defaultDevices.Length);
        }
    }
}

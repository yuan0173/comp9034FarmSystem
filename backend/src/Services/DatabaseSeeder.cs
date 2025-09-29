using Microsoft.EntityFrameworkCore;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;

namespace COMP9034.Backend.Services
{
    /// <summary>
    /// Database seeding service for production environment
    /// Ensures required initial data exists without relying on EF migrations
    /// </summary>
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ensure all required seed data exists in the database
        /// </summary>
        public async Task SeedAsync()
        {
            try
            {
                await SeedStaffAsync();
                await SeedDevicesAsync();
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Database seeding failed: {Message}", ex.Message);
                throw;
            }
        }

        private async Task SeedStaffAsync()
        {
            // Check if admin user exists
            var adminExists = await _context.Staff.AnyAsync(s => s.Email == "admin@farmtimems.com");
            if (!adminExists)
            {
                _logger.LogInformation("🔄 Creating admin users...");

                var staffToAdd = new List<Staff>
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
                        Pin = "9001",
                        Username = null,
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
                        Username = null,
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
                        Username = null,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("worker123"),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Staff
                    {
                        StaffId = 2001,
                        FirstName = "Test",
                        LastName = "Worker",
                        Email = "test@example.com",
                        Role = "Staff",
                        StandardPayRate = 25.00m,
                        OvertimePayRate = 37.50m,
                        ContractType = "Casual",
                        StandardHoursPerWeek = 20,
                        Pin = "2001",
                        Username = null,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await _context.Staff.AddRangeAsync(staffToAdd);
                _logger.LogInformation("✅ Created {Count} staff members", staffToAdd.Count);
            }
            else
            {
                _logger.LogInformation("✅ Admin users already exist");
            }
        }

        private async Task SeedDevicesAsync()
        {
            var deviceExists = await _context.Device.AnyAsync();
            if (!deviceExists)
            {
                _logger.LogInformation("🔄 Creating sample devices...");

                var devicesToAdd = new List<Device>
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

                await _context.Device.AddRangeAsync(devicesToAdd);
                _logger.LogInformation("✅ Created {Count} devices", devicesToAdd.Count);
            }
            else
            {
                _logger.LogInformation("✅ Devices already exist");
            }
        }
    }
}
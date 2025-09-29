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
            // Ensure-required staff accounts exist individually (idempotent)
            var seeds = new List<(int id, string first, string last, string email, string role, decimal stdRate, decimal otRate, string contract, int hours, string password)>
            {
                (9001, "System", "Administrator", "admin@farmtimems.com", "Admin", 50.00m, 75.00m, "FullTime", 40, "admin123"),
                (8001, "Farm",   "Manager",       "manager@farmtimems.com", "Manager", 35.00m, 52.50m, "FullTime", 40, "manager123"),
                (1001, "Farm",   "Worker",        "worker@farmtimems.com",  "Staff",   25.00m, 37.50m, "Casual",   20, "worker123"),
                (2001, "Test",   "Worker",        "test@example.com",       "Staff",   25.00m, 37.50m, "Casual",   20, "test123")
            };

            int created = 0;
            foreach (var s in seeds)
            {
                var exists = await _context.Staff.AnyAsync(x => x.StaffId == s.id || x.Email == s.email);
                if (exists)
                {
                    continue;
                }

                var staff = new Staff
                {
                    StaffId = s.id,
                    FirstName = s.first,
                    LastName = s.last,
                    Email = s.email,
                    Role = s.role,
                    StandardPayRate = s.stdRate,
                    OvertimePayRate = s.otRate,
                    ContractType = s.contract,
                    StandardHoursPerWeek = s.hours,
                    Pin = s.id.ToString(),
                    Username = null,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(s.password),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Staff.AddAsync(staff);
                created++;
            }

            if (created > 0)
            {
                _logger.LogInformation("✅ Seeded {Count} missing staff accounts", created);
            }
            else
            {
                _logger.LogInformation("✅ All seed staff accounts already exist");
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

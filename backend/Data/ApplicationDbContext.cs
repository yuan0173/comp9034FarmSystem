using Microsoft.EntityFrameworkCore;
using COMP9034.Backend.Models;

namespace COMP9034.Backend.Data
{
    /// <summary>
    /// Application database context
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet definitions
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<BiometricData> BiometricData { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Staff entity configuration
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Pin).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Username).HasMaxLength(50);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.HasIndex(e => e.Id).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique().HasFilter("[Email] IS NOT NULL");
                entity.HasIndex(e => e.Username).IsUnique().HasFilter("[Username] IS NOT NULL");
            });

            // Event entity configuration (aligned with frontend)
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.TimeStamp).IsRequired().HasMaxLength(50);
                
                // Staff relationship (optional for API flexibility)
                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);
                
                // Device relationship  
                entity.HasOne(d => d.Device)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Admin relationship (new)
                entity.HasOne(d => d.Admin)
                    .WithMany()
                    .HasForeignKey(d => d.AdminId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Indexes
                entity.HasIndex(e => new { e.StaffId, e.TimeStamp });
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.AdminId);
            });

            // Device entity configuration
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IpAddress).HasMaxLength(15);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // BiometricData entity configuration
            modelBuilder.Entity<BiometricData>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BiometricType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TemplateData).IsRequired();
                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.BiometricData)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.StaffId, e.BiometricType }).IsUnique();
            });

            // LoginLog entity configuration
            modelBuilder.Entity<LoginLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
                entity.Property(e => e.UserAgent).IsRequired().HasMaxLength(200);
                entity.Property(e => e.FailureReason).HasMaxLength(500);
                entity.HasOne(d => d.Staff)
                    .WithMany()
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.Username, e.Timestamp });
            });

            // AuditLog entity configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TableName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Operation).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RecordId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(45);
                entity.HasOne(d => d.PerformedByStaff)
                    .WithMany()
                    .HasForeignKey(d => d.PerformedByStaffId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.TableName, e.RecordId });
            });

            // Seed data
            SeedData(modelBuilder);
        }

        /// <summary>
        /// Initialize seed data
        /// </summary>
        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Administrator user
            modelBuilder.Entity<Staff>().HasData(
                new Staff
                {
                    Id = 9001,
                    Name = "System Administrator",
                    Role = "admin",
                    Pin = "1234",
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Default: admin123
                    HourlyRate = 50.00m,
                    Email = "admin@farmtimems.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Sample manager
            modelBuilder.Entity<Staff>().HasData(
                new Staff
                {
                    Id = 8001,
                    Name = "Farm Manager",
                    Role = "manager",
                    Pin = "8001",
                    Username = "manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), // Default: manager123
                    HourlyRate = 35.00m,
                    Email = "manager@farmtimems.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Sample worker
            modelBuilder.Entity<Staff>().HasData(
                new Staff
                {
                    Id = 1001,
                    Name = "Farm Worker",
                    Role = "staff",
                    Pin = "1001",
                    Username = "worker",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("worker123"), // Default: worker123
                    HourlyRate = 25.00m,
                    Email = "worker@farmtimems.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Sample devices
            modelBuilder.Entity<Device>().HasData(
                new Device
                {
                    Id = 1,
                    Name = "Main Terminal",
                    Type = "terminal",
                    Location = "Main Entrance",
                    Status = "active",
                    IpAddress = "192.168.1.100",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Device
                {
                    Id = 2,
                    Name = "Biometric Scanner",
                    Type = "biometric",
                    Location = "Security Office",
                    Status = "active",
                    IpAddress = "192.168.1.101",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
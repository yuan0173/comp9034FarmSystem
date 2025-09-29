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

        // DbSet definitions - Updated for Tan Architecture
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Device> Device { get; set; }
        public DbSet<BiometricData> BiometricData { get; set; }
        public DbSet<LoginLog> LoginLogs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        
        // New Tan Architecture tables
        public DbSet<WorkSchedule> WorkSchedule { get; set; }
        public DbSet<Salary> Salary { get; set; }
        public DbSet<BiometricVerification> BiometricVerification { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Staff entity configuration - Updated for Tan Architecture
            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("Staff");
                entity.HasKey(e => e.StaffId);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.StandardPayRate).HasColumnType("decimal(10,2)");
                entity.Property(e => e.OvertimePayRate).HasColumnType("decimal(10,2)");
                entity.Property(e => e.ContractType).IsRequired().HasMaxLength(20).HasDefaultValue("Casual");
                entity.Property(e => e.StandardHoursPerWeek).HasDefaultValue(40);
                entity.Property(e => e.Pin).HasMaxLength(10).IsRequired(); // Required field in database
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired(false); // DEPRECATED - use email instead
                entity.Property(e => e.PasswordHash).HasMaxLength(255).IsRequired(false); // Optional field in database
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
                
                entity.HasIndex(e => e.Email).IsUnique();
                // Use partial index to enforce uniqueness only when Username is not null
                entity.HasIndex(e => e.Username).IsUnique().HasFilter("Username IS NOT NULL");
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.IsActive);
            });

            // Event entity configuration - Matches existing database
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Events");
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.OccurredAt).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                // Staff relationship (optional for API flexibility)
                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                // Admin relationship for manual overrides
                entity.HasOne(d => d.Admin)
                    .WithMany()
                    .HasForeignKey(d => d.AdminId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                // Device relationship
                entity.HasOne(d => d.Device)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                // Indexes
                entity.HasIndex(e => new { e.StaffId, e.OccurredAt });
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.OccurredAt);
                entity.HasIndex(e => e.AdminId);
            });

            // Device entity configuration - Updated for Tan Architecture
            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("Device");
                entity.HasKey(e => e.DeviceId);
                entity.Property(e => e.DeviceName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DeviceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
                
                entity.HasIndex(e => e.DeviceName).IsUnique();
                entity.HasIndex(e => e.Status);
            });

            // BiometricData entity configuration - Tan Architecture Schema
            modelBuilder.Entity<BiometricData>(entity =>
            {
                entity.ToTable("BiometricData");
                entity.HasKey(e => e.BiometricId);
                entity.Property(e => e.BiometricTemplate).IsRequired().HasColumnType("text");
                entity.Property(e => e.Salt).IsRequired().HasMaxLength(64);
                entity.Property(e => e.TemplateHash).IsRequired().HasMaxLength(128);
                entity.Property(e => e.DeviceEnrollment).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.Property(e => e.LastUpdated).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.BiometricData)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.DeviceEnrollmentDevice)
                    .WithMany()
                    .HasForeignKey(d => d.DeviceEnrollment)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.StaffId);
                entity.HasIndex(e => e.TemplateHash);
                entity.HasIndex(e => e.IsActive);
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

            // WorkSchedule entity configuration - Tan Architecture Schema
            modelBuilder.Entity<WorkSchedule>(entity =>
            {
                entity.ToTable("WorkSchedule");
                entity.HasKey(e => e.ScheduleID); // Updated to match Tan Schema
                entity.Property(e => e.Date).IsRequired().HasColumnType("date");
                entity.Property(e => e.StartTime).IsRequired().HasColumnType("time");
                entity.Property(e => e.EndTime).IsRequired().HasColumnType("time");
                entity.Property(e => e.ScheduleHours).HasColumnType("decimal(8,2)"); // New field from Tan Schema

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.WorkSchedules)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.StaffId, e.Date });
                entity.HasIndex(e => e.Date);
            });

            // Salary entity configuration - New Tan Architecture table
            modelBuilder.Entity<Salary>(entity =>
            {
                entity.ToTable("Salary");
                entity.HasKey(e => e.SalaryId);
                entity.Property(e => e.PayPeriodStart).IsRequired();
                entity.Property(e => e.PayPeriodEnd).IsRequired();
                entity.Property(e => e.TotalHours).HasColumnType("decimal(8,2)").HasDefaultValue(0);
                entity.Property(e => e.TotalOvertimeHours).HasColumnType("decimal(8,2)").HasDefaultValue(0);
                entity.Property(e => e.GrossPay).HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(e => e.GeneratedDate).HasDefaultValueSql("NOW()");
                
                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Salaries)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.StaffId, e.PayPeriodStart, e.PayPeriodEnd });
                entity.HasIndex(e => e.Status);
            });

            // BiometricVerification entity configuration - Tan Architecture Schema
            modelBuilder.Entity<BiometricVerification>(entity =>
            {
                entity.ToTable("BiometricVerification");
                entity.HasKey(e => e.VerificationId);
                entity.Property(e => e.VerificationResult).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ConfidenceScore).HasColumnType("decimal(5,3)");
                entity.Property(e => e.FailureReason).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).IsRequired();

                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.BiometricVerifications)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                entity.HasOne(d => d.BiometricData)
                    .WithMany(p => p.BiometricVerifications)
                    .HasForeignKey(d => d.BiometricId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                entity.HasOne(d => d.Device)
                    .WithMany(p => p.BiometricVerifications)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                entity.HasOne(d => d.Event)
                    .WithMany()
                    .HasForeignKey(d => d.EventId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired(false);

                entity.HasIndex(e => e.StaffId);
                entity.HasIndex(e => e.BiometricId);
                entity.HasIndex(e => e.DeviceId);
                entity.HasIndex(e => e.EventId);
                entity.HasIndex(e => e.VerificationResult);
            });

            // Seed data now handled by DatabaseSeeder service
            // SeedData(modelBuilder);
        }

        /// <summary>
        /// Initialize seed data
        /// </summary>
        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Administrator user - Email/Password only authentication
            modelBuilder.Entity<Staff>().HasData(
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
                    Pin = "9001", // Admin PIN
                    Username = null, // Username removed - email/password only
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Password: admin123
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Sample manager - Email/Password only authentication
            modelBuilder.Entity<Staff>().HasData(
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
                    Pin = "9001", // Admin PIN
                    Username = null, // Username removed - email/password only
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), // Password: manager123
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Sample worker - Email/Password only authentication
            modelBuilder.Entity<Staff>().HasData(
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
                    Pin = "9001", // Admin PIN
                    Username = null, // Username removed - email/password only
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("worker123"), // Password: worker123
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Test worker - Email/Password only authentication
            modelBuilder.Entity<Staff>().HasData(
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
                    Pin = "9001", // Admin PIN
                    Username = null, // Username removed - email/password only
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("test123"), // Password: test123
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Sample devices - Updated for Tan Architecture
            modelBuilder.Entity<Device>().HasData(
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
            );
        }
    }
}

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
                entity.Property(e => e.Pin).HasMaxLength(10);
                entity.Property(e => e.Username).HasMaxLength(50);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                
                entity.HasIndex(e => e.Email).IsUnique();
                // Unique when not null (PostgreSQL partial index syntax)
                entity.HasIndex(e => e.Username).IsUnique().HasFilter("\"Username\" IS NOT NULL");
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.IsActive);
            });

            // Event entity configuration - Updated for Tan Architecture
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable("Events");
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.OccurredAt).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                
                // Staff relationship (optional for API flexibility)
                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
                
                // Device relationship  
                entity.HasOne(d => d.Device)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.DeviceId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                // Admin (manual override performer)
                entity.HasOne(d => d.Admin)
                    .WithMany()
                    .HasForeignKey(d => d.AdminId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);

                // CHECK constraint for eventType includes MANUAL_OVERRIDE (PostgreSQL syntax)
                entity.HasCheckConstraint("ck_events_type", "\"EventType\" IN ('CLOCK_IN','CLOCK_OUT','BREAK_START','BREAK_END','MANUAL_OVERRIDE')");
                
                // Indexes
                entity.HasIndex(e => new { e.StaffId, e.OccurredAt });
                entity.HasIndex(e => e.EventType);
                entity.HasIndex(e => e.OccurredAt);
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
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                
                entity.HasIndex(e => e.DeviceName).IsUnique();
                entity.HasIndex(e => e.Status);
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

            // WorkSchedule entity configuration - New Tan Architecture table
            modelBuilder.Entity<WorkSchedule>(entity =>
            {
                entity.ToTable("WorkSchedule");
                entity.HasKey(e => e.ScheduleId);
                entity.Property(e => e.ScheduledDate).IsRequired();
                entity.Property(e => e.StartTime).IsRequired();
                entity.Property(e => e.EndTime).IsRequired();
                entity.Property(e => e.ScheduleHours).HasColumnType("decimal(8,2)").HasDefaultValue(0);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Scheduled");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                
                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.WorkSchedules)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.StaffId, e.ScheduledDate });
                entity.HasIndex(e => e.ScheduledDate);
                entity.HasIndex(e => e.Status);
            });

            // Salary entity configuration - New Tan Architecture table
            modelBuilder.Entity<Salary>(entity =>
            {
                entity.ToTable("Salary");
                entity.HasKey(e => e.SalaryId);
                entity.Property(e => e.PayPeriodStart).IsRequired();
                entity.Property(e => e.PayPeriodEnd).IsRequired();
                entity.Property(e => e.TotalHours).HasColumnType("decimal(8,2)").HasDefaultValue(0);
                entity.Property(e => e.OvertimeHours).HasColumnType("decimal(8,2)").HasDefaultValue(0);
                entity.Property(e => e.RegularPay).HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(e => e.OvertimePay).HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(e => e.GrossPay).HasColumnType("decimal(10,2)").HasDefaultValue(0);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Draft");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                
                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.Salaries)
                    .HasForeignKey(d => d.StaffId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.StaffId, e.PayPeriodStart, e.PayPeriodEnd });
                entity.HasIndex(e => e.Status);
            });

            // BiometricVerification entity configuration - New Tan Architecture table
            modelBuilder.Entity<BiometricVerification>(entity =>
            {
                entity.ToTable("BiometricVerification");
                entity.HasKey(e => e.VerificationId);
                entity.Property(e => e.VerificationResult).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ConfidenceScore).HasColumnType("decimal(5,3)");
                entity.Property(e => e.FailureReason).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
                
                entity.HasOne(d => d.Staff)
                    .WithMany(p => p.BiometricVerifications)
                    .HasForeignKey(d => d.StaffId)
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

                entity.HasOne(d => d.Biometric)
                    .WithMany()
                    .HasForeignKey(d => d.BiometricId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false);
                
                entity.HasIndex(e => e.StaffId);
                entity.HasIndex(e => e.EventId);
                entity.HasIndex(e => e.VerificationResult);
            });

            // BiometricData additions for security fields
            modelBuilder.Entity<BiometricData>(entity =>
            {
                entity.Property<string?>("Salt");
                entity.Property<string?>("TemplateHash");
                entity.Property<int?>("DeviceEnrollment");
                entity.HasIndex("TemplateHash");
            });

            // Seed data disabled - using migrated database with existing data
            // SeedData(modelBuilder);
        }

        /// <summary>
        /// Initialize seed data
        /// </summary>
        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Administrator user - Updated for Tan Architecture
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
                    Pin = "1234",
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"), // Default: admin123
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Sample manager
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
                    Pin = "8001",
                    Username = "manager",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager123"), // Default: manager123
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Sample worker
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
                    Pin = "1001",
                    Username = "worker",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("worker123"), // Default: worker123
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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Staff entity model - Built from Tan Architecture Database Schema
    /// Implements complete schema specification for staff management
    /// </summary>
    public class Staff
    {
        /// <summary>
        /// (PK) staffId: Unique identifier for each staff member
        /// </summary>
        [Key]
        public int StaffId { get; set; }

        /// <summary>
        /// firstName: Staff member's first name
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// lastName: Staff member's last name
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// email: Staff member's email address
        /// </summary>
        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// phone: Staff member's phone number
        /// </summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>
        /// address: Staff member's physical address
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// contractType: Type of employment contract (Casual, Full Time, Part Time)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ContractType { get; set; } = "Casual";

        /// <summary>
        /// isActive: Boolean flag indicating if staff member is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// role: Staff member's job role/position
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Staff";

        /// <summary>
        /// standardHoursPerWeek: Standard working hours per week for the staff member
        /// </summary>
        public int StandardHoursPerWeek { get; set; } = 40;

        /// <summary>
        /// standardPayRate: Regular hourly pay rate
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal StandardPayRate { get; set; }

        /// <summary>
        /// overtimePayRate: Overtime hourly pay rate
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal OvertimePayRate { get; set; }

        // Authentication fields (for system functionality)
        /// <summary>
        /// Password hash for email/password authentication
        /// </summary>
        [MaxLength(255)]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// PIN field for authentication (matches database - NOT NULL)
        /// </summary>
        [Required]
        [MaxLength(10)]
        public string Pin { get; set; } = string.Empty;

        /// <summary>
        /// Username field for backward compatibility (deprecated in Tan Schema)
        /// </summary>
        [MaxLength(50)]
        public string? Username { get; set; }

        // Audit fields
        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties based on Schema relationships
        /// <summary>
        /// Staff (1) → (N) WorkSchedule: One staff member can have multiple scheduled shifts
        /// </summary>
        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();

        /// <summary>
        /// Staff (1) → (N) Events: One staff member can generate multiple events
        /// </summary>
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();

        /// <summary>
        /// Staff (1) → (N) Salary: One staff member can have multiple salary records
        /// </summary>
        public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();

        /// <summary>
        /// Staff (1) → (N) BiometricData: One staff member can have multiple biometric templates
        /// </summary>
        public virtual ICollection<BiometricData> BiometricData { get; set; } = new List<BiometricData>();

        /// <summary>
        /// Staff (1) → (N) BiometricVerification: One staff member can have multiple verification attempts
        /// </summary>
        public virtual ICollection<BiometricVerification> BiometricVerifications { get; set; } = new List<BiometricVerification>();

        // Computed properties for backward compatibility
        /// <summary>
        /// Full name property for display purposes
        /// </summary>
        [NotMapped]
        public string Name => $"{FirstName} {LastName}";

        /// <summary>
        /// Legacy property for backward compatibility
        /// </summary>
        [NotMapped]
        public decimal HourlyRate
        {
            get => StandardPayRate;
            set => StandardPayRate = value;
        }

        /// <summary>
        /// Legacy property for backward compatibility
        /// </summary>
        [NotMapped]
        public int Id
        {
            get => StaffId;
            set => StaffId = value;
        }

        /// <summary>
        /// Determine role based on staff ID (legacy business logic)
        /// </summary>
        public string GetRoleFromId()
        {
            if (StaffId >= 9000) return "Admin";
            if (StaffId >= 8000 && StaffId <= 8999) return "Manager";
            return "Staff";
        }
    }
}
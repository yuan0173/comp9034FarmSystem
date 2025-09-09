using System.ComponentModel.DataAnnotations.Schema;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Staff entity model - Updated for Tan Architecture
    /// </summary>
    public class Staff
    {
        /// <summary>
        /// Staff ID (Primary Key)
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Email address (required, unique)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User role: 'Staff', 'Manager', 'Admin'
        /// </summary>
        public string Role { get; set; } = "Staff";

        /// <summary>
        /// Standard hourly pay rate
        /// </summary>
        public decimal StandardPayRate { get; set; }

        /// <summary>
        /// Overtime hourly pay rate
        /// </summary>
        public decimal OvertimePayRate { get; set; }

        /// <summary>
        /// Contract type: 'FullTime', 'PartTime', 'Casual'
        /// </summary>
        public string ContractType { get; set; } = "Casual";

        /// <summary>
        /// Standard working hours per week
        /// </summary>
        public int StandardHoursPerWeek { get; set; } = 40;

        /// <summary>
        /// PIN password for device access (maintained for compatibility)
        /// </summary>
        public string Pin { get; set; } = string.Empty;

        /// <summary>
        /// Username for system login (optional)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Password hash for system login
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Home address
        /// </summary>
        public string? Address { get; set; }


        /// <summary>
        /// Active status
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
        public virtual ICollection<BiometricData> BiometricData { get; set; } = new List<BiometricData>();
        public virtual ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
        public virtual ICollection<Salary> Salaries { get; set; } = new List<Salary>();
        public virtual ICollection<BiometricVerification> BiometricVerifications { get; set; } = new List<BiometricVerification>();

        /// <summary>
        /// Full name property for backward compatibility
        /// </summary>
        [NotMapped]
        public string Name => $"{FirstName} {LastName}";

        /// <summary>
        /// Hourly rate property for backward compatibility
        /// </summary>
        [NotMapped]
        public decimal HourlyRate
        {
            get => StandardPayRate;
            set => StandardPayRate = value;
        }

        /// <summary>
        /// Staff ID property for backward compatibility
        /// </summary>
        [NotMapped]
        public int Id
        {
            get => StaffId;
            set => StaffId = value;
        }

        /// <summary>
        /// Determine role based on staff ID
        /// </summary>
        public string GetRoleFromId()
        {
            if (StaffId >= 9000) return "Admin";
            if (StaffId >= 8000 && StaffId <= 8999) return "Manager";
            return "Staff";
        }
    }
}
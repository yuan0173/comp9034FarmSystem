namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Staff entity model
    /// </summary>
    public class Staff
    {
        /// <summary>
        /// Staff ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Staff name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// User role: 'staff', 'manager', 'admin'
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// PIN password (4 digits)
        /// </summary>
        public string Pin { get; set; } = string.Empty;

        /// <summary>
        /// Username for login (optional, defaults to email or staff ID)
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Password hash for system login
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Hourly rate
        /// </summary>
        public decimal HourlyRate { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string? Email { get; set; }

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

        /// <summary>
        /// Determine role based on staff ID
        /// </summary>
        public string GetRoleFromId()
        {
            if (Id >= 9000) return "admin";
            if (Id >= 8000 && Id <= 8999) return "manager";
            return "staff";
        }
    }
}
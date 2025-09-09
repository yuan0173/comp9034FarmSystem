using System.ComponentModel.DataAnnotations.Schema;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Device entity model - Updated for Tan Architecture
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Device ID (Primary Key)
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Device name
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// Device type: 'biometric', 'terminal', 'card_reader'
        /// </summary>
        public string DeviceType { get; set; } = string.Empty;

        /// <summary>
        /// Device location
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Device status: 'Active', 'Inactive', 'Maintenance'
        /// </summary>
        public string Status { get; set; } = "Active";

        /// <summary>
        /// Device IP address
        /// </summary>
        public string? IpAddress { get; set; }

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
        public virtual ICollection<BiometricVerification> BiometricVerifications { get; set; } = new List<BiometricVerification>();

        // Backward compatibility properties
        /// <summary>
        /// Device ID property for backward compatibility
        /// </summary>
        [NotMapped]
        public int Id
        {
            get => DeviceId;
            set => DeviceId = value;
        }

        /// <summary>
        /// Device name property for backward compatibility
        /// </summary>
        [NotMapped]
        public string Name
        {
            get => DeviceName;
            set => DeviceName = value;
        }

        /// <summary>
        /// Device type property for backward compatibility
        /// </summary>
        [NotMapped]
        public string Type
        {
            get => DeviceType;
            set => DeviceType = value;
        }
    }
}
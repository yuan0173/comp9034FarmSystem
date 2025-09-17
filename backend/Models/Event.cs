using System.ComponentModel.DataAnnotations.Schema;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Attendance event entity model - Updated for Tan Architecture
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Event ID (Primary Key)
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Staff ID (Foreign Key - optional)
        /// </summary>
        public int? StaffId { get; set; }

        /// <summary>
        /// Device ID (Foreign Key - optional)
        /// </summary>
        public int? DeviceId { get; set; }

        /// <summary>
        /// Event type: 'CLOCK_IN', 'CLOCK_OUT', 'BREAK_START', 'BREAK_END'
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Event occurrence timestamp (Tan architecture: occurredAt)
        /// </summary>
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Event notes or reason
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Creation timestamp (for internal tracking)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Staff? Staff { get; set; }
        public virtual Device? Device { get; set; }

        // Backward compatibility properties
        /// <summary>
        /// Legacy timestamp property for backward compatibility
        /// </summary>
        [NotMapped]
        public string TimeStamp
        {
            get => OccurredAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            set
            {
                if (DateTime.TryParse(value, out var parsed))
                    OccurredAt = parsed;
            }
        }

        /// <summary>
        /// Legacy reason property for backward compatibility
        /// </summary>
        [NotMapped]
        public string? Reason
        {
            get => Notes;
            set => Notes = value;
        }

        /// <summary>
        /// Admin ID property for backward compatibility
        /// </summary>
        public int? AdminId { get; set; }

        /// <summary>
        /// Admin navigation property for backward compatibility
        /// </summary>
        public virtual Staff? Admin { get; set; }
    }
}
using System.ComponentModel.DataAnnotations.Schema;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Attendance event entity model - aligned with frontend expectations
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Event ID (aligned with frontend: eventId)
        /// </summary>
        [Column("Id")]
        public int EventId { get; set; }

        /// <summary>
        /// Staff ID
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Device ID (required to match frontend)
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Admin ID (added to match frontend expectations)
        /// </summary>
        public int? AdminId { get; set; }

        /// <summary>
        /// Event type: 'IN', 'OUT', 'BREAK_START', 'BREAK_END', 'OTHER'
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Event timestamp (aligned with frontend: timeStamp)
        /// </summary>
        [Column("Timestamp")]
        public string TimeStamp { get; set; } = string.Empty;

        /// <summary>
        /// Reason/Notes (aligned with frontend: reason)
        /// </summary>
        [Column("Notes")]
        public string? Reason { get; set; }

        /// <summary>
        /// Creation timestamp (for internal tracking)
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (made optional for API flexibility)
        public virtual Staff? Staff { get; set; }
        public virtual Device? Device { get; set; }
        public virtual Staff? Admin { get; set; }
    }
}
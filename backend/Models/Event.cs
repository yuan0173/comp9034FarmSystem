namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Attendance event entity model
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Event ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Staff ID
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Device ID (optional)
        /// </summary>
        public int? DeviceId { get; set; }

        /// <summary>
        /// Event type: 'IN', 'OUT', 'BREAK_START', 'BREAK_END', 'OTHER'
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Event timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Notes
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Staff Staff { get; set; } = null!;
        public virtual Device? Device { get; set; }
    }
}
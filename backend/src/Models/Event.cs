using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Events entity model - Built from Tan Architecture Database Schema
    /// Implements complete schema specification for attendance events
    /// </summary>
    public class Event
    {
        /// <summary>
        /// (PK) eventId: Unique identifier for each event
        /// </summary>
        [Key]
        public int EventId { get; set; }

        /// <summary>
        /// (FK) staffId: Reference to staff member who performed the action
        /// </summary>
        public int? StaffId { get; set; }

        /// <summary>
        /// (FK) deviceId: Reference to device used for the event
        /// </summary>
        public int? DeviceId { get; set; }

        /// <summary>
        /// occurredAt: Date and time when the event occurred (matches database column)
        /// </summary>
        [Required]
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// eventType: Type of event (CLOCK_IN, CLOCK_OUT, BREAK_START, BREAK_END, MANUAL_OVERRIDE)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// notes: Reason for the event (matches database column)
        /// </summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// adminId: ID of admin who performed manual override (nullable - only for manual overrides)
        /// </summary>
        public int? AdminId { get; set; }

        /// <summary>
        /// createdAt: Event creation timestamp (matches database column)
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties based on Schema relationships
        /// <summary>
        /// Related staff member (FK relationship)
        /// </summary>
        public virtual Staff? Staff { get; set; }

        /// <summary>
        /// Related device (FK relationship)
        /// </summary>
        public virtual Device? Device { get; set; }

        /// <summary>
        /// Related admin staff member for manual overrides (FK relationship)
        /// </summary>
        public virtual Staff? Admin { get; set; }

        // Computed properties for business logic
        /// <summary>
        /// Check if this is a manual override event
        /// </summary>
        [NotMapped]
        public bool IsManualOverride => EventType == "MANUAL_OVERRIDE";

        /// <summary>
        /// Check if this is a break-related event
        /// </summary>
        [NotMapped]
        public bool IsBreakEvent => EventType == "BREAK_START" || EventType == "BREAK_END";

        /// <summary>
        /// Check if this is a clock event (in/out)
        /// </summary>
        [NotMapped]
        public bool IsClockEvent => EventType == "CLOCK_IN" || EventType == "CLOCK_OUT";

        /// <summary>
        /// Get formatted timestamp for display
        /// </summary>
        [NotMapped]
        public string FormattedTimestamp => OccurredAt.ToString("yyyy-MM-dd HH:mm:ss");

        /// <summary>
        /// Get event description for logging
        /// </summary>
        [NotMapped]
        public string EventDescription
        {
            get
            {
                var desc = EventType.Replace("_", " ").ToLower();
                if (!string.IsNullOrEmpty(Notes))
                {
                    desc += $" ({Notes})";
                }
                return desc;
            }
        }

        // Legacy compatibility properties
        /// <summary>
        /// Legacy property mapping for backward compatibility
        /// </summary>
        [NotMapped]
        public DateTime TimeStamp
        {
            get => OccurredAt;
            set => OccurredAt = value;
        }

        /// <summary>
        /// Legacy property mapping for backward compatibility
        /// </summary>
        [NotMapped]
        public string? Reason
        {
            get => Notes;
            set => Notes = value;
        }

        /// <summary>
        /// Legacy location property (not stored in database)
        /// </summary>
        [NotMapped]
        public string? Location { get; set; }

        /// <summary>
        /// Validation for event type enum
        /// </summary>
        public bool IsValidEventType()
        {
            var validTypes = new[] { "CLOCK_IN", "CLOCK_OUT", "BREAK_START", "BREAK_END", "MANUAL_OVERRIDE" };
            return validTypes.Contains(EventType);
        }

        /// <summary>
        /// Check if reason is required for this event type
        /// </summary>
        public bool IsReasonRequired()
        {
            return IsBreakEvent || IsManualOverride;
        }
    }
}
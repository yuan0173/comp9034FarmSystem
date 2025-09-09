using System.ComponentModel.DataAnnotations;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Work Schedule entity model for scheduling system
    /// </summary>
    public class WorkSchedule
    {
        /// <summary>
        /// Schedule ID (Primary Key)
        /// </summary>
        public int ScheduleId { get; set; }

        /// <summary>
        /// Staff ID (Foreign Key)
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Scheduled date (ISO 8601 format)
        /// </summary>
        [Required]
        public string ScheduledDate { get; set; } = string.Empty;

        /// <summary>
        /// Start time (ISO 8601 format)
        /// </summary>
        [Required]
        public string StartTime { get; set; } = string.Empty;

        /// <summary>
        /// End time (ISO 8601 format)
        /// </summary>
        [Required]
        public string EndTime { get; set; } = string.Empty;

        /// <summary>
        /// Schedule status: 'Scheduled', 'Completed', 'Cancelled'
        /// </summary>
        public string Status { get; set; } = "Scheduled";

        /// <summary>
        /// Additional notes or comments
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Related staff member
        /// </summary>
        public virtual Staff Staff { get; set; } = null!;

        // Computed properties
        /// <summary>
        /// Calculate scheduled hours duration
        /// </summary>
        public double ScheduledHours
        {
            get
            {
                if (TimeSpan.TryParse(StartTime, out var start) && TimeSpan.TryParse(EndTime, out var end))
                {
                    var duration = end - start;
                    return duration.TotalHours > 0 ? duration.TotalHours : 24 + duration.TotalHours; // Handle overnight shifts
                }
                return 0;
            }
        }

        /// <summary>
        /// Check if schedule is active (today or future)
        /// </summary>
        public bool IsActive
        {
            get
            {
                if (DateTime.TryParse(ScheduledDate, out var scheduleDate))
                {
                    return scheduleDate.Date >= DateTime.Today && Status == "Scheduled";
                }
                return false;
            }
        }
    }
}
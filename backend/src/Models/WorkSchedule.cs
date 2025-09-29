using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// WorkSchedule entity model - Built from Tan Architecture Database Schema
    /// Implements complete schema specification for shift scheduling
    /// </summary>
    public class WorkSchedule
    {
        /// <summary>
        /// (PK) scheduleID: Unique identifier for each scheduled shift
        /// </summary>
        [Key]
        public int ScheduleID { get; set; }

        /// <summary>
        /// (FK) staffId: Reference to staff member assigned to this shift
        /// </summary>
        [Required]
        public int StaffId { get; set; }

        /// <summary>
        /// date: Date of the scheduled shift
        /// </summary>
        [Required]
        [Column(TypeName = "date")]
        public DateTime Date { get; set; }

        /// <summary>
        /// startTime: Shift start time
        /// </summary>
        [Required]
        [Column(TypeName = "time")]
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// endTime: Shift end time
        /// </summary>
        [Required]
        [Column(TypeName = "time")]
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// scheduleHours: Total scheduled hours for this shift
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal ScheduleHours { get; set; }

        // Navigation property based on Schema relationships
        /// <summary>
        /// Related staff member (FK relationship)
        /// </summary>
        public virtual Staff Staff { get; set; } = null!;

        // Computed properties for business logic
        /// <summary>
        /// Calculate scheduled hours duration automatically
        /// </summary>
        [NotMapped]
        public decimal CalculatedHours
        {
            get
            {
                var duration = EndTime - StartTime;
                if (duration.TotalHours < 0) // Handle overnight shifts
                {
                    duration = duration.Add(TimeSpan.FromHours(24));
                }
                return (decimal)duration.TotalHours;
            }
        }

        /// <summary>
        /// Check if schedule is for today or future
        /// </summary>
        [NotMapped]
        public bool IsActive => Date.Date >= DateTime.Today;

        /// <summary>
        /// Check if this is an overnight shift
        /// </summary>
        [NotMapped]
        public bool IsOvernightShift => EndTime < StartTime;

        /// <summary>
        /// Get formatted date string
        /// </summary>
        [NotMapped]
        public string FormattedDate => Date.ToString("yyyy-MM-dd");

        /// <summary>
        /// Get formatted time range
        /// </summary>
        [NotMapped]
        public string TimeRange => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";

        // Legacy compatibility properties
        /// <summary>
        /// Legacy property mapping for backward compatibility
        /// </summary>
        [NotMapped]
        public int ScheduleId
        {
            get => ScheduleID;
            set => ScheduleID = value;
        }

        /// <summary>
        /// Legacy property mapping for backward compatibility
        /// </summary>
        [NotMapped]
        public string ScheduledDate
        {
            get => Date.ToString("yyyy-MM-dd");
            set
            {
                if (DateTime.TryParse(value, out var parsed))
                    Date = parsed.Date;
            }
        }
    }
}
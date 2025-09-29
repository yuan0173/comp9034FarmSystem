using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Salary entity model - Built from Tan Architecture Database Schema
    /// Implements complete schema specification for payroll management
    /// </summary>
    public class Salary
    {
        /// <summary>
        /// (PK) salaryId: Unique identifier for each salary record
        /// </summary>
        [Key]
        public int SalaryId { get; set; }

        /// <summary>
        /// (FK) staffId: Reference to staff member
        /// </summary>
        [Required]
        public int StaffId { get; set; }

        /// <summary>
        /// payPeriodStart: Start date of the pay period
        /// </summary>
        [Required]
        [Column(TypeName = "date")]
        public DateTime PayPeriodStart { get; set; }

        /// <summary>
        /// payPeriodEnd: End date of the pay period
        /// </summary>
        [Required]
        [Column(TypeName = "date")]
        public DateTime PayPeriodEnd { get; set; }

        /// <summary>
        /// totalHours: Total hours worked in the pay period
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal TotalHours { get; set; } = 0;

        /// <summary>
        /// totalOvertimeHours: Total overtime hours worked
        /// </summary>
        [Column(TypeName = "decimal(8,2)")]
        public decimal TotalOvertimeHours { get; set; } = 0;

        /// <summary>
        /// grossPay: Total gross pay for the period
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal GrossPay { get; set; } = 0;

        /// <summary>
        /// generatedDate: Date when the salary record was generated
        /// </summary>
        [Required]
        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        // Navigation property based on Schema relationships
        /// <summary>
        /// Related staff member (FK relationship)
        /// </summary>
        public virtual Staff Staff { get; set; } = null!;

        // Computed properties for business logic
        /// <summary>
        /// Calculate regular hours (total - overtime)
        /// </summary>
        [NotMapped]
        public decimal RegularHours => TotalHours - TotalOvertimeHours;

        /// <summary>
        /// Calculate regular pay based on staff rates
        /// </summary>
        [NotMapped]
        public decimal RegularPay
        {
            get
            {
                if (Staff != null)
                    return RegularHours * Staff.StandardPayRate;
                return 0;
            }
        }

        /// <summary>
        /// Calculate overtime pay based on staff rates
        /// </summary>
        [NotMapped]
        public decimal OvertimePay
        {
            get
            {
                if (Staff != null)
                    return TotalOvertimeHours * Staff.OvertimePayRate;
                return 0;
            }
        }

        /// <summary>
        /// Calculate total calculated pay (may differ from stored GrossPay)
        /// </summary>
        [NotMapped]
        public decimal CalculatedGrossPay => RegularPay + OvertimePay;

        /// <summary>
        /// Get pay period duration in days
        /// </summary>
        [NotMapped]
        public int PayPeriodDays => (PayPeriodEnd - PayPeriodStart).Days + 1;

        /// <summary>
        /// Get formatted pay period string
        /// </summary>
        [NotMapped]
        public string PayPeriodRange => $"{PayPeriodStart:yyyy-MM-dd} to {PayPeriodEnd:yyyy-MM-dd}";

        /// <summary>
        /// Check if this is the current pay period
        /// </summary>
        [NotMapped]
        public bool IsCurrentPeriod
        {
            get
            {
                var today = DateTime.Today;
                return today >= PayPeriodStart && today <= PayPeriodEnd;
            }
        }

        /// <summary>
        /// Get average hours per day in this period
        /// </summary>
        [NotMapped]
        public decimal AverageHoursPerDay => PayPeriodDays > 0 ? TotalHours / PayPeriodDays : 0;

        // Legacy compatibility properties for existing code
        /// <summary>
        /// Legacy property mapping for backward compatibility
        /// </summary>
        [NotMapped]
        public DateTime CreatedAt
        {
            get => GeneratedDate;
            set => GeneratedDate = value;
        }

        /// <summary>
        /// Legacy property mapping for backward compatibility
        /// </summary>
        [NotMapped]
        public DateTime UpdatedAt
        {
            get => GeneratedDate;
            set => GeneratedDate = value;
        }

        /// <summary>
        /// Legacy status property for backward compatibility
        /// </summary>
        [NotMapped]
        public string Status { get; set; } = "Draft";

        // Validation methods
        /// <summary>
        /// Validate that pay period dates are logical
        /// </summary>
        public bool IsValidPayPeriod()
        {
            return PayPeriodStart <= PayPeriodEnd;
        }

        /// <summary>
        /// Validate that overtime hours don't exceed total hours
        /// </summary>
        public bool IsValidOvertimeHours()
        {
            return TotalOvertimeHours <= TotalHours;
        }

        /// <summary>
        /// Check if gross pay matches calculated pay (within tolerance)
        /// </summary>
        public bool IsGrossPayAccurate(decimal tolerance = 0.01m)
        {
            return Math.Abs(GrossPay - CalculatedGrossPay) <= tolerance;
        }
    }
}
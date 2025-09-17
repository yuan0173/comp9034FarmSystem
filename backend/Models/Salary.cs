using System.ComponentModel.DataAnnotations;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Salary entity model for payroll system
    /// </summary>
    public class Salary
    {
        /// <summary>
        /// Salary ID (Primary Key)
        /// </summary>
        public int SalaryId { get; set; }

        /// <summary>
        /// Staff ID (Foreign Key)
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Pay period start date (ISO 8601 format)
        /// </summary>
        [Required]
        public string PayPeriodStart { get; set; } = string.Empty;

        /// <summary>
        /// Pay period end date (ISO 8601 format)
        /// </summary>
        [Required]
        public string PayPeriodEnd { get; set; } = string.Empty;

        /// <summary>
        /// Total working hours in the pay period
        /// </summary>
        public decimal TotalHours { get; set; } = 0;

        /// <summary>
        /// Overtime hours in the pay period
        /// </summary>
        public decimal OvertimeHours { get; set; } = 0;

        /// <summary>
        /// Regular pay amount
        /// </summary>
        public decimal RegularPay { get; set; } = 0;

        /// <summary>
        /// Overtime pay amount
        /// </summary>
        public decimal OvertimePay { get; set; } = 0;

        /// <summary>
        /// Gross pay (total before deductions)
        /// </summary>
        public decimal GrossPay { get; set; } = 0;

        /// <summary>
        /// Salary status: 'Draft', 'Approved', 'Paid'
        /// </summary>
        public string Status { get; set; } = "Draft";

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
        /// Regular working hours (total - overtime)
        /// </summary>
        public decimal RegularHours => Math.Max(0, TotalHours - OvertimeHours);

        /// <summary>
        /// Calculate total pay period days
        /// </summary>
        public int PayPeriodDays
        {
            get
            {
                if (DateTime.TryParse(PayPeriodStart, out var start) && DateTime.TryParse(PayPeriodEnd, out var end))
                {
                    return (int)(end - start).TotalDays + 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// Check if salary record is finalized
        /// </summary>
        public bool IsFinalized => Status == "Approved" || Status == "Paid";

        /// <summary>
        /// Calculate gross pay from hours and rates
        /// </summary>
        public void CalculateGrossPay(decimal standardRate, decimal overtimeRate)
        {
            RegularPay = RegularHours * standardRate;
            OvertimePay = OvertimeHours * overtimeRate;
            GrossPay = RegularPay + OvertimePay;
        }
    }
}
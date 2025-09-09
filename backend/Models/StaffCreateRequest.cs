using System.ComponentModel.DataAnnotations;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Staff creation request model with auto-generated ID
    /// </summary>
    public class StaffCreateRequest
    {
        /// <summary>
        /// First name (required)
        /// </summary>
        [Required]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name (required)
        /// </summary>
        [Required]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Email address (required, unique)
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Role: staff, manager, admin (required)
        /// </summary>
        [Required]
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Phone number (optional)
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Home address (optional)
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Contract type: FullTime, PartTime, Casual (optional, defaults to Casual)
        /// </summary>
        public string? ContractType { get; set; }

        /// <summary>
        /// Standard hourly pay rate (optional, will use role defaults)
        /// </summary>
        public decimal? StandardPayRate { get; set; }

        /// <summary>
        /// Overtime hourly pay rate (optional, defaults to 1.5x standard)
        /// </summary>
        public decimal? OvertimePayRate { get; set; }

        /// <summary>
        /// Standard working hours per week (optional, will use role defaults)
        /// </summary>
        public int? StandardHoursPerWeek { get; set; }

        /// <summary>
        /// Active status (optional, defaults to true)
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
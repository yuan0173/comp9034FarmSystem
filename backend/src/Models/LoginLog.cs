using System.ComponentModel.DataAnnotations;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Login attempt logging model
    /// </summary>
    public class LoginLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string UserAgent { get; set; } = string.Empty;

        [Required]
        public bool Success { get; set; }

        [StringLength(500)]
        public string? FailureReason { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public int? StaffId { get; set; }

        // Navigation property
        public Staff? Staff { get; set; }
    }
}
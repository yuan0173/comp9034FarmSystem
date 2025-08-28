using System.ComponentModel.DataAnnotations;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Audit logging for all CRUD operations
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Operation { get; set; } = string.Empty; // CREATE, UPDATE, DELETE

        [Required]
        public string RecordId { get; set; } = string.Empty;

        [Required]
        public int PerformedByStaffId { get; set; }

        [Required]
        [StringLength(45)]
        public string IpAddress { get; set; } = string.Empty;

        public string? OldValues { get; set; } // JSON string of old values

        public string? NewValues { get; set; } // JSON string of new values

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Staff PerformedByStaff { get; set; } = null!;
    }
}
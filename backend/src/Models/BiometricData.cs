using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// BiometricData entity model - Built from Tan Architecture Database Schema
    /// Implements complete schema specification for biometric template storage with AES-256 encryption
    /// </summary>
    public class BiometricData
    {
        /// <summary>
        /// (PK) biometricId: Unique identifier for biometric record
        /// </summary>
        [Key]
        public int BiometricId { get; set; }

        /// <summary>
        /// (FK1) staffId: Reference to staff member
        /// </summary>
        [Required]
        public int StaffId { get; set; }

        /// <summary>
        /// biometricTemplate: Encrypted biometric data template
        /// Stored as Base64 encoded AES-256 encrypted data
        /// </summary>
        [Required]
        [Column(TypeName = "text")]
        public string BiometricTemplate { get; set; } = string.Empty;

        /// <summary>
        /// salt: Unique salt value used for encryption of the template
        /// Each template uses a unique salt for AES-256 encryption
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string Salt { get; set; } = string.Empty;

        /// <summary>
        /// templateHash: Hash value for fast template searching and indexing
        /// Used for candidate search without exposing raw biometric data
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string TemplateHash { get; set; } = string.Empty;

        /// <summary>
        /// deviceEnrollment: ID of device used for biometric registration
        /// </summary>
        [Required]
        public int DeviceEnrollment { get; set; }

        /// <summary>
        /// createdDate: Date when biometric data was first enrolled
        /// </summary>
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// lastUpdated: Date when biometric data was last modified
        /// </summary>
        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// isActive: Boolean flag indicating if biometric record is currently active
        /// </summary>
        public bool IsActive { get; set; } = true;

        // Navigation properties based on Schema relationships
        /// <summary>
        /// Related staff member (FK relationship)
        /// </summary>
        public virtual Staff Staff { get; set; } = null!;

        /// <summary>
        /// Related device used for enrollment (FK relationship)
        /// </summary>
        public virtual Device DeviceEnrollmentDevice { get; set; } = null!;

        /// <summary>
        /// BiometricData (1) → (N) BiometricVerification: One template can be used in multiple verification attempts
        /// </summary>
        public virtual ICollection<BiometricVerification> BiometricVerifications { get; set; } = new List<BiometricVerification>();

        // Computed properties for business logic
        /// <summary>
        /// Check if template was enrolled recently (within 30 days)
        /// </summary>
        [NotMapped]
        public bool IsRecentlyEnrolled => (DateTime.UtcNow - CreatedDate).TotalDays <= 30;

        /// <summary>
        /// Check if template needs update (not modified in 90 days)
        /// </summary>
        [NotMapped]
        public bool NeedsUpdate => (DateTime.UtcNow - LastUpdated).TotalDays > 90;

        /// <summary>
        /// Get template age in days
        /// </summary>
        [NotMapped]
        public int TemplateAgeInDays => (DateTime.UtcNow - CreatedDate).Days;

        /// <summary>
        /// Get enrollment device information
        /// </summary>
        [NotMapped]
        public string EnrollmentInfo => $"Device {DeviceEnrollment} on {CreatedDate:yyyy-MM-dd}";

        // Security properties
        /// <summary>
        /// Biometric type for backward compatibility
        /// </summary>
        [NotMapped]
        public string BiometricType { get; set; } = "Fingerprint";

        /// <summary>
        /// Template data for backward compatibility (encrypted)
        /// </summary>
        [NotMapped]
        public string TemplateData
        {
            get => BiometricTemplate;
            set => BiometricTemplate = value;
        }

        // Legacy compatibility properties
        /// <summary>
        /// Legacy property mapping for backward compatibility
        /// </summary>
        [NotMapped]
        public int Id
        {
            get => BiometricId;
            set => BiometricId = value;
        }

        /// <summary>
        /// Legacy property mapping for backward compatibility
        /// </summary>
        [NotMapped]
        public DateTime CreatedAt
        {
            get => CreatedDate;
            set => CreatedDate = value;
        }

        /// <summary>
        /// Legacy property mapping for backward compatibility
        /// </summary>
        [NotMapped]
        public DateTime UpdatedAt
        {
            get => LastUpdated;
            set => LastUpdated = value;
        }

        // Security validation methods
        /// <summary>
        /// Validate that all security fields are properly set
        /// </summary>
        public bool IsSecurityValid()
        {
            return !string.IsNullOrEmpty(BiometricTemplate) &&
                   !string.IsNullOrEmpty(Salt) &&
                   !string.IsNullOrEmpty(TemplateHash) &&
                   Salt.Length >= 16; // Minimum salt length for AES-256
        }

        /// <summary>
        /// Check if template can be used for verification
        /// </summary>
        public bool CanBeUsedForVerification()
        {
            return IsActive && IsSecurityValid() && !NeedsUpdate;
        }

        /// <summary>
        /// Generate a new salt for encryption (utility method)
        /// </summary>
        public static string GenerateNewSalt()
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                byte[] saltBytes = new byte[32]; // 256 bits
                rng.GetBytes(saltBytes);
                return Convert.ToBase64String(saltBytes);
            }
        }
    }
}
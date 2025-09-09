using System.ComponentModel.DataAnnotations;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Biometric Verification entity model for tracking verification attempts
    /// </summary>
    public class BiometricVerification
    {
        /// <summary>
        /// Verification ID (Primary Key)
        /// </summary>
        public int VerificationId { get; set; }

        /// <summary>
        /// Staff ID (Foreign Key, optional)
        /// </summary>
        public int? StaffId { get; set; }

        /// <summary>
        /// Device ID (Foreign Key, optional)
        /// </summary>
        public int? DeviceId { get; set; }

        /// <summary>
        /// Event ID (Foreign Key, optional - links to clock in/out event)
        /// </summary>
        public int? EventId { get; set; }

        /// <summary>
        /// Verification result: 'Success', 'Failed', 'Error'
        /// </summary>
        [Required]
        public string VerificationResult { get; set; } = string.Empty;

        /// <summary>
        /// Confidence score (0.000 to 1.000) for successful verifications
        /// </summary>
        public decimal? ConfidenceScore { get; set; }

        /// <summary>
        /// Failure reason for failed verifications
        /// </summary>
        public string? FailureReason { get; set; }

        /// <summary>
        /// Verification timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        /// <summary>
        /// Related staff member (optional)
        /// </summary>
        public virtual Staff? Staff { get; set; }

        /// <summary>
        /// Related device
        /// </summary>
        public virtual Device? Device { get; set; }

        /// <summary>
        /// Related event (clock in/out)
        /// </summary>
        public virtual Event? Event { get; set; }

        // Computed properties
        /// <summary>
        /// Check if verification was successful
        /// </summary>
        public bool IsSuccessful => VerificationResult == "Success";

        /// <summary>
        /// Check if verification had high confidence (> 0.8)
        /// </summary>
        public bool IsHighConfidence => ConfidenceScore.HasValue && ConfidenceScore.Value > 0.8m;

        /// <summary>
        /// Get confidence percentage as string
        /// </summary>
        public string ConfidencePercentage
        {
            get
            {
                if (ConfidenceScore.HasValue)
                {
                    return $"{ConfidenceScore.Value * 100:F1}%";
                }
                return "N/A";
            }
        }

        /// <summary>
        /// Determine verification quality based on confidence score
        /// </summary>
        public string VerificationQuality
        {
            get
            {
                if (!IsSuccessful) return "Failed";
                if (!ConfidenceScore.HasValue) return "Unknown";
                if (ConfidenceScore.Value >= 0.9m) return "Excellent";
                if (ConfidenceScore.Value >= 0.8m) return "Good";
                if (ConfidenceScore.Value >= 0.7m) return "Fair";
                return "Poor";
            }
        }
    }
}
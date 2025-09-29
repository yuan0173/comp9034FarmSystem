using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// BiometricVerification entity model - Built from Tan Architecture Database Schema
    /// Implements complete schema specification for biometric verification attempts and audit trail
    /// </summary>
    public class BiometricVerification
    {
        /// <summary>
        /// (PK) verificationId: Unique identifier for each biometric verification attempt
        /// </summary>
        [Key]
        public int VerificationId { get; set; }

        /// <summary>
        /// (FK) staffId: Reference to staff member attempting verification (nullable if unknown person)
        /// </summary>
        public int? StaffId { get; set; }

        /// <summary>
        /// (FK) biometricId: Reference to biometric template used for matching (nullable if no match found)
        /// </summary>
        public int? BiometricId { get; set; }

        /// <summary>
        /// (FK) deviceId: Reference to device that performed the verification
        /// </summary>
        [Required]
        public int DeviceId { get; set; }

        /// <summary>
        /// (FK) eventId: Reference to event created by successful verification (nullable for failed attempts)
        /// </summary>
        public int? EventId { get; set; }

        /// <summary>
        /// verificationResult: Result of verification attempt (success, failed, no_match, poor_quality)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string VerificationResult { get; set; } = string.Empty;

        /// <summary>
        /// confidenceScore: Confidence level of the biometric match (0.000 to 1.000)
        /// </summary>
        [Column(TypeName = "decimal(5,3)")]
        public decimal? ConfidenceScore { get; set; }

        /// <summary>
        /// failureReason: Detailed reason for verification failure (nullable for successful attempts)
        /// </summary>
        [MaxLength(500)]
        public string? FailureReason { get; set; }

        /// <summary>
        /// createdAt: Timestamp when verification attempt was made
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties based on Schema relationships
        /// <summary>
        /// Related staff member (FK relationship) - nullable for unknown persons
        /// </summary>
        public virtual Staff? Staff { get; set; }

        /// <summary>
        /// Related biometric template (FK relationship) - nullable if no match found
        /// </summary>
        public virtual BiometricData? BiometricData { get; set; }

        /// <summary>
        /// Related device (FK relationship)
        /// </summary>
        public virtual Device Device { get; set; } = null!;

        /// <summary>
        /// Related event (FK relationship) - nullable for failed attempts
        /// BiometricVerification (1) → (0,1) Events: One verification may create one event (only for successful verifications)
        /// </summary>
        public virtual Event? Event { get; set; }

        // Computed properties for business logic
        /// <summary>
        /// Check if verification was successful
        /// </summary>
        [NotMapped]
        public bool IsSuccessful => VerificationResult.Equals("success", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Check if verification failed due to poor quality
        /// </summary>
        [NotMapped]
        public bool IsPoorQuality => VerificationResult.Equals("poor_quality", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Check if no biometric match was found
        /// </summary>
        [NotMapped]
        public bool IsNoMatch => VerificationResult.Equals("no_match", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Check if this represents a security concern (high confidence failure)
        /// </summary>
        [NotMapped]
        public bool IsSecurityConcern
        {
            get
            {
                return !IsSuccessful &&
                       ConfidenceScore.HasValue &&
                       ConfidenceScore.Value > 0.7m &&
                       !IsPoorQuality;
            }
        }

        /// <summary>
        /// Get confidence percentage for display
        /// </summary>
        [NotMapped]
        public string ConfidencePercentage
        {
            get
            {
                if (ConfidenceScore.HasValue)
                    return $"{ConfidenceScore.Value * 100:F1}%";
                return "N/A";
            }
        }

        /// <summary>
        /// Get verification summary for logging
        /// </summary>
        [NotMapped]
        public string VerificationSummary
        {
            get
            {
                var summary = $"{VerificationResult.ToUpper()}";
                if (ConfidenceScore.HasValue)
                    summary += $" ({ConfidencePercentage})";
                if (!string.IsNullOrEmpty(FailureReason))
                    summary += $" - {FailureReason}";
                return summary;
            }
        }

        /// <summary>
        /// Check if verification occurred recently (within 5 minutes)
        /// </summary>
        [NotMapped]
        public bool IsRecent => (DateTime.UtcNow - CreatedAt).TotalMinutes <= 5;

        /// <summary>
        /// Get formatted timestamp for display
        /// </summary>
        [NotMapped]
        public string FormattedTimestamp => CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");

        // Data flow helper properties
        /// <summary>
        /// Check if this verification should create an event
        /// </summary>
        [NotMapped]
        public bool ShouldCreateEvent => IsSuccessful && StaffId.HasValue;

        /// <summary>
        /// Get the staff name if available
        /// </summary>
        [NotMapped]
        public string StaffName
        {
            get
            {
                if (Staff != null)
                    return Staff.Name;
                return StaffId.HasValue ? $"Staff #{StaffId}" : "Unknown Person";
            }
        }

        /// <summary>
        /// Get device name if available
        /// </summary>
        [NotMapped]
        public string DeviceName
        {
            get
            {
                if (Device != null)
                    return Device.DeviceName ?? $"Device #{DeviceId}";
                return $"Device #{DeviceId}";
            }
        }

        // Validation methods
        /// <summary>
        /// Validate that verification result is a valid enum value
        /// </summary>
        public bool IsValidVerificationResult()
        {
            var validResults = new[] { "success", "failed", "no_match", "poor_quality" };
            return validResults.Contains(VerificationResult.ToLower());
        }

        /// <summary>
        /// Validate that confidence score is within valid range
        /// </summary>
        public bool IsValidConfidenceScore()
        {
            return !ConfidenceScore.HasValue ||
                   (ConfidenceScore.Value >= 0.000m && ConfidenceScore.Value <= 1.000m);
        }

        /// <summary>
        /// Validate that failure reason is provided for failed attempts
        /// </summary>
        public bool IsFailureReasonValid()
        {
            if (IsSuccessful)
                return true; // No failure reason needed for success

            return !string.IsNullOrWhiteSpace(FailureReason);
        }

        /// <summary>
        /// Check if verification data is consistent
        /// </summary>
        public bool IsDataConsistent()
        {
            // Successful verifications should have staff and biometric data
            if (IsSuccessful)
                return StaffId.HasValue && BiometricId.HasValue;

            // No match should not have biometric data
            if (IsNoMatch)
                return !BiometricId.HasValue;

            return true;
        }
    }
}
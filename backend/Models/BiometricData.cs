namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Biometric data entity model
    /// </summary>
    public class BiometricData
    {
        /// <summary>
        /// Biometric data ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Staff ID
        /// </summary>
        public int StaffId { get; set; }

        /// <summary>
        /// Biometric type: 'fingerprint', 'face', 'iris'
        /// </summary>
        public string BiometricType { get; set; } = string.Empty;

        /// <summary>
        /// Template data (encrypted storage)
        /// </summary>
        public string TemplateData { get; set; } = string.Empty;

        /// <summary>
        /// Active status
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Staff Staff { get; set; } = null!;
    }
}
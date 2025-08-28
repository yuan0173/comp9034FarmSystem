namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Device entity model
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Device ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Device name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Device type: 'biometric', 'terminal', 'card_reader'
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Device location
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Device status: 'active', 'inactive', 'maintenance'
        /// </summary>
        public string Status { get; set; } = "active";

        /// <summary>
        /// Device IP address
        /// </summary>
        public string? IpAddress { get; set; }

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
        public virtual ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Login request model
    /// </summary>
    public class LoginRequest
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// PIN login request model
    /// </summary>
    public class PinLoginRequest
    {
        [Required]
        public int StaffId { get; set; }

        [Required]
        [StringLength(10)]
        public string Pin { get; set; } = string.Empty;
    }

    /// <summary>
    /// Login response model
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public StaffInfo Staff { get; set; } = null!;
    }

    /// <summary>
    /// Staff information for authentication responses
    /// </summary>
    public class StaffInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Email { get; set; }
    }

    /// <summary>
    /// JWT token claims
    /// </summary>
    public class TokenClaims
    {
        public const string StaffId = "staff_id";
        public const string Name = "name";
        public const string Role = "role";
        public const string Email = "email";
    }
}
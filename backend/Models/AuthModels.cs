using System.ComponentModel.DataAnnotations;

namespace COMP9034.Backend.Models
{
    /// <summary>
    /// Login request model
    /// </summary>
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// User registration request model
    /// </summary>
    public class RegisterRequest
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }
    }

    /// <summary>
    /// Change password request model
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        public string ConfirmNewPassword { get; set; } = string.Empty;
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
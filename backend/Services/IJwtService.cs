using COMP9034.Backend.Models;

namespace COMP9034.Backend.Services
{
    /// <summary>
    /// JWT token service interface
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generate JWT access token
        /// </summary>
        /// <param name="staff">Staff information</param>
        /// <returns>JWT token</returns>
        string GenerateToken(Staff staff);

        /// <summary>
        /// Generate refresh token
        /// </summary>
        /// <returns>Refresh token string</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validate JWT token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Token claims if valid, null otherwise</returns>
        System.Security.Claims.ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Get staff ID from token
        /// </summary>
        /// <param name="token">JWT token</param>
        /// <returns>Staff ID if valid, null otherwise</returns>
        int? GetStaffIdFromToken(string token);
    }
}
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using COMP9034.Backend.Models;

namespace COMP9034.Backend.Services
{
    /// <summary>
    /// JWT token service implementation
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _tokenExpiryMinutes;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _secretKey = _configuration["Jwt:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long-for-security";
            _issuer = _configuration["Jwt:Issuer"] ?? "COMP9034-FarmTimeMS";
            _audience = _configuration["Jwt:Audience"] ?? "COMP9034-FarmTimeMS-Users";
            _tokenExpiryMinutes = int.Parse(_configuration["Jwt:TokenExpiryMinutes"] ?? "480"); // 8 hours default
        }

        public string GenerateToken(Staff staff)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var claims = new List<Claim>
                {
                    new(TokenClaims.StaffId, staff.Id.ToString()),
                    new(TokenClaims.Name, staff.Name),
                    new(TokenClaims.Role, staff.GetRoleFromId()), // 使用标准化的小写角色
                    new(JwtRegisteredClaimNames.Sub, staff.Id.ToString()),
                    new(JwtRegisteredClaimNames.Iat, 
                        new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                        ClaimValueTypes.Integer64)
                };

                if (!string.IsNullOrEmpty(staff.Email))
                {
                    claims.Add(new Claim(TokenClaims.Email, staff.Email));
                    claims.Add(new Claim(JwtRegisteredClaimNames.Email, staff.Email));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(_tokenExpiryMinutes),
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation($"JWT token generated for staff ID: {staff.Id}");
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating JWT token for staff ID: {staff.Id}");
                throw;
            }
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                
                if (validatedToken is not JwtSecurityToken jwtToken || 
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }

        public int? GetStaffIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null) return null;

            var staffIdClaim = principal.FindFirst(TokenClaims.StaffId)?.Value;
            if (int.TryParse(staffIdClaim, out int staffId))
            {
                return staffId;
            }

            return null;
        }
    }
}
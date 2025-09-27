using Microsoft.Extensions.Logging;
using COMP9034.Backend.Models;
using COMP9034.Backend.Common.Results;
using COMP9034.Backend.Common.Exceptions;
using COMP9034.Backend.Repositories.Interfaces;
using COMP9034.Backend.Services.Interfaces;
using BCrypt.Net;

namespace COMP9034.Backend.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IJwtService jwtService, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<ApiResult<string>> AuthenticateAsync(string email, string password)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByEmailAsync(email);
                if (staff == null)
                {
                    _logger.LogWarning("Authentication failed: User not found for email {Email}", email);
                    return ApiResult<string>.ErrorResult("Invalid credentials", "INVALID_CREDENTIALS");
                }

                if (!staff.IsActive)
                {
                    _logger.LogWarning("Authentication failed: Inactive user {Email}", email);
                    return ApiResult<string>.ErrorResult("Account is inactive", "ACCOUNT_INACTIVE");
                }

                if (string.IsNullOrEmpty(staff.PasswordHash) || !BCrypt.Net.BCrypt.Verify(password, staff.PasswordHash))
                {
                    _logger.LogWarning("Authentication failed: Invalid password for {Email}", email);
                    return ApiResult<string>.ErrorResult("Invalid credentials", "INVALID_CREDENTIALS");
                }

                var token = _jwtService.GenerateToken(staff);
                _logger.LogInformation("Authentication successful for user {Email}", email);

                return ApiResult<string>.SuccessResult(token, "Authentication successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during authentication for {Email}", email);
                return ApiResult<string>.ErrorResult("Authentication failed");
            }
        }

        public async Task<ApiResult<Staff>> RegisterAsync(Staff staff, string password)
        {
            try
            {
                if (!await _unitOfWork.StaffRepository.IsEmailUniqueAsync(staff.Email))
                {
                    throw new BusinessException("Email address is already registered", "EMAIL_ALREADY_EXISTS");
                }

                if (!string.IsNullOrEmpty(staff.Username) && !await _unitOfWork.StaffRepository.IsUsernameUniqueAsync(staff.Username))
                {
                    throw new BusinessException("Username is already taken", "USERNAME_ALREADY_EXISTS");
                }

                staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
                staff.CreatedAt = DateTime.UtcNow;
                staff.UpdatedAt = DateTime.UtcNow;
                staff.IsActive = true;

                var createdStaff = await _unitOfWork.StaffRepository.AddAsync(staff);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User registered successfully: {Email}", staff.Email);

                // Remove sensitive data before returning
                createdStaff.PasswordHash = null;

                return ApiResult<Staff>.SuccessResult(createdStaff, "Registration successful");
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration for {Email}", staff.Email);
                return ApiResult<Staff>.ErrorResult("Registration failed");
            }
        }

        public async Task<ApiResult<bool>> ChangePasswordAsync(int staffId, string currentPassword, string newPassword)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByIdAsync(staffId);
                if (staff == null)
                {
                    return ApiResult<bool>.ErrorResult("Staff member not found", "STAFF_NOT_FOUND");
                }

                if (string.IsNullOrEmpty(staff.PasswordHash) || !BCrypt.Net.BCrypt.Verify(currentPassword, staff.PasswordHash))
                {
                    return ApiResult<bool>.ErrorResult("Current password is incorrect", "INVALID_CURRENT_PASSWORD");
                }

                staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                staff.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.StaffRepository.Update(staff);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Password changed successfully for staff {StaffId}", staffId);
                return ApiResult<bool>.SuccessResult(true, "Password changed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for staff {StaffId}", staffId);
                return ApiResult<bool>.ErrorResult("Failed to change password");
            }
        }

        public async Task<ApiResult<bool>> ResetPasswordAsync(string email)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByEmailAsync(email);
                if (staff == null)
                {
                    // Return success even if email doesn't exist to prevent email enumeration
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    return ApiResult<bool>.SuccessResult(true, "If the email exists, a password reset link has been sent");
                }

                // Generate temporary password
                var tempPassword = GenerateTemporaryPassword();
                staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);
                staff.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.StaffRepository.Update(staff);
                await _unitOfWork.SaveChangesAsync();

                // In a real application, you would send this via email
                _logger.LogInformation("Temporary password generated for {Email}: {TempPassword}", email, tempPassword);

                return ApiResult<bool>.SuccessResult(true, "Password reset successful. Check your email for the new password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password reset for {Email}", email);
                return ApiResult<bool>.ErrorResult("Password reset failed");
            }
        }

        public async Task<ApiResult<Staff>> ValidateTokenAsync(string token)
        {
            try
            {
                var claims = _jwtService.ValidateToken(token);
                if (claims == null)
                {
                    return ApiResult<Staff>.ErrorResult("Invalid token", "INVALID_TOKEN");
                }

                var staffIdClaim = claims.FindFirst("staffId")?.Value;
                if (string.IsNullOrEmpty(staffIdClaim) || !int.TryParse(staffIdClaim, out var staffId))
                {
                    return ApiResult<Staff>.ErrorResult("Invalid token claims", "INVALID_TOKEN_CLAIMS");
                }

                var staff = await _unitOfWork.StaffRepository.GetByIdAsync(staffId);
                if (staff == null || !staff.IsActive)
                {
                    return ApiResult<Staff>.ErrorResult("Staff member not found or inactive", "STAFF_NOT_FOUND");
                }

                // Remove sensitive data
                staff.PasswordHash = null;

                return ApiResult<Staff>.SuccessResult(staff, "Token validated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token validation");
                return ApiResult<Staff>.ErrorResult("Token validation failed");
            }
        }

        public async Task<ApiResult<bool>> LogoutAsync(int staffId)
        {
            try
            {
                // In a stateless JWT system, logout is typically handled on the client side
                // But we can log the logout event for auditing purposes
                _logger.LogInformation("Staff member {StaffId} logged out", staffId);
                return ApiResult<bool>.SuccessResult(true, "Logout successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout for staff {StaffId}", staffId);
                return ApiResult<bool>.ErrorResult("Logout failed");
            }
        }

        public async Task<ApiResult<LoginLog>> CreateLoginLogAsync(int staffId, string ipAddress, bool successful, string? errorMessage = null)
        {
            try
            {
                var loginLog = new LoginLog
                {
                    StaffId = staffId,
                    IpAddress = ipAddress,
                    Success = successful,
                    FailureReason = errorMessage,
                    Timestamp = DateTime.UtcNow,
                    Username = $"Staff_{staffId}",
                    UserAgent = "API"
                };

                var createdLog = await _unitOfWork.LoginLogRepository.AddAsync(loginLog);
                await _unitOfWork.SaveChangesAsync();

                return ApiResult<LoginLog>.SuccessResult(createdLog, "Login log created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating login log for staff {StaffId}", staffId);
                return ApiResult<LoginLog>.ErrorResult("Failed to create login log");
            }
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
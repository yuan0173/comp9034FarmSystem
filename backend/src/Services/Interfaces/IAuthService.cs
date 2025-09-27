using COMP9034.Backend.Models;
using COMP9034.Backend.Common.Results;

namespace COMP9034.Backend.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResult<string>> AuthenticateAsync(string email, string password);
        Task<ApiResult<Staff>> RegisterAsync(Staff staff, string password);
        Task<ApiResult<bool>> ChangePasswordAsync(int staffId, string currentPassword, string newPassword);
        Task<ApiResult<bool>> ResetPasswordAsync(string email);
        Task<ApiResult<Staff>> ValidateTokenAsync(string token);
        Task<ApiResult<bool>> LogoutAsync(int staffId);
        Task<ApiResult<LoginLog>> CreateLoginLogAsync(int staffId, string ipAddress, bool successful, string? errorMessage = null);
    }
}
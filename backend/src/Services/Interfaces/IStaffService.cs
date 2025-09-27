using COMP9034.Backend.Models;
using COMP9034.Backend.Common.Results;

namespace COMP9034.Backend.Services.Interfaces
{
    public interface IStaffService
    {
        Task<ApiResult<IEnumerable<Staff>>> GetAllStaffAsync();
        Task<ApiResult<Staff>> GetStaffByIdAsync(int id);
        Task<ApiResult<Staff>> GetStaffByEmailAsync(string email);
        Task<ApiResult<Staff>> GetStaffByUsernameAsync(string username);
        Task<ApiResult<IEnumerable<Staff>>> GetActiveStaffAsync();
        Task<ApiResult<IEnumerable<Staff>>> GetStaffByRoleAsync(string role);
        Task<ApiResult<Staff>> CreateStaffAsync(Staff staff);
        Task<ApiResult<Staff>> UpdateStaffAsync(int id, Staff staff);
        Task<ApiResult<bool>> DeleteStaffAsync(int id);
        Task<ApiResult<bool>> ActivateStaffAsync(int id);
        Task<ApiResult<bool>> DeactivateStaffAsync(int id);
        Task<ApiResult<bool>> ValidateEmailUniqueAsync(string email, int excludeStaffId = 0);
        Task<ApiResult<bool>> ValidateUsernameUniqueAsync(string username, int excludeStaffId = 0);
        Task<ApiResult<Staff>> GetStaffWithSchedulesAsync(int staffId);
        Task<ApiResult<(IEnumerable<Staff> staff, int totalCount)>> GetStaffPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            string? role = null, bool? isActive = null);
    }
}
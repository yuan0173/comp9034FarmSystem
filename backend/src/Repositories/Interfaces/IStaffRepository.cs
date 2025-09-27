using COMP9034.Backend.Models;

namespace COMP9034.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Staff repository interface with staff-specific operations
    /// </summary>
    public interface IStaffRepository : IGenericRepository<Staff>
    {
        /// <summary>
        /// Get staff member by email address
        /// </summary>
        Task<Staff?> GetByEmailAsync(string email);

        /// <summary>
        /// Get staff member by username
        /// </summary>
        Task<Staff?> GetByUsernameAsync(string username);

        /// <summary>
        /// Get all active staff members
        /// </summary>
        Task<IEnumerable<Staff>> GetActiveStaffAsync();

        /// <summary>
        /// Get staff members by role
        /// </summary>
        Task<IEnumerable<Staff>> GetStaffByRoleAsync(string role);

        /// <summary>
        /// Check if email is unique (excluding specific staff member)
        /// </summary>
        Task<bool> IsEmailUniqueAsync(string email, int excludeStaffId = 0);

        /// <summary>
        /// Check if username is unique (excluding specific staff member)
        /// </summary>
        Task<bool> IsUsernameUniqueAsync(string username, int excludeStaffId = 0);

        /// <summary>
        /// Get staff with their work schedules
        /// </summary>
        Task<Staff?> GetStaffWithSchedulesAsync(int staffId);

        /// <summary>
        /// Get staff members with pagination and filtering
        /// </summary>
        Task<IEnumerable<Staff>> GetStaffPagedAsync(int pageNumber, int pageSize,
            string? searchTerm = null, string? role = null, bool? isActive = null);

        /// <summary>
        /// Get total count of staff with filtering
        /// </summary>
        Task<int> GetStaffCountAsync(string? searchTerm = null, string? role = null, bool? isActive = null);
    }
}
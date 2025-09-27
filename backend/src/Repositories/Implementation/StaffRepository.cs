using Microsoft.EntityFrameworkCore;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;
using COMP9034.Backend.Repositories.Interfaces;

namespace COMP9034.Backend.Repositories.Implementation
{
    public class StaffRepository : GenericRepository<Staff>, IStaffRepository
    {
        public StaffRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Staff?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.Email == email);
        }

        public async Task<Staff?> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.Username == username);
        }

        public async Task<IEnumerable<Staff>> GetActiveStaffAsync()
        {
            return await _dbSet.Where(s => s.IsActive).ToListAsync();
        }

        public async Task<IEnumerable<Staff>> GetStaffByRoleAsync(string role)
        {
            return await _dbSet.Where(s => s.Role == role).ToListAsync();
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int excludeStaffId = 0)
        {
            return !await _dbSet.AnyAsync(s => s.Email == email && s.Id != excludeStaffId);
        }

        public async Task<bool> IsUsernameUniqueAsync(string username, int excludeStaffId = 0)
        {
            return !await _dbSet.AnyAsync(s => s.Username == username && s.Id != excludeStaffId);
        }

        public async Task<Staff?> GetStaffWithSchedulesAsync(int staffId)
        {
            return await _dbSet
                .Include(s => s.WorkSchedules)
                .FirstOrDefaultAsync(s => s.Id == staffId);
        }

        public async Task<IEnumerable<Staff>> GetStaffPagedAsync(int pageNumber, int pageSize,
            string? searchTerm = null, string? role = null, bool? isActive = null)
        {
            IQueryable<Staff> query = _dbSet;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.FirstName.Contains(searchTerm) ||
                                       s.LastName.Contains(searchTerm) ||
                                       s.Email.Contains(searchTerm) ||
                                       s.Username.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(s => s.Role == role);
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            return await query
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetStaffCountAsync(string? searchTerm = null, string? role = null, bool? isActive = null)
        {
            IQueryable<Staff> query = _dbSet;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(s => s.FirstName.Contains(searchTerm) ||
                                       s.LastName.Contains(searchTerm) ||
                                       s.Email.Contains(searchTerm) ||
                                       s.Username.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(s => s.Role == role);
            }

            if (isActive.HasValue)
            {
                query = query.Where(s => s.IsActive == isActive.Value);
            }

            return await query.CountAsync();
        }
    }
}
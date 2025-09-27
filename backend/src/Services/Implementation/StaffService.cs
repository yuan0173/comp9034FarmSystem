using Microsoft.Extensions.Logging;
using COMP9034.Backend.Models;
using COMP9034.Backend.Common.Results;
using COMP9034.Backend.Common.Exceptions;
using COMP9034.Backend.Repositories.Interfaces;
using COMP9034.Backend.Services.Interfaces;

namespace COMP9034.Backend.Services.Implementation
{
    public class StaffService : IStaffService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<StaffService> _logger;

        public StaffService(IUnitOfWork unitOfWork, ILogger<StaffService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResult<IEnumerable<Staff>>> GetAllStaffAsync()
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetAllAsync();
                return ApiResult<IEnumerable<Staff>>.SuccessResult(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all staff");
                return ApiResult<IEnumerable<Staff>>.ErrorResult("Failed to retrieve staff members");
            }
        }

        public async Task<ApiResult<Staff>> GetStaffByIdAsync(int id)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByIdAsync(id);
                if (staff == null)
                {
                    return ApiResult<Staff>.ErrorResult("Staff member not found", "STAFF_NOT_FOUND");
                }

                return ApiResult<Staff>.SuccessResult(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting staff by ID: {StaffId}", id);
                return ApiResult<Staff>.ErrorResult("Failed to retrieve staff member");
            }
        }

        public async Task<ApiResult<Staff>> GetStaffByEmailAsync(string email)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByEmailAsync(email);
                if (staff == null)
                {
                    return ApiResult<Staff>.ErrorResult("Staff member not found", "STAFF_NOT_FOUND");
                }

                return ApiResult<Staff>.SuccessResult(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting staff by email: {Email}", email);
                return ApiResult<Staff>.ErrorResult("Failed to retrieve staff member");
            }
        }

        public async Task<ApiResult<Staff>> GetStaffByUsernameAsync(string username)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByUsernameAsync(username);
                if (staff == null)
                {
                    return ApiResult<Staff>.ErrorResult("Staff member not found", "STAFF_NOT_FOUND");
                }

                return ApiResult<Staff>.SuccessResult(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting staff by username: {Username}", username);
                return ApiResult<Staff>.ErrorResult("Failed to retrieve staff member");
            }
        }

        public async Task<ApiResult<IEnumerable<Staff>>> GetActiveStaffAsync()
        {
            try
            {
                var activeStaff = await _unitOfWork.StaffRepository.GetActiveStaffAsync();
                return ApiResult<IEnumerable<Staff>>.SuccessResult(activeStaff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active staff");
                return ApiResult<IEnumerable<Staff>>.ErrorResult("Failed to retrieve active staff members");
            }
        }

        public async Task<ApiResult<IEnumerable<Staff>>> GetStaffByRoleAsync(string role)
        {
            try
            {
                var staffByRole = await _unitOfWork.StaffRepository.GetStaffByRoleAsync(role);
                return ApiResult<IEnumerable<Staff>>.SuccessResult(staffByRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting staff by role: {Role}", role);
                return ApiResult<IEnumerable<Staff>>.ErrorResult("Failed to retrieve staff members by role");
            }
        }

        public async Task<ApiResult<Staff>> CreateStaffAsync(Staff staff)
        {
            try
            {
                if (!await _unitOfWork.StaffRepository.IsEmailUniqueAsync(staff.Email))
                {
                    throw new BusinessException("Email address is already in use", "EMAIL_NOT_UNIQUE");
                }

                if (!await _unitOfWork.StaffRepository.IsUsernameUniqueAsync(staff.Username))
                {
                    throw new BusinessException("Username is already in use", "USERNAME_NOT_UNIQUE");
                }

                staff.CreatedAt = DateTime.UtcNow;
                staff.UpdatedAt = DateTime.UtcNow;
                staff.IsActive = true;

                var createdStaff = await _unitOfWork.StaffRepository.AddAsync(staff);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Staff member created successfully: {StaffId}", createdStaff.Id);
                return ApiResult<Staff>.SuccessResult(createdStaff, "Staff member created successfully");
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating staff member");
                return ApiResult<Staff>.ErrorResult("Failed to create staff member");
            }
        }

        public async Task<ApiResult<Staff>> UpdateStaffAsync(int id, Staff staff)
        {
            try
            {
                var existingStaff = await _unitOfWork.StaffRepository.GetByIdAsync(id);
                if (existingStaff == null)
                {
                    return ApiResult<Staff>.ErrorResult("Staff member not found", "STAFF_NOT_FOUND");
                }

                if (!await _unitOfWork.StaffRepository.IsEmailUniqueAsync(staff.Email, id))
                {
                    throw new BusinessException("Email address is already in use", "EMAIL_NOT_UNIQUE");
                }

                if (!await _unitOfWork.StaffRepository.IsUsernameUniqueAsync(staff.Username, id))
                {
                    throw new BusinessException("Username is already in use", "USERNAME_NOT_UNIQUE");
                }

                existingStaff.FirstName = staff.FirstName;
                existingStaff.LastName = staff.LastName;
                existingStaff.Email = staff.Email;
                existingStaff.Username = staff.Username;
                existingStaff.Role = staff.Role;
                existingStaff.Phone = staff.Phone;
                existingStaff.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.StaffRepository.Update(existingStaff);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Staff member updated successfully: {StaffId}", id);
                return ApiResult<Staff>.SuccessResult(existingStaff, "Staff member updated successfully");
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating staff member: {StaffId}", id);
                return ApiResult<Staff>.ErrorResult("Failed to update staff member");
            }
        }

        public async Task<ApiResult<bool>> DeleteStaffAsync(int id)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByIdAsync(id);
                if (staff == null)
                {
                    return ApiResult<bool>.ErrorResult("Staff member not found", "STAFF_NOT_FOUND");
                }

                _unitOfWork.StaffRepository.Delete(staff);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Staff member deleted successfully: {StaffId}", id);
                return ApiResult<bool>.SuccessResult(true, "Staff member deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting staff member: {StaffId}", id);
                return ApiResult<bool>.ErrorResult("Failed to delete staff member");
            }
        }

        public async Task<ApiResult<bool>> ActivateStaffAsync(int id)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByIdAsync(id);
                if (staff == null)
                {
                    return ApiResult<bool>.ErrorResult("Staff member not found", "STAFF_NOT_FOUND");
                }

                staff.IsActive = true;
                staff.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.StaffRepository.Update(staff);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Staff member activated successfully: {StaffId}", id);
                return ApiResult<bool>.SuccessResult(true, "Staff member activated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while activating staff member: {StaffId}", id);
                return ApiResult<bool>.ErrorResult("Failed to activate staff member");
            }
        }

        public async Task<ApiResult<bool>> DeactivateStaffAsync(int id)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByIdAsync(id);
                if (staff == null)
                {
                    return ApiResult<bool>.ErrorResult("Staff member not found", "STAFF_NOT_FOUND");
                }

                staff.IsActive = false;
                staff.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.StaffRepository.Update(staff);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Staff member deactivated successfully: {StaffId}", id);
                return ApiResult<bool>.SuccessResult(true, "Staff member deactivated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deactivating staff member: {StaffId}", id);
                return ApiResult<bool>.ErrorResult("Failed to deactivate staff member");
            }
        }

        public async Task<ApiResult<bool>> ValidateEmailUniqueAsync(string email, int excludeStaffId = 0)
        {
            try
            {
                bool isUnique = await _unitOfWork.StaffRepository.IsEmailUniqueAsync(email, excludeStaffId);
                return ApiResult<bool>.SuccessResult(isUnique);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating email uniqueness: {Email}", email);
                return ApiResult<bool>.ErrorResult("Failed to validate email uniqueness");
            }
        }

        public async Task<ApiResult<bool>> ValidateUsernameUniqueAsync(string username, int excludeStaffId = 0)
        {
            try
            {
                bool isUnique = await _unitOfWork.StaffRepository.IsUsernameUniqueAsync(username, excludeStaffId);
                return ApiResult<bool>.SuccessResult(isUnique);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while validating username uniqueness: {Username}", username);
                return ApiResult<bool>.ErrorResult("Failed to validate username uniqueness");
            }
        }

        public async Task<ApiResult<Staff>> GetStaffWithSchedulesAsync(int staffId)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetStaffWithSchedulesAsync(staffId);
                if (staff == null)
                {
                    return ApiResult<Staff>.ErrorResult("Staff member not found", "STAFF_NOT_FOUND");
                }

                return ApiResult<Staff>.SuccessResult(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting staff with schedules: {StaffId}", staffId);
                return ApiResult<Staff>.ErrorResult("Failed to retrieve staff member with schedules");
            }
        }

        public async Task<ApiResult<(IEnumerable<Staff> staff, int totalCount)>> GetStaffPagedAsync(
            int pageNumber, int pageSize, string? searchTerm = null,
            string? role = null, bool? isActive = null)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetStaffPagedAsync(
                    pageNumber, pageSize, searchTerm, role, isActive);
                var totalCount = await _unitOfWork.StaffRepository.GetStaffCountAsync(
                    searchTerm, role, isActive);

                return ApiResult<(IEnumerable<Staff> staff, int totalCount)>.SuccessResult((staff, totalCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged staff");
                return ApiResult<(IEnumerable<Staff> staff, int totalCount)>.ErrorResult("Failed to retrieve staff members");
            }
        }
    }
}
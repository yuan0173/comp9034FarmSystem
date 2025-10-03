using COMP9034.Backend.Models;
using COMP9034.Backend.Common.Results;

namespace COMP9034.Backend.Services.Interfaces
{
    public interface IEventService
    {
        Task<ApiResult<IEnumerable<Event>>> GetAllEventsAsync();
        Task<ApiResult<Event>> GetEventByIdAsync(int id);
        Task<ApiResult<IEnumerable<Event>>> GetEventsByStaffIdAsync(int staffId);
        Task<ApiResult<IEnumerable<Event>>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ApiResult<IEnumerable<Event>>> GetEventsByTypeAsync(string eventType);
        Task<ApiResult<Event>> GetLatestEventByStaffIdAsync(int staffId);
        Task<ApiResult<IEnumerable<Event>>> GetEventsByStaffAndDateRangeAsync(int staffId, DateTime startDate, DateTime endDate);
        Task<ApiResult<IEnumerable<Event>>> GetActiveEventsAsync();
        Task<ApiResult<IEnumerable<Event>>> GetEventsWithStaffAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<ApiResult<Event>> CreateEventAsync(Event eventEntity);
        Task<ApiResult<Event>> UpdateEventAsync(int id, Event eventEntity);
        Task<ApiResult<bool>> DeleteEventAsync(int id);
        Task<ApiResult<Event>> ClockInAsync(int staffId, string? location = null);
        Task<ApiResult<Event>> ClockOutAsync(int staffId, string? location = null);
        // Admin overrides
        Task<ApiResult<Event>> ClockInOverrideAsync(int staffId, int adminId, string? reason = null, string? location = null);
        Task<ApiResult<Event>> ClockOutOverrideAsync(int staffId, int adminId, string? reason = null, string? location = null);
        Task<ApiResult<bool>> HasActiveSessionAsync(int staffId);
        Task<ApiResult<double>> GetDailyWorkHoursAsync(int staffId, DateTime date);
        Task<ApiResult<double>> GetWeeklyWorkHoursAsync(int staffId, DateTime weekStartDate);
        Task<ApiResult<(IEnumerable<Event> events, int totalCount)>> GetEventsPagedAsync(
            int pageNumber, int pageSize, int? staffId = null,
            string? eventType = null, DateTime? startDate = null, DateTime? endDate = null);
    }
}

using COMP9034.Backend.Models;

namespace COMP9034.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Event repository interface with event-specific operations
    /// </summary>
    public interface IEventRepository : IGenericRepository<Event>
    {
        /// <summary>
        /// Get events by staff member ID
        /// </summary>
        Task<IEnumerable<Event>> GetEventsByStaffIdAsync(int staffId);

        /// <summary>
        /// Get events within a date range
        /// </summary>
        Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get events by event type
        /// </summary>
        Task<IEnumerable<Event>> GetEventsByTypeAsync(string eventType);

        /// <summary>
        /// Get the latest event for a staff member
        /// </summary>
        Task<Event?> GetLatestEventByStaffIdAsync(int staffId);

        /// <summary>
        /// Get events for a specific staff member within date range
        /// </summary>
        Task<IEnumerable<Event>> GetEventsByStaffAndDateRangeAsync(int staffId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get active (clock-in without clock-out) events
        /// </summary>
        Task<IEnumerable<Event>> GetActiveEventsAsync();

        /// <summary>
        /// Get events with staff information
        /// </summary>
        Task<IEnumerable<Event>> GetEventsWithStaffAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get events with pagination and filtering
        /// </summary>
        Task<IEnumerable<Event>> GetEventsPagedAsync(int pageNumber, int pageSize,
            int? staffId = null, string? eventType = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get total count of events with filtering
        /// </summary>
        Task<int> GetEventsCountAsync(int? staffId = null, string? eventType = null,
            DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Check if staff member has active session (clocked in but not out)
        /// </summary>
        Task<bool> HasActiveSessionAsync(int staffId);

        /// <summary>
        /// Get daily work hours for a staff member
        /// </summary>
        Task<double> GetDailyWorkHoursAsync(int staffId, DateTime date);

        /// <summary>
        /// Get weekly work hours for a staff member
        /// </summary>
        Task<double> GetWeeklyWorkHoursAsync(int staffId, DateTime weekStartDate);
    }
}
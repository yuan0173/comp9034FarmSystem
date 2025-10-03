using Microsoft.EntityFrameworkCore;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;
using COMP9034.Backend.Repositories.Interfaces;

namespace COMP9034.Backend.Repositories.Implementation
{
    public class EventRepository : GenericRepository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Event>> GetEventsByStaffIdAsync(int staffId)
        {
            return await _dbSet
                .Where(e => e.StaffId == staffId)
                .OrderBy(e => e.OccurredAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(e => e.OccurredAt >= startDate && e.OccurredAt <= endDate)
                .OrderBy(e => e.OccurredAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByTypeAsync(string eventType)
        {
            return await _dbSet
                .Where(e => e.EventType == eventType)
                .OrderBy(e => e.OccurredAt)
                .ToListAsync();
        }

        public async Task<Event?> GetLatestEventByStaffIdAsync(int staffId)
        {
            return await _dbSet
                .Where(e => e.StaffId == staffId)
                .OrderByDescending(e => e.OccurredAt)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsByStaffAndDateRangeAsync(int staffId, DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(e => e.StaffId == staffId && e.OccurredAt >= startDate && e.OccurredAt <= endDate)
                .OrderBy(e => e.OccurredAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetActiveEventsAsync()
        {
            return await _dbSet
                .Where(e => e.EventType == "IN")
                .Where(e => !_dbSet.Any(clockOut =>
                    clockOut.StaffId == e.StaffId &&
                    clockOut.EventType == "OUT" &&
                    clockOut.OccurredAt > e.OccurredAt))
                .OrderBy(e => e.OccurredAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsWithStaffAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            IQueryable<Event> query = _dbSet.Include(e => e.Staff);

            if (startDate.HasValue)
                query = query.Where(e => e.OccurredAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.OccurredAt <= endDate.Value);

            return await query
                .OrderBy(e => e.OccurredAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Event>> GetEventsPagedAsync(int pageNumber, int pageSize,
            int? staffId = null, string? eventType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            IQueryable<Event> query = _dbSet.Include(e => e.Staff);

            if (staffId.HasValue)
                query = query.Where(e => e.StaffId == staffId.Value);

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(e => e.EventType == eventType);

            if (startDate.HasValue)
                query = query.Where(e => e.OccurredAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.OccurredAt <= endDate.Value);

            return await query
                .OrderByDescending(e => e.OccurredAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetEventsCountAsync(int? staffId = null, string? eventType = null,
            DateTime? startDate = null, DateTime? endDate = null)
        {
            IQueryable<Event> query = _dbSet;

            if (staffId.HasValue)
                query = query.Where(e => e.StaffId == staffId.Value);

            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(e => e.EventType == eventType);

            if (startDate.HasValue)
                query = query.Where(e => e.OccurredAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.OccurredAt <= endDate.Value);

            return await query.CountAsync();
        }

        public async Task<bool> HasActiveSessionAsync(int staffId)
        {
            var latestEvent = await GetLatestEventByStaffIdAsync(staffId);
            return latestEvent != null && latestEvent.EventType == "IN";
        }

        public async Task<double> GetDailyWorkHoursAsync(int staffId, DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            var events = await GetEventsByStaffAndDateRangeAsync(staffId, startOfDay, endOfDay);

            double totalHours = 0;
            Event? clockInEvent = null;

            foreach (var evt in events.OrderBy(e => e.OccurredAt))
            {
                if (evt.EventType == "IN")
                {
                    clockInEvent = evt;
                }
                else if (evt.EventType == "OUT" && clockInEvent != null)
                {
                    totalHours += (evt.OccurredAt - clockInEvent.OccurredAt).TotalHours;
                    clockInEvent = null;
                }
            }

            return totalHours;
        }

        public async Task<double> GetWeeklyWorkHoursAsync(int staffId, DateTime weekStartDate)
        {
            var weekEndDate = weekStartDate.AddDays(7).AddTicks(-1);
            double totalHours = 0;

            for (int i = 0; i < 7; i++)
            {
                var currentDate = weekStartDate.AddDays(i);
                totalHours += await GetDailyWorkHoursAsync(staffId, currentDate);
            }

            return totalHours;
        }
    }
}

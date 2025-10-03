using Microsoft.Extensions.Logging;
using COMP9034.Backend.Models;
using COMP9034.Backend.Common.Results;
using COMP9034.Backend.Common.Exceptions;
using COMP9034.Backend.Repositories.Interfaces;
using COMP9034.Backend.Services.Interfaces;

namespace COMP9034.Backend.Services.Implementation
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EventService> _logger;

        public EventService(IUnitOfWork unitOfWork, ILogger<EventService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResult<IEnumerable<Event>>> GetAllEventsAsync()
        {
            try
            {
                var events = await _unitOfWork.EventRepository.GetAllAsync();
                return ApiResult<IEnumerable<Event>>.SuccessResult(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all events");
                return ApiResult<IEnumerable<Event>>.ErrorResult("Failed to retrieve events");
            }
        }

        public async Task<ApiResult<Event>> GetEventByIdAsync(int id)
        {
            try
            {
                var eventEntity = await _unitOfWork.EventRepository.GetByIdAsync(id);
                if (eventEntity == null)
                {
                    return ApiResult<Event>.ErrorResult("Event not found", "EVENT_NOT_FOUND");
                }

                return ApiResult<Event>.SuccessResult(eventEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting event by ID: {EventId}", id);
                return ApiResult<Event>.ErrorResult("Failed to retrieve event");
            }
        }

        public async Task<ApiResult<IEnumerable<Event>>> GetEventsByStaffIdAsync(int staffId)
        {
            try
            {
                var events = await _unitOfWork.EventRepository.GetEventsByStaffIdAsync(staffId);
                return ApiResult<IEnumerable<Event>>.SuccessResult(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting events by staff ID: {StaffId}", staffId);
                return ApiResult<IEnumerable<Event>>.ErrorResult("Failed to retrieve events for staff member");
            }
        }

        public async Task<ApiResult<IEnumerable<Event>>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var events = await _unitOfWork.EventRepository.GetEventsByDateRangeAsync(startDate, endDate);
                return ApiResult<IEnumerable<Event>>.SuccessResult(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting events by date range: {StartDate} - {EndDate}", startDate, endDate);
                return ApiResult<IEnumerable<Event>>.ErrorResult("Failed to retrieve events by date range");
            }
        }

        public async Task<ApiResult<IEnumerable<Event>>> GetEventsByTypeAsync(string eventType)
        {
            try
            {
                var events = await _unitOfWork.EventRepository.GetEventsByTypeAsync(eventType);
                return ApiResult<IEnumerable<Event>>.SuccessResult(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting events by type: {EventType}", eventType);
                return ApiResult<IEnumerable<Event>>.ErrorResult("Failed to retrieve events by type");
            }
        }

        public async Task<ApiResult<Event>> GetLatestEventByStaffIdAsync(int staffId)
        {
            try
            {
                var latestEvent = await _unitOfWork.EventRepository.GetLatestEventByStaffIdAsync(staffId);
                if (latestEvent == null)
                {
                    return ApiResult<Event>.ErrorResult("No events found for staff member", "NO_EVENTS_FOUND");
                }

                return ApiResult<Event>.SuccessResult(latestEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting latest event by staff ID: {StaffId}", staffId);
                return ApiResult<Event>.ErrorResult("Failed to retrieve latest event");
            }
        }

        public async Task<ApiResult<IEnumerable<Event>>> GetEventsByStaffAndDateRangeAsync(int staffId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var events = await _unitOfWork.EventRepository.GetEventsByStaffAndDateRangeAsync(staffId, startDate, endDate);
                return ApiResult<IEnumerable<Event>>.SuccessResult(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting events by staff and date range: {StaffId}, {StartDate} - {EndDate}", staffId, startDate, endDate);
                return ApiResult<IEnumerable<Event>>.ErrorResult("Failed to retrieve events");
            }
        }

        public async Task<ApiResult<IEnumerable<Event>>> GetActiveEventsAsync()
        {
            try
            {
                var activeEvents = await _unitOfWork.EventRepository.GetActiveEventsAsync();
                return ApiResult<IEnumerable<Event>>.SuccessResult(activeEvents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting active events");
                return ApiResult<IEnumerable<Event>>.ErrorResult("Failed to retrieve active events");
            }
        }

        public async Task<ApiResult<IEnumerable<Event>>> GetEventsWithStaffAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var events = await _unitOfWork.EventRepository.GetEventsWithStaffAsync(startDate, endDate);
                return ApiResult<IEnumerable<Event>>.SuccessResult(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting events with staff information");
                return ApiResult<IEnumerable<Event>>.ErrorResult("Failed to retrieve events with staff information");
            }
        }

        public async Task<ApiResult<Event>> CreateEventAsync(Event eventEntity)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByIdAsync(eventEntity.StaffId);
                if (staff == null)
                {
                    throw new BusinessException("Staff member not found", "STAFF_NOT_FOUND");
                }

                eventEntity.OccurredAt = DateTime.UtcNow;
                var createdEvent = await _unitOfWork.EventRepository.AddAsync(eventEntity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Event created successfully: {EventId} for staff {StaffId}", createdEvent.EventId, eventEntity.StaffId);
                return ApiResult<Event>.SuccessResult(createdEvent, "Event created successfully");
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating event");
                return ApiResult<Event>.ErrorResult("Failed to create event");
            }
        }

        public async Task<ApiResult<Event>> UpdateEventAsync(int id, Event eventEntity)
        {
            try
            {
                var existingEvent = await _unitOfWork.EventRepository.GetByIdAsync(id);
                if (existingEvent == null)
                {
                    return ApiResult<Event>.ErrorResult("Event not found", "EVENT_NOT_FOUND");
                }

                existingEvent.EventType = eventEntity.EventType;
                existingEvent.Location = eventEntity.Location;
                existingEvent.Notes = eventEntity.Notes;

                _unitOfWork.EventRepository.Update(existingEvent);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Event updated successfully: {EventId}", id);
                return ApiResult<Event>.SuccessResult(existingEvent, "Event updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating event: {EventId}", id);
                return ApiResult<Event>.ErrorResult("Failed to update event");
            }
        }

        public async Task<ApiResult<bool>> DeleteEventAsync(int id)
        {
            try
            {
                var eventEntity = await _unitOfWork.EventRepository.GetByIdAsync(id);
                if (eventEntity == null)
                {
                    return ApiResult<bool>.ErrorResult("Event not found", "EVENT_NOT_FOUND");
                }

                _unitOfWork.EventRepository.Delete(eventEntity);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Event deleted successfully: {EventId}", id);
                return ApiResult<bool>.SuccessResult(true, "Event deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting event: {EventId}", id);
                return ApiResult<bool>.ErrorResult("Failed to delete event");
            }
        }

        public async Task<ApiResult<Event>> ClockInAsync(int staffId, string? location = null)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByIdAsync(staffId);
                if (staff == null)
                {
                    throw new BusinessException("Staff member not found", "STAFF_NOT_FOUND");
                }

                bool hasActiveSession = await _unitOfWork.EventRepository.HasActiveSessionAsync(staffId);
                if (hasActiveSession)
                {
                    throw new BusinessException("Staff member already has an active session", "ALREADY_CLOCKED_IN");
                }

                // Prevent duplicate IN within 1 minute
                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
                var recentEvents = await _unitOfWork.EventRepository.GetEventsByStaffAndDateRangeAsync(staffId, oneMinuteAgo, DateTime.UtcNow);
                if (recentEvents.Any(e => string.Equals(e.EventType, "IN", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new BusinessException("Duplicate clock-in detected within 1 minute", "DUPLICATE_CLOCK_IN");
                }

                var clockInEvent = new Event
                {
                    StaffId = staffId,
                    EventType = "IN",
                    Location = location,
                    OccurredAt = DateTime.UtcNow
                };

                var createdEvent = await _unitOfWork.EventRepository.AddAsync(clockInEvent);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Staff member clocked in successfully: {StaffId}", staffId);
                return ApiResult<Event>.SuccessResult(createdEvent, "Clocked in successfully");
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while clocking in staff member: {StaffId}", staffId);
                return ApiResult<Event>.ErrorResult("Failed to clock in");
            }
        }

        public async Task<ApiResult<Event>> ClockOutAsync(int staffId, string? location = null)
        {
            try
            {
                var staff = await _unitOfWork.StaffRepository.GetByIdAsync(staffId);
                if (staff == null)
                {
                    throw new BusinessException("Staff member not found", "STAFF_NOT_FOUND");
                }

                bool hasActiveSession = await _unitOfWork.EventRepository.HasActiveSessionAsync(staffId);
                if (!hasActiveSession)
                {
                    throw new BusinessException("Staff member does not have an active session", "NOT_CLOCKED_IN");
                }

                // Prevent duplicate OUT within 1 minute
                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
                var recentEvents = await _unitOfWork.EventRepository.GetEventsByStaffAndDateRangeAsync(staffId, oneMinuteAgo, DateTime.UtcNow);
                if (recentEvents.Any(e => string.Equals(e.EventType, "OUT", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new BusinessException("Duplicate clock-out detected within 1 minute", "DUPLICATE_CLOCK_OUT");
                }

                var clockOutEvent = new Event
                {
                    StaffId = staffId,
                    EventType = "OUT",
                    Location = location,
                    OccurredAt = DateTime.UtcNow
                };

                var createdEvent = await _unitOfWork.EventRepository.AddAsync(clockOutEvent);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Staff member clocked out successfully: {StaffId}", staffId);
                return ApiResult<Event>.SuccessResult(createdEvent, "Clocked out successfully");
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while clocking out staff member: {StaffId}", staffId);
                return ApiResult<Event>.ErrorResult("Failed to clock out");
            }
        }

        public async Task<ApiResult<bool>> HasActiveSessionAsync(int staffId)
        {
            try
            {
                bool hasActiveSession = await _unitOfWork.EventRepository.HasActiveSessionAsync(staffId);
                return ApiResult<bool>.SuccessResult(hasActiveSession);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking active session for staff: {StaffId}", staffId);
                return ApiResult<bool>.ErrorResult("Failed to check active session");
            }
        }

        public async Task<ApiResult<double>> GetDailyWorkHoursAsync(int staffId, DateTime date)
        {
            try
            {
                double hours = await _unitOfWork.EventRepository.GetDailyWorkHoursAsync(staffId, date);
                return ApiResult<double>.SuccessResult(hours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting daily work hours for staff: {StaffId}, date: {Date}", staffId, date);
                return ApiResult<double>.ErrorResult("Failed to calculate daily work hours");
            }
        }

        public async Task<ApiResult<double>> GetWeeklyWorkHoursAsync(int staffId, DateTime weekStartDate)
        {
            try
            {
                double hours = await _unitOfWork.EventRepository.GetWeeklyWorkHoursAsync(staffId, weekStartDate);
                return ApiResult<double>.SuccessResult(hours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting weekly work hours for staff: {StaffId}, week start: {WeekStart}", staffId, weekStartDate);
                return ApiResult<double>.ErrorResult("Failed to calculate weekly work hours");
            }
        }

        public async Task<ApiResult<(IEnumerable<Event> events, int totalCount)>> GetEventsPagedAsync(
            int pageNumber, int pageSize, int? staffId = null,
            string? eventType = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var events = await _unitOfWork.EventRepository.GetEventsPagedAsync(
                    pageNumber, pageSize, staffId, eventType, startDate, endDate);
                var totalCount = await _unitOfWork.EventRepository.GetEventsCountAsync(
                    staffId, eventType, startDate, endDate);

                return ApiResult<(IEnumerable<Event> events, int totalCount)>.SuccessResult((events, totalCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged events");
                return ApiResult<(IEnumerable<Event> events, int totalCount)>.ErrorResult("Failed to retrieve events");
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;
using COMP9034.Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace COMP9034.Backend.Controllers
{
    /// <summary>
    /// Attendance event management API controller - aligned with frontend expectations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventsController> _logger;
        private readonly IEventService _eventService;
        private readonly COMP9034.Backend.Services.IAuditService _auditService;

        public EventsController(ApplicationDbContext context, ILogger<EventsController> logger, IEventService eventService, COMP9034.Backend.Services.IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _eventService = eventService;
            _auditService = auditService;
        }

        /// <summary>
        /// Get all events list
        /// </summary>
        /// <param name="limit">Limit return count</param>
        /// <param name="offset">Offset</param>
        /// <param name="staffId">Staff ID filter</param>
        /// <param name="eventType">Event type filter</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Events list</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Event>>> GetEvents(
            [FromQuery] int? limit = null,
            [FromQuery] int offset = 0,
            [FromQuery] int? staffId = null,
            [FromQuery] string? eventType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Query param aliases from frontend: type, from, to
                if (string.IsNullOrWhiteSpace(eventType))
                {
                    var alias = Request.Query["type"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(alias)) eventType = alias;
                }
                if (!startDate.HasValue)
                {
                    var from = Request.Query["from"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(from) && DateTime.TryParse(from, out var parsedFrom))
                    {
                        startDate = parsedFrom;
                    }
                }
                if (!endDate.HasValue)
                {
                    var to = Request.Query["to"].FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(to) && DateTime.TryParse(to, out var parsedTo))
                    {
                        endDate = parsedTo;
                    }
                }

                var query = _context.Events
                    .Include(e => e.Staff)
                    .Include(e => e.Device)
                    .Include(e => e.Admin)
                    .AsQueryable();

                // Apply staff filter
                if (staffId.HasValue)
                {
                    query = query.Where(e => e.StaffId == staffId.Value);
                }

                // Apply event type filter
                if (!string.IsNullOrWhiteSpace(eventType))
                {
                    query = query.Where(e => e.EventType == eventType);
                }

                // Optional filters: deviceId, adminId
                if (int.TryParse(Request.Query["deviceId"].FirstOrDefault(), out var deviceId))
                {
                    query = query.Where(e => e.DeviceId == deviceId);
                }
                if (int.TryParse(Request.Query["adminId"].FirstOrDefault(), out var adminId))
                {
                    query = query.Where(e => e.AdminId == adminId);
                }

                // Apply date range filter
                if (startDate.HasValue)
                {
                    query = query.Where(e => e.OccurredAt >= startDate.Value.Date);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(e => e.OccurredAt <= endDate.Value.Date.AddDays(1));
                }

                // Sort by timestamp descending
                query = query.OrderByDescending(e => e.OccurredAt);

                // Apply pagination
                if (offset > 0)
                {
                    query = query.Skip(offset);
                }

                if (limit.HasValue)
                {
                    query = query.Take(limit.Value);
                }

                var events = await query.Select(e => new
                {
                    eventId = e.EventId,
                    staffId = e.StaffId,
                    staffName = e.Staff.Name,
                    deviceId = e.DeviceId,
                    deviceName = e.Device != null ? e.Device.Name : "Unknown Device",
                    adminId = e.AdminId,
                    adminName = e.Admin != null ? e.Admin.Name : null,
                    eventType = e.EventType,
                    timeStamp = e.TimeStamp,
                    reason = e.Reason,
                    createdAt = e.CreatedAt
                }).ToListAsync();

                _logger.LogInformation($"Returning {events.Count} events");
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting events");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Get specific event by ID
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>Event information</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            try
            {
                var eventItem = await _context.Events
                    .Include(e => e.Staff)
                    .Include(e => e.Device)
                    .Include(e => e.Admin)
                    .FirstOrDefaultAsync(e => e.EventId == id);

                if (eventItem == null)
                {
                    return NotFound(new { message = $"Event with ID {id} not found" });
                }

                var result = new
                {
                    eventId = eventItem.EventId,
                    staffId = eventItem.StaffId,
                    staffName = eventItem.Staff.Name,
                    deviceId = eventItem.DeviceId,
                    deviceName = eventItem.Device?.Name,
                    adminId = eventItem.AdminId,
                    adminName = eventItem.Admin?.Name,
                    eventType = eventItem.EventType,
                    timeStamp = eventItem.TimeStamp,
                    reason = eventItem.Reason,
                    createdAt = eventItem.CreatedAt
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting event ID:{id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new event
        /// </summary>
        /// <param name="eventRequest">Event information</param>
        /// <returns>Created event information</returns>
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent([FromBody] Event eventRequest)
        {
            try
            {
                // Validate staff exists
                var staff = await _context.Staff.FindAsync(eventRequest.StaffId);
                if (staff == null)
                {
                    return BadRequest(new { message = $"Staff with ID {eventRequest.StaffId} not found" });
                }

                // Validate device exists (if provided)
                if (eventRequest.DeviceId > 0)
                {
                    var device = await _context.Device.FindAsync(eventRequest.DeviceId);
                    if (device == null)
                    {
                        return BadRequest(new { message = $"Device with ID {eventRequest.DeviceId} not found" });
                    }
                }

                // Validate admin exists (if provided)
                if (eventRequest.AdminId.HasValue)
                {
                    var admin = await _context.Staff.FindAsync(eventRequest.AdminId.Value);
                    if (admin == null)
                    {
                        return BadRequest(new { message = $"Admin with ID {eventRequest.AdminId} not found" });
                    }
                }

                // Route IN/OUT to EventService for business validation
                var type = eventRequest.EventType?.ToUpperInvariant();
                if (type == "IN")
                {
                    var result = await _eventService.ClockInAsync(eventRequest.StaffId ?? 0);
                        if (!result.Success)
                        {
                            // Log reject when no roster
                            if (string.Equals(result.ErrorCode, "NO_ROSTER", StringComparison.OrdinalIgnoreCase))
                            {
                                var ip = GetClientIpAddress();
                                await _auditService.LogAsync("Events", "REJECT", (eventRequest.StaffId ?? 0).ToString(), 0, ip, null, new { reason = result.Message });
                            }
                            return BadRequest(new { message = result.Message, code = result.ErrorCode });
                        }
                    return CreatedAtAction(nameof(GetEvent), new { id = result.Data!.EventId }, result.Data);
                }
                if (type == "OUT")
                {
                    var result = await _eventService.ClockOutAsync(eventRequest.StaffId ?? 0);
                        if (!result.Success)
                        {
                            return BadRequest(new { message = result.Message, code = result.ErrorCode });
                        }
                    return CreatedAtAction(nameof(GetEvent), new { id = result.Data!.EventId }, result.Data);
                }

                // For other event types, create directly
                if (eventRequest.TimeStamp == default(DateTime))
                {
                    eventRequest.TimeStamp = DateTime.UtcNow;
                }
                eventRequest.CreatedAt = DateTime.UtcNow;

                _context.Events.Add(eventRequest);
                await _context.SaveChangesAsync();

                var createdEvent = await _context.Events
                    .Include(e => e.Staff)
                    .Include(e => e.Device)
                    .Include(e => e.Admin)
                    .FirstOrDefaultAsync(e => e.EventId == eventRequest.EventId);

                _logger.LogInformation($"Created new event: ID={eventRequest.EventId}, StaffID={eventRequest.StaffId}, Type={eventRequest.EventType}");
                return CreatedAtAction(nameof(GetEvent), new { id = eventRequest.EventId }, createdEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating event");
                return StatusCode(500, new { message = "Failed to create event", error = ex.Message });
            }
        }

        /// <summary>
        /// Admin override clock-in (bypass roster rule) with reason; audited
        /// Fulfills F3-FR4 and F3-FR6
        /// </summary>
        [HttpPost("override/clock-in")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> OverrideClockIn([FromBody] OverrideRequest body)
        {
            var adminId = GetCurrentUserId();
            if (!adminId.HasValue) return Unauthorized(new { message = "User not authenticated" });

            var result = await _eventService.ClockInOverrideAsync(body.StaffId, adminId.Value, body.Reason);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message, code = result.ErrorCode });
            }

            var ip = GetClientIpAddress();
            await _auditService.LogAsync("Events", "OVERRIDE", result.Data!.EventId.ToString(), adminId.Value, ip, null, new { reason = body.Reason });
            return Ok(new { message = "Clock-in override recorded", eventId = result.Data!.EventId });
        }

        /// <summary>
        /// Admin override clock-out with reason; audited
        /// </summary>
        [HttpPost("override/clock-out")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> OverrideClockOut([FromBody] OverrideRequest body)
        {
            var adminId = GetCurrentUserId();
            if (!adminId.HasValue) return Unauthorized(new { message = "User not authenticated" });

            var result = await _eventService.ClockOutOverrideAsync(body.StaffId, adminId.Value, body.Reason);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message, code = result.ErrorCode });
            }

            var ip = GetClientIpAddress();
            await _auditService.LogAsync("Events", "OVERRIDE", result.Data!.EventId.ToString(), adminId.Value, ip, null, new { reason = body.Reason });
            return Ok(new { message = "Clock-out override recorded", eventId = result.Data!.EventId });
        }

        private int? GetCurrentUserId()
        {
            var staffIdClaim = User.FindFirst(TokenClaims.StaffId)?.Value;
            if (int.TryParse(staffIdClaim, out int staffId)) return staffId;
            return null;
        }

        private string GetClientIpAddress()
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor)) return forwardedFor.Split(',')[0].Trim();
            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp)) return realIp;
            return Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        public class OverrideRequest
        {
            public int StaffId { get; set; }
            public string? Reason { get; set; }
        }

        /// <summary>
        /// Update event information
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="eventRequest">Updated event information</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, [FromBody] Event eventRequest)
        {
            if (id != eventRequest.EventId)
            {
                return BadRequest(new { message = "ID in path does not match ID in request body" });
            }

            try
            {
                var existingEvent = await _context.Events.FindAsync(id);
                if (existingEvent == null)
                {
                    return NotFound(new { message = $"Event with ID {id} not found" });
                }

                // Update fields (preserve creation time)
                existingEvent.StaffId = eventRequest.StaffId;
                existingEvent.DeviceId = eventRequest.DeviceId;
                existingEvent.AdminId = eventRequest.AdminId;
                existingEvent.EventType = eventRequest.EventType;
                existingEvent.TimeStamp = eventRequest.TimeStamp;
                existingEvent.Reason = eventRequest.Reason;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated event: ID={id}");
                return Ok(new { message = "Event updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating event ID:{id}");
                return StatusCode(500, new { message = "Failed to update event", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete event
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <returns>Delete result</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            try
            {
                var eventItem = await _context.Events.FindAsync(id);
                if (eventItem == null)
                {
                    return NotFound(new { message = $"Event with ID {id} not found" });
                }

                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted event: ID={id}");
                return Ok(new { message = "Event deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting event ID:{id}");
                return StatusCode(500, new { message = "Failed to delete event", error = ex.Message });
            }
        }

        /// <summary>
        /// Get today's events for specific staff
        /// </summary>
        /// <param name="staffId">Staff ID</param>
        /// <returns>Today's events for the staff</returns>
        [HttpGet("staff/{staffId}/today")]
        public async Task<ActionResult<IEnumerable<object>>> GetTodayEvents(int staffId)
        {
            try
            {
                var today = DateTime.Today.ToString("yyyy-MM-dd");
                var tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

                var events = await _context.Events
                    .Include(e => e.Staff)
                    .Include(e => e.Device)
                    .Where(e => e.StaffId == staffId && 
                               e.OccurredAt >= DateTime.Today && 
                               e.OccurredAt < DateTime.Today.AddDays(1))
                    .OrderBy(e => e.OccurredAt)
                    .Select(e => new
                    {
                        eventId = e.EventId,
                        staffId = e.StaffId,
                        staffName = e.Staff.Name,
                        deviceId = e.DeviceId,
                        deviceName = e.Device != null ? e.Device.Name : "Unknown Device",
                        adminId = e.AdminId,
                        eventType = e.EventType,
                        timeStamp = e.TimeStamp,
                        reason = e.Reason,
                        createdAt = e.CreatedAt
                    })
                    .ToListAsync();

                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting today's events for staff ID:{staffId}");
                return StatusCode(500, new { message = "Failed to get today's events", error = ex.Message });
            }
        }
    }
}

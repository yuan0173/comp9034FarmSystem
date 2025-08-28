using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;

namespace COMP9034.Backend.Controllers
{
    /// <summary>
    /// Attendance event management API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventsController> _logger;

        public EventsController(ApplicationDbContext context, ILogger<EventsController> logger)
        {
            _context = context;
            _logger = logger;
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
                var query = _context.Events
                    .Include(e => e.Staff)
                    .Include(e => e.Device)
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

                // Apply date range filter
                if (startDate.HasValue)
                {
                    query = query.Where(e => e.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(e => e.Timestamp <= endDate.Value);
                }

                // Sort by timestamp descending
                query = query.OrderByDescending(e => e.Timestamp);

                // Apply pagination
                if (offset > 0)
                {
                    query = query.Skip(offset);
                }

                if (limit.HasValue)
                {
                    query = query.Take(limit.Value);
                }

                var events = await query.ToListAsync();

                // Create simplified event objects to avoid circular reference
                var simplifiedEvents = events.Select(e => new
                {
                    id = e.Id,
                    staffId = e.StaffId,
                    staffName = e.Staff?.Name,
                    deviceId = e.DeviceId,
                    deviceName = e.Device?.Name,
                    eventType = e.EventType,
                    timestamp = e.Timestamp,
                    notes = e.Notes,
                    createdAt = e.CreatedAt
                }).ToList();

                _logger.LogInformation($"Returning {events.Count} event records");
                return Ok(simplifiedEvents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting events list");
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
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (eventItem == null)
                {
                    return NotFound(new { message = $"Event with ID {id} not found" });
                }

                // Create simplified event object to avoid circular reference
                var simplifiedEvent = new
                {
                    id = eventItem.Id,
                    staffId = eventItem.StaffId,
                    staffName = eventItem.Staff?.Name,
                    deviceId = eventItem.DeviceId,
                    deviceName = eventItem.Device?.Name,
                    eventType = eventItem.EventType,
                    timestamp = eventItem.Timestamp,
                    notes = eventItem.Notes,
                    createdAt = eventItem.CreatedAt
                };

                return Ok(simplifiedEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting event ID:{id}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new attendance event
        /// </summary>
        /// <param name="eventItem">Event information</param>
        /// <returns>Created event information</returns>
        [HttpPost]
        public async Task<ActionResult<Event>> PostEvent(Event eventItem)
        {
            try
            {
                // Verify staff exists and is active
                var staff = await _context.Staffs.FindAsync(eventItem.StaffId);
                if (staff == null || !staff.IsActive)
                {
                    return BadRequest(new { message = $"Staff ID {eventItem.StaffId} does not exist or has been disabled" });
                }

                // Verify device exists and is active (if device is specified)
                if (eventItem.DeviceId.HasValue)
                {
                    var device = await _context.Devices.FindAsync(eventItem.DeviceId.Value);
                    if (device == null || !device.IsActive)
                    {
                        return BadRequest(new { message = $"Device ID {eventItem.DeviceId} does not exist or has been disabled" });
                    }
                }

                // Validate event type
                var validEventTypes = new[] { "IN", "OUT", "BREAK_START", "BREAK_END", "OTHER" };
                if (!validEventTypes.Contains(eventItem.EventType))
                {
                    return BadRequest(new { message = $"Invalid event type: {eventItem.EventType}. Valid types: {string.Join(", ", validEventTypes)}" });
                }

                // Set creation time
                eventItem.CreatedAt = DateTime.UtcNow;
                
                // If timestamp not specified, use current time
                if (eventItem.Timestamp == default)
                {
                    eventItem.Timestamp = DateTime.UtcNow;
                }

                _context.Events.Add(eventItem);
                await _context.SaveChangesAsync();

                // Re-fetch with related data
                var createdEvent = await _context.Events
                    .Include(e => e.Staff)
                    .Include(e => e.Device)
                    .FirstOrDefaultAsync(e => e.Id == eventItem.Id);

                _logger.LogInformation($"Created new event: ID={eventItem.Id}, StaffID={eventItem.StaffId}, Type={eventItem.EventType}");
                return CreatedAtAction(nameof(GetEvent), new { id = eventItem.Id }, createdEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating event");
                return StatusCode(500, new { message = "Failed to create event", error = ex.Message });
            }
        }

        /// <summary>
        /// Update event information
        /// </summary>
        /// <param name="id">Event ID</param>
        /// <param name="eventItem">Updated event information</param>
        /// <returns>Update result</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvent(int id, Event eventItem)
        {
            if (id != eventItem.Id)
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

                // Validate event type
                var validEventTypes = new[] { "IN", "OUT", "BREAK_START", "BREAK_END", "OTHER" };
                if (!validEventTypes.Contains(eventItem.EventType))
                {
                    return BadRequest(new { message = $"Invalid event type: {eventItem.EventType}" });
                }

                // Update fields
                existingEvent.EventType = eventItem.EventType;
                existingEvent.Timestamp = eventItem.Timestamp;
                existingEvent.Notes = eventItem.Notes;
                existingEvent.DeviceId = eventItem.DeviceId;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated event: ID={id}");
                return Ok(new { message = "Event information updated successfully" });
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
        /// Batch create events (for offline sync)
        /// </summary>
        /// <param name="events">Events list</param>
        /// <returns>Creation result</returns>
        [HttpPost("batch")]
        public async Task<ActionResult> PostEventsBatch([FromBody] List<Event> events)
        {
            try
            {
                var createdEvents = new List<Event>();
                var errors = new List<string>();

                foreach (var eventItem in events)
                {
                    try
                    {
                        // Verify staff exists
                        var staff = await _context.Staffs.FindAsync(eventItem.StaffId);
                        if (staff == null || !staff.IsActive)
                        {
                            errors.Add($"Event {eventItem.Id}: Staff ID {eventItem.StaffId} does not exist or has been disabled");
                            continue;
                        }

                        // Validate event type
                        var validEventTypes = new[] { "IN", "OUT", "BREAK_START", "BREAK_END", "OTHER" };
                        if (!validEventTypes.Contains(eventItem.EventType))
                        {
                            errors.Add($"Event {eventItem.Id}: Invalid event type {eventItem.EventType}");
                            continue;
                        }

                        // Set creation time
                        eventItem.CreatedAt = DateTime.UtcNow;
                        if (eventItem.Timestamp == default)
                        {
                            eventItem.Timestamp = DateTime.UtcNow;
                        }

                        _context.Events.Add(eventItem);
                        createdEvents.Add(eventItem);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Event {eventItem.Id}: {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Batch created events: Success {createdEvents.Count}, Failed {errors.Count}");
                
                return Ok(new 
                { 
                    message = $"Batch creation completed: Success {createdEvents.Count}, Failed {errors.Count}",
                    createdCount = createdEvents.Count,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during batch event creation");
                return StatusCode(500, new { message = "Failed to batch create events", error = ex.Message });
            }
        }

        /// <summary>
        /// Get today's events for specified staff
        /// </summary>
        /// <param name="staffId">Staff ID</param>
        /// <returns>Today's events list</returns>
        [HttpGet("staff/{staffId}/today")]
        public async Task<ActionResult<IEnumerable<Event>>> GetTodayEvents(int staffId)
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var events = await _context.Events
                    .Include(e => e.Staff)
                    .Include(e => e.Device)
                    .Where(e => e.StaffId == staffId && 
                               e.Timestamp >= today && 
                               e.Timestamp < tomorrow)
                    .OrderBy(e => e.Timestamp)
                    .ToListAsync();

                // Create simplified event objects to avoid circular reference
                var simplifiedEvents = events.Select(e => new
                {
                    id = e.Id,
                    staffId = e.StaffId,
                    staffName = e.Staff?.Name,
                    deviceId = e.DeviceId,
                    deviceName = e.Device?.Name,
                    eventType = e.EventType,
                    timestamp = e.Timestamp,
                    notes = e.Notes,
                    createdAt = e.CreatedAt
                }).ToList();

                return Ok(simplifiedEvents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting today's events for staff {staffId}");
                return StatusCode(500, new { message = "Failed to get today's events", error = ex.Message });
            }
        }
    }
}
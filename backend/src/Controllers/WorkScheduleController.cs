using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;
using COMP9034.Backend.Services;

namespace COMP9034.Backend.Controllers
{
    /// <summary>
    /// Work schedule (Roster) management API controller
    /// Fulfills Sprint 2 Epic 1 Feature 3 (Roster Assignment)
    /// Implements: F3-FR1/FR2/FR3/FR4 and AC1-AC4
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorkScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WorkScheduleController> _logger;
        private readonly IAuditService _auditService;

        public WorkScheduleController(ApplicationDbContext context,
            ILogger<WorkScheduleController> logger,
            IAuditService auditService)
        {
            _context = context;
            _logger = logger;
            _auditService = auditService;
        }

        /// <summary>
        /// Get schedules with optional filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkSchedule>>> GetSchedules(
            [FromQuery] int? staffId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? limit = null,
            [FromQuery] int offset = 0)
        {
            try
            {
                var query = _context.WorkSchedule
                    .Include(w => w.Staff)
                    .AsQueryable();

                if (staffId.HasValue)
                {
                    query = query.Where(w => w.StaffId == staffId.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(w => w.Date >= startDate.Value.Date);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(w => w.Date <= endDate.Value.Date);
                }

                query = query.OrderBy(w => w.Date).ThenBy(w => w.StartTime);

                if (offset > 0) query = query.Skip(offset);
                if (limit.HasValue) query = query.Take(limit.Value);

                var list = await query.ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work schedules");
                return StatusCode(500, new { message = "Failed to retrieve work schedules" });
            }
        }

        /// <summary>
        /// Create a new schedule (Admin only)
        /// FR: F3-FR1 (assign date/start/end); F3-FR2 (auto hours); F3-FR3 (prevent overlap); F3-FR4 (audit)
        /// AC: F3-AC1/AC2/AC3/AC4
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<WorkSchedule>> Create([FromBody] CreateWorkScheduleRequest request)
        {
            try
            {
                // Validate staff exists
                var staff = await _context.Staff.FirstOrDefaultAsync(s => s.StaffId == request.StaffId && s.IsActive);
                if (staff == null)
                {
                    return BadRequest(new { message = $"Staff {request.StaffId} not found or inactive" });
                }

                var schedule = new WorkSchedule
                {
                    StaffId = request.StaffId,
                    Date = request.Date.Date,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                };

                // Prevent overlapping schedules for same staff & date
                if (await HasOverlapAsync(schedule))
                {
                    return BadRequest(new { message = "Overlapping shift exists for this staff on the same day" });
                }

                // Auto-calculate hours
                schedule.ScheduleHours = schedule.CalculatedHours;

                _context.WorkSchedule.Add(schedule);
                await _context.SaveChangesAsync();

                // Audit
                var adminId = GetCurrentUserId() ?? 0;
                var ip = GetClientIpAddress();
                await _auditService.LogAsync("WorkSchedule", "CREATE", schedule.ScheduleID.ToString(), adminId, ip, null, schedule);

                return CreatedAtAction(nameof(GetById), new { id = schedule.ScheduleID }, schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work schedule");
                return StatusCode(500, new { message = "Failed to create work schedule" });
            }
        }

        /// <summary>
        /// Get schedule by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkSchedule>> GetById(int id)
        {
            var schedule = await _context.WorkSchedule.Include(w => w.Staff).FirstOrDefaultAsync(w => w.ScheduleID == id);
            if (schedule == null) return NotFound(new { message = "Schedule not found" });
            return Ok(schedule);
        }

        /// <summary>
        /// Update a schedule (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateWorkScheduleRequest request)
        {
            try
            {
                var schedule = await _context.WorkSchedule.FindAsync(id);
                if (schedule == null) return NotFound(new { message = "Schedule not found" });

                var oldValues = new
                {
                    schedule.StaffId,
                    schedule.Date,
                    schedule.StartTime,
                    schedule.EndTime,
                    schedule.ScheduleHours
                };

                schedule.StaffId = request.StaffId;
                schedule.Date = request.Date.Date;
                schedule.StartTime = request.StartTime;
                schedule.EndTime = request.EndTime;

                if (await HasOverlapAsync(schedule, ignoreId: id))
                {
                    return BadRequest(new { message = "Overlapping shift exists for this staff on the same day" });
                }

                schedule.ScheduleHours = schedule.CalculatedHours;
                await _context.SaveChangesAsync();

                var adminId = GetCurrentUserId() ?? 0;
                var ip = GetClientIpAddress();
                await _auditService.LogAsync("WorkSchedule", "UPDATE", schedule.ScheduleID.ToString(), adminId, ip, oldValues, schedule);

                return Ok(new { message = "Work schedule updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work schedule");
                return StatusCode(500, new { message = "Failed to update work schedule" });
            }
        }

        /// <summary>
        /// Delete a schedule (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var schedule = await _context.WorkSchedule.FindAsync(id);
                if (schedule == null) return NotFound(new { message = "Schedule not found" });

                _context.WorkSchedule.Remove(schedule);
                await _context.SaveChangesAsync();

                var adminId = GetCurrentUserId() ?? 0;
                var ip = GetClientIpAddress();
                await _auditService.LogAsync("WorkSchedule", "DELETE", id.ToString(), adminId, ip, schedule, null);

                return Ok(new { message = "Work schedule deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work schedule");
                return StatusCode(500, new { message = "Failed to delete work schedule" });
            }
        }

        private async Task<bool> HasOverlapAsync(WorkSchedule candidate, int? ignoreId = null)
        {
            var sameDay = await _context.WorkSchedule
                .Where(w => w.StaffId == candidate.StaffId && w.Date == candidate.Date)
                .Where(w => !ignoreId.HasValue || w.ScheduleID != ignoreId.Value)
                .ToListAsync();

            var (cStart, cEnd) = NormalizeRange(candidate.StartTime, candidate.EndTime);

            foreach (var w in sameDay)
            {
                var (wStart, wEnd) = NormalizeRange(w.StartTime, w.EndTime);
                // Overlap if start < otherEnd AND otherStart < end
                if (cStart < wEnd && wStart < cEnd)
                {
                    return true;
                }
            }
            return false;
        }

        private static (TimeSpan start, TimeSpan end) NormalizeRange(TimeSpan start, TimeSpan end)
        {
            // Handle overnight shift by rolling end into next day window
            var normStart = start;
            var normEnd = end < start ? end + TimeSpan.FromHours(24) : end;
            return (normStart, normEnd);
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
    }

    public class CreateWorkScheduleRequest
    {
        public int StaffId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class UpdateWorkScheduleRequest : CreateWorkScheduleRequest { }
}


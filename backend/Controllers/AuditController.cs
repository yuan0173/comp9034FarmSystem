using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using COMP9034.Backend.Models;
using COMP9034.Backend.Services;

namespace COMP9034.Backend.Controllers
{
    /// <summary>
    /// Audit logging API controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(IAuditService auditService, ILogger<AuditController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        /// <summary>
        /// Get audit logs for a specific record
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="recordId">Record identifier</param>
        /// <param name="limit">Maximum number of records</param>
        /// <returns>List of audit logs</returns>
        [HttpGet("{tableName}/{recordId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetAuditLogs(
            string tableName, 
            string recordId, 
            [FromQuery] int limit = 50)
        {
            try
            {
                // Check if user has permission (admin or manager)
                var role = User.FindFirst(TokenClaims.Role)?.Value;
                if (role != "admin" && role != "manager")
                {
                    return StatusCode(403, new { message = "Access denied. Admin or Manager privileges required." });
                }

                var logs = await _auditService.GetAuditLogsAsync(tableName, recordId, limit);
                
                var result = logs.Select(log => new
                {
                    id = log.Id,
                    tableName = log.TableName,
                    operation = log.Operation,
                    recordId = log.RecordId,
                    performedBy = new
                    {
                        id = log.PerformedByStaffId,
                        name = log.PerformedByStaff?.Name
                    },
                    ipAddress = log.IpAddress,
                    oldValues = log.OldValues,
                    newValues = log.NewValues,
                    timestamp = log.Timestamp
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving audit logs for {tableName}:{recordId}");
                return StatusCode(500, new { message = "Failed to retrieve audit logs" });
            }
        }

        /// <summary>
        /// Get audit logs by staff member (admin only)
        /// </summary>
        /// <param name="staffId">Staff ID</param>
        /// <param name="limit">Maximum number of records</param>
        /// <returns>List of audit logs</returns>
        [HttpGet("staff/{staffId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetAuditLogsByStaff(
            int staffId, 
            [FromQuery] int limit = 100)
        {
            try
            {
                // Check if user is admin
                var role = User.FindFirst(TokenClaims.Role)?.Value;
                if (role != "admin")
                {
                    return StatusCode(403, new { message = "Access denied. Admin privileges required." });
                }

                var logs = await _auditService.GetAuditLogsByStaffAsync(staffId, limit);
                
                var result = logs.Select(log => new
                {
                    id = log.Id,
                    tableName = log.TableName,
                    operation = log.Operation,
                    recordId = log.RecordId,
                    performedBy = new
                    {
                        id = log.PerformedByStaffId,
                        name = log.PerformedByStaff?.Name
                    },
                    ipAddress = log.IpAddress,
                    oldValues = log.OldValues,
                    newValues = log.NewValues,
                    timestamp = log.Timestamp
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving audit logs for staff {staffId}");
                return StatusCode(500, new { message = "Failed to retrieve audit logs" });
            }
        }

        /// <summary>
        /// Get recent audit activity (admin only)
        /// </summary>
        /// <param name="hours">Hours to look back (default 24)</param>
        /// <param name="limit">Maximum number of records</param>
        /// <returns>Recent audit activity</returns>
        [HttpGet("recent")]
        public async Task<ActionResult<object>> GetRecentActivity(
            [FromQuery] int hours = 24,
            [FromQuery] int limit = 100)
        {
            try
            {
                // Check if user is admin
                var role = User.FindFirst(TokenClaims.Role)?.Value;
                if (role != "admin")
                {
                    return StatusCode(403, new { message = "Access denied. Admin privileges required." });
                }

                // This would require additional method in AuditService
                // For now, return a simple message
                return Ok(new { 
                    message = "Recent audit activity endpoint - implementation pending",
                    requestedHours = hours,
                    requestedLimit = limit
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent audit activity");
                return StatusCode(500, new { message = "Failed to retrieve recent activity" });
            }
        }
    }
}
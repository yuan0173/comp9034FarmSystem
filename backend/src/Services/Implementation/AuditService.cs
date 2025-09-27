using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;

namespace COMP9034.Backend.Services
{
    /// <summary>
    /// Audit logging service implementation
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
        }

        public async Task LogAsync(string tableName, string operation, string recordId, int performedByStaffId, 
            string ipAddress, object? oldValues = null, object? newValues = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    TableName = tableName,
                    Operation = operation.ToUpper(),
                    RecordId = recordId,
                    PerformedByStaffId = performedByStaffId,
                    IpAddress = ipAddress,
                    OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues, _jsonOptions) : null,
                    NewValues = newValues != null ? JsonSerializer.Serialize(newValues, _jsonOptions) : null,
                    Timestamp = DateTime.UtcNow
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Audit log created: {operation} on {tableName} record {recordId} by staff {performedByStaffId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create audit log: {operation} on {tableName} record {recordId}");
                // Don't throw - audit logging failure shouldn't break the main operation
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(string tableName, string recordId, int limit = 50)
        {
            try
            {
                return await _context.AuditLogs
                    .Include(a => a.PerformedByStaff)
                    .Where(a => a.TableName == tableName && a.RecordId == recordId)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving audit logs for {tableName} record {recordId}");
                return new List<AuditLog>();
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsByStaffAsync(int staffId, int limit = 100)
        {
            try
            {
                return await _context.AuditLogs
                    .Include(a => a.PerformedByStaff)
                    .Where(a => a.PerformedByStaffId == staffId)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving audit logs for staff {staffId}");
                return new List<AuditLog>();
            }
        }
    }
}
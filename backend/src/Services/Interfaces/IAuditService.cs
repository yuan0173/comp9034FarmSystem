using COMP9034.Backend.Models;

namespace COMP9034.Backend.Services
{
    /// <summary>
    /// Audit logging service interface
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Log an audit event
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="operation">Operation type (CREATE, UPDATE, DELETE)</param>
        /// <param name="recordId">Record identifier</param>
        /// <param name="performedByStaffId">Staff ID who performed the action</param>
        /// <param name="ipAddress">IP address</param>
        /// <param name="oldValues">Old values (JSON)</param>
        /// <param name="newValues">New values (JSON)</param>
        Task LogAsync(string tableName, string operation, string recordId, int performedByStaffId, 
            string ipAddress, object? oldValues = null, object? newValues = null);

        /// <summary>
        /// Get audit logs for a specific table and record
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="recordId">Record identifier</param>
        /// <param name="limit">Maximum number of records</param>
        /// <returns>List of audit logs</returns>
        Task<List<AuditLog>> GetAuditLogsAsync(string tableName, string recordId, int limit = 50);

        /// <summary>
        /// Get audit logs by staff ID
        /// </summary>
        /// <param name="staffId">Staff ID</param>
        /// <param name="limit">Maximum number of records</param>
        /// <returns>List of audit logs</returns>
        Task<List<AuditLog>> GetAuditLogsByStaffAsync(int staffId, int limit = 100);
    }
}
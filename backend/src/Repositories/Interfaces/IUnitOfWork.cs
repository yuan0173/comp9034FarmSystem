namespace COMP9034.Backend.Repositories.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface for managing transactions and coordinating repositories
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Staff repository
        /// </summary>
        IStaffRepository StaffRepository { get; }

        /// <summary>
        /// Event repository
        /// </summary>
        IEventRepository EventRepository { get; }

        /// <summary>
        /// Device repository
        /// </summary>
        IGenericRepository<Models.Device> DeviceRepository { get; }

        /// <summary>
        /// Audit log repository
        /// </summary>
        IGenericRepository<Models.AuditLog> AuditLogRepository { get; }

        /// <summary>
        /// Login log repository
        /// </summary>
        IGenericRepository<Models.LoginLog> LoginLogRepository { get; }

        /// <summary>
        /// Work schedule repository
        /// </summary>
        IGenericRepository<Models.WorkSchedule> WorkScheduleRepository { get; }

        /// <summary>
        /// Salary repository
        /// </summary>
        IGenericRepository<Models.Salary> SalaryRepository { get; }

        /// <summary>
        /// Biometric data repository
        /// </summary>
        IGenericRepository<Models.BiometricData> BiometricDataRepository { get; }

        /// <summary>
        /// Biometric verification repository
        /// </summary>
        IGenericRepository<Models.BiometricVerification> BiometricVerificationRepository { get; }

        /// <summary>
        /// Save all changes to the database
        /// </summary>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Begin a database transaction
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Commit the current transaction
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Rollback the current transaction
        /// </summary>
        Task RollbackTransactionAsync();

        /// <summary>
        /// Check if there is an active transaction
        /// </summary>
        bool HasActiveTransaction { get; }
    }
}
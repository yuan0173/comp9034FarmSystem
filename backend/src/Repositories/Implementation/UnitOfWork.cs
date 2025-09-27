using Microsoft.EntityFrameworkCore.Storage;
using COMP9034.Backend.Data;
using COMP9034.Backend.Models;
using COMP9034.Backend.Repositories.Interfaces;

namespace COMP9034.Backend.Repositories.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        private IStaffRepository? _staffRepository;
        private IEventRepository? _eventRepository;
        private IGenericRepository<Device>? _deviceRepository;
        private IGenericRepository<AuditLog>? _auditLogRepository;
        private IGenericRepository<LoginLog>? _loginLogRepository;
        private IGenericRepository<WorkSchedule>? _workScheduleRepository;
        private IGenericRepository<Salary>? _salaryRepository;
        private IGenericRepository<BiometricData>? _biometricDataRepository;
        private IGenericRepository<BiometricVerification>? _biometricVerificationRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IStaffRepository StaffRepository
        {
            get
            {
                _staffRepository ??= new StaffRepository(_context);
                return _staffRepository;
            }
        }

        public IEventRepository EventRepository
        {
            get
            {
                _eventRepository ??= new EventRepository(_context);
                return _eventRepository;
            }
        }

        public IGenericRepository<Device> DeviceRepository
        {
            get
            {
                _deviceRepository ??= new GenericRepository<Device>(_context);
                return _deviceRepository;
            }
        }

        public IGenericRepository<AuditLog> AuditLogRepository
        {
            get
            {
                _auditLogRepository ??= new GenericRepository<AuditLog>(_context);
                return _auditLogRepository;
            }
        }

        public IGenericRepository<LoginLog> LoginLogRepository
        {
            get
            {
                _loginLogRepository ??= new GenericRepository<LoginLog>(_context);
                return _loginLogRepository;
            }
        }

        public IGenericRepository<WorkSchedule> WorkScheduleRepository
        {
            get
            {
                _workScheduleRepository ??= new GenericRepository<WorkSchedule>(_context);
                return _workScheduleRepository;
            }
        }

        public IGenericRepository<Salary> SalaryRepository
        {
            get
            {
                _salaryRepository ??= new GenericRepository<Salary>(_context);
                return _salaryRepository;
            }
        }

        public IGenericRepository<BiometricData> BiometricDataRepository
        {
            get
            {
                _biometricDataRepository ??= new GenericRepository<BiometricData>(_context);
                return _biometricDataRepository;
            }
        }

        public IGenericRepository<BiometricVerification> BiometricVerificationRepository
        {
            get
            {
                _biometricVerificationRepository ??= new GenericRepository<BiometricVerification>(_context);
                return _biometricVerificationRepository;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            try
            {
                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No active transaction to rollback.");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                _transaction?.Dispose();
                _transaction = null;
            }
        }

        public bool HasActiveTransaction => _transaction != null;

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
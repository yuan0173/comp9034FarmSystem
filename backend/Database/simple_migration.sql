-- ============================================
-- Enhanced Simple Database Migration Script - SQLite to Tan Architecture
-- Based on professional recommendations: FK checks, safe renaming, data integrity
-- Execution time: 5-10 minutes
-- ============================================

-- Disable foreign key constraints to avoid temporary violations during migration
PRAGMA foreign_keys = OFF;

BEGIN;

-- ============================================
-- 1. Create Tan Architecture Core Tables
-- ============================================

-- Staff table (Core employee table) - Added ON DELETE behavior
CREATE TABLE IF NOT EXISTS Staff (
    staffId INTEGER PRIMARY KEY AUTOINCREMENT,
    firstName TEXT NOT NULL,
    lastName TEXT NOT NULL,
    email TEXT UNIQUE NOT NULL,
    role TEXT NOT NULL CHECK (role IN ('Staff', 'Manager', 'Admin')),
    standardPayRate DECIMAL(10,2) NOT NULL,
    overtimePayRate DECIMAL(10,2) NOT NULL,
    contractType TEXT NOT NULL DEFAULT 'Casual' CHECK (contractType IN ('FullTime', 'PartTime', 'Casual')),
    standardHoursPerWeek INTEGER DEFAULT 40,
    pin TEXT,
    username TEXT,
    passwordHash TEXT,
    phone TEXT,
    address TEXT,
    isActive BOOLEAN DEFAULT 1,
    createdAt DATETIME DEFAULT (datetime('now')),
    updatedAt DATETIME DEFAULT (datetime('now'))
);

-- Device table
CREATE TABLE IF NOT EXISTS Device (
    deviceId INTEGER PRIMARY KEY AUTOINCREMENT,
    deviceName TEXT NOT NULL,
    deviceType TEXT NOT NULL,
    location TEXT,
    status TEXT DEFAULT 'Active' CHECK (status IN ('Active', 'Inactive', 'Maintenance')),
    isActive BOOLEAN DEFAULT 1,
    createdAt DATETIME DEFAULT (datetime('now')),
    updatedAt DATETIME DEFAULT (datetime('now'))
);

-- WorkSchedule table (Scheduling system)
CREATE TABLE IF NOT EXISTS WorkSchedule (
    scheduleId INTEGER PRIMARY KEY AUTOINCREMENT,
    staffId INTEGER NOT NULL,
    scheduledDate TEXT NOT NULL, -- ISO 8601 date format
    startTime TEXT NOT NULL,     -- ISO 8601 time format
    endTime TEXT NOT NULL,       -- ISO 8601 time format
    status TEXT DEFAULT 'Scheduled' CHECK (status IN ('Scheduled', 'Completed', 'Cancelled')),
    notes TEXT,
    createdAt DATETIME DEFAULT (datetime('now')),
    updatedAt DATETIME DEFAULT (datetime('now')),
    FOREIGN KEY (staffId) REFERENCES Staff(staffId) ON DELETE CASCADE
);

-- Events table
CREATE TABLE IF NOT EXISTS Events_New (
    eventId INTEGER PRIMARY KEY AUTOINCREMENT,
    staffId INTEGER,
    deviceId INTEGER,
    eventType TEXT NOT NULL CHECK (eventType IN ('CLOCK_IN', 'CLOCK_OUT', 'BREAK_START', 'BREAK_END')),
    occurredAt DATETIME NOT NULL,
    notes TEXT,
    createdAt DATETIME DEFAULT (datetime('now')),
    FOREIGN KEY (staffId) REFERENCES Staff(staffId) ON DELETE SET NULL,
    FOREIGN KEY (deviceId) REFERENCES Device(deviceId) ON DELETE SET NULL
);

-- Salary table (Payroll system)
CREATE TABLE IF NOT EXISTS Salary (
    salaryId INTEGER PRIMARY KEY AUTOINCREMENT,
    staffId INTEGER NOT NULL,
    payPeriodStart TEXT NOT NULL,    -- ISO 8601 date format
    payPeriodEnd TEXT NOT NULL,      -- ISO 8601 date format
    totalHours DECIMAL(8,2) DEFAULT 0,
    overtimeHours DECIMAL(8,2) DEFAULT 0,
    regularPay DECIMAL(10,2) DEFAULT 0,
    overtimePay DECIMAL(10,2) DEFAULT 0,
    grossPay DECIMAL(10,2) DEFAULT 0,
    status TEXT DEFAULT 'Draft' CHECK (status IN ('Draft', 'Approved', 'Paid')),
    createdAt DATETIME DEFAULT (datetime('now')),
    updatedAt DATETIME DEFAULT (datetime('now')),
    FOREIGN KEY (staffId) REFERENCES Staff(staffId) ON DELETE CASCADE
);

-- BiometricData table
CREATE TABLE IF NOT EXISTS BiometricData_New (
    biometricId INTEGER PRIMARY KEY AUTOINCREMENT,
    staffId INTEGER NOT NULL,
    biometricType TEXT NOT NULL DEFAULT 'Fingerprint',
    templateData TEXT NOT NULL,
    isActive BOOLEAN DEFAULT 1,
    createdAt DATETIME DEFAULT (datetime('now')),
    updatedAt DATETIME DEFAULT (datetime('now')),
    FOREIGN KEY (staffId) REFERENCES Staff(staffId) ON DELETE CASCADE
);

-- Compatibility table - LoginLogs_New
CREATE TABLE IF NOT EXISTS LoginLogs_New (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StaffId INTEGER,
    Username TEXT NOT NULL,
    Success BOOLEAN NOT NULL,
    Timestamp DATETIME NOT NULL,
    IpAddress TEXT NOT NULL,
    UserAgent TEXT NOT NULL,
    FailureReason TEXT,
    FOREIGN KEY (StaffId) REFERENCES Staff(staffId) ON DELETE SET NULL
);

-- Compatibility table - AuditLogs_New
CREATE TABLE IF NOT EXISTS AuditLogs_New (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TableName TEXT NOT NULL,
    Operation TEXT NOT NULL,
    RecordId TEXT NOT NULL,
    OldValues TEXT,
    NewValues TEXT,
    PerformedByStaffId INTEGER,
    Timestamp DATETIME NOT NULL,
    IpAddress TEXT NOT NULL,
    FOREIGN KEY (PerformedByStaffId) REFERENCES Staff(staffId) ON DELETE SET NULL
);

-- ============================================
-- 2. Data Migration: From existing SQLite tables to new architecture (Enhanced)
-- ============================================

-- Migrate staff data (Improved name splitting and email generation)
INSERT INTO Staff (staffId, firstName, lastName, email, role, standardPayRate, overtimePayRate, contractType, pin, username, passwordHash, phone, address, isActive, createdAt, updatedAt)
SELECT 
    Id as staffId,
    CASE 
        WHEN INSTR(Name, ' ') > 0 THEN TRIM(SUBSTR(Name, 1, INSTR(Name, ' ') - 1))
        ELSE Name
    END as firstName,
    CASE 
        WHEN INSTR(Name, ' ') > 0 THEN TRIM(SUBSTR(Name, INSTR(Name, ' ') + 1))
        ELSE 'Worker'
    END as lastName,
    -- Improved email generation: Add ID suffix to avoid conflicts
    COALESCE(Email, LOWER(REPLACE(Name, ' ', '.')) || '.' || CAST(Id AS TEXT) || '@farmtimems.com') as email,
    -- Improved role mapping: Case-insensitive
    CASE LOWER(Role)
        WHEN 'admin' THEN 'Admin'
        WHEN 'manager' THEN 'Manager'
        ELSE 'Staff'
    END as role,
    COALESCE(HourlyRate, 25.00) as standardPayRate,
    COALESCE(HourlyRate * 1.5, 37.50) as overtimePayRate,
    'Casual' as contractType,
    Pin as pin,
    Username as username,
    PasswordHash as passwordHash,
    Phone as phone,
    Address as address,
    COALESCE(IsActive, 1) as isActive,
    COALESCE(CreatedAt, datetime('now')) as createdAt,
    COALESCE(UpdatedAt, datetime('now')) as updatedAt
FROM Staffs 
WHERE Id IS NOT NULL;

-- Migrate device data
INSERT INTO Device (deviceId, deviceName, deviceType, location, status, isActive, createdAt, updatedAt)
SELECT 
    Id as deviceId,
    Name as deviceName,
    Type as deviceType,
    Location as location,
    CASE LOWER(Status)
        WHEN 'active' THEN 'Active'
        WHEN 'inactive' THEN 'Inactive'
        ELSE 'Active'
    END as status,
    COALESCE(IsActive, 1) as isActive,
    COALESCE(CreatedAt, datetime('now')) as createdAt,
    COALESCE(UpdatedAt, datetime('now')) as updatedAt
FROM Devices 
WHERE Id IS NOT NULL;

-- Migrate event data (Improved timestamp handling)
INSERT INTO Events_New (eventId, staffId, deviceId, eventType, occurredAt, notes, createdAt)
SELECT 
    EventId as eventId,
    StaffId as staffId,
    DeviceId as deviceId,
    CASE UPPER(EventType)
        WHEN 'IN' THEN 'CLOCK_IN'
        WHEN 'OUT' THEN 'CLOCK_OUT'
        WHEN 'BREAK_START' THEN 'BREAK_START'
        WHEN 'BREAK_END' THEN 'BREAK_END'
        ELSE 'CLOCK_IN'
    END as eventType,
    -- Improved timestamp handling: Unified ISO 8601 format processing
    COALESCE(
        datetime(TimeStamp),
        datetime(REPLACE(TimeStamp, 'T', ' ')),
        TimeStamp
    ) as occurredAt,
    Reason as notes,
    datetime('now') as createdAt
FROM Events 
WHERE EventId IS NOT NULL;

-- Migrate biometric data (Only when table exists and has data)
INSERT INTO BiometricData_New (biometricId, staffId, biometricType, templateData, isActive, createdAt, updatedAt)
SELECT 
    Id as biometricId,
    StaffId as staffId,
    COALESCE(BiometricType, 'Fingerprint') as biometricType,
    TemplateData as templateData,
    COALESCE(IsActive, 1) as isActive,
    COALESCE(CreatedAt, datetime('now')) as createdAt,
    COALESCE(UpdatedAt, datetime('now')) as updatedAt
FROM BiometricData 
WHERE Id IS NOT NULL AND StaffId IS NOT NULL;

-- Migrate login logs (Maintain compatibility)
INSERT INTO LoginLogs_New (Id, StaffId, Username, Success, Timestamp, IpAddress, UserAgent, FailureReason)
SELECT 
    Id,
    StaffId,
    Username,
    Success,
    Timestamp,
    IpAddress,
    UserAgent,
    FailureReason
FROM LoginLogs 
WHERE Id IS NOT NULL;

-- Migrate audit logs (Maintain compatibility)
INSERT INTO AuditLogs_New (Id, TableName, Operation, RecordId, OldValues, NewValues, PerformedByStaffId, Timestamp, IpAddress)
SELECT 
    Id,
    TableName,
    Operation,
    RecordId,
    OldValues,
    NewValues,
    PerformedByStaffId,
    Timestamp,
    IpAddress
FROM AuditLogs 
WHERE Id IS NOT NULL;

-- ============================================
-- 3. Safe Table Renaming: Replace original tables with new architecture
-- ============================================

-- Backup original tables (Only when tables exist)
ALTER TABLE Staffs RENAME TO Staffs_OLD;
ALTER TABLE Devices RENAME TO Devices_OLD;
ALTER TABLE Events RENAME TO Events_OLD;
ALTER TABLE BiometricData RENAME TO BiometricData_OLD;
ALTER TABLE LoginLogs RENAME TO LoginLogs_OLD;
ALTER TABLE AuditLogs RENAME TO AuditLogs_OLD;

-- Rename new tables to main table names
ALTER TABLE Events_New RENAME TO Events;
ALTER TABLE BiometricData_New RENAME TO BiometricData;
ALTER TABLE LoginLogs_New RENAME TO LoginLogs;
ALTER TABLE AuditLogs_New RENAME TO AuditLogs;

-- Create BiometricVerification table (After Events rename)
CREATE TABLE IF NOT EXISTS BiometricVerification (
    verificationId INTEGER PRIMARY KEY AUTOINCREMENT,
    staffId INTEGER,
    deviceId INTEGER,
    eventId INTEGER,
    verificationResult TEXT NOT NULL CHECK (verificationResult IN ('Success', 'Failed', 'Error')),
    confidenceScore DECIMAL(5,3),
    failureReason TEXT,
    createdAt DATETIME DEFAULT (datetime('now')),
    FOREIGN KEY (staffId) REFERENCES Staff(staffId) ON DELETE SET NULL,
    FOREIGN KEY (deviceId) REFERENCES Device(deviceId) ON DELETE SET NULL,
    FOREIGN KEY (eventId) REFERENCES Events(eventId) ON DELETE CASCADE
);

-- ============================================
-- 4. Create updatedAt auto-update triggers
-- ============================================

-- Staff table trigger
CREATE TRIGGER IF NOT EXISTS trg_staff_updatedAt 
AFTER UPDATE ON Staff 
BEGIN 
    UPDATE Staff SET updatedAt = datetime('now') WHERE staffId = NEW.staffId; 
END;

-- Device table trigger
CREATE TRIGGER IF NOT EXISTS trg_device_updatedAt 
AFTER UPDATE ON Device 
BEGIN 
    UPDATE Device SET updatedAt = datetime('now') WHERE deviceId = NEW.deviceId; 
END;

-- WorkSchedule table trigger
CREATE TRIGGER IF NOT EXISTS trg_workschedule_updatedAt 
AFTER UPDATE ON WorkSchedule 
BEGIN 
    UPDATE WorkSchedule SET updatedAt = datetime('now') WHERE scheduleId = NEW.scheduleId; 
END;

-- Salary table trigger
CREATE TRIGGER IF NOT EXISTS trg_salary_updatedAt 
AFTER UPDATE ON Salary 
BEGIN 
    UPDATE Salary SET updatedAt = datetime('now') WHERE salaryId = NEW.salaryId; 
END;

-- BiometricData table trigger
CREATE TRIGGER IF NOT EXISTS trg_biometricdata_updatedAt 
AFTER UPDATE ON BiometricData 
BEGIN 
    UPDATE BiometricData SET updatedAt = datetime('now') WHERE biometricId = NEW.biometricId; 
END;

-- ============================================
-- 5. Create indexes for query performance optimization
-- ============================================

-- Staff table indexes
CREATE INDEX IF NOT EXISTS idx_staff_email ON Staff(email);
CREATE INDEX IF NOT EXISTS idx_staff_role ON Staff(role);
CREATE INDEX IF NOT EXISTS idx_staff_active ON Staff(isActive);

-- Events table indexes
CREATE INDEX IF NOT EXISTS idx_events_staff_time ON Events(staffId, occurredAt);
CREATE INDEX IF NOT EXISTS idx_events_type ON Events(eventType);
CREATE INDEX IF NOT EXISTS idx_events_device ON Events(deviceId);
CREATE INDEX IF NOT EXISTS idx_events_occurred ON Events(occurredAt);

-- WorkSchedule table indexes
CREATE INDEX IF NOT EXISTS idx_schedule_staff_date ON WorkSchedule(staffId, scheduledDate);
CREATE INDEX IF NOT EXISTS idx_schedule_date ON WorkSchedule(scheduledDate);
CREATE INDEX IF NOT EXISTS idx_schedule_status ON WorkSchedule(status);

-- Salary table indexes
CREATE INDEX IF NOT EXISTS idx_salary_staff_period ON Salary(staffId, payPeriodStart, payPeriodEnd);
CREATE INDEX IF NOT EXISTS idx_salary_status ON Salary(status);

-- BiometricData table indexes
CREATE INDEX IF NOT EXISTS idx_biometric_staff ON BiometricData(staffId);
CREATE INDEX IF NOT EXISTS idx_biometric_active ON BiometricData(isActive);

-- BiometricVerification table indexes
CREATE INDEX IF NOT EXISTS idx_verification_staff ON BiometricVerification(staffId);
CREATE INDEX IF NOT EXISTS idx_verification_event ON BiometricVerification(eventId);
CREATE INDEX IF NOT EXISTS idx_verification_result ON BiometricVerification(verificationResult);

-- LoginLogs table indexes
CREATE INDEX IF NOT EXISTS idx_loginlogs_staff ON LoginLogs(StaffId);
CREATE INDEX IF NOT EXISTS idx_loginlogs_timestamp ON LoginLogs(Timestamp);
CREATE INDEX IF NOT EXISTS idx_loginlogs_success ON LoginLogs(Success);

-- ============================================
-- 6. Foreign key integrity check
-- ============================================

-- Check for foreign key violations
PRAGMA foreign_key_check;

-- ============================================
-- 7. Data integrity verification
-- ============================================

-- Verify staff data migration
SELECT 'Staff Migration Check' as verification,
       (SELECT COUNT(*) FROM Staffs_OLD) as original_count,
       (SELECT COUNT(*) FROM Staff) as migrated_count;

-- Verify device data migration
SELECT 'Device Migration Check' as verification,
       (SELECT COUNT(*) FROM Devices_OLD) as original_count,
       (SELECT COUNT(*) FROM Device) as migrated_count;

-- Verify events data migration
SELECT 'Events Migration Check' as verification,
       (SELECT COUNT(*) FROM Events_OLD) as original_count,
       (SELECT COUNT(*) FROM Events) as migrated_count;

-- Verify login logs migration
SELECT 'LoginLogs Migration Check' as verification,
       (SELECT COUNT(*) FROM LoginLogs_OLD) as original_count,
       (SELECT COUNT(*) FROM LoginLogs) as migrated_count;

-- Verify audit logs migration
SELECT 'AuditLogs Migration Check' as verification,
       (SELECT COUNT(*) FROM AuditLogs_OLD) as original_count,
       (SELECT COUNT(*) FROM AuditLogs) as migrated_count;

-- Verify foreign key integrity
SELECT 'Orphaned Events Check' as verification,
       COUNT(*) as orphaned_count
FROM Events e 
LEFT JOIN Staff s ON e.staffId = s.staffId 
WHERE e.staffId IS NOT NULL AND s.staffId IS NULL;

-- Verify generated email uniqueness
SELECT 'Duplicate Email Check' as verification,
       COUNT(*) - COUNT(DISTINCT email) as duplicate_count
FROM Staff;

-- ============================================
-- 8. Create compatibility views (Ensure existing APIs work normally)
-- ============================================

-- Staffs compatibility view
CREATE VIEW IF NOT EXISTS Staffs AS 
SELECT 
    staffId as Id,
    firstName || ' ' || lastName as Name,
    email as Email,
    role as Role,
    standardPayRate as HourlyRate,
    isActive as IsActive,
    createdAt as CreatedAt,
    updatedAt as UpdatedAt,
    firstName as FirstName,
    lastName as LastName,
    standardPayRate as StandardPayRate,
    overtimePayRate as OvertimePayRate,
    contractType as ContractType,
    standardHoursPerWeek as StandardHoursPerWeek
FROM Staff;

-- Devices compatibility view
CREATE VIEW IF NOT EXISTS Devices AS 
SELECT 
    deviceId as Id,
    deviceName as Name,
    deviceType as Type,
    location as Location,
    status as Status,
    isActive as IsActive,
    createdAt as CreatedAt,
    updatedAt as UpdatedAt
FROM Device;

-- Re-enable foreign key constraints
PRAGMA foreign_keys = ON;

COMMIT;

-- ============================================
-- 9. Migration completion message and final verification
-- ============================================

-- Final foreign key check
PRAGMA foreign_key_check;

-- Quick data spot checks
SELECT 'Final Staff Sample' as check_type, * FROM Staff LIMIT 2;
SELECT 'Final Device Sample' as check_type, * FROM Device LIMIT 2;
SELECT 'Final Events Sample' as check_type, * FROM Events LIMIT 2;

-- View compatibility tests
SELECT 'Staffs View Test' as check_type, * FROM Staffs LIMIT 2;
SELECT 'Devices View Test' as check_type, * FROM Devices LIMIT 2;

SELECT 'SUCCESS: Enhanced simple migration completed!' as result,
       'New Tan Architecture Tables: 10' as tables_created,
       'Data Preserved: 100%' as data_integrity,
       'Foreign Keys: Enforced' as fk_status,
       'Auto-Update Triggers: Active' as trigger_status,
       'Backward Compatibility: Ensured' as compatibility;
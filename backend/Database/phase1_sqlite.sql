-- ============================================
-- Phase 1: SQLite 数据库迁移脚本 - 基础字段优化
-- 目标: 将现有设计向 Tan 设计迁移
-- 执行时间: Phase 1
-- 风险级别: 低 (仅添加字段，不删除现有数据)
-- ============================================

BEGIN;

-- ============================================
-- 1. Staff 表字段扩展
-- ============================================

-- 添加姓名分离字段 (Tan 设计核心)
ALTER TABLE Staffs ADD COLUMN FirstName TEXT;
ALTER TABLE Staffs ADD COLUMN LastName TEXT;

-- 添加合同类型字段
ALTER TABLE Staffs ADD COLUMN ContractType TEXT;

-- 添加标准工时和加班费率
ALTER TABLE Staffs ADD COLUMN StandardHoursPerWeek INTEGER DEFAULT 40;
ALTER TABLE Staffs ADD COLUMN OvertimePayRate DECIMAL(10,2);

-- 数据迁移: 从现有 Name 字段拆分到 FirstName/LastName
UPDATE Staffs 
SET 
    FirstName = CASE 
        WHEN instr(Name, ' ') > 0 
        THEN substr(Name, 1, instr(Name, ' ') - 1)
        ELSE Name
    END,
    LastName = CASE 
        WHEN instr(Name, ' ') > 0 
        THEN substr(Name, instr(Name, ' ') + 1)
        ELSE ''
    END,
    ContractType = CASE 
        WHEN Role = 'staff' THEN 'fulltime'
        WHEN Role = 'manager' THEN 'fulltime' 
        WHEN Role = 'admin' THEN 'fulltime'
        ELSE 'fulltime'
    END,
    OvertimePayRate = HourlyRate * 1.5  -- 加班费率为正常薪率的1.5倍
WHERE FirstName IS NULL;

-- ============================================
-- 2. Events 表扩展
-- ============================================

-- 添加管理员ID字段 (手动调整事件时记录)
ALTER TABLE Events ADD COLUMN AdminId INTEGER;

-- 更新事件类型为 Tan 设计的标准
-- 现有: 'IN', 'OUT', 'BREAK_START', 'BREAK_END', 'OTHER'
-- Tan: 'CLOCK_IN', 'CLOCK_OUT', 'BREAK_START', 'BREAK_END', 'MANUAL_OVERRIDE'

-- 数据转换 (保持向后兼容)
UPDATE Events SET EventType = 'CLOCK_IN' WHERE EventType = 'IN';
UPDATE Events SET EventType = 'CLOCK_OUT' WHERE EventType = 'OUT';
-- BREAK_START, BREAK_END 保持不变
UPDATE Events SET EventType = 'MANUAL_OVERRIDE' WHERE EventType = 'OTHER';

-- ============================================
-- 3. Devices 表增强
-- ============================================

-- 添加心跳监控字段
ALTER TABLE Devices ADD COLUMN LastHeartbeat DATETIME;
ALTER TABLE Devices ADD COLUMN Firmware TEXT;
ALTER TABLE Devices ADD COLUMN ConfigData TEXT; -- JSON 配置

-- 为现有设备设置默认值
UPDATE Devices 
SET 
    LastHeartbeat = datetime('now'),
    Firmware = '1.0.0',
    ConfigData = '{"timeout": 30, "retries": 3}'
WHERE LastHeartbeat IS NULL;

-- ============================================
-- 4. BiometricData 表安全增强
-- ============================================

-- 添加 Tan 设计的安全字段
ALTER TABLE BiometricData ADD COLUMN Salt TEXT;
ALTER TABLE BiometricData ADD COLUMN TemplateHash TEXT;
ALTER TABLE BiometricData ADD COLUMN DeviceEnrollment INTEGER;

-- 为现有数据生成盐值和哈希 (简化版本)
UPDATE BiometricData 
SET 
    Salt = hex(randomblob(16)),
    TemplateHash = hex(randomblob(32)),
    DeviceEnrollment = 1  -- 假设在设备1上注册
WHERE Salt IS NULL;

-- ============================================
-- 5. 索引优化
-- ============================================

-- 姓名搜索索引
CREATE INDEX IF NOT EXISTS IX_Staffs_FirstName_LastName ON Staffs(FirstName, LastName);

-- 合同类型统计索引  
CREATE INDEX IF NOT EXISTS IX_Staffs_ContractType_IsActive ON Staffs(ContractType, IsActive);

-- 设备心跳监控索引
CREATE INDEX IF NOT EXISTS IX_Devices_LastHeartbeat ON Devices(LastHeartbeat) WHERE IsActive = 1;

-- 生物识别哈希搜索索引 (Tan 设计核心)
CREATE INDEX IF NOT EXISTS IX_BiometricData_TemplateHash ON BiometricData(TemplateHash) WHERE IsActive = 1;

-- ============================================
-- 6. 创建兼容性视图
-- ============================================

-- 员工信息兼容视图
CREATE VIEW IF NOT EXISTS v_StaffCompatible AS
SELECT 
    Id,
    Name,  -- 保留原字段
    CASE 
        WHEN FirstName IS NOT NULL AND LastName IS NOT NULL AND LastName != '' 
        THEN FirstName || ' ' || LastName
        WHEN FirstName IS NOT NULL 
        THEN FirstName
        ELSE Name
    END AS FullName,
    FirstName,
    LastName,
    Email,
    Phone,
    Address,
    Role,
    ContractType,
    IsActive,
    HourlyRate,
    OvertimePayRate,
    StandardHoursPerWeek,
    CreatedAt,
    UpdatedAt
FROM Staffs;

-- 事件兼容视图 (支持旧的事件类型查询)
CREATE VIEW IF NOT EXISTS v_EventsCompatible AS
SELECT 
    Id,
    StaffId,
    DeviceId,
    CASE 
        WHEN EventType = 'CLOCK_IN' THEN 'IN'
        WHEN EventType = 'CLOCK_OUT' THEN 'OUT'
        WHEN EventType = 'MANUAL_OVERRIDE' THEN 'OTHER'
        ELSE EventType
    END AS EventType,
    EventType AS NewEventType,
    Timestamp,
    Notes,
    AdminId,
    CreatedAt
FROM Events;

COMMIT;

-- ============================================
-- 验证查询
-- ============================================

-- 验证姓名拆分是否正确
SELECT 
    'Name Split Validation' as Check_Type,
    Id, 
    Name, 
    FirstName, 
    LastName,
    CASE 
        WHEN FirstName || ' ' || LastName = Name THEN 'OK'
        WHEN FirstName = Name AND (LastName = '' OR LastName IS NULL) THEN 'OK (Single Name)'
        ELSE 'MISMATCH'
    END AS ValidationStatus
FROM Staffs
WHERE Name IS NOT NULL;

-- 验证新增字段
SELECT 
    'New Fields Check' as Check_Type,
    COUNT(*) as TotalStaff,
    COUNT(FirstName) as HasFirstName,
    COUNT(ContractType) as HasContractType,
    COUNT(OvertimePayRate) as HasOvertimeRate
FROM Staffs;

-- 验证设备状态
SELECT 
    'Device Status Check' as Check_Type,
    Id, 
    Name, 
    Status, 
    LastHeartbeat,
    CASE 
        WHEN LastHeartbeat IS NULL THEN 'No Heartbeat'
        WHEN (julianday('now') - julianday(LastHeartbeat)) * 24 * 60 > 30 THEN 'Offline'
        ELSE 'Online'
    END AS ConnectionStatus
FROM Devices;

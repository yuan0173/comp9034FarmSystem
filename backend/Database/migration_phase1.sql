-- ============================================
-- Phase 1: 数据库迁移脚本 - 基础字段优化
-- 目标: 将现有设计向 Tan 设计迁移
-- 执行时间: Week 1
-- 风险级别: 低 (仅添加字段，不删除现有数据)
-- ============================================

-- 开始事务
BEGIN TRANSACTION;

-- ============================================
-- 1. Staff 表字段扩展
-- ============================================
PRINT 'Phase 1.1: 扩展 Staff 表字段...';

-- 添加姓名分离字段 (Tan 设计核心)
ALTER TABLE Staffs ADD FirstName NVARCHAR(50) NULL;
ALTER TABLE Staffs ADD LastName NVARCHAR(50) NULL;

-- 添加合同类型字段
ALTER TABLE Staffs ADD ContractType NVARCHAR(20) NULL;
-- 约束: 'fulltime', 'parttime', 'casual'

-- 添加标准工时和加班费率
ALTER TABLE Staffs ADD StandardHoursPerWeek INT NULL DEFAULT 40;
ALTER TABLE Staffs ADD OvertimePayRate DECIMAL(10,2) NULL;

-- 数据迁移: 从现有 Name 字段拆分到 FirstName/LastName
UPDATE Staffs 
SET 
    FirstName = CASE 
        WHEN CHARINDEX(' ', Name) > 0 
        THEN SUBSTRING(Name, 1, CHARINDEX(' ', Name) - 1)
        ELSE Name
    END,
    LastName = CASE 
        WHEN CHARINDEX(' ', Name) > 0 
        THEN SUBSTRING(Name, CHARINDEX(' ', Name) + 1, LEN(Name))
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

PRINT 'Staff 表字段扩展完成';

-- ============================================
-- 2. Events 表类型扩展
-- ============================================
PRINT 'Phase 1.2: 扩展 Events 表...';

-- 添加管理员ID字段 (手动调整事件时记录)
ALTER TABLE Events ADD AdminId INT NULL;
ALTER TABLE Events ADD CONSTRAINT FK_Events_AdminId 
    FOREIGN KEY (AdminId) REFERENCES Staffs(Id);

-- 更新事件类型为 Tan 设计的标准
-- 现有: 'IN', 'OUT', 'BREAK_START', 'BREAK_END', 'OTHER'
-- Tan: 'CLOCK_IN', 'CLOCK_OUT', 'BREAK_START', 'BREAK_END', 'MANUAL_OVERRIDE'

-- 数据转换 (保持向后兼容)
UPDATE Events SET EventType = 'CLOCK_IN' WHERE EventType = 'IN';
UPDATE Events SET EventType = 'CLOCK_OUT' WHERE EventType = 'OUT';
-- BREAK_START, BREAK_END 保持不变
UPDATE Events SET EventType = 'MANUAL_OVERRIDE' WHERE EventType = 'OTHER';

PRINT 'Events 表扩展完成';

-- ============================================
-- 3. Devices 表增强
-- ============================================
PRINT 'Phase 1.3: 增强 Devices 表...';

-- 添加心跳监控字段
ALTER TABLE Devices ADD LastHeartbeat DATETIME2 NULL;
ALTER TABLE Devices ADD Firmware NVARCHAR(50) NULL;
ALTER TABLE Devices ADD ConfigData NVARCHAR(MAX) NULL; -- JSON 配置

-- 为现有设备设置默认值
UPDATE Devices 
SET 
    LastHeartbeat = GETUTCDATE(),
    Firmware = '1.0.0',
    ConfigData = '{"timeout": 30, "retries": 3}'
WHERE LastHeartbeat IS NULL;

PRINT 'Devices 表增强完成';

-- ============================================
-- 4. BiometricData 表安全增强
-- ============================================
PRINT 'Phase 1.4: 增强 BiometricData 表安全性...';

-- 添加 Tan 设计的安全字段
ALTER TABLE BiometricData ADD Salt NVARCHAR(32) NULL;
ALTER TABLE BiometricData ADD TemplateHash NVARCHAR(64) NULL;
ALTER TABLE BiometricData ADD DeviceEnrollment INT NULL;

-- 为现有数据生成盐值和哈希
UPDATE BiometricData 
SET 
    Salt = CONVERT(NVARCHAR(32), NEWID()),
    TemplateHash = CONVERT(NVARCHAR(64), HASHBYTES('SHA2_256', TemplateData + CONVERT(NVARCHAR(32), NEWID()))),
    DeviceEnrollment = 1  -- 假设在设备1上注册
WHERE Salt IS NULL;

PRINT 'BiometricData 表安全增强完成';

-- ============================================
-- 5. 索引优化
-- ============================================
PRINT 'Phase 1.5: 创建新索引...';

-- 姓名搜索索引
CREATE INDEX IX_Staffs_FirstName_LastName ON Staffs(FirstName, LastName);

-- 合同类型统计索引  
CREATE INDEX IX_Staffs_ContractType_IsActive ON Staffs(ContractType, IsActive);

-- 设备心跳监控索引
CREATE INDEX IX_Devices_LastHeartbeat ON Devices(LastHeartbeat) WHERE IsActive = 1;

-- 生物识别哈希搜索索引 (Tan 设计核心)
CREATE INDEX IX_BiometricData_TemplateHash ON BiometricData(TemplateHash) WHERE IsActive = 1;

PRINT '索引创建完成';

-- ============================================
-- 6. 数据验证
-- ============================================
PRINT 'Phase 1.6: 数据完整性验证...';

-- 验证姓名拆分是否正确
SELECT 
    Id, Name, FirstName, LastName,
    CASE 
        WHEN FirstName + ' ' + LastName = Name THEN 'OK'
        WHEN FirstName = Name AND LastName = '' THEN 'OK (Single Name)'
        ELSE 'MISMATCH'
    END AS ValidationStatus
FROM Staffs
WHERE Name IS NOT NULL;

-- 验证事件类型转换
SELECT EventType, COUNT(*) as Count
FROM Events 
GROUP BY EventType
ORDER BY EventType;

-- 验证设备状态
SELECT 
    Id, Name, Status, LastHeartbeat,
    CASE 
        WHEN LastHeartbeat IS NULL THEN 'No Heartbeat'
        WHEN DATEDIFF(MINUTE, LastHeartbeat, GETUTCDATE()) > 30 THEN 'Offline'
        ELSE 'Online'
    END AS ConnectionStatus
FROM Devices;

PRINT '数据验证完成';

-- ============================================
-- 7. 创建视图 (向后兼容)
-- ============================================
PRINT 'Phase 1.7: 创建兼容性视图...';

-- 员工信息兼容视图
CREATE VIEW v_StaffCompatible AS
SELECT 
    Id,
    Name,  -- 保留原字段
    FirstName + CASE WHEN LastName != '' THEN ' ' + LastName ELSE '' END AS FullName,
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
CREATE VIEW v_EventsCompatible AS
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

PRINT '兼容性视图创建完成';

-- ============================================
-- 8. 权限和约束
-- ============================================
PRINT 'Phase 1.8: 添加约束和权限...';

-- 合同类型约束
ALTER TABLE Staffs ADD CONSTRAINT CK_Staffs_ContractType 
    CHECK (ContractType IN ('fulltime', 'parttime', 'casual'));

-- 标准工时约束
ALTER TABLE Staffs ADD CONSTRAINT CK_Staffs_StandardHours 
    CHECK (StandardHoursPerWeek BETWEEN 1 AND 80);

-- 加班费率约束
ALTER TABLE Staffs ADD CONSTRAINT CK_Staffs_OvertimeRate 
    CHECK (OvertimePayRate >= HourlyRate);

-- 事件类型约束
ALTER TABLE Events ADD CONSTRAINT CK_Events_EventType 
    CHECK (EventType IN ('CLOCK_IN', 'CLOCK_OUT', 'BREAK_START', 'BREAK_END', 'MANUAL_OVERRIDE'));

PRINT '约束添加完成';

-- ============================================
-- 9. 统计信息更新
-- ============================================
PRINT 'Phase 1.9: 更新统计信息...';

-- 更新表统计信息以优化查询性能
UPDATE STATISTICS Staffs;
UPDATE STATISTICS Events;
UPDATE STATISTICS Devices;
UPDATE STATISTICS BiometricData;

PRINT '统计信息更新完成';

-- ============================================
-- 提交事务
-- ============================================
COMMIT TRANSACTION;

PRINT '===========================================';
PRINT 'Phase 1 迁移完成！';
PRINT '已完成:';
PRINT '- Staff 表字段扩展 (姓名分离、合同类型、薪资结构)';
PRINT '- Events 表类型标准化';
PRINT '- Devices 表监控增强';
PRINT '- BiometricData 表安全升级';
PRINT '- 性能索引优化';
PRINT '- 向后兼容性视图';
PRINT '===========================================';

-- ============================================
-- 回滚脚本 (紧急情况使用)
-- ============================================
/*
-- 紧急回滚脚本 (仅在出现严重问题时使用)
BEGIN TRANSACTION;

-- 删除新增约束
ALTER TABLE Staffs DROP CONSTRAINT CK_Staffs_ContractType;
ALTER TABLE Staffs DROP CONSTRAINT CK_Staffs_StandardHours;
ALTER TABLE Staffs DROP CONSTRAINT CK_Staffs_OvertimeRate;
ALTER TABLE Events DROP CONSTRAINT CK_Events_EventType;

-- 删除新增索引
DROP INDEX IX_Staffs_FirstName_LastName ON Staffs;
DROP INDEX IX_Staffs_ContractType_IsActive ON Staffs;
DROP INDEX IX_Devices_LastHeartbeat ON Devices;
DROP INDEX IX_BiometricData_TemplateHash ON BiometricData;

-- 删除视图
DROP VIEW v_StaffCompatible;
DROP VIEW v_EventsCompatible;

-- 恢复事件类型
UPDATE Events SET EventType = 'IN' WHERE EventType = 'CLOCK_IN';
UPDATE Events SET EventType = 'OUT' WHERE EventType = 'CLOCK_OUT';
UPDATE Events SET EventType = 'OTHER' WHERE EventType = 'MANUAL_OVERRIDE';

-- 删除新增字段 (WARNING: 数据将丢失!)
ALTER TABLE Staffs DROP COLUMN FirstName;
ALTER TABLE Staffs DROP COLUMN LastName;
ALTER TABLE Staffs DROP COLUMN ContractType;
ALTER TABLE Staffs DROP COLUMN StandardHoursPerWeek;
ALTER TABLE Staffs DROP COLUMN OvertimePayRate;
ALTER TABLE Events DROP COLUMN AdminId;
ALTER TABLE Devices DROP COLUMN LastHeartbeat;
ALTER TABLE Devices DROP COLUMN Firmware;
ALTER TABLE Devices DROP COLUMN ConfigData;
ALTER TABLE BiometricData DROP COLUMN Salt;
ALTER TABLE BiometricData DROP COLUMN TemplateHash;
ALTER TABLE BiometricData DROP COLUMN DeviceEnrollment;

COMMIT TRANSACTION;
PRINT 'Phase 1 回滚完成';
*/

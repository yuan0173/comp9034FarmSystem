-- ============================================
-- Phase 3: SQLite 数据库迁移脚本 - 高级安全特性
-- 目标: 实现 Tan 设计的生物识别验证系统
-- 执行时间: Phase 3
-- 风险级别: 中 (复杂业务逻辑，需要充分测试)
-- ============================================

BEGIN;

-- ============================================
-- 1. BiometricVerification 表创建 (Tan 设计核心)
-- ============================================

CREATE TABLE BiometricVerifications (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- 验证参与者
    StaffId INTEGER,  -- 可为 NULL (未识别人员)
    BiometricId INTEGER,  -- 用于匹配的生物识别模板
    DeviceId INTEGER NOT NULL,  -- 执行验证的设备
    EventId INTEGER,  -- 成功验证后创建的事件
    
    -- 验证结果
    VerificationResult TEXT NOT NULL,
    -- 'success', 'failed', 'no_match', 'poor_quality', 'timeout'
    
    ConfidenceScore DECIMAL(5,3),  -- 0.000 to 1.000
    FailureReason TEXT,
    
    -- 原始数据 (调试用，生产环境应加密)
    CapturedTemplate TEXT,  -- 加密存储
    ProcessingTime INTEGER,  -- 处理时间(毫秒)
    
    -- 安全审计
    IpAddress TEXT,
    UserAgent TEXT,
    
    -- 时间戳
    CreatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    
    -- 外键约束
    FOREIGN KEY (StaffId) REFERENCES Staffs(Id) ON DELETE SET NULL,
    FOREIGN KEY (BiometricId) REFERENCES BiometricData(Id) ON DELETE SET NULL,
    FOREIGN KEY (DeviceId) REFERENCES Devices(Id) ON DELETE CASCADE,
    FOREIGN KEY (EventId) REFERENCES Events(Id) ON DELETE SET NULL
);

-- 创建索引
CREATE INDEX IX_BiometricVerifications_DeviceId_CreatedAt 
    ON BiometricVerifications(DeviceId, CreatedAt);
CREATE INDEX IX_BiometricVerifications_StaffId_CreatedAt 
    ON BiometricVerifications(StaffId, CreatedAt);
CREATE INDEX IX_BiometricVerifications_Result_CreatedAt 
    ON BiometricVerifications(VerificationResult, CreatedAt);
CREATE INDEX IX_BiometricVerifications_Score 
    ON BiometricVerifications(ConfidenceScore) WHERE ConfidenceScore IS NOT NULL;

-- ============================================
-- 2. 生物识别管理视图
-- ============================================

-- 验证统计视图
CREATE VIEW v_BiometricVerificationStats AS
SELECT 
    s.Id AS StaffId,
    CASE 
        WHEN s.FirstName IS NOT NULL AND s.LastName IS NOT NULL 
        THEN s.FirstName || ' ' || s.LastName
        ELSE s.Name
    END AS StaffName,
    s.Role,
    COUNT(bv.Id) AS TotalVerifications,
    SUM(CASE WHEN bv.VerificationResult = 'success' THEN 1 ELSE 0 END) AS SuccessfulVerifications,
    SUM(CASE WHEN bv.VerificationResult != 'success' THEN 1 ELSE 0 END) AS FailedVerifications,
    ROUND(AVG(CASE WHEN bv.VerificationResult = 'success' THEN bv.ConfidenceScore END), 3) AS AvgConfidenceScore,
    MAX(bv.CreatedAt) AS LastVerification
FROM Staffs s
LEFT JOIN BiometricVerifications bv ON s.Id = bv.StaffId
WHERE s.IsActive = 1
GROUP BY s.Id, s.FirstName, s.LastName, s.Name, s.Role;

-- 设备性能视图
CREATE VIEW v_DevicePerformanceStats AS
SELECT 
    d.Id AS DeviceId,
    d.Name AS DeviceName,
    d.Type,
    d.Location,
    COUNT(bv.Id) AS TotalVerifications,
    SUM(CASE WHEN bv.VerificationResult = 'success' THEN 1 ELSE 0 END) AS SuccessfulVerifications,
    ROUND(AVG(CAST(bv.ProcessingTime AS REAL)), 1) AS AvgProcessingTime,
    COUNT(CASE WHEN bv.CreatedAt >= datetime('now', '-1 day') THEN 1 END) AS VerificationsLast24h,
    MAX(bv.CreatedAt) AS LastActivity
FROM Devices d
LEFT JOIN BiometricVerifications bv ON d.Id = bv.DeviceId
WHERE d.IsActive = 1
GROUP BY d.Id, d.Name, d.Type, d.Location;

-- ============================================
-- 3. 可疑活动检测视图
-- ============================================

CREATE VIEW v_SuspiciousActivityDetection AS
-- 异常时间段验证
SELECT 
    'OFF_HOURS' AS AlertType,
    StaffId,
    DeviceId,
    COUNT(*) AS EventCount,
    'Verification attempts outside normal hours (6AM-10PM)' AS Description,
    MAX(CreatedAt) AS LastOccurrence
FROM BiometricVerifications
WHERE CreatedAt >= datetime('now', '-24 hours')
    AND (CAST(strftime('%H', CreatedAt) AS INTEGER) < 6 OR CAST(strftime('%H', CreatedAt) AS INTEGER) > 22)
GROUP BY StaffId, DeviceId
HAVING COUNT(*) >= 3

UNION ALL

-- 连续失败验证
SELECT 
    'REPEATED_FAILURES' AS AlertType,
    StaffId,
    DeviceId,
    COUNT(*) AS EventCount,
    'Multiple consecutive verification failures',
    MAX(CreatedAt)
FROM BiometricVerifications
WHERE CreatedAt >= datetime('now', '-24 hours')
    AND VerificationResult IN ('failed', 'no_match')
GROUP BY StaffId, DeviceId
HAVING COUNT(*) >= 5

UNION ALL

-- 异常低置信度但通过验证
SELECT 
    'LOW_CONFIDENCE_SUCCESS' AS AlertType,
    StaffId,
    DeviceId,
    COUNT(*) AS EventCount,
    'Successful verifications with suspiciously low confidence (<0.800)',
    MAX(CreatedAt)
FROM BiometricVerifications
WHERE CreatedAt >= datetime('now', '-24 hours')
    AND VerificationResult = 'success'
    AND ConfidenceScore < 0.800
GROUP BY StaffId, DeviceId
HAVING COUNT(*) >= 2;

-- ============================================
-- 4. 生成测试数据
-- ============================================

-- 创建一些生物识别模板数据
INSERT INTO BiometricData (StaffId, BiometricType, TemplateData, Salt, TemplateHash, DeviceEnrollment, IsActive, CreatedAt, UpdatedAt)
SELECT 
    Id as StaffId,
    'fingerprint' as BiometricType,
    'TEMPLATE_' || Id || '_FINGERPRINT_DATA_' || hex(randomblob(16)) as TemplateData,
    hex(randomblob(16)) as Salt,
    hex(randomblob(32)) as TemplateHash,
    1 as DeviceEnrollment,
    1 as IsActive,
    datetime('now') as CreatedAt,
    datetime('now') as UpdatedAt
FROM Staffs 
WHERE IsActive = 1;

-- 生成一些模拟验证记录
-- 成功验证记录
INSERT INTO BiometricVerifications (
    StaffId, BiometricId, DeviceId, VerificationResult, 
    ConfidenceScore, ProcessingTime, IpAddress, UserAgent, CreatedAt
)
SELECT 
    s.Id,
    bd.Id,
    1 as DeviceId,  -- Main Terminal
    'success' as VerificationResult,
    ROUND(0.850 + (RANDOM() % 150) / 1000.0, 3) as ConfidenceScore,  -- 0.850-0.999
    500 + (RANDOM() % 1000) as ProcessingTime,  -- 500-1500ms
    '192.168.1.100' as IpAddress,
    'BiometricDevice/1.0' as UserAgent,
    datetime('now', '-' || (RANDOM() % 168) || ' hours') as CreatedAt  -- 过去一周随机时间
FROM Staffs s
JOIN BiometricData bd ON s.Id = bd.StaffId
WHERE s.IsActive = 1;

-- 失败验证记录
INSERT INTO BiometricVerifications (
    DeviceId, VerificationResult, ConfidenceScore, FailureReason,
    ProcessingTime, IpAddress, UserAgent, CreatedAt
)
SELECT 
    2 as DeviceId,  -- Biometric Scanner
    'no_match' as VerificationResult,
    ROUND(0.300 + (RANDOM() % 300) / 1000.0, 3) as ConfidenceScore,  -- 0.300-0.599
    'No matching template found' as FailureReason,
    800 + (RANDOM() % 500) as ProcessingTime,  -- 800-1300ms
    '192.168.1.101' as IpAddress,
    'BiometricDevice/1.0' as UserAgent,
    datetime('now', '-' || (RANDOM() % 72) || ' hours') as CreatedAt  -- 过去3天随机时间
FROM (SELECT 1 UNION SELECT 2 UNION SELECT 3);  -- 3条失败记录

-- 模拟基于成功验证自动创建的事件
INSERT INTO Events (StaffId, DeviceId, EventType, Timestamp, Notes, CreatedAt)
SELECT 
    bv.StaffId,
    bv.DeviceId,
    CASE 
        WHEN (bv.Id % 2) = 1 THEN 'CLOCK_IN'
        ELSE 'CLOCK_OUT'
    END as EventType,
    bv.CreatedAt as Timestamp,
    'Biometric verification ID: ' || bv.Id as Notes,
    bv.CreatedAt
FROM BiometricVerifications bv
WHERE bv.VerificationResult = 'success' AND bv.StaffId IS NOT NULL;

-- 更新验证记录中的事件关联
UPDATE BiometricVerifications 
SET EventId = (
    SELECT e.Id 
    FROM Events e 
    WHERE e.Notes LIKE '%Biometric verification ID: ' || BiometricVerifications.Id || '%'
    LIMIT 1
)
WHERE VerificationResult = 'success' AND StaffId IS NOT NULL;

COMMIT;

-- ============================================
-- 验证查询
-- ============================================

-- 测试数据完整性
SELECT 'BiometricData Count' AS Test, COUNT(*) AS Result FROM BiometricData;
SELECT 'BiometricVerification Count' AS Test, COUNT(*) AS Result FROM BiometricVerifications;
SELECT 'Events Created by Biometric' AS Test, COUNT(*) AS Result FROM Events WHERE Notes LIKE '%Biometric verification ID:%';

-- 测试验证统计视图
SELECT 'Biometric Verification Stats' AS Test;
SELECT StaffId, StaffName, TotalVerifications, SuccessfulVerifications, FailedVerifications, AvgConfidenceScore 
FROM v_BiometricVerificationStats 
WHERE TotalVerifications > 0
ORDER BY TotalVerifications DESC;

-- 测试设备性能视图
SELECT 'Device Performance Stats' AS Test;
SELECT DeviceId, DeviceName, TotalVerifications, SuccessfulVerifications, AvgProcessingTime, VerificationsLast24h 
FROM v_DevicePerformanceStats 
WHERE TotalVerifications > 0;

-- 测试可疑活动检测
SELECT 'Suspicious Activity Detection' AS Test;
SELECT AlertType, COUNT(*) as AlertCount 
FROM v_SuspiciousActivityDetection 
GROUP BY AlertType;

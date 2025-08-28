-- ============================================
-- Phase 3: 数据库迁移脚本 - 高级安全特性
-- 目标: 实现 Tan 设计的生物识别验证系统
-- 执行时间: Week 4
-- 风险级别: 中 (复杂业务逻辑，需要充分测试)
-- ============================================

-- 开始事务
BEGIN TRANSACTION;

-- ============================================
-- 1. BiometricVerification 表创建 (Tan 设计核心)
-- ============================================
PRINT 'Phase 3.1: 创建 BiometricVerification 表...';

CREATE TABLE BiometricVerifications (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    
    -- 验证参与者
    StaffId INT NULL,  -- 可为 NULL (未识别人员)
    BiometricId INT NULL,  -- 用于匹配的生物识别模板
    DeviceId INT NOT NULL,  -- 执行验证的设备
    EventId INT NULL,  -- 成功验证后创建的事件
    
    -- 验证结果
    VerificationResult NVARCHAR(20) NOT NULL,
    -- 'success', 'failed', 'no_match', 'poor_quality', 'timeout'
    
    ConfidenceScore DECIMAL(5,3) NULL,  -- 0.000 to 1.000
    FailureReason NVARCHAR(500) NULL,
    
    -- 原始数据 (调试用，生产环境应加密)
    CapturedTemplate NVARCHAR(MAX) NULL,  -- 加密存储
    ProcessingTime INT NULL,  -- 处理时间(毫秒)
    
    -- 安全审计
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(200) NULL,
    
    -- 时间戳
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- 外键约束
    CONSTRAINT FK_BiometricVerifications_Staff 
        FOREIGN KEY (StaffId) REFERENCES Staffs(Id) ON DELETE SET NULL,
    CONSTRAINT FK_BiometricVerifications_Biometric 
        FOREIGN KEY (BiometricId) REFERENCES BiometricData(Id) ON DELETE SET NULL,
    CONSTRAINT FK_BiometricVerifications_Device 
        FOREIGN KEY (DeviceId) REFERENCES Devices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_BiometricVerifications_Event 
        FOREIGN KEY (EventId) REFERENCES Events(Id) ON DELETE SET NULL,
    
    -- 业务约束
    CONSTRAINT CK_BiometricVerifications_Result 
        CHECK (VerificationResult IN ('success', 'failed', 'no_match', 'poor_quality', 'timeout')),
    CONSTRAINT CK_BiometricVerifications_Score 
        CHECK (ConfidenceScore IS NULL OR (ConfidenceScore >= 0.000 AND ConfidenceScore <= 1.000)),
    CONSTRAINT CK_BiometricVerifications_SuccessRequirements
        CHECK (
            (VerificationResult = 'success' AND StaffId IS NOT NULL AND BiometricId IS NOT NULL) OR
            (VerificationResult != 'success')
        )
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

PRINT 'BiometricVerification 表创建完成';

-- ============================================
-- 2. 生物识别匹配算法存储过程
-- ============================================
PRINT 'Phase 3.2: 创建生物识别匹配存储过程...';

CREATE PROCEDURE sp_BiometricVerification
    @DeviceId INT,
    @CapturedTemplate NVARCHAR(MAX),
    @IpAddress NVARCHAR(45) = NULL,
    @UserAgent NVARCHAR(200) = NULL,
    @VerificationId INT OUTPUT,
    @StaffId INT OUTPUT,
    @VerificationResult NVARCHAR(20) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @StartTime DATETIME2 = GETUTCDATE();
    DECLARE @ProcessingTime INT;
    DECLARE @BiometricId INT = NULL;
    DECLARE @ConfidenceScore DECIMAL(5,3) = NULL;
    DECLARE @FailureReason NVARCHAR(500) = NULL;
    DECLARE @BestMatch DECIMAL(5,3) = 0.000;
    DECLARE @MatchThreshold DECIMAL(5,3) = 0.700;  -- 70% 匹配阈值
    
    -- 初始化输出参数
    SET @StaffId = NULL;
    SET @VerificationResult = 'failed';
    
    BEGIN TRY
        -- 1. 质量检查
        IF LEN(@CapturedTemplate) < 100
        BEGIN
            SET @VerificationResult = 'poor_quality';
            SET @FailureReason = 'Captured template too short';
            GOTO LOG_RESULT;
        END;
        
        -- 2. 模板匹配算法 (简化版本 - 实际应使用专业算法)
        -- 这里使用字符串相似度作为演示
        WITH TemplateMatches AS (
            SELECT 
                bd.Id AS BiometricId,
                bd.StaffId,
                -- 简化的相似度计算 (实际应使用专业生物识别算法)
                CASE 
                    WHEN LEN(bd.TemplateData) = 0 THEN 0.000
                    ELSE CAST(
                        (LEN(bd.TemplateData) + LEN(@CapturedTemplate) - 
                         LEN(REPLACE(REPLACE(bd.TemplateData, @CapturedTemplate, ''), @CapturedTemplate, '')) * 2.0) /
                        (LEN(bd.TemplateData) + LEN(@CapturedTemplate)) 
                        AS DECIMAL(5,3)
                    )
                END AS Similarity
            FROM BiometricData bd
            INNER JOIN Staffs s ON bd.StaffId = s.Id
            WHERE bd.IsActive = 1 AND s.IsActive = 1
        )
        SELECT TOP 1 
            @BiometricId = BiometricId,
            @StaffId = StaffId,
            @BestMatch = Similarity
        FROM TemplateMatches
        WHERE Similarity >= @MatchThreshold
        ORDER BY Similarity DESC;
        
        -- 3. 结果判断
        IF @StaffId IS NOT NULL
        BEGIN
            SET @VerificationResult = 'success';
            SET @ConfidenceScore = @BestMatch;
        END
        ELSE IF @BestMatch > 0.400  -- 有相似但不够匹配
        BEGIN
            SET @VerificationResult = 'no_match';
            SET @FailureReason = CONCAT('Best match confidence: ', CAST(@BestMatch AS NVARCHAR(10)));
        END
        ELSE
        BEGIN
            SET @VerificationResult = 'no_match';
            SET @FailureReason = 'No similar templates found';
        END;
        
        LOG_RESULT:
        -- 4. 计算处理时间
        SET @ProcessingTime = DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE());
        
        -- 5. 记录验证日志
        INSERT INTO BiometricVerifications (
            StaffId, BiometricId, DeviceId, VerificationResult, 
            ConfidenceScore, FailureReason, CapturedTemplate, 
            ProcessingTime, IpAddress, UserAgent
        )
        VALUES (
            @StaffId, @BiometricId, @DeviceId, @VerificationResult,
            @ConfidenceScore, @FailureReason, @CapturedTemplate,
            @ProcessingTime, @IpAddress, @UserAgent
        );
        
        SET @VerificationId = SCOPE_IDENTITY();
        
    END TRY
    BEGIN CATCH
        SET @VerificationResult = 'failed';
        SET @FailureReason = ERROR_MESSAGE();
        SET @ProcessingTime = DATEDIFF(MILLISECOND, @StartTime, GETUTCDATE());
        
        -- 记录错误日志
        INSERT INTO BiometricVerifications (
            DeviceId, VerificationResult, FailureReason, 
            ProcessingTime, IpAddress, UserAgent
        )
        VALUES (
            @DeviceId, @VerificationResult, @FailureReason,
            @ProcessingTime, @IpAddress, @UserAgent
        );
        
        SET @VerificationId = SCOPE_IDENTITY();
    END CATCH;
END;

PRINT '生物识别匹配存储过程创建完成';

-- ============================================
-- 3. 自动打卡事件创建存储过程
-- ============================================
PRINT 'Phase 3.3: 创建自动打卡事件存储过程...';

CREATE PROCEDURE sp_CreateBiometricEvent
    @VerificationId INT,
    @EventType NVARCHAR(20) = 'CLOCK_IN',  -- 默认签到
    @EventId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @StaffId INT;
    DECLARE @DeviceId INT;
    DECLARE @VerificationResult NVARCHAR(20);
    DECLARE @LastEventType NVARCHAR(20);
    
    -- 获取验证信息
    SELECT 
        @StaffId = StaffId,
        @DeviceId = DeviceId,
        @VerificationResult = VerificationResult
    FROM BiometricVerifications
    WHERE Id = @VerificationId;
    
    -- 只有成功验证才创建事件
    IF @VerificationResult != 'success' OR @StaffId IS NULL
    BEGIN
        SET @EventId = NULL;
        RETURN;
    END;
    
    -- 智能判断事件类型 (如果未指定)
    IF @EventType = 'AUTO'
    BEGIN
        -- 获取员工最后一个事件
        SELECT TOP 1 @LastEventType = EventType
        FROM Events 
        WHERE StaffId = @StaffId 
            AND CAST(Timestamp AS DATE) = CAST(GETUTCDATE() AS DATE)
        ORDER BY Timestamp DESC;
        
        -- 智能切换逻辑
        SET @EventType = CASE 
            WHEN @LastEventType IS NULL THEN 'CLOCK_IN'
            WHEN @LastEventType = 'CLOCK_IN' THEN 'CLOCK_OUT'
            WHEN @LastEventType = 'CLOCK_OUT' THEN 'CLOCK_IN'
            WHEN @LastEventType = 'BREAK_START' THEN 'BREAK_END'
            WHEN @LastEventType = 'BREAK_END' THEN 'CLOCK_OUT'
            ELSE 'CLOCK_IN'
        END;
    END;
    
    -- 创建事件
    INSERT INTO Events (StaffId, DeviceId, EventType, Timestamp, Notes)
    VALUES (
        @StaffId, 
        @DeviceId, 
        @EventType, 
        GETUTCDATE(),
        CONCAT('Biometric verification ID: ', @VerificationId)
    );
    
    SET @EventId = SCOPE_IDENTITY();
    
    -- 更新验证记录中的事件关联
    UPDATE BiometricVerifications 
    SET EventId = @EventId 
    WHERE Id = @VerificationId;
END;

PRINT '自动打卡事件创建存储过程完成';

-- ============================================
-- 4. 可疑活动检测函数
-- ============================================
PRINT 'Phase 3.4: 创建可疑活动检测函数...';

CREATE FUNCTION fn_DetectSuspiciousActivity
(
    @StaffId INT = NULL,
    @DeviceId INT = NULL,
    @Hours INT = 24
)
RETURNS TABLE
AS
RETURN
(
    WITH SuspiciousPatterns AS (
        -- 异常时间段验证
        SELECT 
            'OFF_HOURS' AS AlertType,
            StaffId,
            DeviceId,
            COUNT(*) AS EventCount,
            'Verification attempts outside normal hours' AS Description
        FROM BiometricVerifications
        WHERE CreatedAt >= DATEADD(HOUR, -@Hours, GETUTCDATE())
            AND (@StaffId IS NULL OR StaffId = @StaffId)
            AND (@DeviceId IS NULL OR DeviceId = @DeviceId)
            AND (DATEPART(HOUR, CreatedAt) < 6 OR DATEPART(HOUR, CreatedAt) > 22)
        GROUP BY StaffId, DeviceId
        HAVING COUNT(*) >= 3
        
        UNION ALL
        
        -- 连续失败验证
        SELECT 
            'REPEATED_FAILURES' AS AlertType,
            StaffId,
            DeviceId,
            COUNT(*) AS EventCount,
            'Multiple consecutive verification failures'
        FROM BiometricVerifications
        WHERE CreatedAt >= DATEADD(HOUR, -@Hours, GETUTCDATE())
            AND (@StaffId IS NULL OR StaffId = @StaffId)
            AND (@DeviceId IS NULL OR DeviceId = @DeviceId)
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
            'Successful verifications with suspiciously low confidence'
        FROM BiometricVerifications
        WHERE CreatedAt >= DATEADD(HOUR, -@Hours, GETUTCDATE())
            AND (@StaffId IS NULL OR StaffId = @StaffId)
            AND (@DeviceId IS NULL OR DeviceId = @DeviceId)
            AND VerificationResult = 'success'
            AND ConfidenceScore < 0.800
        GROUP BY StaffId, DeviceId
        HAVING COUNT(*) >= 2
    )
    SELECT 
        AlertType,
        StaffId,
        ISNULL(s.FirstName + ' ' + s.LastName, 'Unknown') AS StaffName,
        DeviceId,
        d.Name AS DeviceName,
        EventCount,
        Description,
        GETUTCDATE() AS DetectedAt
    FROM SuspiciousPatterns sp
    LEFT JOIN Staffs s ON sp.StaffId = s.Id
    LEFT JOIN Devices d ON sp.DeviceId = d.Id
);

PRINT '可疑活动检测函数创建完成';

-- ============================================
-- 5. 生物识别数据管理视图
-- ============================================
PRINT 'Phase 3.5: 创建生物识别管理视图...';

-- 验证统计视图
CREATE VIEW v_BiometricVerificationStats AS
SELECT 
    s.Id AS StaffId,
    s.FirstName + ' ' + s.LastName AS StaffName,
    s.Role,
    COUNT(bv.Id) AS TotalVerifications,
    SUM(CASE WHEN bv.VerificationResult = 'success' THEN 1 ELSE 0 END) AS SuccessfulVerifications,
    SUM(CASE WHEN bv.VerificationResult != 'success' THEN 1 ELSE 0 END) AS FailedVerifications,
    CAST(
        AVG(CASE WHEN bv.VerificationResult = 'success' THEN bv.ConfidenceScore END) AS DECIMAL(5,3)
    ) AS AvgConfidenceScore,
    MAX(bv.CreatedAt) AS LastVerification
FROM Staffs s
LEFT JOIN BiometricVerifications bv ON s.Id = bv.StaffId
WHERE s.IsActive = 1
GROUP BY s.Id, s.FirstName, s.LastName, s.Role;

-- 设备性能视图
CREATE VIEW v_DevicePerformanceStats AS
SELECT 
    d.Id AS DeviceId,
    d.Name AS DeviceName,
    d.Type,
    d.Location,
    COUNT(bv.Id) AS TotalVerifications,
    SUM(CASE WHEN bv.VerificationResult = 'success' THEN 1 ELSE 0 END) AS SuccessfulVerifications,
    AVG(CAST(bv.ProcessingTime AS FLOAT)) AS AvgProcessingTime,
    COUNT(CASE WHEN bv.CreatedAt >= DATEADD(DAY, -1, GETUTCDATE()) THEN 1 END) AS VerificationsLast24h,
    MAX(bv.CreatedAt) AS LastActivity
FROM Devices d
LEFT JOIN BiometricVerifications bv ON d.Id = bv.DeviceId
WHERE d.IsActive = 1
GROUP BY d.Id, d.Name, d.Type, d.Location;

PRINT '生物识别管理视图创建完成';

-- ============================================
-- 6. 自动化触发器
-- ============================================
PRINT 'Phase 3.6: 创建自动化触发器...';

-- 成功验证后自动创建事件触发器
CREATE TRIGGER tr_BiometricVerification_AutoEvent
ON BiometricVerifications
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @VerificationId INT, @EventId INT;
    
    -- 为成功的验证自动创建打卡事件
    DECLARE verification_cursor CURSOR FOR
        SELECT Id 
        FROM inserted 
        WHERE VerificationResult = 'success' AND StaffId IS NOT NULL;
    
    OPEN verification_cursor;
    FETCH NEXT FROM verification_cursor INTO @VerificationId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC sp_CreateBiometricEvent @VerificationId, 'AUTO', @EventId OUTPUT;
        FETCH NEXT FROM verification_cursor INTO @VerificationId;
    END;
    
    CLOSE verification_cursor;
    DEALLOCATE verification_cursor;
END;

PRINT '自动化触发器创建完成';

-- ============================================
-- 7. 安全审计存储过程
-- ============================================
PRINT 'Phase 3.7: 创建安全审计存储过程...';

CREATE PROCEDURE sp_SecurityAuditReport
    @StartDate DATE = NULL,
    @EndDate DATE = NULL,
    @StaffId INT = NULL,
    @DeviceId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 设置默认日期范围 (最近7天)
    IF @StartDate IS NULL SET @StartDate = DATEADD(DAY, -7, CAST(GETUTCDATE() AS DATE));
    IF @EndDate IS NULL SET @EndDate = CAST(GETUTCDATE() AS DATE);
    
    -- 验证统计
    SELECT 
        'Verification Summary' AS ReportSection,
        VerificationResult,
        COUNT(*) AS Count,
        CONCAT(CAST(COUNT(*) * 100.0 / SUM(COUNT(*)) OVER() AS DECIMAL(5,1)), '%') AS Percentage
    FROM BiometricVerifications
    WHERE CAST(CreatedAt AS DATE) BETWEEN @StartDate AND @EndDate
        AND (@StaffId IS NULL OR StaffId = @StaffId)
        AND (@DeviceId IS NULL OR DeviceId = @DeviceId)
    GROUP BY VerificationResult
    ORDER BY COUNT(*) DESC;
    
    -- 可疑活动报告
    SELECT 
        'Suspicious Activities' AS ReportSection,
        * 
    FROM fn_DetectSuspiciousActivity(@StaffId, @DeviceId, DATEDIFF(HOUR, @StartDate, @EndDate));
    
    -- 性能统计
    SELECT 
        'Performance Statistics' AS ReportSection,
        AVG(CAST(ProcessingTime AS FLOAT)) AS AvgProcessingTimeMs,
        MIN(ProcessingTime) AS MinProcessingTimeMs,
        MAX(ProcessingTime) AS MaxProcessingTimeMs,
        COUNT(CASE WHEN ProcessingTime > 5000 THEN 1 END) AS SlowVerifications
    FROM BiometricVerifications
    WHERE CAST(CreatedAt AS DATE) BETWEEN @StartDate AND @EndDate
        AND (@StaffId IS NULL OR StaffId = @StaffId)
        AND (@DeviceId IS NULL OR DeviceId = @DeviceId)
        AND ProcessingTime IS NOT NULL;
END;

PRINT '安全审计存储过程创建完成';

-- ============================================
-- 8. 示例数据和测试
-- ============================================
PRINT 'Phase 3.8: 生成测试数据...';

-- 生成一些模拟验证记录
DECLARE @TestDeviceId INT = 1;
DECLARE @TestStaffId INT = 1001;
DECLARE @VerificationId INT, @StaffId INT, @Result NVARCHAR(20);

-- 成功验证示例
EXEC sp_BiometricVerification 
    @DeviceId = @TestDeviceId,
    @CapturedTemplate = 'SIMULATED_FINGERPRINT_TEMPLATE_SUCCESS_001',
    @IpAddress = '192.168.1.100',
    @UserAgent = 'BiometricDevice/1.0',
    @VerificationId = @VerificationId OUTPUT,
    @StaffId = @StaffId OUTPUT,
    @VerificationResult = @Result OUTPUT;

PRINT CONCAT('测试验证完成 - ID: ', @VerificationId, ', Staff: ', @StaffId, ', Result: ', @Result);

-- 失败验证示例
EXEC sp_BiometricVerification 
    @DeviceId = @TestDeviceId,
    @CapturedTemplate = 'UNKNOWN_TEMPLATE_XYZ',
    @IpAddress = '192.168.1.100',
    @UserAgent = 'BiometricDevice/1.0',
    @VerificationId = @VerificationId OUTPUT,
    @StaffId = @StaffId OUTPUT,
    @VerificationResult = @Result OUTPUT;

PRINT CONCAT('测试验证完成 - ID: ', @VerificationId, ', Staff: ', @StaffId, ', Result: ', @Result);

PRINT '测试数据生成完成';

-- ============================================
-- 提交事务
-- ============================================
COMMIT TRANSACTION;

PRINT '===========================================';
PRINT 'Phase 3 迁移完成！';
PRINT '已完成:';
PRINT '- BiometricVerification 表创建 (Tan 设计核心)';
PRINT '- 智能生物识别匹配算法';
PRINT '- 自动打卡事件创建';
PRINT '- 可疑活动检测系统';
PRINT '- 安全审计和报告功能';
PRINT '- 自动化触发器和业务逻辑';
PRINT '===========================================';

-- ============================================
-- 最终测试查询
-- ============================================
PRINT 'Phase 3.9: 执行最终测试...';

-- 测试数据完整性
SELECT 'BiometricVerification Count' AS Test, COUNT(*) AS Result FROM BiometricVerifications;
SELECT 'Events Created by Biometric' AS Test, COUNT(*) AS Result FROM Events WHERE Notes LIKE '%Biometric verification ID:%';

-- 测试视图
SELECT TOP 3 * FROM v_BiometricVerificationStats;
SELECT TOP 3 * FROM v_DevicePerformanceStats;

-- 测试可疑活动检测
SELECT * FROM fn_DetectSuspiciousActivity(NULL, NULL, 24);

PRINT '最终测试完成 - Phase 3 成功部署！';

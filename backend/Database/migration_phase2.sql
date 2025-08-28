-- ============================================
-- Phase 2: 数据库迁移脚本 - 新业务表创建
-- 目标: 添加 Tan 设计的核心业务表
-- 执行时间: Week 2-3
-- 风险级别: 中 (新表创建，不影响现有功能)
-- ============================================

-- 开始事务
BEGIN TRANSACTION;

-- ============================================
-- 1. WorkSchedule 表创建 (排班系统)
-- ============================================
PRINT 'Phase 2.1: 创建 WorkSchedule 表...';

CREATE TABLE WorkSchedules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    
    -- 基本信息
    StaffId INT NOT NULL,
    Date DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    ScheduleHours DECIMAL(4,2) NOT NULL,
    
    -- 状态管理
    Status NVARCHAR(20) NOT NULL DEFAULT 'scheduled',
    -- 'scheduled', 'completed', 'absent', 'cancelled'
    
    -- 备注信息
    Notes NVARCHAR(500) NULL,
    CreatedBy INT NULL,  -- 创建排班的管理员
    
    -- 审计字段
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    -- 外键约束
    CONSTRAINT FK_WorkSchedules_Staff FOREIGN KEY (StaffId) REFERENCES Staffs(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WorkSchedules_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Staffs(Id) ON DELETE SET NULL,
    
    -- 业务约束
    CONSTRAINT CK_WorkSchedules_Times CHECK (EndTime > StartTime),
    CONSTRAINT CK_WorkSchedules_Hours CHECK (ScheduleHours BETWEEN 0.5 AND 24),
    CONSTRAINT CK_WorkSchedules_Status CHECK (Status IN ('scheduled', 'completed', 'absent', 'cancelled')),
    
    -- 唯一约束 (一个员工在同一天只能有一个排班)
    CONSTRAINT UQ_WorkSchedules_Staff_Date UNIQUE (StaffId, Date)
);

-- 创建索引
CREATE INDEX IX_WorkSchedules_Date ON WorkSchedules(Date);
CREATE INDEX IX_WorkSchedules_StaffId_Date ON WorkSchedules(StaffId, Date);
CREATE INDEX IX_WorkSchedules_Status_Date ON WorkSchedules(Status, Date);

PRINT 'WorkSchedule 表创建完成';

-- ============================================
-- 2. Salary 表创建 (薪资系统)
-- ============================================
PRINT 'Phase 2.2: 创建 Salary 表...';

CREATE TABLE Salaries (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    
    -- 基本信息
    StaffId INT NOT NULL,
    PayPeriodStart DATE NOT NULL,
    PayPeriodEnd DATE NOT NULL,
    
    -- 工时统计
    TotalHours DECIMAL(8,2) NOT NULL DEFAULT 0,
    TotalOvertimeHours DECIMAL(8,2) NOT NULL DEFAULT 0,
    ScheduledHours DECIMAL(8,2) NULL,  -- 计划工时
    
    -- 薪资计算
    RegularPay DECIMAL(10,2) NOT NULL DEFAULT 0,
    OvertimePay DECIMAL(10,2) NOT NULL DEFAULT 0,
    GrossPay DECIMAL(10,2) NOT NULL DEFAULT 0,
    
    -- 扣除项 (可扩展)
    Deductions DECIMAL(10,2) NOT NULL DEFAULT 0,
    NetPay DECIMAL(10,2) NOT NULL DEFAULT 0,
    
    -- 状态管理
    Status NVARCHAR(20) NOT NULL DEFAULT 'draft',
    -- 'draft', 'calculated', 'approved', 'paid'
    
    -- 审计信息
    CalculatedBy INT NULL,  -- 计算薪资的管理员
    ApprovedBy INT NULL,    -- 审批薪资的管理员
    GeneratedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ApprovedAt DATETIME2 NULL,
    
    -- 外键约束
    CONSTRAINT FK_Salaries_Staff FOREIGN KEY (StaffId) REFERENCES Staffs(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Salaries_CalculatedBy FOREIGN KEY (CalculatedBy) REFERENCES Staffs(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Salaries_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Staffs(Id) ON DELETE SET NULL,
    
    -- 业务约束
    CONSTRAINT CK_Salaries_Period CHECK (PayPeriodEnd >= PayPeriodStart),
    CONSTRAINT CK_Salaries_Hours CHECK (TotalHours >= 0 AND TotalOvertimeHours >= 0),
    CONSTRAINT CK_Salaries_Pay CHECK (GrossPay >= 0 AND NetPay >= 0),
    CONSTRAINT CK_Salaries_Status CHECK (Status IN ('draft', 'calculated', 'approved', 'paid')),
    
    -- 唯一约束 (一个员工在同一薪资周期只能有一条记录)
    CONSTRAINT UQ_Salaries_Staff_Period UNIQUE (StaffId, PayPeriodStart, PayPeriodEnd)
);

-- 创建索引
CREATE INDEX IX_Salaries_PayPeriod ON Salaries(PayPeriodStart, PayPeriodEnd);
CREATE INDEX IX_Salaries_StaffId_Period ON Salaries(StaffId, PayPeriodStart);
CREATE INDEX IX_Salaries_Status_Generated ON Salaries(Status, GeneratedAt);

PRINT 'Salary 表创建完成';

-- ============================================
-- 3. 为现有数据生成示例排班
-- ============================================
PRINT 'Phase 2.3: 生成示例排班数据...';

-- 为活跃员工生成未来一周的排班
WITH ActiveStaff AS (
    SELECT Id, Role FROM Staffs WHERE IsActive = 1
),
DateRange AS (
    SELECT DATEADD(DAY, number, CAST(GETDATE() AS DATE)) AS WorkDate
    FROM master.dbo.spt_values
    WHERE type = 'P' AND number BETWEEN 1 AND 7  -- 未来一周
)
INSERT INTO WorkSchedules (StaffId, Date, StartTime, EndTime, ScheduleHours, Status, CreatedBy)
SELECT 
    s.Id,
    d.WorkDate,
    CASE 
        WHEN s.Role = 'admin' THEN '09:00'
        WHEN s.Role = 'manager' THEN '08:00'
        ELSE '07:00'
    END AS StartTime,
    CASE 
        WHEN s.Role = 'admin' THEN '17:00'
        WHEN s.Role = 'manager' THEN '17:00'
        ELSE '15:00'
    END AS EndTime,
    CASE 
        WHEN s.Role = 'admin' THEN 8.0
        WHEN s.Role = 'manager' THEN 9.0
        ELSE 8.0
    END AS ScheduleHours,
    'scheduled',
    9001  -- 系统管理员创建
FROM ActiveStaff s
CROSS JOIN DateRange d
WHERE DATEPART(WEEKDAY, d.WorkDate) NOT IN (1, 7);  -- 排除周末

PRINT '示例排班数据生成完成';

-- ============================================
-- 4. 自动薪资计算存储过程
-- ============================================
PRINT 'Phase 2.4: 创建薪资计算存储过程...';

CREATE PROCEDURE sp_CalculateSalary
    @StaffId INT,
    @PayPeriodStart DATE,
    @PayPeriodEnd DATE,
    @CalculatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalHours DECIMAL(8,2) = 0;
    DECLARE @OvertimeHours DECIMAL(8,2) = 0;
    DECLARE @ScheduledHours DECIMAL(8,2) = 0;
    DECLARE @HourlyRate DECIMAL(10,2);
    DECLARE @OvertimeRate DECIMAL(10,2);
    DECLARE @StandardHours INT;
    
    -- 获取员工薪资信息
    SELECT 
        @HourlyRate = HourlyRate,
        @OvertimeRate = ISNULL(OvertimePayRate, HourlyRate * 1.5),
        @StandardHours = ISNULL(StandardHoursPerWeek, 40)
    FROM Staffs 
    WHERE Id = @StaffId;
    
    -- 计算实际工时 (基于 Events 表)
    WITH EventPairs AS (
        SELECT 
            StaffId,
            CAST(Timestamp AS DATE) AS WorkDate,
            SUM(CASE WHEN EventType = 'CLOCK_IN' THEN 1 ELSE -1 END) AS Balance,
            MIN(CASE WHEN EventType = 'CLOCK_IN' THEN Timestamp END) AS ClockIn,
            MAX(CASE WHEN EventType = 'CLOCK_OUT' THEN Timestamp END) AS ClockOut
        FROM Events 
        WHERE StaffId = @StaffId 
            AND CAST(Timestamp AS DATE) BETWEEN @PayPeriodStart AND @PayPeriodEnd
            AND EventType IN ('CLOCK_IN', 'CLOCK_OUT')
        GROUP BY StaffId, CAST(Timestamp AS DATE)
        HAVING SUM(CASE WHEN EventType = 'CLOCK_IN' THEN 1 ELSE -1 END) = 0  -- 配对成功
    )
    SELECT @TotalHours = SUM(DATEDIFF(MINUTE, ClockIn, ClockOut) / 60.0)
    FROM EventPairs;
    
    -- 计算计划工时
    SELECT @ScheduledHours = SUM(ScheduleHours)
    FROM WorkSchedules
    WHERE StaffId = @StaffId 
        AND Date BETWEEN @PayPeriodStart AND @PayPeriodEnd;
    
    -- 计算加班时间 (超过标准工时的部分)
    SET @OvertimeHours = CASE WHEN @TotalHours > @StandardHours THEN @TotalHours - @StandardHours ELSE 0 END;
    
    -- 插入或更新薪资记录
    MERGE Salaries AS target
    USING (SELECT @StaffId AS StaffId, @PayPeriodStart AS PeriodStart, @PayPeriodEnd AS PeriodEnd) AS source
    ON target.StaffId = source.StaffId 
        AND target.PayPeriodStart = source.PeriodStart 
        AND target.PayPeriodEnd = source.PeriodEnd
    WHEN MATCHED THEN
        UPDATE SET
            TotalHours = @TotalHours,
            TotalOvertimeHours = @OvertimeHours,
            ScheduledHours = @ScheduledHours,
            RegularPay = (@TotalHours - @OvertimeHours) * @HourlyRate,
            OvertimePay = @OvertimeHours * @OvertimeRate,
            GrossPay = ((@TotalHours - @OvertimeHours) * @HourlyRate) + (@OvertimeHours * @OvertimeRate),
            NetPay = ((@TotalHours - @OvertimeHours) * @HourlyRate) + (@OvertimeHours * @OvertimeRate) - Deductions,
            Status = 'calculated',
            CalculatedBy = @CalculatedBy,
            GeneratedAt = GETUTCDATE()
    WHEN NOT MATCHED THEN
        INSERT (StaffId, PayPeriodStart, PayPeriodEnd, TotalHours, TotalOvertimeHours, ScheduledHours,
                RegularPay, OvertimePay, GrossPay, NetPay, Status, CalculatedBy)
        VALUES (@StaffId, @PayPeriodStart, @PayPeriodEnd, @TotalHours, @OvertimeHours, @ScheduledHours,
                (@TotalHours - @OvertimeHours) * @HourlyRate, @OvertimeHours * @OvertimeRate,
                ((@TotalHours - @OvertimeHours) * @HourlyRate) + (@OvertimeHours * @OvertimeRate),
                ((@TotalHours - @OvertimeHours) * @HourlyRate) + (@OvertimeHours * @OvertimeRate),
                'calculated', @CalculatedBy);
END;

PRINT '薪资计算存储过程创建完成';

-- ============================================
-- 5. 排班冲突检查函数
-- ============================================
PRINT 'Phase 2.5: 创建排班冲突检查函数...';

CREATE FUNCTION fn_CheckScheduleConflict
(
    @StaffId INT,
    @Date DATE,
    @StartTime TIME,
    @EndTime TIME,
    @ExcludeScheduleId INT = NULL
)
RETURNS BIT
AS
BEGIN
    DECLARE @HasConflict BIT = 0;
    
    IF EXISTS (
        SELECT 1 FROM WorkSchedules
        WHERE StaffId = @StaffId 
            AND Date = @Date
            AND Status IN ('scheduled', 'completed')
            AND (@ExcludeScheduleId IS NULL OR Id != @ExcludeScheduleId)
            AND (
                (@StartTime >= StartTime AND @StartTime < EndTime) OR
                (@EndTime > StartTime AND @EndTime <= EndTime) OR
                (@StartTime <= StartTime AND @EndTime >= EndTime)
            )
    )
    SET @HasConflict = 1;
    
    RETURN @HasConflict;
END;

PRINT '排班冲突检查函数创建完成';

-- ============================================
-- 6. 业务视图创建
-- ============================================
PRINT 'Phase 2.6: 创建业务视图...';

-- 员工排班概览视图
CREATE VIEW v_StaffScheduleOverview AS
SELECT 
    s.Id AS StaffId,
    s.FirstName + ' ' + s.LastName AS FullName,
    s.Role,
    ws.Date,
    ws.StartTime,
    ws.EndTime,
    ws.ScheduleHours,
    ws.Status,
    CASE 
        WHEN ws.Status = 'completed' THEN 'On Time'
        WHEN ws.Status = 'absent' THEN 'Absent'
        WHEN ws.Date < CAST(GETDATE() AS DATE) AND ws.Status = 'scheduled' THEN 'Overdue'
        ELSE 'Pending'
    END AS ScheduleStatus
FROM Staffs s
LEFT JOIN WorkSchedules ws ON s.Id = ws.StaffId
WHERE s.IsActive = 1;

-- 薪资汇总视图
CREATE VIEW v_SalarySummary AS
SELECT 
    s.Id AS StaffId,
    s.FirstName + ' ' + s.LastName AS FullName,
    s.Role,
    s.HourlyRate,
    sal.PayPeriodStart,
    sal.PayPeriodEnd,
    sal.TotalHours,
    sal.TotalOvertimeHours,
    sal.GrossPay,
    sal.NetPay,
    sal.Status,
    sal.GeneratedAt
FROM Staffs s
LEFT JOIN Salaries sal ON s.Id = sal.StaffId
WHERE s.IsActive = 1;

PRINT '业务视图创建完成';

-- ============================================
-- 7. 触发器创建 (自动化业务逻辑)
-- ============================================
PRINT 'Phase 2.7: 创建自动化触发器...';

-- 排班更新时间触发器
CREATE TRIGGER tr_WorkSchedules_UpdateTime
ON WorkSchedules
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE WorkSchedules 
    SET UpdatedAt = GETUTCDATE()
    FROM WorkSchedules ws
    INNER JOIN inserted i ON ws.Id = i.Id;
END;

-- 薪资计算验证触发器
CREATE TRIGGER tr_Salaries_ValidateCalculation
ON Salaries
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- 验证薪资计算是否正确
    IF EXISTS (
        SELECT 1 FROM inserted 
        WHERE ABS(GrossPay - (RegularPay + OvertimePay)) > 0.01
    )
    BEGIN
        RAISERROR('薪资计算错误: GrossPay 不等于 RegularPay + OvertimePay', 16, 1);
        ROLLBACK TRANSACTION;
    END;
    
    -- 验证净薪资计算
    IF EXISTS (
        SELECT 1 FROM inserted 
        WHERE ABS(NetPay - (GrossPay - Deductions)) > 0.01
    )
    BEGIN
        RAISERROR('薪资计算错误: NetPay 不等于 GrossPay - Deductions', 16, 1);
        ROLLBACK TRANSACTION;
    END;
END;

PRINT '自动化触发器创建完成';

-- ============================================
-- 8. 示例数据生成
-- ============================================
PRINT 'Phase 2.8: 生成示例薪资数据...';

-- 为活跃员工生成上个月的薪资记录
DECLARE @LastMonthStart DATE = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()) - 1, 1);
DECLARE @LastMonthEnd DATE = EOMONTH(@LastMonthStart);

DECLARE @StaffId INT;
DECLARE staff_cursor CURSOR FOR 
    SELECT Id FROM Staffs WHERE IsActive = 1;

OPEN staff_cursor;
FETCH NEXT FROM staff_cursor INTO @StaffId;

WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC sp_CalculateSalary @StaffId, @LastMonthStart, @LastMonthEnd, 9001;
    FETCH NEXT FROM staff_cursor INTO @StaffId;
END;

CLOSE staff_cursor;
DEALLOCATE staff_cursor;

PRINT '示例薪资数据生成完成';

-- ============================================
-- 提交事务
-- ============================================
COMMIT TRANSACTION;

PRINT '===========================================';
PRINT 'Phase 2 迁移完成！';
PRINT '已完成:';
PRINT '- WorkSchedule 表创建 (完整排班系统)';
PRINT '- Salary 表创建 (薪资计算系统)';
PRINT '- 自动薪资计算存储过程';
PRINT '- 排班冲突检查函数';
PRINT '- 业务视图和触发器';
PRINT '- 示例数据生成';
PRINT '===========================================';

-- ============================================
-- 测试查询
-- ============================================
PRINT 'Phase 2.9: 执行测试查询...';

-- 测试排班数据
SELECT 'WorkSchedule Count' AS Test, COUNT(*) AS Result FROM WorkSchedules;

-- 测试薪资数据
SELECT 'Salary Count' AS Test, COUNT(*) AS Result FROM Salaries;

-- 测试员工排班概览
SELECT TOP 5 * FROM v_StaffScheduleOverview ORDER BY Date DESC;

-- 测试薪资汇总
SELECT TOP 5 * FROM v_SalarySummary ORDER BY PayPeriodStart DESC;

PRINT '测试查询完成';

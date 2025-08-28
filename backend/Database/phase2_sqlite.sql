-- ============================================
-- Phase 2: SQLite 数据库迁移脚本 - 新业务表创建
-- 目标: 添加 Tan 设计的核心业务表
-- 执行时间: Phase 2
-- 风险级别: 中 (新表创建，不影响现有功能)
-- ============================================

BEGIN;

-- ============================================
-- 1. WorkSchedule 表创建 (排班系统)
-- ============================================

CREATE TABLE WorkSchedules (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- 基本信息
    StaffId INTEGER NOT NULL,
    Date DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    ScheduleHours DECIMAL(4,2) NOT NULL,
    
    -- 状态管理
    Status TEXT NOT NULL DEFAULT 'scheduled',
    -- 'scheduled', 'completed', 'absent', 'cancelled'
    
    -- 备注信息
    Notes TEXT,
    CreatedBy INTEGER,  -- 创建排班的管理员
    
    -- 审计字段
    CreatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    UpdatedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    
    -- 外键约束
    FOREIGN KEY (StaffId) REFERENCES Staffs(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedBy) REFERENCES Staffs(Id) ON DELETE SET NULL,
    
    -- 唯一约束 (一个员工在同一天只能有一个排班)
    UNIQUE (StaffId, Date)
);

-- 创建索引
CREATE INDEX IX_WorkSchedules_Date ON WorkSchedules(Date);
CREATE INDEX IX_WorkSchedules_StaffId_Date ON WorkSchedules(StaffId, Date);
CREATE INDEX IX_WorkSchedules_Status_Date ON WorkSchedules(Status, Date);

-- ============================================
-- 2. Salary 表创建 (薪资系统)
-- ============================================

CREATE TABLE Salaries (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- 基本信息
    StaffId INTEGER NOT NULL,
    PayPeriodStart DATE NOT NULL,
    PayPeriodEnd DATE NOT NULL,
    
    -- 工时统计
    TotalHours DECIMAL(8,2) NOT NULL DEFAULT 0,
    TotalOvertimeHours DECIMAL(8,2) NOT NULL DEFAULT 0,
    ScheduledHours DECIMAL(8,2),  -- 计划工时
    
    -- 薪资计算
    RegularPay DECIMAL(10,2) NOT NULL DEFAULT 0,
    OvertimePay DECIMAL(10,2) NOT NULL DEFAULT 0,
    GrossPay DECIMAL(10,2) NOT NULL DEFAULT 0,
    
    -- 扣除项 (可扩展)
    Deductions DECIMAL(10,2) NOT NULL DEFAULT 0,
    NetPay DECIMAL(10,2) NOT NULL DEFAULT 0,
    
    -- 状态管理
    Status TEXT NOT NULL DEFAULT 'draft',
    -- 'draft', 'calculated', 'approved', 'paid'
    
    -- 审计信息
    CalculatedBy INTEGER,  -- 计算薪资的管理员
    ApprovedBy INTEGER,    -- 审批薪资的管理员
    GeneratedAt DATETIME NOT NULL DEFAULT (datetime('now')),
    ApprovedAt DATETIME,
    
    -- 外键约束
    FOREIGN KEY (StaffId) REFERENCES Staffs(Id) ON DELETE CASCADE,
    FOREIGN KEY (CalculatedBy) REFERENCES Staffs(Id) ON DELETE SET NULL,
    FOREIGN KEY (ApprovedBy) REFERENCES Staffs(Id) ON DELETE SET NULL,
    
    -- 唯一约束 (一个员工在同一薪资周期只能有一条记录)
    UNIQUE (StaffId, PayPeriodStart, PayPeriodEnd)
);

-- 创建索引
CREATE INDEX IX_Salaries_PayPeriod ON Salaries(PayPeriodStart, PayPeriodEnd);
CREATE INDEX IX_Salaries_StaffId_Period ON Salaries(StaffId, PayPeriodStart);
CREATE INDEX IX_Salaries_Status_Generated ON Salaries(Status, GeneratedAt);

-- ============================================
-- 3. 为现有数据生成示例排班
-- ============================================

-- 为活跃员工生成未来一周的排班
-- 生成日期序列 (未来7天的工作日)
WITH RECURSIVE DateSeries(x, WorkDate) AS (
    SELECT 0, date('now', '+1 day')
    UNION ALL 
    SELECT x+1, date(WorkDate, '+1 day') 
    FROM DateSeries 
    WHERE x < 7
),
ActiveStaff AS (
    SELECT Id, Role FROM Staffs WHERE IsActive = 1
),
WorkDays AS (
    SELECT WorkDate FROM DateSeries 
    WHERE CAST(strftime('%w', WorkDate) AS INTEGER) NOT IN (0, 6)  -- 排除周末
)
INSERT INTO WorkSchedules (StaffId, Date, StartTime, EndTime, ScheduleHours, Status, CreatedBy)
SELECT 
    s.Id,
    w.WorkDate,
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
CROSS JOIN WorkDays w;

-- ============================================
-- 4. 业务视图创建
-- ============================================

-- 员工排班概览视图
CREATE VIEW v_StaffScheduleOverview AS
SELECT 
    s.Id AS StaffId,
    CASE 
        WHEN s.FirstName IS NOT NULL AND s.LastName IS NOT NULL 
        THEN s.FirstName || ' ' || s.LastName
        ELSE s.Name
    END AS FullName,
    s.Role,
    ws.Date,
    ws.StartTime,
    ws.EndTime,
    ws.ScheduleHours,
    ws.Status,
    CASE 
        WHEN ws.Status = 'completed' THEN 'On Time'
        WHEN ws.Status = 'absent' THEN 'Absent'
        WHEN ws.Date < date('now') AND ws.Status = 'scheduled' THEN 'Overdue'
        ELSE 'Pending'
    END AS ScheduleStatus
FROM Staffs s
LEFT JOIN WorkSchedules ws ON s.Id = ws.StaffId
WHERE s.IsActive = 1;

-- 薪资汇总视图
CREATE VIEW v_SalarySummary AS
SELECT 
    s.Id AS StaffId,
    CASE 
        WHEN s.FirstName IS NOT NULL AND s.LastName IS NOT NULL 
        THEN s.FirstName || ' ' || s.LastName
        ELSE s.Name
    END AS FullName,
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

-- ============================================
-- 5. 示例薪资数据生成
-- ============================================

-- 为活跃员工生成上个月的薪资记录
INSERT INTO Salaries (
    StaffId, PayPeriodStart, PayPeriodEnd, 
    TotalHours, TotalOvertimeHours, ScheduledHours,
    RegularPay, OvertimePay, GrossPay, NetPay, 
    Status, CalculatedBy
)
SELECT 
    s.Id,
    date('now', 'start of month', '-1 month') as PayPeriodStart,
    date('now', 'start of month', '-1 day') as PayPeriodEnd,
    -- 模拟工时数据
    CASE 
        WHEN s.Role = 'admin' THEN 168.0  -- 21天 * 8小时
        WHEN s.Role = 'manager' THEN 189.0  -- 21天 * 9小时
        ELSE 168.0  -- 21天 * 8小时
    END as TotalHours,
    -- 模拟加班时间
    CASE 
        WHEN s.Role = 'manager' THEN 21.0  -- 每天1小时加班
        ELSE 0.0
    END as TotalOvertimeHours,
    -- 计划工时
    CASE 
        WHEN s.Role = 'admin' THEN 168.0
        WHEN s.Role = 'manager' THEN 168.0  -- 标准工时
        ELSE 168.0
    END as ScheduledHours,
    -- 正常薪资
    CASE 
        WHEN s.Role = 'admin' THEN 168.0 * s.HourlyRate
        WHEN s.Role = 'manager' THEN 168.0 * s.HourlyRate
        ELSE 168.0 * s.HourlyRate
    END as RegularPay,
    -- 加班薪资
    CASE 
        WHEN s.Role = 'manager' THEN 21.0 * s.OvertimePayRate
        ELSE 0.0
    END as OvertimePay,
    -- 总薪资
    CASE 
        WHEN s.Role = 'admin' THEN 168.0 * s.HourlyRate
        WHEN s.Role = 'manager' THEN (168.0 * s.HourlyRate) + (21.0 * s.OvertimePayRate)
        ELSE 168.0 * s.HourlyRate
    END as GrossPay,
    -- 净薪资 (暂无扣除)
    CASE 
        WHEN s.Role = 'admin' THEN 168.0 * s.HourlyRate
        WHEN s.Role = 'manager' THEN (168.0 * s.HourlyRate) + (21.0 * s.OvertimePayRate)
        ELSE 168.0 * s.HourlyRate
    END as NetPay,
    'calculated' as Status,
    9001 as CalculatedBy
FROM Staffs s 
WHERE s.IsActive = 1;

COMMIT;

-- ============================================
-- 验证查询
-- ============================================

-- 测试排班数据
SELECT 'WorkSchedule Count' AS Test, COUNT(*) AS Result FROM WorkSchedules;

-- 测试薪资数据
SELECT 'Salary Count' AS Test, COUNT(*) AS Result FROM Salaries;

-- 测试员工排班概览 (前5条)
SELECT 'Staff Schedule Overview' AS Test;
SELECT StaffId, FullName, Role, Date, StartTime, EndTime, ScheduleHours, ScheduleStatus 
FROM v_StaffScheduleOverview 
WHERE Date IS NOT NULL
ORDER BY Date 
LIMIT 5;

-- 测试薪资汇总
SELECT 'Salary Summary' AS Test;
SELECT StaffId, FullName, Role, HourlyRate, TotalHours, GrossPay, NetPay, Status 
FROM v_SalarySummary 
WHERE PayPeriodStart IS NOT NULL
ORDER BY PayPeriodStart DESC;

# 🚀 数据库迁移实施总计划

## 📅 时间表与里程碑

### Week 1: Phase 1 - 基础优化

- **目标**: 优化现有表结构，零风险字段扩展
- **执行**: 运行 `migration_phase1.sql`
- **验证**: 数据完整性检查，API 兼容性测试
- **回滚**: 如有问题，可安全回滚

### Week 2-3: Phase 2 - 业务扩展

- **目标**: 添加排班和薪资系统
- **执行**: 运行 `migration_phase2.sql`
- **验证**: 新功能单元测试，业务逻辑验证
- **风险**: 中等，新表创建不影响现有功能

### Week 4: Phase 3 - 高级特性

- **目标**: 生物识别验证系统
- **执行**: 运行 `migration_phase3.sql`
- **验证**: 安全功能测试，性能压力测试
- **风险**: 较高，复杂业务逻辑需充分测试

## 🛡️ 风险管理策略

### 高风险项目

1. **数据丢失风险**

   - **缓解**: 每阶段前完整备份
   - **监控**: 实时数据完整性检查
   - **应急**: 准备快速回滚脚本

2. **性能下降风险**

   - **缓解**: 分批执行，监控查询性能
   - **监控**: 设置性能告警阈值
   - **应急**: 索引优化和查询调优

3. **业务中断风险**
   - **缓解**: 选择业务低峰期执行
   - **监控**: 应用健康检查
   - **应急**: 快速回滚和服务重启

### 中等风险项目

1. **API 兼容性问题**

   - **缓解**: 向后兼容视图和字段保留
   - **监控**: API 响应格式验证
   - **应急**: 兼容性补丁快速部署

2. **新功能缺陷**
   - **缓解**: 充分的单元测试和集成测试
   - **监控**: 错误日志实时监控
   - **应急**: 功能开关控制

## 📊 详细执行步骤

### Phase 1 执行清单

```bash
# 1. 环境准备
□ 数据库完整备份
□ 应用服务暂停 (可选，建议凌晨执行)
□ 执行用户权限确认

# 2. 脚本执行
□ 运行 migration_phase1.sql
□ 验证执行结果
□ 检查错误日志

# 3. 数据验证
□ 字段完整性检查
□ 索引性能测试
□ 兼容性视图验证

# 4. 应用更新
□ 更新 Entity Framework 模型
□ API 响应格式验证
□ 前端兼容性测试

# 5. 完成确认
□ 功能回归测试
□ 性能基线对比
□ 恢复正常服务
```

### 回滚程序

```sql
-- 紧急回滚脚本模板
-- 警告: 仅在出现严重问题时使用

-- 1. 立即停止应用服务
-- 2. 恢复数据库备份
RESTORE DATABASE [FarmTimeMS] FROM DISK = 'backup_before_migration.bak'
WITH REPLACE, STATS = 10;

-- 3. 重启应用服务
-- 4. 验证功能正常

-- 或者使用渐进回滚 (如果数据未损坏)
-- 详见各 Phase 脚本中的回滚注释部分
```

## 🧪 测试验证方案

### 自动化测试脚本

```sql
-- 数据完整性验证
CREATE PROCEDURE sp_ValidateDataIntegrity
AS
BEGIN
    -- 1. 记录数统计
    SELECT 'Staffs' AS TableName, COUNT(*) AS RecordCount FROM Staffs;
    SELECT 'Events' AS TableName, COUNT(*) AS RecordCount FROM Events;
    SELECT 'Devices' AS TableName, COUNT(*) AS RecordCount FROM Devices;

    -- 2. 外键完整性
    SELECT 'Orphaned Events' AS Issue, COUNT(*) AS Count
    FROM Events e LEFT JOIN Staffs s ON e.StaffId = s.Id
    WHERE s.Id IS NULL;

    -- 3. 数据类型验证
    SELECT 'Invalid HourlyRate' AS Issue, COUNT(*) AS Count
    FROM Staffs WHERE HourlyRate < 0 OR HourlyRate > 1000;

    -- 4. 新字段填充检查
    SELECT 'Missing FirstName' AS Issue, COUNT(*) AS Count
    FROM Staffs WHERE FirstName IS NULL AND IsActive = 1;
END;
```

### 性能基线测试

```sql
-- 核心查询性能测试
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

-- 测试1: 员工考勤查询
SELECT s.FirstName, s.LastName, COUNT(e.Id) AS EventCount
FROM Staffs s
LEFT JOIN Events e ON s.Id = e.StaffId
WHERE s.IsActive = 1
GROUP BY s.Id, s.FirstName, s.LastName;

-- 测试2: 排班查询 (Phase 2 后)
SELECT s.FirstName, s.LastName, ws.Date, ws.StartTime, ws.EndTime
FROM WorkSchedules ws
INNER JOIN Staffs s ON ws.StaffId = s.Id
WHERE ws.Date BETWEEN GETDATE() AND DATEADD(DAY, 7, GETDATE());

-- 测试3: 生物识别验证查询 (Phase 3 后)
SELECT bv.VerificationResult, COUNT(*) AS Count
FROM BiometricVerifications bv
WHERE bv.CreatedAt >= DATEADD(DAY, -1, GETUTCDATE())
GROUP BY bv.VerificationResult;

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
```

## 📈 成功指标

### Phase 1 成功标准

- ✅ 零数据丢失
- ✅ API 响应时间增加 < 10%
- ✅ 所有现有功能正常工作
- ✅ 新字段正确填充 > 95%

### Phase 2 成功标准

- ✅ 排班功能可用性 > 99%
- ✅ 薪资计算准确率 100%
- ✅ 新表查询性能 < 100ms
- ✅ 存储过程执行成功率 > 99%

### Phase 3 成功标准

- ✅ 生物识别匹配准确率 > 95%
- ✅ 验证响应时间 < 2 秒
- ✅ 安全审计日志完整性 100%
- ✅ 可疑活动检测有效性验证

## 🔧 Entity Framework 模型更新

### Phase 1 后的模型更新

```csharp
// 更新 Staff.cs 模型
public class Staff
{
    // 现有字段保留...

    // 新增字段
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ContractType { get; set; }
    public int? StandardHoursPerWeek { get; set; }
    public decimal? OvertimePayRate { get; set; }

    // 计算属性
    public string FullName => $"{FirstName} {LastName}".Trim();
}

// 更新 ApplicationDbContext.cs
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // 现有配置保留...

    // 新增字段配置
    modelBuilder.Entity<Staff>(entity =>
    {
        entity.Property(e => e.FirstName).HasMaxLength(50);
        entity.Property(e => e.LastName).HasMaxLength(50);
        entity.Property(e => e.ContractType).HasMaxLength(20);
    });
}
```

### Phase 2 后的新模型

```csharp
// 新增 WorkSchedule.cs
public class WorkSchedule
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal ScheduleHours { get; set; }
    public string Status { get; set; } = "scheduled";
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual Staff Staff { get; set; } = null!;
    public virtual Staff? Creator { get; set; }
}

// 新增 Salary.cs
public class Salary
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    public decimal? ScheduledHours { get; set; }
    public decimal RegularPay { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal GrossPay { get; set; }
    public decimal Deductions { get; set; }
    public decimal NetPay { get; set; }
    public string Status { get; set; } = "draft";
    public int? CalculatedBy { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Navigation properties
    public virtual Staff Staff { get; set; } = null!;
    public virtual Staff? Calculator { get; set; }
    public virtual Staff? Approver { get; set; }
}
```

### Phase 3 后的高级模型

```csharp
// 新增 BiometricVerification.cs
public class BiometricVerification
{
    public int Id { get; set; }
    public int? StaffId { get; set; }
    public int? BiometricId { get; set; }
    public int DeviceId { get; set; }
    public int? EventId { get; set; }
    public string VerificationResult { get; set; } = string.Empty;
    public decimal? ConfidenceScore { get; set; }
    public string? FailureReason { get; set; }
    public string? CapturedTemplate { get; set; }
    public int? ProcessingTime { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual Staff? Staff { get; set; }
    public virtual BiometricData? BiometricData { get; set; }
    public virtual Device Device { get; set; } = null!;
    public virtual Event? Event { get; set; }
}
```

## 🎯 后续维护建议

### 日常监控

1. **性能监控**: 设置查询性能告警
2. **数据增长**: 监控表大小增长趋势
3. **索引优化**: 定期分析索引使用情况
4. **清理策略**: 制定历史数据清理计划

### 扩展计划

1. **多租户支持**: 添加 TenantId 字段
2. **时区处理**: 升级时间字段为时区感知
3. **国际化**: 添加多语言支持字段
4. **移动端 API**: 优化移动设备访问

---

**总结**: 这个迁移计划提供了从现有设计到 Tan 设计的完整转换路径，确保了零停机时间和数据安全。通过分阶段实施和充分的测试验证，可以安全地实现数据库架构的现代化升级。

记住：**渐进式迁移 + 充分测试 + 快速回滚 = 成功的数据库演进** 🚀

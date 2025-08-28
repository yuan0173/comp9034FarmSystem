-- COMP9034 FarmTimeMS 数据库初始化脚本
-- 创建时间: 2025-08-28
-- 数据库: SQLite (开发环境) / SQL Server (生产环境)

-- ============================================
-- 员工表 (Staffs)
-- ============================================
-- 员工表在代码中通过Entity Framework自动创建
-- 默认包含以下示例数据:
-- ID 9001: System Administrator (admin)
-- ID 8001: Farm Manager (manager)  
-- ID 1001: Farm Worker (staff)

-- ============================================
-- 设备表 (Devices)
-- ============================================
-- 设备表在代码中通过Entity Framework自动创建
-- 默认包含以下示例数据:
-- ID 1: Main Terminal (terminal)
-- ID 2: Biometric Scanner (biometric)

-- ============================================
-- 事件表 (Events)
-- ============================================
-- 考勤事件表，用于记录员工打卡记录
-- 支持的事件类型: IN, OUT, BREAK_START, BREAK_END, OTHER

-- ============================================
-- 生物识别数据表 (BiometricData)
-- ============================================
-- 生物识别模板数据表，支持指纹、人脸等识别类型
-- 模板数据加密存储

-- ============================================
-- 示例数据插入 (可选)
-- ============================================

-- 插入更多示例员工数据
-- INSERT INTO Staffs (Id, Name, Role, Pin, HourlyRate, Email, Phone, IsActive, CreatedAt, UpdatedAt) VALUES
-- (1002, '张三', 'staff', '1002', 25.00, 'zhangsan@farmtimems.com', '13800138001', 1, datetime('now'), datetime('now')),
-- (1003, '李四', 'staff', '1003', 25.00, 'lisi@farmtimems.com', '13800138002', 1, datetime('now'), datetime('now')),
-- (8002, '王经理', 'manager', '8002', 35.00, 'wangmanager@farmtimems.com', '13800138003', 1, datetime('now'), datetime('now'));

-- 插入示例考勤事件
-- INSERT INTO Events (StaffId, DeviceId, EventType, Timestamp, CreatedAt) VALUES
-- (1001, 1, 'IN', datetime('now', '-8 hours'), datetime('now')),
-- (1001, 1, 'BREAK_START', datetime('now', '-4 hours'), datetime('now')),
-- (1001, 1, 'BREAK_END', datetime('now', '-3 hours'), datetime('now')),
-- (1001, 1, 'OUT', datetime('now', '-1 hours'), datetime('now'));

-- ============================================
-- 索引创建 (Entity Framework自动处理)
-- ============================================
-- 以下索引会自动创建:
-- - Staffs表: Id (主键), Email (唯一)
-- - Events表: StaffId + Timestamp (复合索引)
-- - Devices表: Name (唯一)
-- - BiometricData表: StaffId + BiometricType (复合唯一)

-- ============================================
-- 权限设置
-- ============================================
-- 生产环境中应该:
-- 1. 创建专门的数据库用户
-- 2. 授予适当的权限
-- 3. 启用数据库审计
-- 4. 配置数据备份策略

-- ============================================
-- 性能优化建议
-- ============================================
-- 1. Events表按日期分区
-- 2. 定期清理历史数据
-- 3. 监控查询性能
-- 4. 适当添加覆盖索引
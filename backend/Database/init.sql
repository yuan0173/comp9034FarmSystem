-- COMP9034 FarmTimeMS Database Initialization Script
-- Created: 2025-08-28
-- Database: SQLite (Development) / SQL Server (Production)

-- ============================================
-- Staff Table (Staffs)
-- ============================================
-- Staff table is automatically created through Entity Framework in code
-- Default includes the following sample data:
-- ID 9001: System Administrator (admin)
-- ID 8001: Farm Manager (manager)  
-- ID 1001: Farm Worker (staff)

-- ============================================
-- Device Table (Devices)
-- ============================================
-- Device table is automatically created through Entity Framework in code
-- Default includes the following sample data:
-- ID 1: Main Terminal (terminal)
-- ID 2: Biometric Scanner (biometric)

-- ============================================
-- Event Table (Events)
-- ============================================
-- Attendance event table for recording employee clock-in records
-- Supported event types: IN, OUT, BREAK_START, BREAK_END, OTHER

-- ============================================
-- Biometric Data Table (BiometricData)
-- ============================================
-- Biometric template data table, supports fingerprint, face recognition types
-- Template data stored encrypted

-- ============================================
-- Sample Data Insertion (Optional)
-- ============================================

-- Insert additional sample staff data
-- INSERT INTO Staffs (Id, Name, Role, Pin, HourlyRate, Email, Phone, IsActive, CreatedAt, UpdatedAt) VALUES
-- (1002, 'John Smith', 'staff', '1002', 25.00, 'john.smith@farmtimems.com', '+61 400 123 001', 1, datetime('now'), datetime('now')),
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
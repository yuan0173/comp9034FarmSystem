#!/bin/bash

# ============================================
# Farm Time Management System 数据库迁移执行脚本
# 用途: 自动化执行三阶段数据库迁移
# 作者: Database Migration Team
# 版本: 1.0
# ============================================

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 配置参数
DB_SERVER="localhost"
DB_NAME="farmtimems-dev"
DB_USER="sa"
BACKUP_DIR="./backups"
LOG_FILE="./migration_$(date +%Y%m%d_%H%M%S).log"

# 创建备份目录
mkdir -p "$BACKUP_DIR"

# 日志函数
log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}

log_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1" | tee -a "$LOG_FILE"
}

log_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1" | tee -a "$LOG_FILE"
}

log_info() {
    echo -e "${BLUE}[INFO]${NC} $1" | tee -a "$LOG_FILE"
}

# 检查依赖
check_dependencies() {
    log_info "检查系统依赖..."
    
    # 检查 SQLite CLI (开发环境)
    if command -v sqlite3 &> /dev/null; then
        log_success "SQLite3 CLI 已安装"
        DB_TYPE="sqlite"
        DB_FILE="../farmtimems-dev.db"
    else
        log_error "SQLite3 CLI 未找到"
        return 1
    fi
    
    # 检查备份目录权限
    if [ -w "$BACKUP_DIR" ]; then
        log_success "备份目录权限正常"
    else
        log_error "备份目录无写入权限"
        return 1
    fi
    
    return 0
}

# 创建数据库备份
create_backup() {
    local phase=$1
    local backup_file="$BACKUP_DIR/backup_before_phase${phase}_$(date +%Y%m%d_%H%M%S).db"
    
    log_info "创建 Phase $phase 前的数据库备份..."
    
    if [ "$DB_TYPE" = "sqlite" ]; then
        cp "$DB_FILE" "$backup_file"
        if [ $? -eq 0 ]; then
            log_success "备份创建成功: $backup_file"
            echo "$backup_file"
            return 0
        else
            log_error "备份创建失败"
            return 1
        fi
    fi
}

# 执行SQL脚本
execute_sql() {
    local script_file=$1
    local phase=$2
    
    log_info "执行 $script_file..."
    
    if [ ! -f "$script_file" ]; then
        log_error "脚本文件不存在: $script_file"
        return 1
    fi
    
    if [ "$DB_TYPE" = "sqlite" ]; then
        # 为SQLite适配SQL脚本 (移除SQL Server特定语法)
        local adapted_script="/tmp/adapted_$(basename $script_file)"
        
        # 简单的SQL Server到SQLite的语法转换
        sed -e 's/BEGIN TRANSACTION/BEGIN;/g' \
            -e 's/COMMIT TRANSACTION/COMMIT;/g' \
            -e 's/ROLLBACK TRANSACTION/ROLLBACK;/g' \
            -e 's/NVARCHAR/TEXT/g' \
            -e 's/DATETIME2/DATETIME/g' \
            -e 's/IDENTITY(1,1)/AUTOINCREMENT/g' \
            -e 's/GETUTCDATE()/datetime('"'"'now'"'"', '"'"'utc'"'"')/g' \
            -e 's/DATEADD([^,]*,\s*[^,]*,\s*\([^)]*\))/datetime(\1, "+1 day")/g' \
            -e '/PRINT /d' \
            -e '/SET NOCOUNT/d' \
            -e '/STATS = /d' \
            "$script_file" > "$adapted_script"
        
        sqlite3 "$DB_FILE" < "$adapted_script"
        local result=$?
        rm -f "$adapted_script"
        
        if [ $result -eq 0 ]; then
            log_success "Phase $phase SQL脚本执行成功"
            return 0
        else
            log_error "Phase $phase SQL脚本执行失败"
            return 1
        fi
    fi
}

# 验证数据完整性
validate_data() {
    local phase=$1
    
    log_info "验证 Phase $phase 后的数据完整性..."
    
    # 基础表记录统计
    local tables=("Staffs" "Events" "Devices" "BiometricData" "LoginLogs" "AuditLogs")
    
    for table in "${tables[@]}"; do
        if [ "$DB_TYPE" = "sqlite" ]; then
            local count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM $table;" 2>/dev/null)
            if [ $? -eq 0 ]; then
                log_info "$table: $count 条记录"
            else
                log_warning "$table: 表不存在或查询失败"
            fi
        fi
    done
    
    # Phase 特定验证
    case $phase in
        1)
            # 验证姓名字段拆分
            local split_names=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM Staffs WHERE FirstName IS NOT NULL;" 2>/dev/null)
            if [ "$split_names" -gt 0 ]; then
                log_success "姓名字段拆分成功: $split_names 条记录"
            else
                log_warning "姓名字段拆分可能有问题"
            fi
            ;;
        2)
            # 验证新表创建
            local ws_count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM WorkSchedules;" 2>/dev/null)
            local sal_count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM Salaries;" 2>/dev/null)
            log_info "WorkSchedules: $ws_count 条记录"
            log_info "Salaries: $sal_count 条记录"
            ;;
        3)
            # 验证生物识别功能
            local bv_count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM BiometricVerifications;" 2>/dev/null)
            log_info "BiometricVerifications: $bv_count 条记录"
            ;;
    esac
    
    return 0
}

# 回滚函数
rollback() {
    local backup_file=$1
    
    log_warning "开始回滚操作..."
    
    if [ -f "$backup_file" ]; then
        cp "$backup_file" "$DB_FILE"
        if [ $? -eq 0 ]; then
            log_success "数据库已回滚至: $backup_file"
            return 0
        else
            log_error "回滚失败"
            return 1
        fi
    else
        log_error "备份文件不存在: $backup_file"
        return 1
    fi
}

# 执行单个阶段
execute_phase() {
    local phase=$1
    local script_file="migration_phase${phase}.sql"
    
    log_info "========================================"
    log_info "开始执行 Phase $phase"
    log_info "========================================"
    
    # 创建备份
    local backup_file=$(create_backup $phase)
    if [ $? -ne 0 ]; then
        log_error "Phase $phase 备份创建失败，停止执行"
        return 1
    fi
    
    # 执行SQL脚本
    execute_sql "$script_file" $phase
    if [ $? -ne 0 ]; then
        log_error "Phase $phase SQL执行失败"
        read -p "是否回滚? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            rollback "$backup_file"
        fi
        return 1
    fi
    
    # 验证数据
    validate_data $phase
    if [ $? -ne 0 ]; then
        log_warning "Phase $phase 数据验证出现警告"
    fi
    
    log_success "Phase $phase 执行完成"
    echo
    return 0
}

# 性能测试
performance_test() {
    log_info "执行性能基线测试..."
    
    # 员工查询性能
    local start_time=$(date +%s%N)
    sqlite3 "$DB_FILE" "SELECT s.Id, s.Name, COUNT(e.Id) as EventCount FROM Staffs s LEFT JOIN Events e ON s.Id = e.StaffId WHERE s.IsActive = 1 GROUP BY s.Id, s.Name;" > /dev/null
    local end_time=$(date +%s%N)
    local duration=$(((end_time - start_time) / 1000000))
    
    log_info "员工考勤查询耗时: ${duration}ms"
    
    if [ $duration -lt 100 ]; then
        log_success "查询性能良好"
    elif [ $duration -lt 500 ]; then
        log_warning "查询性能一般"
    else
        log_error "查询性能较差，建议优化"
    fi
}

# 生成迁移报告
generate_report() {
    local report_file="migration_report_$(date +%Y%m%d_%H%M%S).html"
    
    log_info "生成迁移报告: $report_file"
    
    cat > "$report_file" << EOF
<!DOCTYPE html>
<html>
<head>
    <title>数据库迁移报告</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .success { color: green; }
        .error { color: red; }
        .warning { color: orange; }
        .info { color: blue; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
    </style>
</head>
<body>
    <h1>Farm Time Management System - 数据库迁移报告</h1>
    <p><strong>迁移时间:</strong> $(date)</p>
    <p><strong>数据库:</strong> $DB_FILE</p>
    
    <h2>表统计信息</h2>
    <table>
        <tr><th>表名</th><th>记录数</th></tr>
EOF

    # 添加表统计
    local tables=("Staffs" "Events" "Devices" "BiometricData" "WorkSchedules" "Salaries" "BiometricVerifications")
    for table in "${tables[@]}"; do
        local count=$(sqlite3 "$DB_FILE" "SELECT COUNT(*) FROM $table;" 2>/dev/null || echo "N/A")
        echo "        <tr><td>$table</td><td>$count</td></tr>" >> "$report_file"
    done
    
    cat >> "$report_file" << EOF
    </table>
    
    <h2>迁移日志</h2>
    <pre>$(cat "$LOG_FILE")</pre>
</body>
</html>
EOF

    log_success "迁移报告已生成: $report_file"
}

# 主函数
main() {
    log_info "=========================================="
    log_info "Farm Time Management System 数据库迁移"
    log_info "开始时间: $(date)"
    log_info "=========================================="
    
    # 检查依赖
    check_dependencies
    if [ $? -ne 0 ]; then
        log_error "依赖检查失败，退出"
        exit 1
    fi
    
    # 询问执行哪些阶段
    echo -e "\n${BLUE}请选择要执行的迁移阶段:${NC}"
    echo "1) 仅 Phase 1 (基础字段优化)"
    echo "2) Phase 1 + Phase 2 (添加业务表)"
    echo "3) 全部阶段 (Phase 1-3)"
    echo "4) 仅 Phase 2 (需要先完成 Phase 1)"
    echo "5) 仅 Phase 3 (需要先完成 Phase 1-2)"
    read -p "请输入选择 (1-5): " choice
    
    case $choice in
        1)
            execute_phase 1
            ;;
        2)
            execute_phase 1 && execute_phase 2
            ;;
        3)
            execute_phase 1 && execute_phase 2 && execute_phase 3
            ;;
        4)
            execute_phase 2
            ;;
        5)
            execute_phase 3
            ;;
        *)
            log_error "无效选择"
            exit 1
            ;;
    esac
    
    if [ $? -eq 0 ]; then
        log_success "所选阶段执行完成"
        
        # 性能测试
        performance_test
        
        # 生成报告
        generate_report
        
        log_info "=========================================="
        log_success "数据库迁移成功完成!"
        log_info "日志文件: $LOG_FILE"
        log_info "备份目录: $BACKUP_DIR"
        log_info "=========================================="
    else
        log_error "迁移过程中出现错误"
        exit 1
    fi
}

# 信号处理 (Ctrl+C)
trap 'log_warning "迁移被用户中断"; exit 1' INT

# 执行主函数
main "$@"

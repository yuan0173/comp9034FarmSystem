# Git分支命名规范 - COMP9034 Farm Time Management System

## 🎯 分支命名规范

### 基本结构
```
[type]/yuan0173/[scope]/[description]
```

### 分支类型 (Type)
- `feature` - 新功能开发
- `bugfix` - Bug修复
- `enhancement` - 功能增强/改进
- `refactor` - 代码重构
- `hotfix` - 紧急修复
- `experiment` - 实验性功能
- `docs` - 文档更新
- `chore` - 构建、工具等维护性工作

### 开发者标识
- `yuan0173` - 主要开发者标识符

### 项目范围 (Scope)
- `frontend` - 前端React应用 (frontendWebsite/)
- `backend` - 后端.NET API (backend/)
- `fullstack` - 涉及前后端的功能
- `database` - 数据库相关修改
- `infrastructure` - CI/CD、部署、配置
- `docs` - 文档系统改进

### 描述部分 (Description)
- 使用小写字母和短横线连接
- 简洁描述分支的主要目的
- 避免使用下划线或空格
- 优先使用英文，如需中文用拼音

## 📋 分支命名示例

### 前端相关分支
```bash
# Bug修复
bugfix/yuan0173/frontend/staff-dropdown-default-selection
bugfix/yuan0173/frontend/login-form-validation-error
bugfix/yuan0173/frontend/attendance-clock-display-issue

# 新功能
feature/yuan0173/frontend/employee-photo-upload
feature/yuan0173/frontend/bulk-staff-import
feature/yuan0173/frontend/advanced-search-filters
feature/yuan0173/frontend/offline-mode-pwa

# 性能优化
enhancement/yuan0173/frontend/dashboard-loading-optimization
enhancement/yuan0173/frontend/mobile-responsive-improvements
```

### 后端相关分支
```bash
# API开发
feature/yuan0173/backend/biometric-authentication-api
feature/yuan0173/backend/payroll-calculation-service
feature/yuan0173/backend/audit-logging-system

# Bug修复
bugfix/yuan0173/backend/jwt-token-expiration-issue
bugfix/yuan0173/backend/database-connection-timeout

# 架构改进
refactor/yuan0173/backend/repository-pattern-implementation
refactor/yuan0173/backend/service-layer-abstraction
```

### 全栈功能分支
```bash
# 涉及前后端的完整功能
feature/yuan0173/fullstack/real-time-attendance-tracking
feature/yuan0173/fullstack/manager-dashboard-analytics
feature/yuan0173/fullstack/staff-schedule-management
feature/yuan0173/fullstack/role-based-access-control

# 系统级改进
enhancement/yuan0173/fullstack/error-handling-standardization
enhancement/yuan0173/fullstack/api-response-optimization
```

### 基础设施分支
```bash
# 部署和CI/CD
feature/yuan0173/infrastructure/azure-pipelines-setup
feature/yuan0173/infrastructure/docker-containerization
enhancement/yuan0173/infrastructure/render-deployment-optimization

# 数据库
feature/yuan0173/database/postgresql-migration
feature/yuan0173/database/performance-indexes
bugfix/yuan0173/database/foreign-key-constraints
```

### 文档分支
```bash
docs/yuan0173/docs/api-documentation-update
docs/yuan0173/docs/deployment-guide-enhancement
docs/yuan0173/docs/architecture-documentation
```

## 🚀 Git工作流程

### 1. 创建分支
```bash
# 从main分支创建新分支
git checkout main
git pull origin main
git checkout -b [branch-name]

# 示例
git checkout -b feature/yuan0173/frontend/staff-management-ui
```

### 2. 开发过程
```bash
# 定期提交代码
git add .
git commit -m "feat: implement staff creation form with validation"

# 定期推送到远程
git push origin [branch-name]
```

### 3. 合并流程
```bash
# 完成开发后，确保与main同步
git checkout main
git pull origin main
git checkout [branch-name]
git merge main

# 推送最终版本
git push origin [branch-name]

# 创建Pull Request（通过GitHub界面）
```

## 🎨 提交信息规范

### 提交类型前缀
- `feat:` - 新功能
- `fix:` - Bug修复
- `docs:` - 文档更新
- `style:` - 代码格式调整
- `refactor:` - 代码重构
- `perf:` - 性能优化
- `test:` - 测试相关
- `chore:` - 构建、工具等维护

### 提交信息示例
```bash
git commit -m "feat: add staff photo upload functionality to admin panel"
git commit -m "fix: resolve JWT token expiration handling in authentication service"
git commit -m "docs: update API documentation for biometric endpoints"
git commit -m "refactor: extract common database repository patterns"
git commit -m "perf: optimize dashboard loading with lazy loading and caching"
```

## 📚 当前项目分支状态

### 技术栈对应
- **Frontend**: React 18 + TypeScript + Material-UI + Vite + PWA
- **Backend**: .NET 8 Web API + Entity Framework Core + PostgreSQL
- **Infrastructure**: Azure Pipelines + Docker + Render Deployment

### 活跃分支
- `main` - 主分支，包含稳定的生产代码
- `feature/inactive-staff-management` - 员工状态管理功能分支
- `gh-pages` - GitHub Pages部署分支

### 分支保护规则
- `main`分支受保护，需要通过Pull Request合并
- 所有变更需要通过代码审查
- 合并前需要确保功能测试通过

## 🔧 Farm Time Management System特定约定

### 业务功能分支
```bash
# 考勤系统
feature/yuan0173/fullstack/clock-in-out-system
feature/yuan0173/frontend/attendance-history-view
feature/yuan0173/backend/time-calculation-service

# 员工管理
feature/yuan0173/fullstack/staff-crud-operations
feature/yuan0173/frontend/staff-search-filter
feature/yuan0173/backend/staff-data-validation

# 设备管理
feature/yuan0173/fullstack/device-registration-system
feature/yuan0173/backend/biometric-device-integration

# 报表系统
feature/yuan0173/frontend/manager-analytics-dashboard
feature/yuan0173/backend/payroll-calculation-engine
feature/yuan0173/fullstack/csv-export-functionality
```

### 角色特定分支
```bash
# 管理员功能
feature/yuan0173/frontend/admin-staff-management
feature/yuan0173/frontend/admin-device-configuration

# 经理功能
feature/yuan0173/frontend/manager-attendance-reports
feature/yuan0173/fullstack/manager-dashboard-analytics

# 员工功能
feature/yuan0173/frontend/staff-clock-station
feature/yuan0173/frontend/staff-attendance-history
```

### 安全和性能分支
```bash
# 安全功能
feature/yuan0173/backend/jwt-authentication-system
feature/yuan0173/fullstack/role-based-authorization
security/yuan0173/backend/password-encryption-upgrade

# 性能优化
perf/yuan0173/frontend/pwa-offline-mode
perf/yuan0173/backend/database-query-optimization
perf/yuan0173/fullstack/real-time-sync-performance
```

## 📋 最佳实践

### 1. 分支命名最佳实践
- **具体性**: 分支名应明确表达要解决的问题或实现的功能
- **一致性**: 团队内统一遵循命名规范
- **简洁性**: 避免过长的分支名，控制在50字符以内

### 2. 开发流程最佳实践
- **小而频繁的提交**: 便于代码审查和问题追溯
- **定期同步主分支**: 避免大型合并冲突
- **功能完整性**: 确保每个分支实现完整的功能单元
- **测试验证**: 合并前确保功能正常工作

### 3. 分支管理最佳实践
- **及时清理**: 合并后删除feature分支
- **命名一致**: 遵循既定的命名模式
- **文档记录**: 重要分支应有详细的PR描述

## 🚫 避免的命名模式

❌ **不好的示例：**
```bash
fix-bug                                    # 太模糊
yuan0173_feature_branch                   # 使用下划线
StaffManagementFeature                    # 使用大写字母
temp-branch-for-testing                   # 临时性命名
feature/frontend/staff-management         # 缺少开发者标识
很长的中文分支名称包含特殊字符            # 中文+特殊字符
```

✅ **好的示例：**
```bash
feature/yuan0173/frontend/staff-management-ui
bugfix/yuan0173/backend/authentication-token-refresh
enhancement/yuan0173/fullstack/offline-sync-optimization
refactor/yuan0173/backend/repository-pattern-implementation
```

## 🏗️ 企业级项目考虑

### 分支策略
- **Git Flow**: 适用于有明确发布周期的项目
- **GitHub Flow**: 适用于持续部署的项目
- **现在采用**: 简化的GitHub Flow + Feature Branches

### 发布分支
```bash
release/yuan0173/v1.0.0-production-ready
release/yuan0173/v1.1.0-biometric-features
hotfix/yuan0173/v1.0.1-critical-security-fix
```

### 环境分支
```bash
deploy/yuan0173/staging-environment
deploy/yuan0173/production-deployment
experiment/yuan0173/performance-testing
```

## 📊 分支命名统计

### 当前项目历史分支模式分析
基于项目历史，我们已使用的模式：
- `feature/project-structure-optimization` ✅
- `feature/inactive-staff-management` ✅
- `feat/backend-yuan0173` ✅
- `chore/update-readme-yuan0173` ✅

### 推荐改进
未来分支应遵循完整的三段式命名：
- `[type]/yuan0173/[scope]/[description]`

---

**遵循这些规范有助于保持COMP9034 Farm Time Management System代码库的组织性和团队协作效率！**

_文档版本: v1.0 | 最后更新: September 2025_
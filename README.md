# COMP9034 Farm Time Management System

A comprehensive modern farm attendance and payroll management system with offline-first PWA capabilities, intelligent synchronization, comprehensive audit logging, and enterprise-grade security.

## Tech Stack

- **Frontend**: React 18 + TypeScript + Material-UI v5 + Vite + PWA
- **Backend**: .NET 8 Web API + Entity Framework Core
- **Database**: SQLite (Development) / SQL Server (Production)
- **Authentication**: JWT Bearer Token + PIN/Password + Role-based Access Control
- **Features**: Offline Support, Real-time Sync, Audit Logging, RESTful API with Swagger

## Project Structure

```
COMP9034-FarmTimeMS/
├── frontendWebsite/          # React + TypeScript frontend
│   ├── src/
│   │   ├── components/       # Reusable UI components
│   │   ├── pages/           # Application pages/views
│   │   ├── api/             # API client and HTTP utilities
│   │   ├── hooks/           # Custom React hooks
│   │   ├── offline/         # PWA offline functionality
│   │   ├── types/           # TypeScript type definitions
│   │   └── utils/           # Utility functions
│   └── public/              # Static assets and PWA icons
├── backend/                  # .NET 8 Web API backend
│   ├── Controllers/         # API controllers
│   ├── Models/              # Entity models
│   ├── Services/            # Business logic services
│   ├── Data/                # Database context
│   ├── DTOs/                # Data Transfer Objects
│   ├── Middlewares/         # Custom middleware
│   ├── Database/            # Migration scripts
│   └── logs/                # Application logs
├── docs/                    # Project documentation
└── ai-docs/                 # AI-generated documentation
```

## Server Configuration & Setup

### Environment Overview

| Environment     | Frontend URL             | Backend URL                                | Database                     | Purpose                     |
| --------------- | ------------------------ | ------------------------------------------ | ---------------------------- | --------------------------- |
| **Development** | `http://localhost:3000+` | `http://localhost:4000`                    | SQLite (`farmtimems-dev.db`) | Local development & testing |
| **Production**  | Cloud deployment         | `https://flindersdevops.azurewebsites.net` | Azure SQL Server             | Live deployment & usage     |

**🔧 智能端口管理策略：**

- **后端固定端口**: `4000` (可配置)
- **前端动态端口**: `3000, 3001, 5173...` (自动检测可用端口)
- **端口冲突处理**: 自动递增查找可用端口
- **CORS 动态配置**: 开发环境允许所有本地端口访问

**🌐 环境变量配置：**

```bash
# 开发环境变量
ASPNETCORE_ENVIRONMENT=Development  # 后端环境
VITE_API_BASE_URL=http://localhost:4000  # 前端API配置 (可选)

# 生产环境变量
ASPNETCORE_ENVIRONMENT=Production
AllowedOrigins__0=https://your-domain.com
```

### Prerequisites

- **Node.js 18+** (for frontend development)
- **.NET 8 SDK** (for backend development)
- **SQLite** (embedded database for development)

### Development Server Setup

#### 🔥 **推荐方案: 热重载开发环境 (Hot Reload)**

**最佳开发体验，支持实时代码更新，无需手动重启：**

```bash
# Terminal 1: Backend Hot Reload (dotnet watch)
cd backend
ASPNETCORE_ENVIRONMENT=Development dotnet watch run --urls=http://localhost:4000

# Terminal 2: Frontend Hot Module Replacement (Vite HMR)
cd frontendWebsite
npm install
npm run dev
```

**热重载功能特性：**

- ✅ **后端热重载**: 修改 `.cs` 文件自动重启服务器
- ✅ **前端热更新**: 修改 React 组件毫秒级更新，保持页面状态
- ✅ **自动 CORS 配置**: 开发环境动态允许所有本地端口
- ✅ **实时错误反馈**: 编译错误和运行时错误立即显示
- ✅ **智能端口管理**: 自动检测端口冲突并使用可用端口

**热重载工作原理：**

```bash
# 后端 (dotnet watch)
dotnet watch ⌚ File changed: StaffsController.cs
dotnet watch 🔥 Hot reload of changes succeeded.
# 或者需要重启时：
dotnet watch 🔄 Restarting due to file change...
dotnet watch 🚀 Started

# 前端 (Vite HMR)
[vite] connecting...
[vite] connected.
[vite] hmr update /src/pages/AdminStaffs.tsx
```

#### 📊 **开发效率对比**

| 开发模式       | 修改后操作     | 等待时间 | 效率提升         |
| -------------- | -------------- | -------- | ---------------- |
| **传统模式**   | 手动重启前后端 | 10-30 秒 | 基准             |
| **热重载模式** | 自动检测更新   | 1-3 秒   | **10 倍提升** ✨ |

#### Option 2: 传统启动方式

**同时启动双服务器：**

```bash
# Terminal 1: Backend Server
cd backend
dotnet restore
dotnet run --urls=http://localhost:4000

# Terminal 2: Frontend Server
cd frontendWebsite
npm install
npm run dev
```

**单独管理服务器：**

**Backend Only:**

```bash
cd backend
dotnet restore
dotnet build
dotnet run --urls=http://localhost:4000
# API Documentation: http://localhost:4000/swagger
```

**Frontend Only:**

```bash
cd frontendWebsite
npm install
npm run dev
# Application: http://localhost:3000 (自动检测可用端口)
```

### Server Status Verification

```bash
# Check if servers are running
curl http://localhost:4000/health    # Backend health check
curl http://localhost:3000           # Frontend accessibility

# Check running processes
lsof -ti:4000,3000,3001,5173        # List processes using these ports
ps -p <PID>                          # Check specific process details
```

### 🛠️ **故障排除 & 最佳实践**

#### 常见问题解决

**1. 端口被占用错误：**

```bash
# 查找占用端口的进程
lsof -ti:4000
# 终止指定进程
kill <PID>
# 或者终止所有 dotnet 进程
pkill -f "dotnet"
```

**2. CORS 错误：**

```bash
# 确保后端在开发环境运行
ASPNETCORE_ENVIRONMENT=Development dotnet watch run --urls=http://localhost:4000
# 检查控制台输出应显示: "🔧 CORS: 开发环境 - 允许所有本地来源"
```

**3. 热重载不工作：**

```bash
# 后端：确保使用 dotnet watch
dotnet watch run --urls=http://localhost:4000

# 前端：确保使用 npm run dev (不是 npm start)
npm run dev
```

#### 🎯 **热重载最佳实践**

**开发工作流程：**

1. **一次启动**: 使用热重载启动双服务器
2. **专注编码**: 修改代码后自动更新，无需手动操作
3. **实时测试**: 立即查看更改效果
4. **快速迭代**: 从修改到测试仅需 1-3 秒

**支持的热重载操作：**

- ✅ **后端**: 修改方法体、添加新 API、更新配置文件
- ✅ **前端**: React 组件更新、样式修改、状态管理更改
- ✅ **配置**: appsettings.json、环境变量更改
- ❌ **需重启**: 依赖注入配置、数据库迁移

**键盘快捷键：**

```bash
# 后端热重载
Ctrl + R          # 手动重启后端
Ctrl + C          # 停止服务器

# 前端热重载
Ctrl + R          # 浏览器刷新
r + Enter         # Vite 手动重启
```

## Key Features

### Core Functionality

- **Multi-role Dashboard**: Staff, Manager, Admin interfaces with role-based permissions
- **Smart Clock System**: IN/OUT/BREAK events with intelligent validation
- **Offline-First PWA**: Full offline functionality with automatic synchronization
- **Advanced Time Tracking**: Precise work hours calculation with break management
- **Real-time Status**: Live connection status and sync indicators

### Database & Management

- **Comprehensive Staff Profiles**: Extended information with contact details
- **Device Management**: Multi-device support with biometric integration
- **Audit Logging**: Complete system audit trail with user action tracking
- **Login History**: Detailed login logs with security monitoring
- **Work Scheduling**: Advanced roster management (foundation implemented)
- **Payroll Integration**: Salary calculation framework (foundation implemented)
- **Biometric Verification**: Fingerprint/face recognition support (foundation implemented)

### Technical Excellence

- **Frontend-Backend Alignment**: Unified data models and naming conventions
- **Complete Internationalization**: Full English localization across all components
- **🔥 Hot Reload Development**: Backend `dotnet watch` + Frontend Vite HMR for 10x faster development
- **🔧 Smart Port Management**: Dynamic port detection with automatic CORS configuration
- **🌐 Environment-Aware Configuration**: Dynamic development/production environment handling
- **Robust Error Handling**: Comprehensive global exception middleware
- **API Documentation**: Complete Swagger documentation with interactive examples

## Test Accounts

### 1. **System Administrator**

- **Staff ID**: `9001`
- **PIN**: `1234`
- **Name**: System Administrator
- **Email**: admin@farmtimems.com
- **Role**: admin
- **Access**: Complete system administration, staff management, device configuration

### 2. **Farm Manager**

- **Staff ID**: `8001`
- **PIN**: `8001`
- **Name**: Farm Manager
- **Email**: manager@farmtimems.com
- **Role**: manager
- **Access**: Attendance reports, payroll management, CSV exports, dashboard analytics

### 3. **Farm Worker (Primary)**

- **Staff ID**: `1001`
- **PIN**: `1001`
- **Name**: Farm Worker
- **Email**: worker@farmtimems.com
- **Role**: staff
- **Access**: Clock in/out, personal attendance, basic station functions

### 4. **Test Worker (Secondary)**

- **Staff ID**: `2001`
- **PIN**: `2001`
- **Name**: Test Worker
- **Email**: test@example.com
- **Role**: staff
- **Access**: Clock in/out, personal attendance, basic station functions

## Testing & Usage

### Quick Testing Steps

1. **Start Servers**: Follow server startup instructions above
2. **Access Application**: Navigate to `http://localhost:3000`
3. **Login**: Use test accounts below to access different role interfaces
4. **Test Features**:
   - Clock in/out functionality
   - Offline mode (disconnect internet)
   - Data synchronization (reconnect internet)
   - Role-specific features

### Expected Behavior

- **Admin (9001)**: Complete system access, staff management, device configuration
- **Manager (8001)**: Reports, analytics, payroll management, CSV exports
- **Staff (1001/2001)**: Personal clock station, attendance history, limited access

## User Role Permissions

### Staff (ID: 1000-7999)

- Clock in/out functionality
- View personal attendance history
- Access work station interface
- Offline clock operations

### Manager (ID: 8000-8999)

- All staff permissions
- Generate attendance reports
- Export data to CSV
- View team analytics
- Manage work schedules

### Admin (ID: 9000+)

- All manager permissions
- Staff account management (CRUD operations)
- Device management and configuration
- System audit logs
- Biometric data management
- System configuration

## Development Status

### Completed Components

**Frontend Application**

- React 18 + TypeScript + Material-UI v5 + PWA
- Offline functionality with smart synchronization
- Role-based interfaces and responsive design

**Backend API**

- .NET 8 Web API + Entity Framework Core
- RESTful endpoints + JWT authentication + role-based authorization
- Global exception handling and comprehensive logging

**Database & Integration**

- SQLite development database with comprehensive schema
- Complete sample data and migration scripts
- Frontend-backend alignment + unified error handling

## API Endpoints

### Authentication

- `POST /api/Auth/login-pin` - PIN-based authentication
- `POST /api/Auth/login` - Username/password authentication

### Staff Management

- `GET /api/Staffs` - List all staff with filtering
- `POST /api/Staffs` - Create new staff member
- `PUT /api/Staffs/{id}` - Update staff information
- `DELETE /api/Staffs/{id}` - Remove staff member
- `POST /api/Staffs/{id}/verify` - PIN verification

### Event Tracking

- `GET /api/Events` - Retrieve attendance events
- `POST /api/Events` - Record new attendance event
- `GET /api/Events/today` - Today's events
- `POST /api/Events/batch` - Bulk event creation

### Device Management

- `GET /api/Devices` - List all devices
- `POST /api/Devices` - Register new device
- `PUT /api/Devices/{id}` - Update device configuration

### System Monitoring

- `GET /health` - Health check endpoint
- `GET /api/Audit` - Audit log retrieval

## Documentation

### Available Documentation

- **API Documentation**: `http://localhost:4000` (Swagger UI)
- **Project Analysis**: `docs/PROJECT_ANALYSIS.md`
- **Backend Integration**: `docs/BACKEND_INTEGRATION.md`
- **Setup Guide**: `docs/SETUP.md`
- **Frontend Status**: `docs/FRONTEND_STATUS_REPORT.md`
- **AI Documentation**: `ai-docs/` folder (excluded from Git)

### Development Guides

- **Frontend README**: `frontendWebsite/README.md`
- **Backend README**: `backend/README.md`
- **API Connection Guide**: `frontendWebsite/API-Connection-Documentation.md`

### Technical Implementation

- Frontend components with English labels
- API documentation in English
- Database initialization scripts in English

## Security Features

### Authentication & Authorization

- JWT Bearer token authentication
- Role-based access control (RBAC)
- PIN-based quick login for staff
- Session management and timeout

### Data Protection

- Secure password hashing (BCrypt)
- Input validation and sanitization
- SQL injection prevention (Entity Framework)
- 🔧 **Smart CORS Configuration**:
  - Development: Dynamic localhost port allowance
  - Production: Strict domain whitelist
- 🛡️ **Self-Deletion Prevention**: Administrators cannot delete themselves or last admin

### Audit & Monitoring

- Comprehensive audit logging
- Login attempt tracking
- System activity monitoring
- Error logging and reporting

## Development Team

- **Tian Yuan (Tim)**: Lead Developer + Scrum Master
- **Zichun Zhang**: Developer
- **Qiutong Li**: Developer

## Project Metrics

- **Total Files**: 100+ source files
- **Code Coverage**: Frontend & Backend fully functional
- **Test Accounts**: 4 comprehensive test scenarios
- **API Endpoints**: 15+ RESTful endpoints
- **Database Tables**: 7+ entity models with relationships

---

**Project Status**: **Production Ready**

_Developed for COMP9034 DevOps and Enterprise Systems Project_  
_Last Updated\*\*: August 2025_

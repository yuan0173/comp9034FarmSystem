# 🚜 COMP9034 Farm Time Management System

## 📊 Project Overview
A modern farm attendance and payroll management system built with React frontend and .NET backend, featuring offline-first PWA capabilities and smart synchronization.

## 🏗️ Architecture
- **Frontend**: React 18 + TypeScript + Material-UI + Vite
- **Backend**: .NET 8 + SQLite + Entity Framework
- **Database**: SQLite (Development) / SQL Server (Production)
- **Authentication**: JWT Bearer Token + PIN/Password Login
- **API**: RESTful API with Swagger documentation

## 📁 Project Structure
```
COMP9034-FarmTimeMS/
├── frontendWebsite/   # 🖥️ React + TypeScript frontend (Vite)
│   ├── src/          # React source code
│   ├── public/       # Static assets
│   └── package.json  # Frontend dependencies
├── backend/           # ⚙️ .NET 8 Web API backend  
│   ├── Program.cs    # API entry point
│   ├── Controllers/  # API controllers
│   ├── Models/       # Data models
│   ├── Services/     # Business logic
│   ├── Data/         # Database context
│   └── farmtimems-dev.db # SQLite database
├── docs/             # 📄 Project documentation
└── README.md         # This file
```

## 🚀 Quick Start

### Prerequisites
- Node.js 18+ (for frontend)
- .NET 8 SDK (for backend)
- SQLite (embedded database)

### Frontend Development
```bash
cd frontendWebsite
npm install
npm run dev
# Runs on http://localhost:3000
```

### Backend Development  
```bash
cd backend
dotnet restore
dotnet run --urls=http://0.0.0.0:4000
# Runs on http://localhost:4000
# API Documentation: http://localhost:4000 (Swagger UI)
```

### Full System Setup
```bash
# Terminal 1: Start Backend
cd backend
./start-backend.sh

# Terminal 2: Start Frontend  
cd frontendWebsite
npm run dev
```

## 🎯 Key Features
- **Multi-role Interface**: Staff, Manager, Admin dashboards
- **Smart Clock System**: IN/OUT/BREAK events with offline support
- **Intelligent Sync**: Automatic data synchronization when online
- **Time Calculation**: Advanced work hours calculation with anomaly detection
- **PWA Ready**: Installable app with offline capabilities
- **Responsive Design**: Works on desktop, tablet, and mobile

## 🧪 Test Accounts

### 1. **管理员账号 (Admin)**
- **员工ID**: `9001`
- **PIN码**: `1234` 
- **姓名**: System Administrator
- **邮箱**: admin@farmtimems.com
- **登录后跳转**: `/admin/staffs` - 员工管理页面

### 2. **经理账号 (Manager)**
- **员工ID**: `8001`
- **PIN码**: `8001`
- **姓名**: Farm Manager  
- **邮箱**: manager@farmtimems.com
- **登录后跳转**: `/manager` - 经理管理页面

### 3. **员工账号 (Staff) - 选项1**
- **员工ID**: `1001`
- **PIN码**: `1001`
- **姓名**: Farm Worker
- **邮箱**: worker@farmtimems.com
- **登录后跳转**: `/station` - 工作站页面
- 

### 4. **员工账号 (Staff) - 选项2**
- **员工ID**: `2001`
- **PIN码**: `2001`
- 
- **姓名**: Test Worker
- **邮箱**: test@example.com
- **登录后跳转**: `/station` - 工作站页面

## 🎯 测试建议

**测试流程**：
1. 访问 http://localhost:3000
2. 依次使用上述4个账号登录
3. 验证每个角色跳转到正确的页面
4. 测试各角色的权限和功能差异

**预期结果**：
- **Admin (9001)**: 可访问员工管理、系统设置等高级功能
- **Manager (8001)**: 可访问中级管理功能
- **Staff (1001/2001)**: 只能访问基础工作站功能

## 👥 User Roles  
- **Staff (ID: 1000-7999)**: Clock in/out, view personal roster
- **Manager (ID: 8000-8999)**: Attendance reports, payroll, CSV export  
- **Admin (ID: 9000+)**: Staff management, device management, system audit

## 🛠️ Development Status
- **Frontend**: ✅ **Complete** - All core functionality implemented
- **Backend**: ✅ **Complete** - Full REST API with authentication
- **Database**: ✅ **Complete** - SQLite with sample data loaded
- **Integration**: ✅ **Complete** - Frontend-backend integration working

## 🔧 API Endpoints
- **Authentication**: `POST /api/Auth/login-pin`, `POST /api/Auth/login`
- **Staff Management**: `GET /api/Staffs`, `POST /api/Staffs`, `PUT /api/Staffs/{id}`, `DELETE /api/Staffs/{id}`
- **Events**: `GET /api/Events`, `POST /api/Events`
- **Biometric**: `GET /api/Biometric`, `POST /api/Biometric`
- **Devices**: `GET /api/Devices`, `POST /api/Devices`
- **Health Check**: `GET /health`

## 📚 Documentation
- **API Documentation**: http://localhost:4000 (Swagger UI)
- **Project Documentation**: See `docs/` folder for detailed guides

## 🤝 Team
- **Tian Yuan (Tim)**: Developer + Scrum Master
- **Zichun Zhang**: Developer
- **Qiutong Li**: Developer

---
*Generated for COMP9034 DevOps and Enterprise Systems Project*

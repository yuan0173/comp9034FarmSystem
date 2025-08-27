# 🚜 COMP9034 Farm Time Management System

## 📊 Project Overview
A modern farm attendance and payroll management system built with React frontend and .NET backend, featuring offline-first PWA capabilities and smart synchronization.

## 🏗️ Architecture
- **Frontend**: React 18 + TypeScript + Material-UI + PWA
- **Backend**: .NET 8 + SQL Server + Entity Framework
- **Database**: SQL Server 2022 (Docker containerized)
- **Offline Support**: IndexedDB + Service Worker

## 📁 Project Structure
```
COMP9034-FarmTimeMS/
├── frontend/          # 🖥️ React + TypeScript frontend application
│   ├── src/          # React source code
│   ├── public/       # Static assets
│   └── package.json  # Frontend dependencies
├── backend/           # ⚙️ .NET Core Web API backend  
│   ├── Program.cs    # API entry point
│   └── *.csproj      # Backend project file
├── docs/             # 📄 Project documentation
│   ├── FRONTEND_STATUS_REPORT.md
│   ├── BACKEND_INTEGRATION.md
│   └── SETUP.md
└── README.md         # This file
```

## 🚀 Quick Start

### Prerequisites
- Node.js 22+ (for frontend)
- .NET 8 SDK (for backend)
- Docker Desktop (for SQL Server)

### Frontend Development
```bash
cd frontend
npm install
npm run dev
# Runs on http://localhost:3000
```

### Backend Development  
```bash
cd backend
dotnet restore
dotnet run
# Runs on http://localhost:5000
```

### Database Setup
```bash
# Start SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd123" \
   -p 1433:1433 --name comp9034-sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

## 🎯 Key Features
- **Multi-role Interface**: Staff, Manager, Admin dashboards
- **Smart Clock System**: IN/OUT/BREAK events with offline support
- **Intelligent Sync**: Automatic data synchronization when online
- **Time Calculation**: Advanced work hours calculation with anomaly detection
- **PWA Ready**: Installable app with offline capabilities
- **Responsive Design**: Works on desktop, tablet, and mobile

## 👥 User Roles
- **Staff (ID: 1000-7999)**: Clock in/out, view personal roster
- **Manager (ID: 8000-8999)**: Attendance reports, payroll, CSV export
- **Admin (ID: 9000+)**: Staff management, device management, system audit

## 🛠️ Development Status
- Frontend: **80% complete** - Core functionality implemented
- Backend: **In development** - API endpoints being built
- Database: **Ready** - Schema and containers configured

## 📚 Documentation
- [Frontend Status Report](docs/FRONTEND_STATUS_REPORT.md) - Detailed frontend progress
- [Backend Integration Guide](docs/BACKEND_INTEGRATION.md) - API integration instructions
- [Setup Guide](docs/SETUP.md) - Development environment setup

## 🤝 Team
- **Tian Yuan (Tim)**: Developer + Scrum Master
- **Kevin**: Project Manager
- **Tan**: System Architect

---
*Generated for COMP9034 DevOps and Enterprise Systems Project*
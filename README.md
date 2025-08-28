# 🚜 COMP9034 Farm Time Management System

## 📊 Project Overview

A comprehensive modern farm attendance and payroll management system built with React frontend and .NET backend. Features offline-first PWA capabilities, intelligent synchronization, comprehensive audit logging, and enterprise-grade security.

## 🏗️ Architecture

- **Frontend**: React 18 + TypeScript + Material-UI v5 + Vite + PWA
- **Backend**: .NET 8 Web API + Entity Framework Core + SQLite/SQL Server
- **Database**: SQLite (Development) / SQL Server (Production) with comprehensive schema
- **Authentication**: JWT Bearer Token + PIN/Password Login + Role-based Access Control
- **API**: RESTful API with Swagger documentation and comprehensive error handling
- **Offline Support**: IndexedDB + Service Worker + Smart Synchronization

## 📁 Project Structure

```
COMP9034-FarmTimeMS/
├── frontendWebsite/          # 🖥️ React + TypeScript frontend (Vite)
│   ├── src/
│   │   ├── components/       # Reusable UI components
│   │   ├── pages/           # Application pages/views
│   │   ├── api/             # API client and HTTP utilities
│   │   ├── hooks/           # Custom React hooks
│   │   ├── offline/         # PWA offline functionality
│   │   ├── types/           # TypeScript type definitions
│   │   └── utils/           # Utility functions
│   ├── public/              # Static assets and PWA icons
│   └── package.json         # Frontend dependencies
├── backend/                  # ⚙️ .NET 8 Web API backend
│   ├── Program.cs           # API entry point and configuration
│   ├── Controllers/         # API controllers (Auth, Staff, Events, etc.)
│   ├── Models/              # Entity models (Staff, Event, Device, etc.)
│   ├── Services/            # Business logic services
│   ├── Data/                # Database context and configuration
│   ├── DTOs/                # Data Transfer Objects
│   ├── Middlewares/         # Custom middleware (logging, exceptions)
│   ├── Database/            # Migration scripts and initialization
│   └── farmtimems-dev.db    # SQLite database (development)
├── docs/                    # 📄 Project documentation
├── ai-docs/                 # 🤖 AI-generated documentation
└── README.md                # This file
```

## 🚀 Quick Start

### Prerequisites

- **Node.js 18+** (for frontend development)
- **.NET 8 SDK** (for backend development)
- **SQLite** (embedded database for development)

### Standard Port Configuration

- **Frontend**: `http://localhost:3000` (React/Vite development server)
- **Backend**: `http://localhost:4000` (ASP.NET Core Web API)

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
dotnet run --urls=http://localhost:4000
# Runs on http://localhost:4000
# API Documentation: http://localhost:4000 (Swagger UI)
```

### Full System Setup

```bash
# Terminal 1: Start Backend
cd backend
dotnet run --urls=http://localhost:4000

# Terminal 2: Start Frontend
cd frontendWebsite
npm run dev
```

## 🎯 Enhanced Features

### Core Functionality

- **Multi-role Interface**: Staff, Manager, Admin dashboards with role-specific permissions
- **Smart Clock System**: IN/OUT/BREAK events with intelligent validation
- **Offline-First PWA**: Full offline functionality with automatic synchronization
- **Advanced Time Tracking**: Precise work hours calculation with break management
- **Real-time Status**: Live connection status and sync indicators

### New Database Features (Post-Migration)

- **Comprehensive Staff Management**: Extended staff profiles with contact information
- **Device Management**: Multi-device support with biometric integration
- **Audit Logging**: Complete system audit trail with user action tracking
- **Login History**: Detailed login logs with security monitoring
- **Work Scheduling**: Advanced roster management (foundation implemented)
- **Payroll Integration**: Salary calculation framework (foundation implemented)
- **Biometric Verification**: Fingerprint/face recognition support (foundation implemented)

### Technical Enhancements

- **Frontend-Backend Alignment**: Unified data models and naming conventions
- **Internationalization**: Complete English localization across all components
- **Port Standardization**: Consistent port configuration (3000/4000)
- **Error Handling**: Comprehensive global exception middleware
- **API Documentation**: Complete Swagger documentation with examples

## 🧪 Test Accounts

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

## 🎯 Testing Guidelines

### Testing Workflow

1. **Access Application**: Navigate to `http://localhost:3000`
2. **Role-based Testing**: Login with each test account to verify role-specific features
3. **Offline Testing**: Disconnect internet and test offline clock-in/out functionality
4. **Sync Testing**: Reconnect and verify automatic data synchronization
5. **Cross-device Testing**: Test responsive design on different screen sizes

### Expected Results

- **Admin (9001)**: Full access to staff management, device configuration, system audit
- **Manager (8001)**: Access to reports, analytics, payroll features
- **Staff (1001/2001)**: Limited access to personal station and attendance features

## 👥 User Role Permissions

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

## 🛠️ Development Status

### Frontend (✅ Complete)

- React 18 with TypeScript strict mode
- Material-UI v5 modern design system
- PWA with offline functionality
- Smart synchronization system
- Responsive design for all devices

### Backend (✅ Complete)

- .NET 8 Web API with Entity Framework Core
- Comprehensive RESTful API endpoints
- JWT authentication with role-based authorization
- Global exception handling and logging
- Database migration system

### Database (✅ Complete)

- SQLite development database with comprehensive schema
- Sample data for testing all features
- Migration scripts for schema updates
- Audit logging infrastructure
- Biometric data support

### Integration (✅ Complete)

- Frontend-backend data model alignment
- Unified error handling
- Consistent API response format
- Real-time connection status monitoring

## 🔧 API Endpoints

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

## 📚 Documentation

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

## 🌍 Internationalization Status

### Language Support

- **Primary Language**: English (100% complete)
- **Legacy Chinese Content**: Fully migrated to English
- **Code Comments**: Fully translated to English
- **Documentation**: Completely localized to English
- **User Interface**: English-only interface

### Technical Implementation

- All backend error messages in English
- Frontend components with English labels
- API documentation in English
- Database initialization scripts in English

## 🔒 Security Features

### Authentication & Authorization

- JWT Bearer token authentication
- Role-based access control (RBAC)
- PIN-based quick login for staff
- Session management and timeout

### Data Protection

- Secure password hashing (BCrypt)
- Input validation and sanitization
- SQL injection prevention (Entity Framework)
- CORS configuration for secure cross-origin requests

### Audit & Monitoring

- Comprehensive audit logging
- Login attempt tracking
- System activity monitoring
- Error logging and reporting

## 🤝 Development Team

- **Tian Yuan (Tim)**: Lead Developer + Scrum Master
- **Zichun Zhang**: Developer
- **Qiutong Li**: Developer

## 📊 Project Metrics

- **Total Files**: 100+ source files
- **Code Coverage**: Frontend & Backend fully functional
- **Test Accounts**: 4 comprehensive test scenarios
- **API Endpoints**: 15+ RESTful endpoints
- **Database Tables**: 7+ entity models with relationships

---

**Project Status**: ✅ **Production Ready**

_Developed for COMP9034 DevOps and Enterprise Systems Project_  
_Last Updated\*\*: August 2025_

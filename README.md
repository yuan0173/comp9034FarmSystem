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

**🔧 Smart Port Management Strategy:**

- **Backend Fixed Port**: `4000` (configurable)
- **Frontend Dynamic Port**: `3000, 3001, 5173...` (auto-detect available port)
- **Port Conflict Handling**: Auto-increment to find available port
- **Dynamic CORS Configuration**: Development environment allows all local ports

**🌐 Environment Variable Configuration:**

```bash
# Development Environment Variables
ASPNETCORE_ENVIRONMENT=Development  # Backend environment
VITE_API_BASE_URL=http://localhost:4000  # Frontend API configuration (optional)

# Production Environment Variables
ASPNETCORE_ENVIRONMENT=Production
AllowedOrigins__0=https://your-domain.com
```

### Prerequisites

- **Node.js 18+** (for frontend development)
- **.NET 8 SDK** (for backend development)
- **SQLite** (embedded database for development)

### Development Server Setup

#### 🔥 **Recommended Solution: Hot Reload Development Environment**

**Best development experience with real-time code updates, no manual restart required:**

```bash
# Terminal 1: Backend Hot Reload (dotnet watch)
cd backend
ASPNETCORE_ENVIRONMENT=Development dotnet watch run --urls=http://localhost:4000

# Terminal 2: Frontend Hot Module Replacement (Vite HMR)
cd frontendWebsite
npm install
npm run dev
```

**Hot Reload Feature Highlights:**

- ✅ **Backend Hot Reload**: Automatically restart server when `.cs` files are modified
- ✅ **Frontend Hot Update**: Update React components in milliseconds while preserving page state
- ✅ **Automatic CORS Configuration**: Development environment dynamically allows all local ports
- ✅ **Real-time Error Feedback**: Compile errors and runtime errors displayed immediately
- ✅ **Smart Port Management**: Auto-detect port conflicts and use available ports

**Hot Reload Working Principle:**

```bash
# Backend (dotnet watch)
dotnet watch ⌚ File changed: StaffsController.cs
dotnet watch 🔥 Hot reload of changes succeeded.
# Or when restart is needed:
dotnet watch 🔄 Restarting due to file change...
dotnet watch 🚀 Started

# Frontend (Vite HMR)
[vite] connecting...
[vite] connected.
[vite] hmr update /src/pages/AdminStaffs.tsx
```

#### 📊 **Development Efficiency Comparison**

| Development Mode    | Post-Modification Action | Wait Time | Efficiency Gain  |
| ------------------- | ----------------------- | --------- | ---------------- |
| **Traditional Mode** | Manual restart both ends| 10-30 sec | Baseline         |
| **Hot Reload Mode**  | Auto-detect updates     | 1-3 sec   | **10x Faster** ✨ |

#### Option 2: Traditional Startup Method

**Start both servers simultaneously:**

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

**Manage servers separately:**

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
# Application: http://localhost:3000 (auto-detect available port)
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

### 🛠️ **Troubleshooting & Best Practices**

#### Common Issue Resolution

**1. Port Occupied Error:**

```bash
# Find process occupying the port
lsof -ti:4000
# Terminate specific process
kill <PID>
# Or terminate all dotnet processes
pkill -f "dotnet"
```

**2. CORS Error:**

```bash
# Ensure backend runs in development environment
ASPNETCORE_ENVIRONMENT=Development dotnet watch run --urls=http://localhost:4000
# Check console output should show: "🔧 CORS: Development environment - Allow all local sources"
```

**3. Hot Reload Not Working:**

```bash
# Backend: Ensure using dotnet watch
dotnet watch run --urls=http://localhost:4000

# Frontend: Ensure using npm run dev (not npm start)
npm run dev
```

#### 🎯 **Hot Reload Best Practices**

**Development Workflow:**

1. **One-Time Startup**: Use hot reload to start both servers
2. **Focus on Coding**: Code changes auto-update without manual intervention
3. **Real-time Testing**: See changes immediately
4. **Rapid Iteration**: From modification to testing takes only 1-3 seconds

**Supported Hot Reload Operations:**

- ✅ **Backend**: Modify method body, add new APIs, update configuration files
- ✅ **Frontend**: React component updates, style modifications, state management changes
- ✅ **Configuration**: appsettings.json, environment variable changes
- ❌ **Requires Restart**: Dependency injection configuration, database migrations

**Keyboard Shortcuts:**

```bash
# Backend Hot Reload
Ctrl + R          # Manual restart backend
Ctrl + C          # Stop server

# Frontend Hot Reload
Ctrl + R          # Browser refresh
r + Enter         # Vite manual restart
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

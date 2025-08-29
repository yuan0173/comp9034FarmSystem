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

| Environment     | Frontend URL            | Backend URL                                | Database                     | Purpose                     |
| --------------- | ----------------------- | ------------------------------------------ | ---------------------------- | --------------------------- |
| **Development** | `http://localhost:3000` | `http://localhost:4000`                    | SQLite (`farmtimems-dev.db`) | Local development & testing |
| **Production**  | Cloud deployment        | `https://flindersdevops.azurewebsites.net` | Azure SQL Server             | Live deployment & usage     |

### Prerequisites

- **Node.js 18+** (for frontend development)
- **.NET 8 SDK** (for backend development)
- **SQLite** (embedded database for development)

### Server Startupx

#### Option 1: Start Both Servers Simultaneously

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

#### Option 2: Individual Server Management

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
# Application: http://localhost:3000
```

### Server Status Verification

```bash
# Check if servers are running
curl http://localhost:4000/health    # Backend health check
curl http://localhost:3000           # Frontend accessibility
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
- **Standardized Configuration**: Consistent port setup (Frontend:3000, Backend:4000)
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
- CORS configuration for secure cross-origin requests

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

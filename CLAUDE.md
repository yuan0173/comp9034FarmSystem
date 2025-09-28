# CLAUDE.md - COMP9034 Farm Time Management System

This file provides guidance for Claude Code when working in the COMP9034 Farm Time Management System repository.

## 🔧 MANDATORY: Claude Code Development Standards

**CRITICAL**: Claude Code MUST strictly follow these enterprise development standards in ALL sessions:

### Code Comment Standards (NON-NEGOTIABLE)
- **ALL code comments MUST be written in English only**
- **Bug fixes MUST include proper reference headers:**
  ```csharp
  // =================================================================
  // Bug Fix: [Descriptive title of the bug fix]
  // Developer: [Developer Name]
  // Date: [YYYY-MM-DD]
  // Description: [Detailed description of what was fixed]
  // Issue: [Description of the original problem]
  // Bug Reference: Bug #[number] or Issue #[number]
  // =================================================================
  ```
- **Feature additions MUST use:** `// Add: [Brief description]`
- **Enhancements MUST use:** `// Update: [Brief description]`
- **Temporary code MUST use:** `// Temporary: [Purpose] - Remove before production deployment`

### Git Commit Standards (MANDATORY)
- **ALL commits MUST follow prefix standards:**
  - `feat:` - New features
  - `fix:` - Bug fixes
  - `docs:` - Documentation updates
  - `refactor:` - Code restructuring
  - `perf:` - Performance improvements
  - `test:` - Adding or updating tests
  - `chore:` - Maintenance tasks

### Prohibited Practices (NEVER ALLOWED)
- ❌ Chinese comments in code files
- ❌ AI tool identity markers (Claude, ChatGPT, etc.)
- ❌ Auto-generated signatures in commits
- ❌ Personal information unrelated to code changes

### Architecture Compliance (STRICT ENFORCEMENT)
- **MUST follow Repository + Unit of Work + Service Layer pattern**
- **MUST implement proper error handling with ApiResult<T>**
- **MUST use dependency injection and proper separation of concerns**
- **MUST validate all inputs and handle exceptions properly**

### Documentation References
- **Primary Standards**: `/docs/development/DEVELOPMENT_STANDARDS_TEMPLATE.md`
- **Git Branch Naming**: `/docs/development/GIT_BRANCH_NAMING_CONVENTIONS.md`
- **Architecture Guide**: `/docs/architecture/BACKEND_INTEGRATION.md`

**ENFORCEMENT**: Claude Code will be reminded of these standards at the start of EVERY new session.

## 🚀 MANDATORY WORKFLOW: Pre-Work Checklist

**CRITICAL**: Claude Code MUST execute this checklist at the start of EVERY new session:

### Phase 1: Essential Documentation Review (REQUIRED)
Before starting any development work, Claude Code MUST read these documents in order:

1. **📋 Development Standards (PRIORITY 1)**
   ```
   Read: /docs/development/DEVELOPMENT_STANDARDS_TEMPLATE.md
   Purpose: Understand enterprise architecture patterns, code standards, and quality requirements
   Action: Apply ALL coding standards, error handling patterns, and testing requirements
   ```

2. **🔧 Git Branch Naming (PRIORITY 1)**
   ```
   Read: /docs/development/GIT_BRANCH_NAMING_CONVENTIONS.md
   Purpose: Follow project-specific branch naming and commit standards
   Action: Use proper branch naming format: [type]/yuan0173/[scope]/[description]
   ```

3. **🏗️ Architecture Integration (PRIORITY 1)**
   ```
   Read: /docs/architecture/BACKEND_INTEGRATION.md
   Purpose: Understand full-stack architecture and integration patterns
   Action: Follow Repository + Unit of Work + Service Layer pattern
   ```

### Phase 2: Project Context Understanding (REQUIRED)
4. **📊 Project Analysis (PRIORITY 2)**
   ```
   Read: /docs/analysis/PROJECT_ANALYSIS.md
   Purpose: Understand current project state and technical decisions
   Action: Align development with existing architecture
   ```

5. **📋 Development Phases (PRIORITY 2)**
   ```
   Read: /docs/phases/PHASE_1_CRITICAL_FIXES.md
   Read: /docs/phases/PHASE_2_ARCHITECTURAL_IMPROVEMENTS.md
   Read: /docs/phases/PHASE_3_ENTERPRISE_ENHANCEMENTS.md
   Read: /docs/phases/PHASE_4_FRONTEND_MODERNIZATION.md
   Purpose: Understand project roadmap and current phase priorities
   Action: Align work with current phase objectives
   ```

### Phase 3: Quick Reference (AS NEEDED)
6. **🚀 Startup Guide**
   ```
   Reference: /docs/guides/STARTUP_GUIDE.md
   Purpose: Quick project setup and development environment
   ```

7. **🤖 Agent Guidelines**
   ```
   Reference: /docs/guides/AGENTS.md
   Purpose: Understanding of AI-assisted development workflow
   ```

### ✅ Session Initialization Checklist

Claude Code MUST confirm completion of:
- [ ] Read all PRIORITY 1 documents (/docs/development/ and /docs/architecture/)
- [ ] Understand enterprise architecture patterns (Repository + UoW + Service)
- [ ] Know coding standards (English comments, Git prefixes, error handling)
- [ ] Understand branch naming conventions ([type]/yuan0173/[scope]/[description])
- [ ] Read current project analysis and phase documentation
- [ ] Ready to apply ALL standards consistently throughout the session

### 🔄 Session Recovery Protocol

**If Claude Code session is interrupted and restarted:**
1. Immediately re-read this CLAUDE.md file
2. Execute the Pre-Work Checklist above
3. Ask user for context about current work state
4. Resume development with full compliance to all standards

### 📝 Development Session Commands

**Before starting any coding task:**
```
Confirm: "I have read all required documentation and will apply enterprise development standards throughout this session."
```

**For each code change:**
- Apply proper English comments with Bug #/Issue # references
- Use correct Git commit prefixes (feat:, fix:, docs:, etc.)
- Follow Repository + Unit of Work + Service Layer pattern
- Implement proper error handling with ApiResult<T>

**CRITICAL REMINDER**: This checklist ensures consistency across ALL Claude Code sessions, regardless of interruptions or restarts.

## 🚀 COMMAND: comment pr

**Purpose**: When user inputs "comment pr", execute complete Git commit and PR creation workflow

**IMPORTANT**: This command provides suggestions only. Claude Code MUST NOT execute any Git operations without explicit user authorization.

### Git Commit Command Template
```bash
# Analyze current changes first
git status
git diff --name-only HEAD

# Prepare commit with proper format
git add [files]
git commit -m "[prefix]: [concise description]

[detailed description of changes]
- [specific change 1]
- [specific change 2]
- [specific change 3]

[impact statement and purpose]"

# Push to remote
git push origin [branch-name]
```

### PR Description Template
```markdown
## Summary
[Concise overview of the changes and their purpose]

## Changes Made
### [Category 1]
- [Specific change with file reference]
- [Specific change with file reference]

### [Category 2]
- [Specific change with file reference]

## Key Features
- [Feature/improvement 1]
- [Feature/improvement 2]
- [Feature/improvement 3]

## Testing
- [x] [Test item 1]
- [x] [Test item 2]
- [ ] [Pending test item]

## Impact
[Description of how these changes improve the project]

## Deployment Notes
[Any special considerations for deployment]
```

### Execution Rules
1. **Analysis First**: Always run `git status` and `git diff` to understand changes
2. **User Authorization Required**: Present commands for user approval before execution
3. **No Tool References**: Never include AI, Claude, or tool identity markers in commits/PRs
4. **Standard Compliance**: Follow all Git commit prefix standards (feat:, fix:, docs:, etc.)
5. **Professional Tone**: Maintain enterprise-level professional language
6. **Complete Information**: Include file references, impact statements, and testing notes

### Quick Reference - Commit Prefixes
- `feat:` - New features or functionality
- `fix:` - Bug fixes and corrections
- `docs:` - Documentation updates
- `refactor:` - Code restructuring without functional changes
- `perf:` - Performance improvements
- `test:` - Test additions or updates
- `chore:` - Maintenance, builds, or tooling updates

## Project Overview

**COMP9034 Farm Time Management System**
- A comprehensive modern farm attendance and payroll management system
- Full-stack enterprise application with React frontend and .NET backend
- PWA capabilities with offline-first functionality
- Enterprise-grade security and audit logging

## Technology Stack

### Frontend (frontendWebsite/)
- **React 18** + **TypeScript** + **Material-UI v5**
- **Vite** build tool with Hot Module Replacement (HMR)
- **Zustand** for state management
- **PWA** with Workbox for offline functionality
- **React Query** for API data management

### Backend (backend/)
- **.NET 8 Web API** with Enterprise Architecture Pattern
- **Entity Framework Core** with PostgreSQL
- **Repository Pattern** + **Unit of Work** + **Service Layer**
- **JWT Authentication** with role-based access control
- **Global Exception Handling** with structured responses

### Infrastructure
- **PostgreSQL** (Production) / **SQLite** (Development)
- **Azure Pipelines** for CI/CD
- **Docker** containerization support
- **Render** deployment platform

## Project Structure

### Enterprise Architecture Organization

```
COMP9034-FarmTimeMS/
├── frontendWebsite/          # React + TypeScript frontend
│   ├── src/
│   │   ├── components/       # Reusable UI components
│   │   ├── pages/           # Application pages/views
│   │   ├── api/             # API client and HTTP utilities
│   │   ├── stores/          # Zustand state management
│   │   ├── types/           # TypeScript type definitions
│   │   ├── hooks/           # Custom React hooks
│   │   ├── offline/         # PWA offline functionality
│   │   └── utils/           # Utility functions
│   ├── build/dist/          # Production build output
│   ├── config/              # Configuration files
│   ├── docs/                # Frontend documentation
│   └── tests/               # Test files
├── backend/                  # .NET 8 Web API backend
│   ├── src/                 # Enterprise source structure
│   │   ├── Controllers/     # API controllers with authorization
│   │   ├── Services/        # Business logic layer
│   │   ├── Repositories/    # Data access layer
│   │   ├── Models/          # Entity models
│   │   ├── Data/            # EF Core DbContext
│   │   ├── DTOs/            # Data Transfer Objects
│   │   ├── Middlewares/     # Global exception handling
│   │   └── Common/          # Shared utilities
│   ├── config/              # Configuration files
│   ├── backup/              # Backup files
│   ├── tests/               # Test utilities
│   └── Database/            # SQL migration scripts
├── docs/                    # Organized project documentation
│   ├── analysis/           # Project analysis
│   ├── architecture/       # Architecture documents
│   ├── phases/             # Implementation phases
│   ├── development/        # Development standards
│   └── guides/             # User guides
├── testing/                # Comprehensive testing framework
└── scripts/                # Project utility scripts
```

## Development Environment

### Prerequisites
- **Node.js 18+** (frontend development)
- **.NET 8 SDK** (backend development)
- **Docker** (PostgreSQL database)
- **PostgreSQL** (production database)

### Hot Reload Development Setup
```bash
# Terminal 1: PostgreSQL Database (Docker)
docker run --name postgres-farmtime -e POSTGRES_USER=devuser -e POSTGRES_PASSWORD=devpass -e POSTGRES_DB=farmtimems -p 5434:5432 -d postgres:15

# Terminal 2: Backend Hot Reload
cd backend
DATABASE_URL="postgres://devuser:devpass@localhost:5434/farmtimems" ASPNETCORE_ENVIRONMENT=Development dotnet watch run --urls=http://localhost:4000

# Terminal 3: Frontend Hot Module Replacement
cd frontendWebsite
npm install
npm run dev
```

### API Configuration
- **Development**: `http://localhost:4000`
- **Production**: `https://flindersdevops.azurewebsites.net`
- **Swagger UI**: `http://localhost:4000` (development only)

## Authentication & Authorization

### Role-Based System
- **Admin (ID: 9000+)**: Full system access, staff management, device configuration
- **Manager (ID: 8000-8999)**: Reports, analytics, payroll management
- **Staff (ID: 1000-7999)**: Clock in/out, personal attendance

### Test Accounts
```javascript
// System Administrator
{ staffId: "9001", pin: "1234", role: "admin" }

// Farm Manager
{ staffId: "8001", pin: "8001", role: "manager" }

// Farm Workers
{ staffId: "1001", pin: "1001", role: "staff" }
{ staffId: "2001", pin: "2001", role: "staff" }
```

### JWT Token Handling
```typescript
// Frontend authentication patterns
const useUserStore = () => {
  const token = localStorage.getItem('authToken')
  const headers = token ? { Authorization: `Bearer ${token}` } : {}
  return { token, headers }
}
```

## Common Development Tasks

### Frontend Development
```bash
# Start development server
cd frontendWebsite
npm run dev

# Build for production
npm run build

# Type checking
npm run lint
```

### Backend Development
```bash
# Start with hot reload
cd backend
dotnet watch run --urls=http://localhost:4000

# Build project
dotnet build

# Run tests
dotnet test
```

### Database Operations
```bash
# Check database status
curl http://localhost:4000/health

# View API documentation
# Navigate to http://localhost:4000 (Swagger UI)
```

## Key Features & Components

### Core Functionality
- **Multi-role Dashboard**: Staff, Manager, Admin interfaces
- **Smart Clock System**: IN/OUT/BREAK events with validation
- **Offline-First PWA**: Full offline functionality with sync
- **Advanced Time Tracking**: Work hours calculation with break management
- **Real-time Status**: Live connection and sync indicators

### Frontend Components
- **AdminStaffs**: Staff management with CRUD operations
- **AdminDevices**: Device registration and configuration
- **ManagerAttendance**: Attendance reports and analytics
- **StaffClock**: Employee clock in/out station
- **OfflineSync**: PWA synchronization management

### Backend Services
- **StaffService**: Employee management business logic
- **AuthService**: JWT authentication and authorization
- **EventService**: Attendance event processing
- **AuditService**: System audit logging

## Code Patterns & Standards

### Frontend Patterns
```typescript
// Zustand state management
const useAppStore = create<AppState>((set) => ({
  currentUser: null,
  setCurrentUser: (user) => set({ currentUser: user }),
}))

// API client pattern
const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:4000',
  timeout: 10000,
})

// React Query pattern
const { data: staffs, isLoading } = useQuery({
  queryKey: ['staffs'],
  queryFn: () => api.getStaffs(),
})
```

### Backend Patterns
```csharp
// Repository pattern
public interface IStaffRepository : IGenericRepository<Staff>
{
    Task<Staff?> GetByStaffIdAsync(string staffId);
    Task<List<Staff>> GetActiveStaffsAsync();
}

// Service pattern
public class StaffService : IStaffService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;
}

// API result pattern
public class ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
}
```

## Development Guidelines

### Git Branch Naming
Follow the established conventions in `docs/development/GIT_BRANCH_NAMING_CONVENTIONS.md`:
```bash
# Format: [type]/yuan0173/[scope]/[description]
feature/yuan0173/frontend/staff-management-ui
bugfix/yuan0173/backend/authentication-token-refresh
enhancement/yuan0173/fullstack/offline-sync-optimization
```

### Commit Message Standards
```bash
feat: add staff photo upload functionality
fix: resolve JWT token expiration handling
docs: update API documentation for biometric endpoints
refactor: implement repository pattern for data access
perf: optimize dashboard loading with lazy loading
```

### Code Quality Standards
1. **TypeScript**: Strict type checking enabled
2. **ESLint**: Code quality enforcement
3. **Prettier**: Code formatting consistency
4. **Enterprise Patterns**: Repository, Service, Unit of Work
5. **Error Handling**: Global exception middleware
6. **Security**: JWT authentication, role-based authorization

## Testing Approach

### Manual Testing Workflow
1. **Start Development Environment**: Use hot reload setup
2. **Authentication Testing**: Test all role-based access
3. **Feature Testing**: Verify CRUD operations
4. **Offline Testing**: Disconnect internet, test PWA functionality
5. **Sync Testing**: Reconnect and verify data synchronization
6. **Cross-browser Testing**: Chrome, Firefox, Safari, Edge

### API Testing
- **Swagger UI**: `http://localhost:4000` (interactive API testing)
- **Health Check**: `http://localhost:4000/health`
- **Authentication**: Test JWT token generation and validation

## Security Considerations

### Authentication Security
- **JWT Tokens**: Secure token generation and validation
- **Password Hashing**: BCrypt for secure password storage
- **Session Management**: Proper token expiration and refresh
- **Role-based Access**: Granular permission system

### Data Protection
- **Input Validation**: Both frontend and backend validation
- **SQL Injection Prevention**: Entity Framework parameterized queries
- **XSS Prevention**: React built-in protection + input sanitization
- **CORS Configuration**: Environment-specific origin restrictions

## Performance Optimization

### Frontend Optimizations
- **Lazy Loading**: Component-based code splitting
- **React Query**: Intelligent caching and background updates
- **PWA Caching**: Workbox service worker configuration
- **Bundle Optimization**: Vite tree-shaking and minification

### Backend Optimizations
- **Database Indexing**: Optimized query performance
- **Caching**: In-memory caching for frequently accessed data
- **Connection Pooling**: Efficient database connection management
- **Async Operations**: Non-blocking API endpoints

## Debugging & Troubleshooting

### Common Issues
1. **CORS Errors**: Check development environment configuration
2. **Authentication Failures**: Verify JWT token format and expiration
3. **Database Connection**: Ensure PostgreSQL is running on port 5434
4. **Build Errors**: Check TypeScript types and dependencies

### Debugging Tools
- **Browser DevTools**: React Developer Tools, Network tab
- **Backend Logging**: Comprehensive logging with different levels
- **Health Checks**: Real-time system status monitoring
- **Database Tools**: pgAdmin or similar for PostgreSQL inspection

## Deployment

### Development Deployment
- **Frontend**: Vite dev server with HMR
- **Backend**: dotnet watch with auto-reload
- **Database**: Docker PostgreSQL container

### Production Deployment
- **Frontend**: Static build deployed to CDN/hosting
- **Backend**: .NET application on Azure/Render
- **Database**: Managed PostgreSQL service

## Documentation Structure

### Available Documentation
- **API Documentation**: Swagger UI at backend root
- **Architecture**: `docs/architecture/BACKEND_INTEGRATION.md`
- **Startup Guide**: `docs/guides/STARTUP_GUIDE.md`
- **Development Standards**: `docs/development/`
- **Project Analysis**: `docs/analysis/PROJECT_ANALYSIS.md`

### Development Team
- **Lead Developer**: Tim Yuan (yuan0173)
- **Contributors**: Zichun Zhang, Qiutong Li

---

**This configuration ensures Claude Code has comprehensive understanding of the COMP9034 Farm Time Management System's enterprise architecture and development workflow.**

_Last Updated: September 2025_
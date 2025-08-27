# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Core Development
- `npm run dev` - Start development server (runs on port 3000)
- `npm run build` - Build for production (TypeScript compilation + Vite build)
- `npm run preview` - Preview production build

### Code Quality
- `npm run lint` - Run ESLint with TypeScript extensions
- `npm run lint:fix` - Auto-fix ESLint issues
- `npm run format` - Format code with Prettier

### Type Checking
- `tsc` - Run TypeScript compiler for type checking (build command includes this)

## Project Architecture

### Technology Stack
- **React 18** with TypeScript and Vite
- **Material UI v5** for components and theming
- **React Query** (@tanstack/react-query) for server state management
- **React Router v6** for navigation
- **IndexedDB** (via idb) for offline storage
- **Axios** for HTTP requests
- **PWA** with service worker and caching

### Smart Mode Architecture
The app operates in two modes based on backend connectivity:

**Demo Mode (Backend Unavailable)**:
- Uses mock data and local storage
- Shows blue info alerts on pages
- Events stored in IndexedDB for later sync

**Production Mode (Backend Connected)**:
- Real API calls to backend
- Full offline-first functionality with sync
- No demo alerts shown

Mode switching is automatic based on backend availability checks to `/api/Staffs?limit=1`.

### Core Application Structure

**API Layer** (`src/api/`):
- `client.ts` - Typed API functions for all endpoints (staffApi, eventApi, deviceApi, biometricApi)
- `http.ts` - Axios configuration with interceptors and error handling

**Offline System** (`src/offline/`):
- `db.ts` - IndexedDB operations for local storage
- `sync.ts` - Queue management and automatic sync logic

**Time Calculation Engine** (`src/utils/time.ts`):
- Event pairing algorithm for IN/OUT and BREAK_START/BREAK_END
- Work hours calculation with anomaly detection
- Cross-day event handling

**User Roles & Authentication**:
- Staff ID ranges determine roles: <8000=staff, 8000-8999=manager, ≥9000=admin
- PIN-based authentication with localStorage persistence
- Role-based routing and navigation

### Key Components

**AppShell** (`src/components/AppShell.tsx`):
- Main layout with Material UI navigation
- Role-based menu items and routing
- Logout functionality

**NetworkBadge** (`src/components/NetworkBadge.tsx`):
- Shows online/offline status
- Displays sync queue count
- Manual sync trigger

**BackendStatus Components**:
- Real-time backend connectivity indicators
- Auto-refresh every 30 seconds
- Manual connection testing

### Page Structure

**Staff Interface**:
- `Station.tsx` - Clock in/out with large touch-friendly buttons
- `Roster.tsx` - Work schedule view (demo data)

**Manager Interface**:
- `ManagerDashboard.tsx` - Overview with active staff and metrics  
- `ManagerAttendance.tsx` - Attendance analysis and CSV export
- `ManagerPayslips.tsx` - Payroll calculations

**Admin Interface** (placeholder):
- `AdminStaffs.tsx` - Staff management
- `AdminDevices.tsx` - Device management
- `AdminEvents.tsx` - Event audit trail

### Data Types & API Integration

**Main Entities** (defined in `src/types/api.ts`):
- `Staff` - Employee records with role, pay rates, contact info
- `Event` - Clock in/out events with timestamps and event types
- `Device` - Clock terminals/devices
- `BiometricData` - Biometric templates for staff

**API Base URL**: `https://flindersdevops.azurewebsites.net/api` (configurable via VITE_API_BASE_URL)

**Event Types**:
- `IN` / `OUT` - Work shift events
- `BREAK_START` / `BREAK_END` - Break periods
- `OTHER` - Other events

### Offline-First Design

**Event Queuing**:
- Failed API calls automatically queue events in IndexedDB
- FIFO sync order when connection restored
- Visual indicators for queue status

**Auto-Sync Triggers**:
- Network online event
- App focus/visibility change
- Manual user action
- Periodic checks when online

### PWA Configuration

**Service Worker** (vite-plugin-pwa):
- NetworkFirst caching strategy for API calls
- 24-hour cache expiration
- Automatic updates

**Manifest**:
- Standalone display mode
- Icons: 192x192 and 512x512 PNG files in public/
- Theme color: #1976d2 (Material UI primary)

### Path Aliases
- `@/*` maps to `src/*` for cleaner imports

### TypeScript Configuration
- Strict mode enabled with comprehensive linting
- ES2020 target with DOM libraries
- Bundler module resolution
- React JSX transform

### Development Notes

**Demo Credentials**:
- Staff: ID 1001-7999, any 4-digit PIN
- Manager: ID 8001-8999, any 4-digit PIN  
- Admin: ID 9000+, any 4-digit PIN

**Time Calculations**:
- Intelligent event pairing handles unpaired events
- Cross-day shifts supported
- Anomaly detection for invalid durations
- Break time subtracted from work time

**CSV Export**:
- Manager attendance reports in `src/utils/csv.ts`
- Includes work hours, break hours, net hours, and anomalies

**Connection Testing**:
- Backend availability tested via lightweight `/api/Staffs?limit=1` endpoint
- 5-second timeout with retry logic
- Status considered "connected" if HTTP status < 600 (even 404/500 means server reachable)
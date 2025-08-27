# Demo Mode Configuration

## Overview

This application supports intelligent mode switching:

- **Auto Demo Mode**: Automatically detects backend connection, uses mock data when offline
- **Production Mode**: Forces use of real backend API
- **Demo Mode**: Forces use of mock data

## Smart Mode Switching (Recommended)

### Auto Detection Mode (Default)

In `src/config/demo.ts`:

```typescript
export const DEMO_MODE = true // Enable auto detection
```

**How it works**:

- Automatically detects backend connection on app startup
- Re-checks every 30 seconds
- Detects immediately on network status changes
- Checks when window regains focus
- Backend connection success → automatically switches to production mode
- Backend connection failure → automatically uses demo data

## Manual Mode Switching

### Force Production Mode

```typescript
export const DEMO_MODE = false // Always use real API
```

### Force Demo Mode

```typescript
export const DEMO_MODE = true // Always use mock data
// And return false directly in checkBackendConnection
```

## Demo Mode Features

### 1. Page Alerts

Each page displays a blue information alert indicating demo mode is active.

### 2. Mock Data

- **Dashboard**: Simulated device and staff data
- **Station**: Locally stored clock-in events
- **Roster**: Example scheduling data
- **Attendance**: Simulated attendance events
- **Payslips**: Simulated payroll calculation data
- **Admin Pages**: Simulated staff, device, and event management data

### 3. Status Indicators

- **BackendStatus**: Shows backend connection status (Demo Mode/Connected/Offline)
- **NetworkBadge**: Shows network sync status

## Production Mode Features

### 1. No Alerts

All demo alerts are automatically hidden.

### 2. API Calls

- Enables real API calls
- Connects to `https://flindersdevops.azurewebsites.net`
- Uses React Query for data management

### 3. Offline Sync

Complete offline event queue and sync functionality.

## Development Recommendations

1. **Development Phase**: Keep `DEMO_MODE = true` for UI testing
2. **API Testing**: Set `DEMO_MODE = false` to test backend connection
3. **Production Deployment**: Ensure `DEMO_MODE = false`

## Technical Details

### Backend Detection Mechanism

- **Endpoint**: `GET /api/Staffs?limit=1`
- **Timeout**: 5 seconds
- **Detection Frequency**: 30 seconds
- **Trigger Conditions**: App startup, network changes, window focus, manual detection

### Status Indicators

- **Top Bar Right**: BackendStatus + NetworkBadge
- **Page Top**: DemoAlert (only shown in demo mode)
- **Click Detection**: Click BackendStatus for manual detection

### Auto Switch Logic

```
DEMO_MODE = true:
  ├── Backend connection success → Production mode (no alerts)
  └── Backend connection failure → Demo mode (show alerts)

DEMO_MODE = false:
  └── Force production mode (regardless of connection status)
```

## Environment Variables

- `VITE_API_BASE_URL`: Backend API address
- Default: `https://flindersdevops.azurewebsites.net`

## Usage Recommendations

1. **Development Phase**: Use auto detection mode (`DEMO_MODE = true`)
2. **Demo Presentation**: No configuration needed, app adapts automatically
3. **Production Deployment**: Ensure backend is accessible, app switches automatically
4. **Offline Demo**: Demo data displays automatically when backend unavailable

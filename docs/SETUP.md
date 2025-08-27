# Assignment Frontend - Setup Guide

## Quick Setup Instructions

### 1. Install Dependencies
```bash
npm install
```

### 2. Create PWA Icons (Optional)
Create these icon files in the `public/` folder for PWA functionality:
- `pwa-192x192.png` - 192x192 pixel icon
- `pwa-512x512.png` - 512x512 pixel icon

Or use any PNG images with appropriate dimensions.

### 3. Environment Configuration
The `.env` file has been created with default settings:
```
VITE_API_BASE_URL=https://flindersdevops.azurewebsites.net
```

You can modify this URL if needed.

### 4. Start Development Server
```bash
npm run dev
```

The application will be available at `http://localhost:3000`

### 5. Demo Login Credentials

| Role | Staff ID | PIN | Example Login |
|------|----------|-----|---------------|
| Staff | 1001-7999 | Any 4 digits | ID: 1234, PIN: 0000 |
| Manager | 8001-8999 | Any 4 digits | ID: 8001, PIN: 1234 |
| Admin | 9001+ | Any 4 digits | ID: 9001, PIN: 9999 |

### 6. Key Features to Test

#### Staff Features
- Login as staff (ID: 1234, PIN: 0000)
- Navigate to `/station` to test clock in/out functionality
- Test offline mode by disconnecting internet and clocking in/out
- Check `/roster` for schedule view

#### Manager Features  
- Login as manager (ID: 8001, PIN: 1234)
- View `/manager` dashboard for overview
- Check `/manager/attendance` for attendance reports
- Test CSV export functionality
- Review `/manager/payslips` for payroll calculations

#### Admin Features
- Login as admin (ID: 9001, PIN: 9999)  
- Access `/admin/staffs` for staff management

### 7. Building for Production
```bash
npm run build
```

Deploy the `dist/` folder to any static web server.

### 8. Offline Testing
1. Start the development server
2. Open browser dev tools → Network tab
3. Enable "Offline" mode
4. Try clocking in/out - events should be queued
5. Disable offline mode - events should auto-sync

## Architecture Overview

```
Frontend (React + TS) ←→ REST API (Swagger/OAS 3.0)
         ↓
   IndexedDB (Offline Storage)
         ↓ 
   Auto-sync when online
```

## Key Files Created

- **API Integration**: `src/api/client.ts`, `src/api/http.ts`
- **Offline Support**: `src/offline/db.ts`, `src/offline/sync.ts`  
- **Time Calculations**: `src/utils/time.ts`
- **CSV Export**: `src/utils/csv.ts`
- **Main Components**: `src/components/AppShell.tsx`, `src/components/NetworkBadge.tsx`
- **Core Pages**: `src/pages/Station.tsx`, `src/pages/ManagerDashboard.tsx`, etc.

## Next Steps for Production

1. **Add proper authentication** - Replace mock PIN system
2. **Implement full CRUD** - Complete admin interfaces for devices/events  
3. **Add more payroll features** - Tax calculations, overtime, deductions
4. **Enhanced reporting** - More detailed analytics and charts
5. **Push notifications** - For schedule changes, reminders
6. **Biometric integration** - Connect with actual hardware devices
7. **Error handling** - More robust error states and recovery
8. **Testing** - Unit tests, integration tests, E2E tests
9. **Security** - Input validation, CSP, secure headers
10. **Performance** - Code splitting, lazy loading, caching strategies

The application is now ready for development and testing! 
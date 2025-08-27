# Assignment Frontend - Attendance & Payroll Management System

A modern, offline-first Progressive Web App (PWA) built with React 18, TypeScript, and Material UI for managing employee attendance, scheduling, and payroll.

## Features

### 🚀 Core Technologies
- **React 18** with TypeScript
- **Material UI (MUI v5)** for modern, touch-friendly UI
- **React Router v6** for navigation
- **React Query** (@tanstack/react-query) for data management
- **Vite** for fast development and building
- **PWA** support with offline capabilities

### 📱 User Roles & Features

#### Staff/Employee
- **Clock Station**: Clock in/out, start/end breaks with large, touch-friendly buttons
- **Offline Support**: Continue clocking even when offline - events sync automatically when connection is restored
- **Work Schedule**: View upcoming shifts and schedule (demo data)
- **Status Display**: Real-time work status (clocked in/out, on break)

#### Manager
- **Dashboard**: Overview of active staff, device status, and real-time metrics
- **Attendance Management**: View, analyze, and export employee attendance data
- **Work Hours Calculation**: Automatic calculation of work hours, break hours, and net hours
- **Anomaly Detection**: Identify and report attendance irregularities
- **CSV Export**: Export attendance reports for payroll processing

#### Administrator (Placeholder)
- Staff management interface
- Device management interface  
- Event management and audit trail

### 🔧 Technical Features
- **Offline-First**: Works without internet connection using IndexedDB
- **Auto-Sync**: Offline events automatically sync when connection is restored
- **Real-time Updates**: Live status updates and network monitoring
- **Responsive Design**: Works on desktop, tablet, and mobile devices
- **PWA**: Can be installed as an app on mobile devices and desktops

## Quick Start

### Prerequisites
- Node.js 18+ 
- npm or yarn

### Installation

1. **Clone and install dependencies:**
```bash
git clone <repository-url>
cd assignment-frontend
npm install
```

2. **Start development server:**
```bash
npm run dev
```

3. **Open in browser:**
```
http://localhost:3000
```

### Environment Configuration

Create a `.env` file in the project root (optional):
```
VITE_API_BASE_URL=https://flindersdevops.azurewebsites.net
```

Default API base URL is `https://flindersdevops.azurewebsites.net` if not specified.

## Demo Credentials

The app uses a simple authentication system for demonstration:

| Role | Staff ID Range | PIN | Example |
|------|---------------|-----|---------|
| **Staff** | 1001-7999 | Any 4-digit PIN | ID: 1234, PIN: 0000 |
| **Manager** | 8001-8999 | Any 4-digit PIN | ID: 8001, PIN: 1234 |
| **Admin** | 9000+ | Any 4-digit PIN | ID: 9001, PIN: 9999 |

## API Integration

The app integrates with the following REST API endpoints:

### Base URL
`https://flindersdevops.azurewebsites.net/api`

### Endpoints
- `GET|POST /Biometric` - Biometric data management
- `GET|POST|PUT|DELETE /Devices` - Clock device management  
- `GET|POST|PUT|DELETE /Events` - Attendance events (clock in/out)
- `GET|POST|PUT|DELETE /Staffs` - Employee management

See `/src/api/client.ts` for full API integration details.

## Project Structure

```
src/
├── api/                    # API client and HTTP configuration
│   ├── http.ts            # Axios setup with interceptors
│   └── client.ts          # Typed API SDK for all endpoints
├── types/                 # TypeScript type definitions
│   └── api.ts             # API interfaces and types
├── offline/               # Offline functionality
│   ├── db.ts              # IndexedDB setup and operations
│   └── sync.ts            # Offline queue and sync logic
├── utils/                 # Utility functions
│   ├── csv.ts             # CSV export functionality
│   └── time.ts            # Time calculations and event pairing
├── components/            # Reusable UI components
│   ├── AppShell.tsx       # Main app layout with navigation
│   ├── NetworkBadge.tsx   # Network status indicator
│   ├── ExportCsvButton.tsx# CSV export button
│   ├── DateRangePicker.tsx# Date range selector
│   └── ConfirmDialog.tsx  # Confirmation dialog
├── pages/                 # Application pages
│   ├── Login.tsx          # Authentication page
│   ├── Station.tsx        # Employee clock station
│   ├── Roster.tsx         # Work schedule (demo)
│   ├── ManagerDashboard.tsx # Manager overview
│   └── ManagerAttendance.tsx # Attendance management
├── App.tsx                # Main app with routing
└── main.tsx               # Application entry point
```

## Key Features Details

### Offline Support
- Uses **IndexedDB** for local storage of offline events
- Automatic background sync when connection is restored
- Visual indicators for network status and pending sync items
- Queue management with retry logic

### Time Calculation Engine
- Intelligent pairing of clock-in/out and break events
- Handles cross-day events and multiple break periods
- Anomaly detection for unpaired events or invalid durations
- Accurate work hour calculations for payroll

### PWA Capabilities
- Service worker for offline functionality
- App manifest for installation on devices
- Cached API responses for offline browsing
- Background sync for pending events

## Development

### Available Scripts
```bash
npm run dev        # Start development server
npm run build      # Build for production
npm run preview    # Preview production build
npm run lint       # Run ESLint
npm run lint:fix   # Fix ESLint issues
npm run format     # Format code with Prettier
```

### Building for Production
```bash
npm run build
```

The built app will be in the `dist/` folder, ready for deployment to any static hosting service (IIS, Apache, Nginx, Netlify, Vercel, etc.).

### Code Quality
- **ESLint** for code linting
- **Prettier** for code formatting
- **TypeScript** for type safety
- **Material UI** for consistent design system

## Deployment

### Windows Server / IIS
1. Build the application: `npm run build`
2. Copy `dist/` folder contents to IIS website directory
3. Configure URL rewriting for SPA routing
4. Set up HTTPS (recommended for PWA features)

### Other Platforms
The built application is a static website and can be deployed to:
- Netlify
- Vercel
- GitHub Pages
- AWS S3 + CloudFront
- Any web server with static file hosting

## Browser Support

- **Chrome/Edge** 88+ (recommended)
- **Firefox** 85+
- **Safari** 14+
- **Mobile browsers** with PWA support

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

## License

This project is proprietary software. All rights reserved.

## Support

For technical support or questions about this application, please contact the development team.

---

**Built with ❤️ using React, TypeScript, and Material UI** 
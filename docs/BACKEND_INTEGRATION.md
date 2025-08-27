# Backend Integration Guide

## Overview

This document provides comprehensive guidance for backend developers on how to integrate with the Assignment Frontend application. The frontend is built with React, TypeScript, and Material-UI, featuring intelligent mode switching between demo and production modes.

## Architecture Overview

The frontend follows a modular architecture with clear separation of concerns:

- **API Layer**: Centralized HTTP client and API functions
- **Smart Mode Switching**: Automatic detection between demo and production modes
- **Offline Support**: Event queuing and synchronization when offline
- **PWA Features**: Service worker for caching and offline functionality
- **Type Safety**: Full TypeScript implementation with strict typing

## Base Configuration

### Environment Variables

The frontend uses the following environment variable for backend configuration:

```bash
VITE_API_BASE_URL=https://flindersdevops.azurewebsites.net
```

**Default Fallback**: If not set, defaults to `https://flindersdevops.azurewebsites.net`

### HTTP Client Configuration

The frontend uses Axios with the following configuration:

```typescript
// Base configuration
{
  baseURL: import.meta.env.VITE_API_BASE_URL || 'https://flindersdevops.azurewebsites.net',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  }
}
```

**Key Features**:

- 10-second request timeout
- Automatic JSON content-type headers
- Built-in error handling for offline scenarios
- Response/request interceptors ready for authentication

## API Endpoints

The frontend expects the following RESTful API endpoints:

### 1. Staff Management (`/api/Staffs`)

#### Get All Staffs

```
GET /api/Staffs?query={searchTerm}
```

- **Query Parameters**:
  - `query` (optional): Search term for filtering staff
  - `limit` (optional): Used for connection testing
- **Response**: Array of Staff objects

#### Get Staff by ID

```
GET /api/Staffs/{id}
```

- **Response**: Single Staff object

#### Create Staff

```
POST /api/Staffs
```

- **Body**: Staff object (without staffId)
- **Response**: Created Staff object with staffId

#### Update Staff

```
PUT /api/Staffs/{id}
```

- **Body**: Partial Staff object
- **Response**: 204 No Content

#### Delete Staff

```
DELETE /api/Staffs/{id}
```

- **Response**: 204 No Content

### 2. Device Management (`/api/Devices`)

#### Get All Devices

```
GET /api/Devices?query={searchTerm}
```

- **Query Parameters**:
  - `query` (optional): Search term for filtering devices
- **Response**: Array of Device objects

#### Get Device by ID

```
GET /api/Devices/{id}
```

- **Response**: Single Device object

#### Create Device

```
POST /api/Devices
```

- **Body**: Device object (without deviceId)
- **Response**: Created Device object with deviceId

#### Update Device

```
PUT /api/Devices/{id}
```

- **Body**: Partial Device object
- **Response**: 204 No Content

#### Delete Device

```
DELETE /api/Devices/{id}
```

- **Response**: 204 No Content

### 3. Event Management (`/api/Events`)

#### Get All Events

```
GET /api/Events?staffId={id}&deviceId={id}&adminId={id}&type={type}&from={date}&to={date}
```

- **Query Parameters** (all optional):
  - `staffId`: Filter by staff ID
  - `deviceId`: Filter by device ID
  - `adminId`: Filter by admin ID
  - `type`: Filter by event type
  - `from`: Start date filter (ISO string)
  - `to`: End date filter (ISO string)
- **Response**: Array of Event objects

#### Get Event by ID

```
GET /api/Events/{id}
```

- **Response**: Single Event object

#### Create Event

```
POST /api/Events
```

- **Body**: Event object (without eventId)
- **Response**: Created Event object with eventId

#### Update Event

```
PUT /api/Events/{id}
```

- **Body**: Partial Event object
- **Response**: 204 No Content

#### Delete Event

```
DELETE /api/Events/{id}
```

- **Response**: 204 No Content

### 4. Biometric Management (`/api/Biometric`)

#### Get All Biometric Data

```
GET /api/Biometric?staffId={id}
```

- **Query Parameters**:
  - `staffId` (optional): Filter by staff ID
- **Response**: Array of BiometricData objects

#### Get Biometric by ID

```
GET /api/Biometric/{id}
```

- **Response**: Single BiometricData object

#### Create Biometric Data

```
POST /api/Biometric
```

- **Body**: BiometricData object (without biometricId)
- **Response**: Created BiometricData object with biometricId

#### Update Biometric Data

```
PUT /api/Biometric/{id}
```

- **Body**: Partial BiometricData object
- **Response**: 204 No Content

#### Delete Biometric Data

```
DELETE /api/Biometric/{id}
```

- **Response**: 204 No Content

## Data Structures

### Staff Object

```typescript
interface Staff {
  staffId: number
  firstName: string
  lastName: string
  email: string
  phone: string
  address: string
  contractType: string
  isActive: boolean
  role: string
  standardHoursPerWeek: number
  standardPayRate: number
  overtimePayRate: number
}
```

### Device Object

```typescript
interface Device {
  deviceId: number
  location: string
  type: string
  status: string
}
```

### Event Object

```typescript
interface Event {
  eventId: number
  staffId: number
  timeStamp: string // ISO 8601 format
  eventType: string // 'IN' | 'OUT' | 'BREAK_START' | 'BREAK_END' | 'OTHER'
  reason: string
  deviceId: number
  adminId: number
}
```

### BiometricData Object

```typescript
interface BiometricData {
  biometricId: number
  staffId: number
  template: string
}
```

## Backend Connection Detection

The frontend automatically detects backend availability using the following mechanism:

### Connection Check Endpoint

```
GET /api/Staffs?limit=1
```

**Purpose**: Lightweight endpoint to test backend connectivity
**Timeout**: 5 seconds
**Frequency**: Every 30 seconds
**Triggers**:

- App startup
- Network status changes
- Window focus events
- Manual user-triggered checks

### Connection Logic

```typescript
// The frontend considers backend "connected" if:
// 1. HTTP response status < 600 (even 404/500 means server is reachable)
// 2. Response received within 5 seconds
// 3. No network/timeout errors

const isConnected = response.status !== 0 && response.status < 600
```

## Smart Mode Switching

The frontend operates in two modes based on backend availability:

### Demo Mode (Backend Unavailable)

- Shows blue info alerts on each page
- Uses mock data for all operations
- Events stored locally in IndexedDB
- Automatically queues events for later synchronization

### Production Mode (Backend Available)

- Hides all demo alerts
- Makes real API calls to backend
- Enables full offline synchronization
- Uses React Query for data management

### Configuration

In `src/config/demo.ts`:

```typescript
export const DEMO_MODE = true // Enable auto-detection
// Set to false to force production mode regardless of connectivity
```

## Offline Functionality

### Event Queuing

When the backend is unavailable, the frontend:

1. Stores events locally in IndexedDB
2. Displays queue status to users
3. Automatically attempts sync when connection restored

### Sync Mechanism

```typescript
// Events are synced in FIFO order
// Sync stops on first failure to maintain chronological order
// Successful events are removed from queue
// Failed events remain for retry
```

### Sync Triggers

- Network connection restored (`online` event)
- Manual sync via UI
- Periodic checks when online

## Authentication & Session Management

### Current Implementation

The frontend uses a simple PIN-based authentication system:

```typescript
interface CurrentUser {
  staffId: number
  firstName: string
  lastName: string
  role: 'staff' | 'manager' | 'admin'
  pin: string
}
```

### Login Flow

1. User enters Staff ID and 4-digit PIN
2. Frontend calls `GET /api/Staffs/{id}` to fetch staff details
3. If API fails, creates mock user for demo purposes
4. Role determined by Staff ID range:
   - `>= 9000`: admin
   - `>= 8000`: manager
   - `< 8000`: staff
5. User stored in localStorage for persistence

### Role-Based Navigation

- **Staff**: Redirected to `/station` (clock in/out)
- **Manager**: Redirected to `/manager` (dashboard)
- **Admin**: Redirected to `/admin/staffs` (administration)

### Authentication Headers (Ready for Implementation)

The HTTP client has prepared interceptors for JWT tokens:

```typescript
// Uncomment and implement when backend supports authentication
// const token = localStorage.getItem('authToken')
// if (token) {
//   config.headers.Authorization = `Bearer ${token}`
// }
```

## Error Handling

### Network Errors

- Automatic offline detection
- Graceful degradation to demo mode
- User-friendly error messages
- Automatic retry mechanisms

### API Errors

- Structured error responses expected
- Fallback to mock data when appropriate
- Error logging for debugging
- Non-blocking error handling

### Error Response Format

The frontend expects standard HTTP status codes:

- `200-299`: Success
- `400-499`: Client errors (validation, not found, etc.)
- `500-599`: Server errors
- `>= 600`: Connection/network issues

## CORS Configuration

Ensure your backend allows the frontend domain:

```javascript
// Example CORS configuration
{
  origin: [
    'http://localhost:3000',  // Development
    'https://your-frontend-domain.com'  // Production
  ],
  methods: ['GET', 'POST', 'PUT', 'DELETE'],
  allowedHeaders: ['Content-Type', 'Authorization'],
  credentials: true  // If using authentication cookies
}
```

## PWA & Caching

The frontend implements PWA features with API caching:

### Service Worker Configuration

```javascript
// API responses cached for 24 hours
urlPattern: /^https:\/\/flindersdevops\.azurewebsites\.net\/api\//
handler: 'NetworkFirst'
maxAgeSeconds: 60 * 60 * 24 // 24 hours
```

### Cache Strategy

- **NetworkFirst**: Always try network, fallback to cache
- **Cached Responses**: GET requests cached for offline access
- **Cache Invalidation**: Automatic on successful network requests

## Development & Testing

### Local Development

```bash
# Frontend development server
npm run dev  # Runs on http://localhost:3000

# Environment variables
VITE_API_BASE_URL=http://localhost:5000  # Point to local backend
```

### Testing Backend Integration

1. Set `DEMO_MODE = false` in `src/config/demo.ts`
2. Ensure backend is running and accessible
3. Check browser network tab for API calls
4. Monitor console for connection status logs

### Connection Status Monitoring

The frontend provides real-time connection status:

- **BackendStatus component**: Shows current connection state
- **NetworkBadge**: Shows sync queue status
- **Manual testing**: Click BackendStatus to force connection check

## Deployment Considerations

### Backend Requirements

- HTTPS enabled (required for PWA features)
- CORS properly configured
- Consistent API response format
- Error handling for malformed requests

### Performance Recommendations

- Implement pagination for large datasets
- Use appropriate HTTP status codes
- Enable compression (gzip/brotli)
- Set proper cache headers for static content

### Security Considerations

- Validate all input data
- Implement rate limiting
- Use HTTPS for all communications
- Sanitize error messages (don't expose internal details)

## Troubleshooting

### Common Issues

1. **CORS Errors**
   - Check browser console for CORS messages
   - Verify backend CORS configuration
   - Ensure preflight requests are handled

2. **Connection Detection Fails**
   - Verify `/api/Staffs` endpoint exists and responds
   - Check network timeout settings
   - Monitor browser network tab during connection checks

3. **Data Not Loading**
   - Check API response format matches expected interfaces
   - Verify HTTP status codes are correct
   - Ensure JSON content-type headers are set

4. **Offline Sync Issues**
   - Check browser IndexedDB for queued events
   - Verify event creation endpoint accepts expected format
   - Monitor sync status in NetworkBadge component

### Debug Tools

- Browser Developer Tools (Network, Console, Application tabs)
- React Developer Tools
- Backend connection status in top-right corner
- Demo alerts show current mode status

## Contact & Support

For integration questions or issues:

1. Check browser console for detailed error messages
2. Verify API endpoints match expected format
3. Test with demo mode disabled for production API calls
4. Monitor network requests in browser developer tools

The frontend is designed to be resilient and will gracefully handle backend unavailability while providing clear feedback to users about the current operational mode.

# Frontend API Connection Documentation

> **Note**: This document focuses on frontend implementation details. For architectural integration guidance, see [`/docs/architecture/BACKEND_INTEGRATION.md`](../../docs/architecture/BACKEND_INTEGRATION.md).

## Technology Used

Frontend uses **Axios** to connect to backend API

## Configuration Location

**File Location**: `src/api/http.ts`

```javascript
const httpClient = axios.create({
  baseURL: 'http://localhost:4000',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})
```

## Backend API Address

**Default Address**: http://localhost:4000

**Environment Variable**: Can be modified through `VITE_API_BASE_URL` environment variable

## Frontend API Endpoints

**File Location**: `src/api/client.ts`

### Staff Management

- `GET /api/Staffs` - Get staff list
- `GET /api/Staffs/{id}` - Get single staff member
- `POST /api/Staffs` - Create staff member
- `PUT /api/Staffs/{id}` - Update staff member
- `DELETE /api/Staffs/{id}` - Delete staff member

### Device Management

- `GET /api/Devices` - Get device list
- `GET /api/Devices/{id}` - Get single device
- `POST /api/Devices` - Create device
- `PUT /api/Devices/{id}` - Update device
- `DELETE /api/Devices/{id}` - Delete device

### Clock Events

- `GET /api/Events` - Get event list
- `GET /api/Events/{id}` - Get single event
- `POST /api/Events` - Create event
- `PUT /api/Events/{id}` - Update event
- `DELETE /api/Events/{id}` - Delete event

### Biometric Data

- `GET /api/Biometric` - Get biometric data list
- `GET /api/Biometric/{id}` - Get single biometric data
- `POST /api/Biometric` - Create biometric data
- `PUT /api/Biometric/{id}` - Update biometric data
- `DELETE /api/Biometric/{id}` - Delete biometric data

## Request Configuration

- **Timeout**: 10 seconds
- **Headers**: Content-Type: application/json
- **Error Handling**: Automatic handling of network errors and HTTP status code errors

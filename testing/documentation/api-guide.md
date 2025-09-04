# API Testing Guide

## Base Configuration

### Connection Details
- **Base URL:** `https://comp9034farmsystem-backend.onrender.com`
- **Authentication:** Bearer JWT Token
- **Content-Type:** `application/json`
- **Timeout:** 10 seconds (60 seconds for first request after sleep)

### Database Testing Information
**❌ Direct Database Access NOT Available**
- **Database Type:** SQLite (containerized, planned migration to SQL Server)
- **Location:** Inside Docker container (`/app/Data/farmtimems-dev.db`)
- **Access Method:** API endpoints only (for security and isolation)
- **Why No Direct Access:** 
  - Container filesystem isolation
  - Security best practices
  - SQLite file not network-accessible
  - Data persistence through API layer testing

**✅ Database Testing Through APIs**
- **Data Persistence:** Test via CRUD operations
- **Data Integrity:** Verify foreign key constraints
- **Transaction Testing:** Confirm atomic operations
- **Performance:** Monitor API response times
- **See:** `documentation/database-testing.md` for detailed database testing guide

## Authentication Endpoints

### POST /api/auth/pin-login
**Purpose:** Staff authentication using PIN
**Method:** POST
**URL:** `/api/auth/pin-login`
**Headers:** `Content-Type: application/json`

**Request Body:**
```json
{
  "staffId": 9001,
  "pin": "1234"
}
```

**Success Response (200):**
```json
{
  "token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...",
  "refreshToken": "abc123def456...",
  "expiresAt": "2025-09-04T10:00:00Z",
  "staff": {
    "id": 9001,
    "name": "System Administrator",
    "role": "admin",
    "email": "admin@farmtimems.com"
  }
}
```

**Error Response (401):**
```json
{
  "error": "Invalid staff ID or PIN"
}
```

### GET /api/auth/login-logs
**Purpose:** Retrieve login history (Admin access required)
**Method:** GET
**URL:** `/api/auth/login-logs`
**Headers:** `Authorization: Bearer {jwt_token}`

**Query Parameters:**
- `page` (optional): Page number, default 1
- `pageSize` (optional): Records per page, default 10

**Success Response (200):**
```json
[
  {
    "id": 1,
    "username": "admin",
    "ipAddress": "192.168.1.100",
    "success": true,
    "failureReason": null,
    "timestamp": "2025-09-04T01:30:00Z",
    "staffId": 9001,
    "staffName": "System Administrator"
  }
]
```

## Staff Management Endpoints

### GET /api/staffs
**Purpose:** Get all active staff members
**Method:** GET
**URL:** `/api/staffs`
**Headers:** `Authorization: Bearer {jwt_token}`
**Access:** Admin and Manager roles

**Success Response (200):**
```json
[
  {
    "id": 9001,
    "name": "System Administrator",
    "role": "admin",
    "email": "admin@farmtimems.com",
    "hourlyRate": 50.00,
    "isActive": true,
    "createdAt": "2025-09-04T00:00:00Z"
  }
]
```

### GET /api/staffs/{id}
**Purpose:** Get specific staff member details
**Method:** GET
**URL:** `/api/staffs/{staffId}`
**Headers:** `Authorization: Bearer {jwt_token}`
**Access:** Admin only

## Event Management Endpoints

### GET /api/events
**Purpose:** Get attendance events
**Method:** GET  
**URL:** `/api/events`
**Headers:** `Authorization: Bearer {jwt_token}`
**Access:** Admin and Manager roles

### POST /api/events
**Purpose:** Create new attendance event
**Method:** POST
**URL:** `/api/events`
**Headers:** 
- `Authorization: Bearer {jwt_token}`
- `Content-Type: application/json`

**Request Body:**
```json
{
  "staffId": 1001,
  "deviceId": 1,
  "eventType": "IN",
  "notes": "Regular check-in"
}
```

### GET /api/events/staff/{staffId}
**Purpose:** Get events for specific staff member
**Method:** GET
**URL:** `/api/events/staff/{staffId}`
**Headers:** `Authorization: Bearer {jwt_token}`
**Access:** Admin, Manager, or own data only

## Device Management Endpoints

### GET /api/devices
**Purpose:** Get all devices
**Method:** GET
**URL:** `/api/devices`
**Headers:** `Authorization: Bearer {jwt_token}`
**Access:** All authenticated users

**Success Response (200):**
```json
[
  {
    "id": 1,
    "name": "Main Terminal",
    "type": "terminal",
    "location": "Main Entrance",
    "status": "active",
    "ipAddress": "192.168.1.100"
  }
]
```

## Health Check Endpoint

### GET /health
**Purpose:** Service health verification
**Method:** GET
**URL:** `/health`
**Headers:** None required
**Access:** Public

**Success Response (200):**
```json
{
  "status": "healthy",
  "timestamp": "2025-09-04T02:00:00Z",
  "version": "1.0.0",
  "environment": "Production"
}
```

## Error Responses

### Common HTTP Status Codes
- **200 OK:** Request successful
- **401 Unauthorized:** Invalid or missing JWT token
- **403 Forbidden:** Insufficient permissions
- **404 Not Found:** Resource not found
- **500 Internal Server Error:** Server error

### JWT Token Handling
- **Token Expiry:** 8 hours from issue time
- **Refresh:** Re-login required after expiry
- **Header Format:** `Authorization: Bearer {token}`

## Testing Tips

### Performance Testing
- **Cold Start:** First request may take 30-60 seconds
- **Normal Response:** Should be under 2 seconds
- **Concurrent Users:** Recommended maximum 5 simultaneous users

### Data Validation
- **Test Data Persistence:** Basic test accounts and devices remain constant
- **Event Data:** New events are persistent across sessions
- **Login Logs:** All login attempts are recorded

---

📋 **Document Version:** 2.0.0  
📅 **Last Updated:** 2025-09-04  
🔄 **Sync with:** API v1.0.0
# Database Testing Guide

## 🔍 Database Testing Approach

### ❌ Direct Database Access NOT Provided

**Why direct database connection is not available:**

1. **Container Isolation**
   - SQLite file is located inside Docker container: `/app/Data/farmtimems-dev.db`
   - Container filesystem is not accessible from external systems
   - Database file is ephemeral and recreated on each deployment

2. **Security Isolation**
   - Direct database access poses security risks
   - Test teams should not have production database credentials
   - API layer provides controlled and secure data access

3. **Platform Limitations**
   - Render free tier does not expose database ports
   - SQLite is file-based, not network-accessible like PostgreSQL/MySQL
   - Container restarts reset all local file storage

4. **Best Practices**
   - API testing is the industry standard approach
   - Database should be treated as black-box for external testing
   - Data integrity testing through application layer is more realistic

## ✅ How to Test Database Functionality

### Database Testing Through API Layer

#### 1. **Data Persistence Testing**
```bash
# Test data persistence across requests
# Step 1: Create an event
curl -X POST "$BASE_URL/api/events" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"staffId": 1001, "deviceId": 1, "eventType": "IN", "notes": "Persistence test"}'

# Step 2: Retrieve events to verify storage
curl -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/events"

# Step 3: Restart service (or wait for sleep/wake cycle)
# Step 4: Verify data still exists
curl -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/events"
```

#### 2. **Data Integrity Testing**
```bash
# Test referential integrity
# Create event with invalid staff ID
curl -X POST "$BASE_URL/api/events" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"staffId": 99999, "deviceId": 1, "eventType": "IN"}'
# Expected: Error response (referential integrity maintained)

# Create event with invalid device ID  
curl -X POST "$BASE_URL/api/events" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"staffId": 1001, "deviceId": 99999, "eventType": "IN"}'
# Expected: Error response (foreign key constraint)
```

#### 3. **Transaction Testing**
```bash
# Test atomic operations
# Login and immediately check if login log is created
curl -X POST "$BASE_URL/api/auth/pin-login" \
  -H "Content-Type: application/json" \
  -d '{"staffId": 9001, "pin": "1234"}'

# Then check login logs to verify transaction completed
curl -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/auth/login-logs"
```

#### 4. **Concurrent Access Testing**
```bash
# Test multiple simultaneous requests
# Run these commands in parallel (different terminals)
curl -H "Authorization: Bearer $TOKEN1" "$BASE_URL/api/staffs" &
curl -H "Authorization: Bearer $TOKEN2" "$BASE_URL/api/events" &
curl -H "Authorization: Bearer $TOKEN3" "$BASE_URL/api/devices" &
wait
# Verify all requests complete successfully
```

## 📊 Database Schema Understanding

### Core Tables and Relationships
```
Staffs (Primary Entity)
├── id: Primary Key
├── name, email, role: User information
├── pin: Authentication credential
└── hourlyRate: Payroll information

Events (Attendance Records)  
├── id: Primary Key
├── staffId: Foreign Key → Staffs.id
├── deviceId: Foreign Key → Devices.id
├── eventType: IN/OUT/BREAK_START/BREAK_END
└── timestamp: Event occurrence time

Devices (Check-in Points)
├── id: Primary Key  
├── name: Device identifier
├── location: Physical location
└── status: active/inactive

LoginLogs (Audit Trail)
├── id: Primary Key
├── staffId: Foreign Key → Staffs.id
├── success: boolean
├── timestamp: Login attempt time
└── ipAddress: Source IP
```

### Data Validation Rules
- **Staff ID:** Must be unique, 4-digit format preferred
- **PIN:** 4-digit string, unique per staff
- **Event Type:** Must be one of: IN, OUT, BREAK_START, BREAK_END
- **Email:** Must be unique if provided
- **Timestamps:** Stored in UTC format

## 🧪 Database Testing Scenarios

### 1. CRUD Operations Testing

#### Create Operations
- **Test:** Create new event record
- **API:** `POST /api/events`
- **Validation:** Event appears in `GET /api/events`

#### Read Operations  
- **Test:** Retrieve staff information
- **API:** `GET /api/staffs`, `GET /api/staffs/{id}`
- **Validation:** Correct data format and content

#### Update Operations
- **Test:** Modify staff information (if endpoint available)
- **API:** `PUT /api/staffs/{id}` 
- **Validation:** Changes persist across requests

#### Delete Operations
- **Note:** Currently not exposed for security reasons
- **Alternative:** Test soft delete via status flags

### 2. Data Consistency Testing

#### Referential Integrity
```bash
# Test foreign key constraints
# Invalid staff ID in event creation should fail
curl -X POST "$BASE_URL/api/events" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"staffId": 99999, "deviceId": 1, "eventType": "IN"}'
# Expected: 400 Bad Request or similar error
```

#### Data Type Validation
```bash
# Test with invalid data types
curl -X POST "$BASE_URL/api/events" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"staffId": "invalid", "deviceId": 1, "eventType": "IN"}'
# Expected: Validation error response
```

### 3. Performance Testing

#### Query Performance
```bash
# Time API response for database queries
time curl -s -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/staffs"
# Expected: Response time < 2 seconds (after service wake-up)
```

#### Concurrent Operations
```bash
# Test database handles concurrent requests
for i in {1..5}; do
  curl -s -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/events" &
done
wait
# Expected: All requests complete successfully
```

## 📈 Database Monitoring

### Available Metrics Through API
- **Record Counts:** Count of staffs, events, devices
- **Data Integrity:** Foreign key relationships maintained
- **Performance:** Response times for database operations
- **Audit Trail:** Login logs and system operations

### Health Indicators
```bash
# Check database connectivity through health endpoint
curl "$BASE_URL/health"
# Should include database status information

# Verify core data exists
curl -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/staffs"
# Should return at least the 3 test accounts
```

## 🔧 Troubleshooting Database Issues

### Common Database-Related Problems

#### Issue: Empty or Missing Data
**Symptoms:** API returns empty arrays or null responses
**Cause:** Database initialization failed or data not seeded
**Testing:** Check if test accounts exist via `/api/staffs`

#### Issue: Slow Database Queries
**Symptoms:** API responses taking >5 seconds
**Cause:** Cold start or database connection issues
**Testing:** Monitor response times across multiple requests

#### Issue: Data Inconsistency
**Symptoms:** Related data doesn't match (e.g., events without valid staff)
**Cause:** Database constraint violations or migration issues
**Testing:** Verify foreign key relationships through API calls

## 📝 Database Testing Checklist

### Pre-Testing Verification
- [ ] Health check endpoint responds successfully
- [ ] Test accounts are available via `/api/staffs`
- [ ] Basic authentication works (can obtain JWT token)
- [ ] Test devices exist via `/api/devices`

### Core Database Function Tests
- [ ] **Create:** Can create new events via API
- [ ] **Read:** Can retrieve existing data via API
- [ ] **Update:** Can modify data where endpoints exist
- [ ] **Delete:** Verify soft delete behavior
- [ ] **Relationships:** Foreign key constraints working
- [ ] **Validation:** Invalid data properly rejected

### Data Quality Tests
- [ ] **Consistency:** Related records maintain relationships
- [ ] **Integrity:** Data constraints are enforced
- [ ] **Persistence:** Data survives service restarts
- [ ] **Security:** Access controls prevent unauthorized data access

---

📋 **Document Version:** 2.0.0  
📅 **Last Updated:** 2025-09-04  
🔄 **Database:** SQLite (planned migration to SQL Server)  
📊 **Testing Method:** API-based indirect testing only
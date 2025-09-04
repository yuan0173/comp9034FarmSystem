# Test Scenarios and Use Cases

## 🎯 Functional Testing Scenarios

### 1. User Authentication Flow

#### 1.1 PIN Login Success Test
- **Test ID:** AUTH_001
- **Objective:** Verify successful login with valid credentials
- **Steps:**
  1. Navigate to login page: `/login`
  2. Enter Staff ID: `9001`
  3. Enter PIN: `1234`
  4. Click "Sign In"
- **Expected Result:** 
  - Redirect to appropriate dashboard based on role
  - JWT token stored locally
  - User information displayed correctly

#### 1.2 PIN Login Failure Test  
- **Test ID:** AUTH_002
- **Objective:** Verify proper error handling for invalid credentials
- **Steps:**
  1. Navigate to login page: `/login`
  2. Enter Staff ID: `9001`
  3. Enter incorrect PIN: `0000`
  4. Click "Sign In"
- **Expected Result:**
  - Error message displayed
  - No redirect occurs
  - Login attempt logged

#### 1.3 JWT Token Expiry Test
- **Test ID:** AUTH_003
- **Objective:** Verify token expiration handling
- **Steps:**
  1. Login successfully and note token expiry time
  2. Wait for token to expire (8 hours) or manually test with expired token
  3. Attempt to access protected endpoint
- **Expected Result:**
  - 401 Unauthorized response
  - Automatic redirect to login page

### 2. Role-Based Access Control

#### 2.1 Administrator Access Test
- **Test ID:** RBAC_001
- **Account:** Admin (9001/1234)
- **Accessible Features:**
  - ✅ Dashboard
  - ✅ Staff Management
  - ✅ Device Management  
  - ✅ Event Management
  - ✅ Login History
  - ✅ All CRUD operations

#### 2.2 Manager Access Test
- **Test ID:** RBAC_002  
- **Account:** Manager (8001/8001)
- **Accessible Features:**
  - ✅ Dashboard
  - ✅ Attendance Monitoring
  - ✅ Payslip Generation
  - ❌ Staff Management (view only)
  - ❌ Device Management
  - ❌ Login History

#### 2.3 Staff Access Test
- **Test ID:** RBAC_003
- **Account:** Staff (1001/1001)
- **Accessible Features:**
  - ✅ Personal Dashboard
  - ✅ Clock In/Out
  - ✅ Personal Attendance History
  - ❌ Other staff data
  - ❌ System management features

### 3. API Endpoint Testing

#### 3.1 Health Check Test
- **Test ID:** API_001
- **Method:** GET
- **Endpoint:** `/health`
- **Authentication:** None required
- **Expected Response:** 200 OK with health status

#### 3.2 Staff List Retrieval
- **Test ID:** API_002
- **Method:** GET
- **Endpoint:** `/api/staffs`
- **Authentication:** Required (Admin/Manager)
- **Expected Response:** Array of active staff members

#### 3.3 Event Creation Test
- **Test ID:** API_003
- **Method:** POST
- **Endpoint:** `/api/events`
- **Authentication:** Required
- **Request Body:**
```json
{
  "staffId": 1001,
  "deviceId": 1,
  "eventType": "IN",
  "notes": "Test clock-in event"
}
```
- **Expected Response:** 201 Created with event details

#### 3.4 Login History Retrieval (Admin Only)
- **Test ID:** API_004
- **Method:** GET
- **Endpoint:** `/api/auth/login-logs`
- **Authentication:** Required (Admin only)
- **Expected Response:** Array of login records

## 🔒 Security Testing Scenarios

### 4. Authorization Testing

#### 4.1 Unauthorized Access Test
- **Test ID:** SEC_001
- **Objective:** Verify protected endpoints reject unauthenticated requests
- **Steps:**
  1. Make API request without Authorization header
  2. Make API request with invalid/expired token
- **Expected Result:** 401 Unauthorized

#### 4.2 Role Privilege Escalation Test
- **Test ID:** SEC_002
- **Objective:** Verify role-based restrictions are enforced
- **Steps:**
  1. Login as Staff (1001/1001)
  2. Attempt to access admin-only endpoints
- **Expected Result:** 403 Forbidden

### 5. Data Validation Testing

#### 5.1 Input Validation Test
- **Test ID:** VAL_001
- **Objective:** Verify proper input validation
- **Test Cases:**
  - Empty PIN field
  - Non-numeric Staff ID
  - SQL injection attempts
  - XSS payload attempts
- **Expected Result:** Appropriate error messages, no system compromise

## 🚀 Performance Testing Scenarios

### 6. Load and Performance Tests

#### 6.1 Cold Start Performance
- **Test ID:** PERF_001
- **Objective:** Measure service wake-up time
- **Steps:**
  1. Wait for service to sleep (15+ minutes)
  2. Make first API request
  3. Measure response time
- **Expected Result:** Response within 60 seconds

#### 6.2 Normal Operation Performance  
- **Test ID:** PERF_002
- **Objective:** Measure normal API response time
- **Steps:**
  1. Ensure service is awake
  2. Make multiple API requests
  3. Measure average response time
- **Expected Result:** Response within 2 seconds

#### 6.3 Concurrent User Test
- **Test ID:** PERF_003
- **Objective:** Test multiple simultaneous users
- **Steps:**
  1. Login with different test accounts simultaneously
  2. Perform various operations concurrently
- **Expected Result:** All operations complete successfully

## 🌐 Cross-Browser Compatibility

### 7. Browser Testing Matrix

| Feature | Chrome | Firefox | Safari | Edge |
|---------|--------|---------|--------|------|
| Login Flow | ✅ | ✅ | ✅ | ✅ |
| Dashboard | ✅ | ✅ | ✅ | ✅ |
| API Calls | ✅ | ✅ | ✅ | ✅ |
| Local Storage | ✅ | ✅ | ✅ | ✅ |

### Mobile Responsiveness
- **Test ID:** UI_001
- **Devices:** iPhone, Android, Tablet
- **Features:** Login, Dashboard navigation, Core functions

## 📊 Test Data Management

### Test Data Categories
- **Static Data:** Test accounts, devices (persistent)
- **Dynamic Data:** Events, logs (accumulated during testing)
- **Temporary Data:** Session tokens (expires after 8 hours)

### Data Reset Procedures
1. **Soft Reset:** Clear browser cache and local storage
2. **Account Reset:** Contact development team if account lock occurs
3. **Full Reset:** Not typically required due to test data isolation

---

📋 **Test Plan Version:** 2.0.0  
📅 **Compatible with API:** v1.0.0  
👥 **Test Team Contact:** QA Team
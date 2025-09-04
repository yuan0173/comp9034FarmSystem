# 🧪 COMP9034 Farm Time Management System - Testing Package

## ⚠️ Important Security Notice
- This directory is for testing purposes only
- Contains no production environment sensitive information  
- Test accounts are for testing environment only
- Prohibited to use test credentials in production environment

## 🌐 Quick Access Information

### Application URLs
- **Frontend Application:** https://yuan0173.github.io/comp9034FarmSystem/
- **Backend API:** https://comp9034farmsystem-backend.onrender.com
- **Health Check:** https://comp9034farmsystem-backend.onrender.com/health

### 🔑 Test Accounts (PIN Login)

| Role | Staff ID | PIN | Email | Permission Description |
|------|----------|-----|-------|------------------------|
| Administrator | 9001 | 1234 | admin@farmtimems.com | Full system access |
| Manager | 8001 | 8001 | manager@farmtimems.com | Staff management and attendance view |
| Staff | 1001 | 1001 | worker@farmtimems.com | Personal attendance records only |

## 📁 Testing Resources Navigation

### Core Documentation
- **API Guide:** `documentation/api-guide.md` - Complete API interface documentation
- **Test Scenarios:** `documentation/test-scenarios.md` - Functional test cases
- **Troubleshooting:** `documentation/troubleshooting.md` - Common issues and solutions

### Configuration Files  
- **Environment Config:** `config/environment.json` - Testing environment parameters
- **Test Data:** `config/test-accounts.json` - Detailed account information

### Testing Tools
- **Postman Collection:** `tools/postman-collection.json` - API testing toolkit
- **Sample Scripts:** `tools/curl-examples.sh` - Command line testing examples

## 🚀 Quick 10-Step Smoke Test

**Before detailed testing, run this quick verification:**

1. ✅ **Health Check:** Visit `/health` endpoint → expect "healthy" status
2. ✅ **Admin Login:** Use Staff ID 9001, PIN 1234 → expect JWT token  
3. ✅ **Login Logs:** Access Login History page → expect 200 response
4. ✅ **Staff List:** Get `/api/staffs` → expect 3 staff members
5. ✅ **Device List:** Get `/api/devices` → expect 2 devices
6. ✅ **Create Event:** POST event with Staff 1001 → expect 201 created
7. ✅ **Event Query:** Get `/api/events` → verify new event exists
8. ✅ **Role Test:** Login as Manager (8001/8001) → limited access
9. ✅ **Permission Test:** Staff login (1001/1001) → personal data only
10. ✅ **Frontend Flow:** Complete login via UI → dashboard access

**Expected Time:** 5-10 minutes | **All steps should pass for healthy system**

## 🛠️ Prerequisites and Tools

### Required Tools for Command-Line Testing
- **curl:** HTTP client (pre-installed on most systems)
- **jq:** JSON processor for formatted output
  - **macOS:** `brew install jq`
  - **Ubuntu/Debian:** `sudo apt-get install jq`
  - **Windows:** Download from https://jqlang.github.io/jq/

### Optional Tools
- **Postman:** GUI API testing (recommended)
- **Browser Dev Tools:** Network tab for debugging
- **HTTPie:** Alternative to curl (`pip install httpie`)

## ⚠️ Important Usage Guidelines

### 🕐 Service Wake-up Mechanism
- **Free Tier Limitation:** Service automatically sleeps after 15 minutes of inactivity
- **First Access:** Requires 30-60 seconds for service wake-up
- **Recommendation:** Access health check endpoint to warm up service

### 🌐 Browser Recommendations
- **Recommended:** Use incognito/private mode for testing
- **Reason:** Avoid cache interference with test results
- **Compatibility:** Supports Chrome, Firefox, Safari

### 🔄 Test Data Information
- **Data Persistence:** Test accounts and basic data remain stable
- **Reset Mechanism:** Contact development team if test data reset is needed
- **Concurrency Limitations:** 
  - Keep concurrent testing sessions < 5 users
  - Avoid multiple simultaneous logins with same account
  - Cold start requires 30-60 seconds before concurrent testing

## 🆘 Getting Help

### Technical Support
- **Development Team:** Code and deployment related issues
- **Test Lead:** Testing process and test case questions
- **Project Manager:** Feature requirements and priority issues

### Emergency Contact
If you encounter complete service inaccessibility, please contact the development team immediately to check Render service status.

---

📋 **Version:** v2.0.0-sprint2-testing  
📅 **Last Updated:** 2025-09-04  
👥 **Maintained by:** COMP9034 Project Team
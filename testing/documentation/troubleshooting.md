# Troubleshooting Guide

## 🚨 Common Issues and Solutions

### Service Connectivity Issues

#### ❌ Issue: 504 Gateway Timeout / Network Error
**Symptoms:**
- API requests fail with network error
- Frontend shows "Network error - please check your connection"
- Status code 0 or 504

**Root Cause:** Render service is in sleep mode (free tier limitation)
**Solution:**
1. Wait 30-60 seconds for service to wake up
2. Refresh the application  
3. Try accessing health check endpoint first: `/health`

**Prevention:** Keep service active by accessing it regularly

---

#### ❌ Issue: 404 Not Found (Frontend)
**Symptoms:**
- Blank page or GitHub 404 error
- Direct access to `/login` fails
- Works from homepage but not direct links

**Root Cause:** GitHub Pages SPA routing issue
**Solution:**
1. Always start from root URL: `https://yuan0173.github.io/comp9034FarmSystem/`
2. Navigate through the application rather than direct links
3. Use incognito mode to avoid cache issues

**For Developers:** Ensure 404.html and .nojekyll are in deployment

---

### Authentication Issues

#### ❌ Issue: 401 Unauthorized
**Symptoms:**
- "signature key was not found" error
- Login succeeds but API calls fail
- Token validation errors

**Root Cause:** JWT token issues
**Solutions:**
1. **Token Expired:** Re-login to get fresh token
2. **Invalid Token:** Clear browser storage and login again
3. **Server Restart:** Backend service may have restarted with new keys

**Steps to resolve:**
1. Open browser dev tools → Application → Local Storage
2. Clear all stored tokens
3. Login again with test credentials

---

#### ❌ Issue: 403 Forbidden  
**Symptoms:**
- "Access denied" messages
- User logged in but cannot access features

**Root Cause:** Insufficient role permissions
**Solution:**
1. Verify you're using correct test account for required access level
2. Check role assignments:
   - Admin (9001): Full access
   - Manager (8001): Limited access  
   - Staff (1001): Personal data only

---

### Data and Performance Issues

#### ❌ Issue: Slow Response Times
**Symptoms:**
- API calls take longer than 10 seconds
- Page loading is slow

**Root Causes and Solutions:**
1. **Cold Start:** Service waking up - wait and retry
2. **Network Issues:** Check internet connection
3. **Server Load:** Retry after few minutes

---

#### ❌ Issue: Missing or Incorrect Data
**Symptoms:**
- Empty lists where data expected
- Login history shows no records
- Staff list incomplete

**Root Cause:** Database initialization or data sync issues
**Solution:**
1. Verify you're using correct test environment
2. Check if you have appropriate permissions to view data
3. Contact development team if data appears corrupted

---

### Browser-Specific Issues

#### ❌ Issue: Cache-Related Problems
**Symptoms:**
- Old version of application loads
- Features not working as expected
- Inconsistent behavior

**Solutions:**
1. **Hard Refresh:** Ctrl+Shift+R (Windows) or Cmd+Shift+R (Mac)
2. **Incognito Mode:** Open private/incognito browser window
3. **Clear Cache:** Browser settings → Clear browsing data
4. **Disable Cache:** Dev tools → Network tab → Disable cache checkbox

---

#### ❌ Issue: CORS Errors  
**Symptoms:**
- "Cross-Origin Request Blocked" in console
- API calls fail with CORS error

**Root Cause:** Incorrect domain or protocol
**Solution:**
1. Ensure accessing from correct domain: `yuan0173.github.io`
2. Use HTTPS only, not HTTP
3. Check browser console for specific CORS error details

---

## 🔧 Advanced Debugging

### Network Analysis
**Steps to debug API issues:**
1. Open browser Dev Tools (F12)
2. Go to Network tab
3. Reproduce the issue
4. Check failed requests for:
   - Response status codes
   - Request headers
   - Response content
   - Timing information

### Console Logging
**Check browser console for:**
- JavaScript errors
- Authentication warnings  
- Network request details
- Application state information

### Service Status Verification
**Verify backend service status:**
1. **Health Check:** `GET /health` should return 200 OK
2. **Service Logs:** Check Render dashboard for error logs
3. **Database Connection:** Verify database queries in logs

## 📞 Escalation Procedures

### When to Contact Development Team
- Service completely inaccessible for >5 minutes
- Database errors or data corruption
- Security vulnerabilities discovered
- New bugs not covered in troubleshooting

### When to Contact QA Lead
- Test scenarios unclear or incomplete
- Test data needs modification
- New test cases required
- Test environment configuration issues

### When to Contact Project Manager
- Feature requirements clarification
- Test timeline adjustments
- Access permission requests
- Priority changes

## 🛠️ Self-Service Tools

### Quick Diagnostic Commands
```bash
# Test backend connectivity
curl -I https://comp9034farmsystem-backend.onrender.com/health

# Test frontend accessibility  
curl -I https://yuan0173.github.io/comp9034FarmSystem/

# Wake up sleeping service
curl https://comp9034farmsystem-backend.onrender.com/health
```

### Browser Reset Checklist
- [ ] Clear browser cache and cookies
- [ ] Disable browser extensions
- [ ] Use incognito/private mode
- [ ] Check internet connection
- [ ] Verify correct URL spelling

---

📋 **Document Version:** 2.0.0  
📅 **Last Updated:** 2025-09-04  
🔄 **Next Review:** Sprint 3 completion
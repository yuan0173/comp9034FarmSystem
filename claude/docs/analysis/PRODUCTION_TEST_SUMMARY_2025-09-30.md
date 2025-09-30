# Farm Time Management System — Production Test Summary (2025-09-30)

Environment
- Frontend: GitHub Pages (SPA)
  - URL: https://yuan0173.github.io/comp9034FarmSystem/
- Backend: Render (ASP.NET Core)
  - URL: https://comp9034farmsystem-backend.onrender.com
- Database: Render PostgreSQL
- Status: All core flows validated; production-ready

Key Issues Resolved
1) StaffsController not compiled (404)
- Fix: Include StaffsController in build; inject ApplicationDbContext
- Result: Endpoints available; CRUD works

2) WorkSchedule JSON property collision (409)
- Problem: ScheduleID vs ScheduleId collide under camelCase → scheduleId
- Fix: [JsonPropertyName("scheduleId")] on ScheduleID; [JsonIgnore] on ScheduleId
- Result: Serialization stable; endpoints return 200

3) CORS misconfiguration (blocked requests)
- Problem: Env var name mismatch; required AllowedOrigins__0
- Fix: Set AllowedOrigins__0=https://yuan0173.github.io
- Result: Preflight and requests pass with credentials

4) JWT token expiry during long tests (401)
- Fix: Re-login flow tested; tokens regenerated
- Result: Auth stable; role enforcement correct

Functional Validation
- Staff Management
  - Data: 4 users (Admin 9001, Manager 8001, Staff 1001/2001)
  - Actions: Search, filter, sort, CRUD; current user delete disabled
  - Metrics: Total count correct; average pay rate $33.75 correct
- Device Management
  - Data: 2 devices (Main Terminal, Biometric Scanner)
  - Actions: Search, refresh, add device (UI present)
- Login History (Audit)
  - Data: 4 login records; timestamps, user, IP captured
  - Actions: Search, filter, delete
- Attendance Management
  - Actions: Date-range selector, queries, empty-state UX verified

Production Verification
- Frontend (GitHub Pages)
  - SPA routes work; responsive UI OK
- Backend (Render)
  - /health 200; all API routes correct; CORS logs show allowed origins
- Database (PostgreSQL)
  - Connectivity stable; InitialCreate migration applied
  - Seed data: 4 users, 2 devices; idempotent seeding fills missing accounts

Security And Performance
- AuthN/AuthZ
  - JWT generation/validation OK; roles enforced (admin/manager/staff)
  - Password hashing stored securely
- CORS
  - Origin allow-list correct; credentials allowed; preflight passes
- Network
  - HTTPS; reasonable API response times; unified error handling

Technical Validation
- CORS Preflight (example)
  - curl -i -X OPTIONS https://comp9034farmsystem-backend.onrender.com/api/Staffs \
    -H "Origin: https://yuan0173.github.io" \
    -H "Access-Control-Request-Method: GET"
  - Result: HTTP/2 204 with correct CORS headers
- Browser Console
  - No CORS or API errors; only health-check logs; no runtime JS errors

Conclusion
- Status: Production-ready
- All core features validated in production environment (GitHub Pages + Render)

Recommended Next Steps
- Add more attendance data for reporting verification
- Enable production monitoring and structured logging
- Establish backup policy and routine checks
- Consider API rate limiting

#!/bin/bash

echo "🔍 Testing Login History API..."

# Test 1: Check if backend is running
echo "1. Checking backend health..."
curl -s http://localhost:4000/health | jq '.' 2>/dev/null || echo "Backend not responding"

# Test 2: Test login history API without auth (should return 401)
echo -e "\n2. Testing login history API without authentication..."
curl -X GET "http://localhost:4000/api/Auth/login-logs?limit=5" \
  -H "Content-Type: application/json" \
  -w "\nHTTP Status: %{http_code}\n" \
  -s

# Test 3: Check database directly
echo -e "\n3. Checking database directly..."
sqlite3 backend/farmtimems-dev.db "SELECT COUNT(*) as total_records FROM LoginLogs;"
sqlite3 backend/farmtimems-dev.db "SELECT Id, Username, Success, Timestamp FROM LoginLogs ORDER BY Timestamp DESC LIMIT 3;"

echo -e "\n✅ Test completed!"

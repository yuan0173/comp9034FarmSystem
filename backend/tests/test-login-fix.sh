#!/bin/bash

echo "🔍 Testing Login and Token Fix..."

# Test 1: Check if backend is running
echo "1. Checking backend health..."
curl -s http://localhost:4000/health | jq '.' 2>/dev/null || echo "Backend not responding"

# Test 2: Test login to get a fresh token
echo -e "\n2. Testing login to get fresh token..."
LOGIN_RESPONSE=$(curl -s -X POST "http://localhost:4000/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "9001", "password": "password123"}')

echo "Login response:"
echo "$LOGIN_RESPONSE" | jq '.' 2>/dev/null || echo "$LOGIN_RESPONSE"

# Extract token from response
TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token' 2>/dev/null)
if [ "$TOKEN" != "null" ] && [ "$TOKEN" != "" ]; then
    echo -e "\n✅ Token extracted successfully"
    
    # Test 3: Test login history API with fresh token
    echo -e "\n3. Testing login history API with fresh token..."
    curl -X GET "http://localhost:4000/api/Auth/login-logs?limit=5" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -w "\nHTTP Status: %{http_code}\n" \
      -s
else
    echo -e "\n❌ Failed to extract token from login response"
fi

echo -e "\n✅ Test completed!"

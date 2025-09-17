#!/bin/bash

# Test script for Render backend deployment
# Tests health, database connection, and login functionality

BACKEND_URL="https://comp9034farmsystem-backend.onrender.com"

echo "🔍 Testing Render Backend Deployment"
echo "======================================"

# Test 1: Health Check
echo "1. Health Check..."
HEALTH_RESPONSE=$(curl -s "$BACKEND_URL/health")
echo "Response: $HEALTH_RESPONSE"

if echo "$HEALTH_RESPONSE" | grep -q "healthy"; then
    echo "✅ Health check passed"
else
    echo "❌ Health check failed"
    exit 1
fi

# Test 2: API Endpoints
echo ""
echo "2. Testing API Endpoints..."

# Test Staffs endpoint
echo "Testing /api/Staffs..."
STAFFS_RESPONSE=$(curl -s "$BACKEND_URL/api/Staffs")
echo "Staffs Response: $STAFFS_RESPONSE"

# Test Devices endpoint  
echo "Testing /api/Devices..."
DEVICES_RESPONSE=$(curl -s "$BACKEND_URL/api/Devices")
echo "Devices Response: $DEVICES_RESPONSE"

# Test 3: Login Test
echo ""
echo "3. Testing Login..."

# Test admin login
echo "Testing admin login..."
LOGIN_RESPONSE=$(curl -s -X POST "$BACKEND_URL/api/Auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email": "admin@farmtimems.com", "password": "admin123"}')

echo "Login Response: $LOGIN_RESPONSE"

if echo "$LOGIN_RESPONSE" | grep -q "token"; then
    echo "✅ Admin login successful"
else
    echo "❌ Admin login failed"
fi

# Test manager login
echo "Testing manager login..."
MANAGER_LOGIN=$(curl -s -X POST "$BACKEND_URL/api/Auth/login" \
    -H "Content-Type: application/json" \
    -d '{"email": "manager@farmtimems.com", "password": "manager123"}')

echo "Manager Login Response: $MANAGER_LOGIN"

# Test 4: Database Data Check
echo ""
echo "4. Database Data Summary..."
echo "Staffs count: $(echo "$STAFFS_RESPONSE" | jq '. | length' 2>/dev/null || echo "Unable to parse")"
echo "Devices count: $(echo "$DEVICES_RESPONSE" | jq '. | length' 2>/dev/null || echo "Unable to parse")"

echo ""
echo "🎯 Test Summary Complete"
echo "Check the responses above to verify backend functionality"

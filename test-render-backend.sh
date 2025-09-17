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

# Test 2: Database Diagnostics
echo ""
echo "2. Database Diagnostics..."
DB_STATUS=$(curl -s "$BACKEND_URL/api/Diagnostics/database-status")
echo "Database Status: $DB_STATUS"

# Test 3: Staff Sample Data
echo ""
echo "3. Staff Sample Data..."
STAFF_SAMPLE=$(curl -s "$BACKEND_URL/api/Diagnostics/staff-sample")
echo "Staff Sample: $STAFF_SAMPLE"

# Test 4: Force Init (if needed)
echo ""
echo "4. Force Database Initialization..."
FORCE_INIT=$(curl -s -X POST "$BACKEND_URL/api/Diagnostics/force-init")
echo "Force Init Response: $FORCE_INIT"

# Test 5: Login Test
echo ""
echo "5. Testing Login..."

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

# Test 6: API Endpoints
echo ""
echo "6. Testing API Endpoints..."

# Test Staffs endpoint
echo "Testing /api/Staffs..."
STAFFS_RESPONSE=$(curl -s "$BACKEND_URL/api/Staffs")
echo "Staffs Response: $STAFFS_RESPONSE"

# Test Devices endpoint
echo "Testing /api/Devices..."
DEVICES_RESPONSE=$(curl -s "$BACKEND_URL/api/Devices")
echo "Devices Response: $DEVICES_RESPONSE"

echo ""
echo "🎯 Test Summary Complete"
echo "Check the responses above to verify backend functionality"

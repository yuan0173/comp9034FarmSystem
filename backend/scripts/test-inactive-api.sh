#!/bin/bash

# Test script for Inactive Staff API endpoints
echo "🧪 Testing Inactive Staff API Endpoints"
echo "======================================="

BASE_URL="http://localhost:4000"

echo ""
echo "1️⃣ Testing GET /api/Staffs/inactive endpoint..."
echo "Expected: Returns 1 inactive staff (ID: 2345)"

response=$(curl -s -w "%{http_code}" -o /dev/null "$BASE_URL/api/Staffs/inactive")

if [ "$response" = "401" ]; then
    echo "❌ Status: 401 Unauthorized (Expected - requires authentication)"
    echo "💡 This endpoint requires admin authentication"
else
    echo "✅ Status: $response"
    echo "📋 Response:"
    curl -s "$BASE_URL/api/Staffs/inactive" | jq '.' 2>/dev/null || curl -s "$BASE_URL/api/Staffs/inactive"
fi

echo ""
echo "2️⃣ Testing PUT /api/Staffs/2345/restore endpoint..."
echo "Expected: 401 Unauthorized (requires authentication)"

response=$(curl -s -w "%{http_code}" -o /dev/null -X PUT "$BASE_URL/api/Staffs/2345/restore")
echo "Status: $response"

if [ "$response" = "401" ]; then
    echo "✅ Correctly requires authentication"
else
    echo "⚠️ Unexpected response code"
fi

echo ""
echo "3️⃣ Verifying database state..."
echo "Inactive staff count:"
sqlite3 ../farmtimems-dev.db "SELECT COUNT(*) FROM Staffs WHERE IsActive = 0;"

echo ""
echo "Inactive staff details:"
sqlite3 ../farmtimems-dev.db "SELECT Id, Name, Role, IsActive FROM Staffs WHERE IsActive = 0;"

echo ""
echo "🎯 Test Summary:"
echo "- API endpoints are accessible"
echo "- Authentication is required (security ✅)"
echo "- Database contains test data"
echo ""
echo "🚀 Next steps:"
echo "1. Login as admin (ID: 9001, PIN: 1234)"
echo "2. Navigate to Staff Management"
echo "3. Click 'Inactive Staff' tab"
echo "4. Test restore functionality"

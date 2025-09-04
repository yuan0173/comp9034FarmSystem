#!/bin/bash

# COMP9034 FarmTimeMS API Testing Examples
# Purpose: Command-line API testing using curl

# Color codes for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m'

# Configuration
BASE_URL="https://comp9034farmsystem-backend.onrender.com"

echo -e "${BLUE}🧪 COMP9034 FarmTimeMS API Testing Suite${NC}"
echo "=========================================="
echo ""

echo -e "${BLUE}🏥 Step 1: Testing Health Check...${NC}"
HEALTH_RESPONSE=$(curl -s -w "HTTPSTATUS:%{http_code}" "$BASE_URL/health")
HTTP_STATUS=$(echo $HEALTH_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
HEALTH_BODY=$(echo $HEALTH_RESPONSE | sed -E 's/HTTPSTATUS\:[0-9]{3}$//')

if [ "$HTTP_STATUS" -eq 200 ]; then
    echo -e "${GREEN}✅ Health check passed${NC}"
    echo "$HEALTH_BODY" | jq '.' 2>/dev/null || echo "$HEALTH_BODY"
else
    echo -e "${RED}❌ Health check failed - Status: $HTTP_STATUS${NC}"
    echo -e "${YELLOW}⏳ Service may be sleeping, waiting 30 seconds...${NC}"
    sleep 30
    curl -s "$BASE_URL/health" | jq '.' 2>/dev/null || echo "Service still starting up..."
fi

echo ""
echo -e "${BLUE}🔑 Step 2: Testing Authentication...${NC}"

# Test Admin Login
echo "Testing Admin Login (Staff ID: 9001)..."
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/pin-login" \
  -H "Content-Type: application/json" \
  -d '{"staffId": 9001, "pin": "1234"}' \
  -w "HTTPSTATUS:%{http_code}")

LOGIN_STATUS=$(echo $LOGIN_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
LOGIN_BODY=$(echo $LOGIN_RESPONSE | sed -E 's/HTTPSTATUS\:[0-9]{3}$//')

if [ "$LOGIN_STATUS" -eq 200 ]; then
    echo -e "${GREEN}✅ Admin login successful${NC}"
    TOKEN=$(echo "$LOGIN_BODY" | jq -r '.token // empty')
    STAFF_NAME=$(echo "$LOGIN_BODY" | jq -r '.staff.name // empty')
    echo "Logged in as: $STAFF_NAME"
    echo "Token (first 20 chars): ${TOKEN:0:20}..."
else
    echo -e "${RED}❌ Admin login failed - Status: $LOGIN_STATUS${NC}"
    echo "$LOGIN_BODY"
fi

# Test invalid login
echo ""
echo "Testing Invalid Login (Wrong PIN)..."
INVALID_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/pin-login" \
  -H "Content-Type: application/json" \
  -d '{"staffId": 9001, "pin": "0000"}' \
  -w "HTTPSTATUS:%{http_code}")

INVALID_STATUS=$(echo $INVALID_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
if [ "$INVALID_STATUS" -eq 401 ]; then
    echo -e "${GREEN}✅ Invalid login properly rejected${NC}"
else
    echo -e "${YELLOW}⚠️ Unexpected status for invalid login: $INVALID_STATUS${NC}"
fi

echo ""
echo -e "${BLUE}👥 Step 3: Testing Staff Management APIs...${NC}"

if [ -n "$TOKEN" ]; then
    # Test get all staff
    echo "Testing Get All Staff..."
    STAFF_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/staffs" \
      -w "HTTPSTATUS:%{http_code}")
    
    STAFF_STATUS=$(echo $STAFF_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    STAFF_BODY=$(echo $STAFF_RESPONSE | sed -E 's/HTTPSTATUS\:[0-9]{3}$//')
    
    if [ "$STAFF_STATUS" -eq 200 ]; then
        echo -e "${GREEN}✅ Staff list retrieved successfully${NC}"
        STAFF_COUNT=$(echo "$STAFF_BODY" | jq '. | length' 2>/dev/null || echo "unknown")
        echo "Staff count: $STAFF_COUNT"
    else
        echo -e "${RED}❌ Staff list failed - Status: $STAFF_STATUS${NC}"
    fi
    
    # Test get specific staff
    echo ""
    echo "Testing Get Specific Staff (ID: 9001)..."
    SPECIFIC_STAFF=$(curl -s -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/staffs/9001" \
      -w "HTTPSTATUS:%{http_code}")
    
    SPECIFIC_STATUS=$(echo $SPECIFIC_STAFF | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    if [ "$SPECIFIC_STATUS" -eq 200 ]; then
        echo -e "${GREEN}✅ Specific staff retrieved successfully${NC}"
    else
        echo -e "${RED}❌ Specific staff failed - Status: $SPECIFIC_STATUS${NC}"
    fi
else
    echo -e "${YELLOW}⚠️ Skipping authenticated tests - no valid token${NC}"
fi

echo ""
echo -e "${BLUE}📊 Step 4: Testing Device Management...${NC}"

if [ -n "$TOKEN" ]; then
    DEVICES_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/devices" \
      -w "HTTPSTATUS:%{http_code}")
    
    DEVICES_STATUS=$(echo $DEVICES_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    DEVICES_BODY=$(echo $DEVICES_RESPONSE | sed -E 's/HTTPSTATUS\:[0-9]{3}$//')
    
    if [ "$DEVICES_STATUS" -eq 200 ]; then
        echo -e "${GREEN}✅ Devices list retrieved successfully${NC}"
        DEVICE_COUNT=$(echo "$DEVICES_BODY" | jq '. | length' 2>/dev/null || echo "unknown")
        echo "Device count: $DEVICE_COUNT"
    else
        echo -e "${RED}❌ Devices list failed - Status: $DEVICES_STATUS${NC}"
    fi
fi

echo ""
echo -e "${BLUE}📅 Step 5: Testing Event Management...${NC}"

if [ -n "$TOKEN" ]; then
    # Test create event
    echo "Testing Create Event..."
    CREATE_EVENT=$(curl -s -X POST "$BASE_URL/api/events" \
      -H "Authorization: Bearer $TOKEN" \
      -H "Content-Type: application/json" \
      -d '{"staffId": 1001, "deviceId": 1, "eventType": "IN", "notes": "Test event from curl"}' \
      -w "HTTPSTATUS:%{http_code}")
    
    CREATE_STATUS=$(echo $CREATE_EVENT | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    if [ "$CREATE_STATUS" -eq 200 ] || [ "$CREATE_STATUS" -eq 201 ]; then
        echo -e "${GREEN}✅ Event created successfully${NC}"
    else
        echo -e "${RED}❌ Event creation failed - Status: $CREATE_STATUS${NC}"
    fi
    
    # Test get events
    echo ""
    echo "Testing Get All Events..."
    EVENTS_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/events" \
      -w "HTTPSTATUS:%{http_code}")
    
    EVENTS_STATUS=$(echo $EVENTS_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    if [ "$EVENTS_STATUS" -eq 200 ]; then
        echo -e "${GREEN}✅ Events list retrieved successfully${NC}"
    else
        echo -e "${RED}❌ Events list failed - Status: $EVENTS_STATUS${NC}"
    fi
fi

echo ""
echo -e "${BLUE}📋 Step 6: Testing Admin Features...${NC}"

if [ -n "$TOKEN" ]; then
    # Test login logs (admin only)
    echo "Testing Login Logs (Admin only)..."
    LOGS_RESPONSE=$(curl -s -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/auth/login-logs" \
      -w "HTTPSTATUS:%{http_code}")
    
    LOGS_STATUS=$(echo $LOGS_RESPONSE | tr -d '\n' | sed -e 's/.*HTTPSTATUS://')
    if [ "$LOGS_STATUS" -eq 200 ]; then
        echo -e "${GREEN}✅ Login logs retrieved successfully${NC}"
    else
        echo -e "${RED}❌ Login logs failed - Status: $LOGS_STATUS${NC}"
        echo "Note: This endpoint requires Admin role"
    fi
fi

echo ""
echo "=========================================="
echo -e "${GREEN}🎉 API Testing Complete!${NC}"
echo ""
echo -e "${YELLOW}📋 Test Summary:${NC}"
echo "  - Health Check: Service connectivity"
echo "  - Authentication: PIN login validation"  
echo "  - Authorization: JWT token functionality"
echo "  - CRUD Operations: Staff, Events, Devices"
echo "  - Role Permissions: Admin-only features"
echo ""
echo -e "${BLUE}💡 Usage Tips:${NC}"
echo "  - Run this script anytime to verify API functionality"
echo "  - First run may be slow due to service wake-up"
echo "  - Check individual test results above"
echo "  - Use Postman collection for detailed testing"

# Example of role-based testing
echo ""
echo -e "${YELLOW}🔄 Testing Different Roles:${NC}"
echo "To test Manager role:"
echo "  curl -X POST $BASE_URL/api/auth/pin-login -H 'Content-Type: application/json' -d '{\"staffId\": 8001, \"pin\": \"8001\"}'"
echo ""
echo "To test Staff role:"
echo "  curl -X POST $BASE_URL/api/auth/pin-login -H 'Content-Type: application/json' -d '{\"staffId\": 1001, \"pin\": \"1001\"}'"
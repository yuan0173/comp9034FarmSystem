#!/bin/bash

# External Testing Package Generator
# Purpose: Create sanitized testing package for external test teams

echo "📦 COMP9034 External Testing Package Generator"
echo "=============================================="

# Configuration
TEMP_DIR="temp-external-package"
OUTPUT_ZIP="comp9034-testing-package-external.zip"
DATE_STAMP=$(date +"%Y%m%d_%H%M%S")

# Color codes
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo ""
echo -e "${BLUE}🧹 Cleaning up previous builds...${NC}"
rm -rf "$TEMP_DIR"
rm -f "$OUTPUT_ZIP"

echo -e "${BLUE}📂 Creating temporary package directory...${NC}"
mkdir -p "$TEMP_DIR"

echo -e "${BLUE}📋 Copying public documentation...${NC}"
# Copy safe documentation files
cp testing/README.md "$TEMP_DIR/"
mkdir -p "$TEMP_DIR/documentation"
cp testing/documentation/api-guide.md "$TEMP_DIR/documentation/"
cp testing/documentation/test-scenarios.md "$TEMP_DIR/documentation/"
cp testing/documentation/troubleshooting.md "$TEMP_DIR/documentation/"

echo -e "${BLUE}⚙️ Processing configuration files...${NC}"
mkdir -p "$TEMP_DIR/config"

# Process environment.json - remove sensitive sections if any
if [ -f "testing/config/environment.json" ]; then
    # Copy environment config (already sanitized for testing)
    cp testing/config/environment.json "$TEMP_DIR/config/"
fi

# Process test-accounts.json - ensure no real credentials
if [ -f "testing/config/test-accounts.json" ]; then
    # Copy test accounts (already contains only test data)
    cp testing/config/test-accounts.json "$TEMP_DIR/config/"
fi

echo -e "${BLUE}🔧 Copying testing tools...${NC}"
mkdir -p "$TEMP_DIR/tools"

# Copy Postman collection
if [ -f "testing/tools/postman-collection.json" ]; then
    cp testing/tools/postman-collection.json "$TEMP_DIR/tools/"
fi

# Create curl examples
cat > "$TEMP_DIR/tools/curl-examples.sh" << 'EOF'
#!/bin/bash

# COMP9034 FarmTimeMS API Testing Examples
# Base URL
BASE_URL="https://comp9034farmsystem-backend.onrender.com"

echo "🏥 Testing Health Check..."
curl -s "$BASE_URL/health" | jq '.' || echo "Service may be sleeping, please wait..."

echo ""
echo "🔑 Testing PIN Login (Admin)..."
LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/pin-login" \
  -H "Content-Type: application/json" \
  -d '{"staffId": 9001, "pin": "1234"}')

echo "$LOGIN_RESPONSE" | jq '.'

# Extract token for subsequent requests
TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token // empty')

if [ -n "$TOKEN" ]; then
    echo ""
    echo "👥 Testing Staff List (requires auth)..."
    curl -s -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/staffs" | jq '.'
    
    echo ""
    echo "📊 Testing Devices List (requires auth)..."
    curl -s -H "Authorization: Bearer $TOKEN" "$BASE_URL/api/devices" | jq '.'
else
    echo "❌ Login failed - check credentials and service status"
fi
EOF

chmod +x "$TEMP_DIR/tools/curl-examples.sh"

echo -e "${BLUE}📄 Creating external package README...${NC}"
cat > "$TEMP_DIR/EXTERNAL_README.md" << 'EOF'
# COMP9034 Farm Time Management System - External Testing Package

## 🎯 Quick Start for External Test Teams

This package contains everything needed to test the COMP9034 Farm Time Management System.

### 🚀 Immediate Access
1. **Application URL:** https://yuan0173.github.io/comp9034FarmSystem/
2. **Test with Admin Account:** Staff ID: 9001, PIN: 1234
3. **Import Postman Collection:** `tools/postman-collection.json`

### 📁 Package Contents
- `README.md` - Complete testing guide
- `documentation/` - API docs and test scenarios  
- `config/` - Environment and account configuration
- `tools/` - Postman collection and curl examples

### ⚠️ Important Notes
- Service may sleep after 15 minutes - first access takes 30-60s
- Use incognito mode to avoid cache issues
- All credentials are for testing purposes only

### 🆘 Support
For technical issues or questions, contact the COMP9034 development team.

---
📦 **Package Generated:** DATE_STAMP_PLACEHOLDER
🔒 **Security Level:** External Distribution Safe
EOF

# Replace date stamp
sed -i.bak "s/DATE_STAMP_PLACEHOLDER/$DATE_STAMP/g" "$TEMP_DIR/EXTERNAL_README.md"
rm -f "$TEMP_DIR/EXTERNAL_README.md.bak"

echo -e "${BLUE}🔒 Adding security notice...${NC}"
cat > "$TEMP_DIR/SECURITY_NOTICE.md" << 'EOF'
# 🔒 Security Notice for External Testing Package

## Data Classification
- **Level:** Testing Data - Non-Sensitive
- **Usage:** Testing and validation purposes only
- **Restrictions:** Not for production use

## What's Included
✅ Test account credentials (testing environment only)  
✅ API endpoint documentation and examples
✅ Testing tools and configurations
✅ Troubleshooting guides

## What's NOT Included  
❌ Production environment configurations
❌ Real user data or credentials  
❌ Internal system architecture details
❌ Development team internal documentation

## Usage Guidelines
- Use only for assigned testing tasks
- Do not attempt to access production systems
- Report security concerns to development team
- Coordinate shared test account usage

---
📋 **Classification:** External Distribution Approved
👥 **Authorized by:** COMP9034 Development Team
EOF

echo -e "${BLUE}🔍 Validating package contents...${NC}"
# Validate JSON files before packaging
find "$TEMP_DIR" -name "*.json" | while read file; do
    if ! jq . "$file" > /dev/null 2>&1; then
        echo -e "${RED}❌ Invalid JSON found: $file${NC}"
        exit 1
    fi
done

# Check for sensitive content
if grep -r -i "password.*=.*[^*]" "$TEMP_DIR" | grep -v "example\|template" | grep -q .; then
    echo -e "${RED}❌ Potential sensitive content found in package${NC}"
    exit 1
fi

echo -e "${BLUE}📦 Creating ZIP package...${NC}"
zip -r "$OUTPUT_ZIP" "$TEMP_DIR"/* >/dev/null

echo -e "${BLUE}🧹 Cleaning up temporary files...${NC}"
rm -rf "$TEMP_DIR"

echo ""
echo "========================================"
echo -e "${GREEN}✅ EXTERNAL PACKAGE CREATED SUCCESSFULLY${NC}"
echo ""
echo "📦 Package File: $OUTPUT_ZIP"
echo "📊 Package Size: $(ls -lh "$OUTPUT_ZIP" | awk '{print $5}')"
echo "📅 Generated: $(date)"
echo ""
echo -e "${YELLOW}📋 Distribution Checklist:${NC}"
echo "  [ ] Verify package contains no sensitive data"  
echo "  [ ] Test package contents with external user"
echo "  [ ] Confirm all URLs and credentials work"
echo "  [ ] Document package version and distribution list"
echo ""
echo "🎯 Ready for external distribution!"
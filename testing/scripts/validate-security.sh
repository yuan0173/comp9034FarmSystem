#!/bin/bash

# Testing Directory Security Validation Script
# Purpose: Ensure testing directory maintains security isolation

echo "🔒 COMP9034 Testing Security Validation"
echo "======================================="

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

VALIDATION_PASSED=true

echo ""
echo "1. Checking for sensitive files in testing directory..."

# Check for potentially sensitive files
SENSITIVE_PATTERNS=(
    "*.env"
    "*.secret" 
    "*credentials*"
    "*password*"
    "*.key"
    "*.pem"
    "*.p12"
)

for pattern in "${SENSITIVE_PATTERNS[@]}"; do
    if find testing/ -name "$pattern" -type f | grep -q .; then
        echo -e "${RED}❌ SECURITY RISK: Found sensitive files matching pattern: $pattern${NC}"
        find testing/ -name "$pattern" -type f
        VALIDATION_PASSED=false
    fi
done

if [ "$VALIDATION_PASSED" = true ]; then
    echo -e "${GREEN}✅ No sensitive files found${NC}"
fi

echo ""
echo "2. Checking build artifacts for testing directory inclusion..."

# Check if testing directory would be included in builds
if [ -d "frontendWebsite/dist" ]; then
    if [ -d "frontendWebsite/dist/testing" ]; then
        echo -e "${RED}❌ SECURITY RISK: Testing directory found in build output${NC}"
        VALIDATION_PASSED=false
    else
        echo -e "${GREEN}✅ Testing directory properly excluded from build${NC}"
    fi
else
    echo -e "${YELLOW}⚠️ No build output found - run 'npm run build' to verify${NC}"
fi

echo ""
echo "3. Checking gitignore configuration..."

if [ -f "testing/.gitignore" ]; then
    echo -e "${GREEN}✅ Testing directory has dedicated .gitignore${NC}"
else
    echo -e "${YELLOW}⚠️ Testing .gitignore not found - consider creating one${NC}"
fi

# Check main .gitignore for testing exclusions
if [ -f ".gitignore" ]; then
    if grep -q "testing/" ".gitignore"; then
        echo -e "${GREEN}✅ Main .gitignore includes testing directory exclusions${NC}"
    else
        echo -e "${YELLOW}⚠️ Consider adding testing/ exclusions to main .gitignore${NC}"
    fi
fi

echo ""
echo "4. Checking for hardcoded secrets in documentation..."

SECRET_PATTERNS=(
    "password.*=.*[^*]"
    "secret.*=.*[^*]"
    "key.*=.*[^*]"
    "token.*=.*eyJ"
)

for pattern in "${SECRET_PATTERNS[@]}"; do
    if grep -r -i "$pattern" testing/documentation/ testing/config/ 2>/dev/null | grep -v "example\|template\|sample" | grep -q .; then
        echo -e "${RED}❌ POTENTIAL RISK: Found possible hardcoded secrets${NC}"
        grep -r -i "$pattern" testing/documentation/ testing/config/ 2>/dev/null | grep -v "example\|template\|sample"
        VALIDATION_PASSED=false
    fi
done

if [ "$VALIDATION_PASSED" = true ]; then
    echo -e "${GREEN}✅ No hardcoded secrets found in documentation${NC}"
fi

echo ""
echo "5. Checking file permissions..."

# Check for overly permissive files
find testing/ -type f -perm -o+w 2>/dev/null | while read file; do
    echo -e "${YELLOW}⚠️ World-writable file found: $file${NC}"
done

echo ""
echo "======================================="

if [ "$VALIDATION_PASSED" = true ]; then
    echo -e "${GREEN}🎉 SECURITY VALIDATION PASSED${NC}"
    echo "Testing directory maintains proper security isolation"
    exit 0
else
    echo -e "${RED}❌ SECURITY VALIDATION FAILED${NC}"
    echo "Please address the security issues found above"
    exit 1
fi
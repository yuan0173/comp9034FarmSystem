#!/usr/bin/env bash

# Simple user registration smoke test
# Usage: ./test-register.sh <firstName> <lastName> <email> <password> [base_url]

set -euo pipefail

FIRST=${1:-}
LAST=${2:-}
EMAIL=${3:-}
PASSWORD=${4:-}
BASE_URL=${5:-http://localhost:4000}

if [[ -z "$FIRST" || -z "$LAST" || -z "$EMAIL" || -z "$PASSWORD" ]]; then
  echo "Usage: $0 <firstName> <lastName> <email> <password> [base_url]" 1>&2
  exit 1
fi

echo "🔗 Base URL: $BASE_URL"
echo "📝 Registering: $FIRST $LAST <$EMAIL>"

RESP=$(curl -sS -w "\n%{http_code}" \
  -H 'Content-Type: application/json' \
  -X POST "$BASE_URL/api/Auth/register" \
  -d "{\"firstName\":\"$FIRST\",\"lastName\":\"$LAST\",\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\",\"confirmPassword\":\"$PASSWORD\"}")

BODY=$(echo "$RESP" | sed '$d')
STATUS=$(echo "$RESP" | tail -n1)

echo "📦 Status: $STATUS"

if [[ "$STATUS" == "200" ]]; then
  echo "✅ Registration successful"
else
  echo "❌ Registration failed"
fi

echo "--- Response Body ---"
echo "$BODY" | jq . 2>/dev/null || echo "$BODY"


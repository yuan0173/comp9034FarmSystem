#!/usr/bin/env bash

# Simple Email+Password login smoke test
# Usage: ./test-login.sh <email> <password> [base_url]

set -euo pipefail

EMAIL=${1:-}
PASSWORD=${2:-}
BASE_URL=${3:-http://localhost:4000}

if [[ -z "$EMAIL" || -z "$PASSWORD" ]]; then
  echo "Usage: $0 <email> <password> [base_url]" 1>&2
  exit 1
fi

echo "🔗 Base URL: $BASE_URL"
echo "👤 Email: $EMAIL"

RESP=$(curl -sS -w "\n%{http_code}" \
  -H 'Content-Type: application/json' \
  -X POST "$BASE_URL/api/Auth/login" \
  -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")

BODY=$(echo "$RESP" | sed '$d')
STATUS=$(echo "$RESP" | tail -n1)

echo "📦 Status: $STATUS"

if [[ "$STATUS" == "200" ]]; then
  TOKEN=$(echo "$BODY" | sed -n 's/.*"token":"\([^"]*\)".*/\1/p' | head -n1)
  echo "✅ Login successful"
  if [[ -n "$TOKEN" ]]; then
    echo "🔑 Token (first 32 chars): ${TOKEN:0:32}..."
  fi
else
  echo "❌ Login failed"
fi

echo "--- Response Body ---"
echo "$BODY" | jq . 2>/dev/null || echo "$BODY"


#!/usr/bin/env bash

# Simple device API smoke test against local backend
# Prereq: backend running on http://localhost:4000

set -euo pipefail

BASE_URL=${BASE_URL:-http://localhost:4000}

echo "==> GET /api/Devices"
curl -sS "${BASE_URL}/api/Devices" | jq '.[0] // {}'

echo "\n==> POST /api/Devices (create)"
create_resp=$(curl -sS -X POST "${BASE_URL}/api/Devices" \
  -H 'Content-Type: application/json' \
  -d '{
    "deviceName":"CLI Test Device",
    "deviceType":"biometric",
    "location":"QA Lab",
    "status":"Active",
    "ipAddress":"192.168.1.123"
  }')
echo "$create_resp" | jq '{deviceId: .deviceId, status: .status, name: .deviceName}'
id=$(echo "$create_resp" | jq -r '.deviceId')

echo "\n==> GET /api/Devices/${id}"
curl -sS "${BASE_URL}/api/Devices/${id}" | jq '{deviceId, status, deviceName}'

echo "\n==> PUT /api/Devices/${id} (update to Maintenance)"
curl -sS -X PUT "${BASE_URL}/api/Devices/${id}" \
  -H 'Content-Type: application/json' \
  -d "$(jq -n --arg id "$id" '{
    deviceId: ($id|tonumber),
    deviceName: "CLI Test Device",
    deviceType: "biometric",
    location: "QA Lab",
    status: "Maintenance",
    ipAddress: "192.168.1.123"
  }')" | jq '.'

echo "\n==> PATCH /api/Devices/${id}/status (Inactive)"
curl -sS -X PATCH "${BASE_URL}/api/Devices/${id}/status" \
  -H 'Content-Type: application/json' \
  -d '"Inactive"' | jq '.'

echo "\n==> DELETE /api/Devices/${id} (soft delete)"
curl -sS -X DELETE "${BASE_URL}/api/Devices/${id}" | jq '.'

echo "\n==> GET /api/Devices/statistics"
curl -sS "${BASE_URL}/api/Devices/statistics" | jq '.'

echo "\nDone."


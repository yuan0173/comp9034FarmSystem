#!/bin/bash

# Emergency Admin Recovery Script
# Use this script when all admin accounts are accidentally deleted

echo "🚨 Emergency Admin Recovery Script"
echo "=================================="

# Check if database exists
if [ ! -f "../farmtimems-dev.db" ]; then
    echo "❌ Database file not found!"
    exit 1
fi

echo "📊 Current admin status:"
sqlite3 ../farmtimems-dev.db "SELECT Id, Name, Role, IsActive FROM Staffs WHERE Id >= 9000 ORDER BY Id;"

echo ""
echo "🔧 Restoring admin account 9001..."

# Restore admin account
sqlite3 ../farmtimems-dev.db "UPDATE Staffs SET IsActive = 1, UpdatedAt = datetime('now') WHERE Id = 9001;"

echo "✅ Admin account restored!"
echo ""
echo "📊 Updated admin status:"
sqlite3 ../farmtimems-dev.db "SELECT Id, Name, Role, IsActive, Pin FROM Staffs WHERE Id = 9001;"

echo ""
echo "🎯 Login credentials:"
echo "Staff ID: 9001"
echo "PIN: 1234"
echo ""
echo "⚠️ Please change the PIN after logging in for security!"

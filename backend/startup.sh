#!/bin/bash

echo "🚀 Starting COMP9034 FarmTimeMS Backend..."

# Wait for database to be ready if using external database
if [ ! -z "$DATABASE_URL" ]; then
    echo "📊 Waiting for database connection..."
    sleep 5
fi

# Run database migrations
echo "🔄 Running database migrations..."
dotnet ef database update --no-build

# Check if migrations were successful
if [ $? -eq 0 ]; then
    echo "✅ Database migrations completed successfully"
else
    echo "❌ Database migrations failed"
    exit 1
fi

# Start the application
echo "🌟 Starting the application..."
exec dotnet COMP9034-Backend.dll
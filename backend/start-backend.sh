#!/bin/bash

# COMP9034 FarmTimeMS Backend Startup Script
# Usage: ./start-backend.sh

echo "🚀 Starting COMP9034 FarmTimeMS Backend Service..."

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not installed. Please install .NET 8.0 SDK first"
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "✅ .NET Version: $DOTNET_VERSION"

# Backup original files (if they exist)
if [ -f "COMP9034-Backend.csproj.backup" ]; then
    echo "⚠️  Backup files detected, skipping backup step"
else
    if [ -f "COMP9034-Backend.csproj" ]; then
        echo "💾 Backing up original project files..."
        mv COMP9034-Backend.csproj COMP9034-Backend.csproj.backup
        mv Program.cs Program.cs.backup
    fi
fi

# Use new project files
if [ -f "COMP9034-Backend-New.csproj" ]; then
    echo "🔄 Using new project configuration..."
    cp COMP9034-Backend-New.csproj COMP9034-Backend.csproj
fi

if [ -f "Program-New.cs" ]; then
    echo "🔄 Using new program entry point..."
    cp Program-New.cs Program.cs
fi

# Restore NuGet packages
echo "📦 Restoring NuGet packages..."
dotnet restore

# Add SQLite support (development environment)
echo "📦 Adding SQLite support..."
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0

# Build project
echo "🔨 Building project..."
dotnet build

if [ $? -ne 0 ]; then
    echo "❌ Project build failed"
    exit 1
fi

# Check database tools
echo "🗄️  Checking database tools..."
dotnet tool install --global dotnet-ef 2>/dev/null || echo "EF Core tools already installed"

# Create database migrations
echo "📋 Creating database migrations..."
dotnet ef migrations add InitialCreate --force 2>/dev/null || echo "Migrations already exist"

# Apply database migrations
echo "🗄️  Applying database migrations..."
dotnet ef database update

# Set environment variables
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://localhost:4000"

# Start server
echo "🌟 Starting development server..."
echo "📱 API Address: http://localhost:4000"
echo "📖 Swagger Documentation: http://localhost:4000"
echo "🔗 Health Check: http://localhost:4000/health"
echo ""
echo "Press Ctrl+C to stop server"
echo ""

dotnet run --urls="http://localhost:4000"
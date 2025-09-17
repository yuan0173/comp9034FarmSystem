#!/bin/bash

echo "🚀 Starting COMP9034 Farm Time Management System - Development Environment"
echo "=================================================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
echo -e "${BLUE}📋 Checking prerequisites...${NC}"

if ! command_exists docker; then
    echo -e "${RED}❌ Docker is not installed. Please install Docker first.${NC}"
    exit 1
fi

if ! command_exists dotnet; then
    echo -e "${RED}❌ .NET 8 SDK is not installed. Please install .NET 8 SDK first.${NC}"
    exit 1
fi

if ! command_exists node; then
    echo -e "${RED}❌ Node.js is not installed. Please install Node.js 18+ first.${NC}"
    exit 1
fi

echo -e "${GREEN}✅ All prerequisites are installed${NC}"

# Start PostgreSQL database
echo -e "${BLUE}🗄️  Starting PostgreSQL database...${NC}"
docker-compose -f docker-compose.dev.yml up -d postgres-dev

# Wait for database to be ready
echo -e "${YELLOW}⏳ Waiting for database to be ready...${NC}"
timeout=60
counter=0
while ! docker exec farmtimems-postgres-dev pg_isready -U devuser -d farmtimems >/dev/null 2>&1; do
    if [ $counter -ge $timeout ]; then
        echo -e "${RED}❌ Database failed to start within $timeout seconds${NC}"
        exit 1
    fi
    sleep 1
    counter=$((counter + 1))
    echo -n "."
done
echo -e "\n${GREEN}✅ Database is ready${NC}"

# Set environment variables
export DATABASE_URL="postgres://devuser:devpass@localhost:5434/farmtimems"
export ASPNETCORE_ENVIRONMENT=Development

# Apply database migrations
echo -e "${BLUE}📋 Applying database migrations...${NC}"
cd backend
dotnet ef database update
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Database migration failed${NC}"
    exit 1
fi
cd ..

echo -e "${GREEN}✅ Database migrations applied successfully${NC}"

# Install frontend dependencies if needed
if [ ! -d "frontendWebsite/node_modules" ]; then
    echo -e "${BLUE}📦 Installing frontend dependencies...${NC}"
    cd frontendWebsite
    npm install
    cd ..
fi

echo -e "${GREEN}🎉 Development environment is ready!${NC}"
echo ""
echo -e "${BLUE}📱 Access URLs:${NC}"
echo -e "   Frontend: ${GREEN}http://localhost:3000/comp9034FarmSystem/${NC}"
echo -e "   Backend API: ${GREEN}http://localhost:4000${NC}"
echo -e "   Swagger Docs: ${GREEN}http://localhost:4000${NC}"
echo -e "   Database: ${GREEN}postgresql://localhost:5434/farmtimems${NC}"
echo ""
echo -e "${BLUE}🔧 To start the servers:${NC}"
echo -e "   Backend: ${YELLOW}cd backend && dotnet watch run --urls=http://localhost:4000${NC}"
echo -e "   Frontend: ${YELLOW}cd frontendWebsite && npm run dev${NC}"
echo ""
echo -e "${BLUE}🛑 To stop the database:${NC}"
echo -e "   ${YELLOW}docker-compose -f docker-compose.dev.yml down${NC}"

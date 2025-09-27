```markdown
# 🚀 COMP9034 Farm Time Management System - Complete Startup Guide

## 📋 System Requirements

### Backend Requirements
- **.NET 8.0 SDK** or later
- **Entity Framework Core Tools** (auto-installed)
- **SQLite** (development, built-in support)
- **SQL Server** (production, optional)

### Frontend Requirements
- **Node.js 18+** or later
- **npm** or **yarn** package manager

## 🏗️ Project Structure
```

COMP9034-City--Farming-Industry-Time-Management-System/
├── backend/                     # .NET 8 Backend API
│   ├── src/                     # Source code (Controllers/Services/Repositories/...)
│   │   ├── Controllers/         # API Controllers
│   │   ├── Services/            # Business Logic Services
│   │   ├── Repositories/        # Data Access Layer
│   │   ├── Models/              # Data Models
│   │   ├── Data/                # EF Core DbContext
│   │   ├── DTOs/                # Transfer Objects
│   │   ├── Middlewares/         # Middlewares
│   │   └── Common/              # Exceptions/Results
│   ├── Database/                # SQL/migration scripts
│   ├── tests/                   # Utility scripts, manual tests
│   ├── Program.cs               # Entry point
│   └── COMP9034-Backend.csproj
├── frontendWebsite/             # React Frontend Application
│   ├── src/                     # Source Code
│   ├── public/                  # Static Assets
│   └── package.json             # Dependency Config
└── docs/                        # Project Documentation

```

## 🔧 Environment Setup

### 1. Clone Project
```bash
git clone <repository-url>
cd COMP9034-City--Farming-Industry-Time-Management-System
````

### 2. Backend Setup

#### Check .NET Version

```bash
dotnet --version
# Should display 8.0.x or higher
```

#### Configure Environment Variables (Optional)

```bash
cd backend
cp .env.example .env
# Edit .env file for production configs
```

### 3. Frontend Setup

#### Check Node.js Version

```bash
node --version  # Should be 18.x or higher
npm --version
```

#### Install Frontend Dependencies

```bash
cd frontendWebsite
npm install
```

#### Configure Environment Variables (Optional)

```bash
cp .env.example .env
# Edit .env file to set API URL, etc.
```

## 🚀 Startup by Environment

### 🔧 Development Environment (Recommended for daily use)

#### Option 1: Script Startup (Recommended)

```bash
# 1. Start backend (first terminal)
cd backend && ./tests/start-backend.sh

# 2. Start frontend (second terminal)
cd frontendWebsite && npm run dev
```

#### Option 2: Manual Startup

```bash
# Backend
cd backend
dotnet restore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
dotnet build
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --force
dotnet ef database update
dotnet run --urls="http://localhost:4000"

# Frontend (new terminal)
cd frontendWebsite
npm install
npm run dev
```

**Development URLs:**

* Backend API: [http://localhost:4000](http://localhost:4000)
* Frontend App: [http://localhost:3000](http://localhost:3000)
* Swagger Docs: [http://localhost:4000](http://localhost:4000)

### 🚀 Production Environment

#### Backend Startup

```bash
cd backend

export ASPNETCORE_ENVIRONMENT=Production
export JWT_SECRET_KEY="your-production-jwt-key-minimum-32-characters"
export CONNECTION_STRING_PRODUCTION="Server=your-server;Database=FarmTimeMS;User Id=your-user;Password=your-password;TrustServerCertificate=true"

dotnet build --configuration Release
dotnet run --configuration Release --urls="https://localhost:5001;http://localhost:5000"
```

#### Frontend Startup

```bash
cd frontendWebsite
export VITE_API_BASE_URL=https://flindersdevops.azurewebsites.net
npm run build
npm run preview
```

**Production URLs:**

* Backend API: [https://localhost:5001](https://localhost:5001)
* Frontend App: [http://localhost:3000](http://localhost:3000)
* Cloud Backend: [https://flindersdevops.azurewebsites.net](https://flindersdevops.azurewebsites.net)

### 🧪 Staging Environment

```bash
cd backend
export ASPNETCORE_ENVIRONMENT=Staging
export JWT_SECRET_KEY="your-staging-jwt-key"
export CONNECTION_STRING_DEFAULT="Data Source=farmtimems-staging.db"
dotnet run --urls="http://localhost:4000"
```

```bash
cd frontendWebsite
export VITE_API_BASE_URL=http://localhost:4000
export VITE_APP_ENV=staging
npm run build
npm run preview
```

### 🐳 Docker Startup

```bash
docker-compose up --build
docker-compose up -d
docker-compose ps
docker-compose logs -f
docker-compose down
```

### ☁️ Cloud Deployment

#### Azure

```bash
# Backend
cd backend
az webapp deploy --resource-group myResourceGroup --name farmtimems-api --src-path .

# Frontend
cd frontendWebsite
npm run build
az staticwebapp deploy --name farmtimems-frontend --source-location ./dist
```

#### CI/CD with Azure DevOps

```bash
git push origin main
```

### 📊 Environment Comparison

| Env     | Backend Port | Frontend Port | HTTPS        | Database   | JWT Key       |
| ------- | ------------ | ------------- | ------------ | ---------- | ------------- |
| Dev     | 4000         | 3000          | ❌            | SQLite     | Default Dev   |
| Staging | 4000         | 3000          | ❌            | SQLite     | Staging Key   |
| Prod    | 5001/5000    | 3000          | ✅            | SQL Server | Env Variable  |
| Docker  | 4000         | 3000          | Configurable | Container  | Container Key |
| Azure   | 443/80       | 443/80        | ✅            | Azure SQL  | Key Vault     |

---

## 🎯 Quick Start (Most Common)

```bash
cd backend && ./tests/start-backend.sh
cd frontendWebsite && npm run dev
```

---

## ❓ Repeated Startup

* Backend: will fail with `Address already in use`
* Frontend: Vite auto-picks new ports (3001, 3002...)

Fix:

```bash
pkill -f "dotnet.*COMP9034"
pkill -f "vite"
```

---

## 🔍 Verification

```bash
curl http://localhost:4000/health
```

Expected:

```json
{ "status": "healthy", "environment": "Development" }
```

Frontend: Open [http://localhost:3000](http://localhost:3000)

---

## ⚙️ Configurations

* Backend port: 4000
* Frontend port: 3000
* DB: SQLite for dev, SQL Server for prod
* CORS: all localhost in dev, whitelist in prod

---

## 🛠️ Common Commands

### Backend

```bash
dotnet build
dotnet test
dotnet clean
dotnet ef database drop --force
dotnet ef database update
dotnet ef migrations list
```

### Frontend

```bash
npm run dev
npm run build
npm run preview
npm run lint
npm run format
npm run lint:fix
```

---

## 🐛 Troubleshooting

### Dev

* Backend fails → check .NET version, port, DB
* Frontend fails → check backend running, CORS
* DB errors → remove `farmtimems-dev.db`, re-run migrations

### Prod

* HTTPS certs → `dotnet dev-certs https --trust`
* Missing env vars → check `echo $JWT_SECRET_KEY`
* DB issues → check connection strings and permissions

### Docker

* Check logs: `docker-compose logs backend`
* Check ports: `docker-compose ps`

### Azure

* Check logs, app settings, and CORS in Azure Portal

---

## 🔒 Security Checklist

* Dev: default keys, HTTP allowed
* Prod: strong JWT, HTTPS enforced, CORS whitelist, secure DB
* Azure: Key Vault, App Insights, AD auth

---

## 📝 Dev Workflow

1. Start dev environment
2. Frontend → edit `frontendWebsite/src/`
3. Backend → edit `backend/`
4. DB changes → migrations
5. Test API + UI
6. Commit via Git

---

🎉 **Farm Time Management System is ready!**

```markdown
```

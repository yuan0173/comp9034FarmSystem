# 🚀 COMP9034 Farm Time Management System - 完整启动指南

## 📋 系统要求

### 后端要求
- **.NET 8.0 SDK** 或更高版本
- **Entity Framework Core Tools** (自动安装)
- **SQLite** (开发环境，内置支持)
- **SQL Server** (生产环境，可选)

### 前端要求
- **Node.js 18+** 或更高版本
- **npm** 或 **yarn** 包管理器

## 🏗️ 项目结构
```
COMP9034-City--Farming-Industry-Time-Management-System/
├── backend/                    # .NET 8 后端API
│   ├── Controllers/           # API控制器
│   ├── Models/               # 数据模型
│   ├── Services/             # 业务逻辑服务
│   ├── scripts/              # 启动脚本
│   └── farmtimems-dev.db     # SQLite开发数据库
├── frontendWebsite/          # React前端应用
│   ├── src/                  # 源代码
│   ├── public/               # 静态资源
│   └── package.json         # 依赖配置
└── docs/                     # 项目文档
```

## 🔧 环境配置

### 1. 克隆项目
```bash
git clone <repository-url>
cd COMP9034-City--Farming-Industry-Time-Management-System
```

### 2. 后端环境配置

#### 检查.NET版本
```bash
dotnet --version
# 应该显示 8.0.x 或更高版本
```

#### 配置环境变量 (可选)
```bash
cd backend
cp .env.example .env
# 编辑 .env 文件，设置生产环境配置
```

### 3. 前端环境配置

#### 检查Node.js版本
```bash
node --version  # 应该是 18.x 或更高
npm --version
```

#### 安装前端依赖
```bash
cd frontendWebsite
npm install
```

#### 配置环境变量 (可选)
```bash
cp .env.example .env
# 编辑 .env 文件，设置API地址等配置
```

## 🚀 不同环境启动方式

### 🔧 开发环境 (Development) - 推荐日常使用

这是你日常开发使用的环境配置。

#### 方式一：脚本启动 (推荐)
```bash
# 1. 启动后端 (第一个终端)
cd backend && ./scripts/start-backend.sh

# 2. 启动前端 (第二个终端)  
cd frontendWebsite && npm run dev
```

#### 方式二：手动启动
```bash
# 后端手动启动
cd backend
dotnet restore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.0
dotnet build
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate --force
dotnet ef database update
dotnet run --urls="http://localhost:4000"

# 前端手动启动 (新终端)
cd frontendWebsite
npm install
npm run dev
```

**开发环境访问地址：**
- 后端API: http://localhost:4000
- 前端应用: http://localhost:3000
- Swagger文档: http://localhost:4000

### 🚀 生产环境 (Production)

#### 后端生产启动
```bash
cd backend

# 设置生产环境变量
export ASPNETCORE_ENVIRONMENT=Production
export JWT_SECRET_KEY="your-production-jwt-key-minimum-32-characters"
export CONNECTION_STRING_PRODUCTION="Server=your-server;Database=FarmTimeMS;User Id=your-user;Password=your-password;TrustServerCertificate=true"

# 或者使用.env文件
cp .env.example .env
# 编辑.env文件填入生产配置

# 构建并启动生产版本
dotnet build --configuration Release
dotnet run --configuration Release --urls="https://localhost:5001;http://localhost:5000"
```

#### 前端生产启动
```bash
cd frontendWebsite

# 设置生产API地址
export VITE_API_BASE_URL=https://flindersdevops.azurewebsites.net

# 构建生产版本
npm run build

# 预览生产构建 (本地测试)
npm run preview

# 或使用静态文件服务器
npx serve -s dist -l 3000
```

**生产环境访问地址：**
- 后端API: https://localhost:5001 (HTTPS)
- 前端应用: http://localhost:3000
- 云端后端: https://flindersdevops.azurewebsites.net

### 🧪 测试环境 (Staging)

#### 后端测试环境
```bash
cd backend

# 设置测试环境变量
export ASPNETCORE_ENVIRONMENT=Staging
export JWT_SECRET_KEY="your-staging-jwt-key"
export CONNECTION_STRING_DEFAULT="Data Source=farmtimems-staging.db"

# 启动测试环境
dotnet run --urls="http://localhost:4000"
```

#### 前端测试环境
```bash
cd frontendWebsite

# 使用测试环境API地址
export VITE_API_BASE_URL=http://localhost:4000
export VITE_APP_ENV=staging

# 构建并预览
npm run build
npm run preview
```

### 🐳 Docker容器启动 (如果有Docker配置)

```bash
# 检查Docker配置文件
ls docker-compose.yml Dockerfile

# 构建并启动所有服务
docker-compose up --build

# 后台运行
docker-compose up -d

# 查看服务状态
docker-compose ps

# 查看实时日志
docker-compose logs -f

# 停止服务
docker-compose down
```

### ☁️ 云端部署启动

#### Azure App Service部署
```bash
# 后端部署
cd backend
az webapp deploy --resource-group myResourceGroup --name farmtimems-api --src-path .

# 前端部署到Azure Static Web Apps
cd frontendWebsite
npm run build
az staticwebapp deploy --name farmtimems-frontend --source-location ./dist
```

#### 使用Azure DevOps Pipeline
```bash
# 触发自动部署 (如果配置了CI/CD)
git push origin main
```

### 📊 环境对比表

| 环境 | 后端端口 | 前端端口 | HTTPS | 数据库 | JWT密钥 |
|------|----------|----------|-------|---------|---------|
| **开发** | 4000 | 3000 | ❌ | SQLite | 开发默认 |
| **测试** | 4000 | 3000 | ❌ | SQLite | 测试专用 |
| **生产** | 5001/5000 | 3000 | ✅ | SQL Server | 环境变量 |
| **Docker** | 4000 | 3000 | 可配置 | 容器内 | 容器配置 |
| **Azure** | 443/80 | 443/80 | ✅ | Azure SQL | Key Vault |

## 🎯 快速启动指南 (最常用)

### 日常开发启动 (推荐)
```bash
# 1. 启动后端 (第一个终端)
cd backend && ./scripts/start-backend.sh

# 2. 启动前端 (第二个终端)
cd frontendWebsite && npm run dev
```

## ❓ 重复启动会怎样？

### 检查服务状态
```bash
# 检查后端是否运行
curl http://localhost:4000/health

# 检查端口占用
lsof -i :4000  # 后端
lsof -i :3000  # 前端
```

### 重复启动的结果

#### 后端重复启动
再次运行 `cd backend && ./scripts/start-backend.sh` 会报错：
```
❌ Failed to bind to address http://localhost:4000: Address already in use
```

解决方法：
```bash
# 杀死现有进程
pkill -f "dotnet.*COMP9034"
# 然后重新启动
cd backend && ./scripts/start-backend.sh
```

#### 前端重复启动
再次运行 `npm run dev` 时，Vite会：
- 自动选择新端口 (3001, 3002...)
- 或提示是否使用不同端口

### 强制重启
```bash
# 停止所有服务
pkill -f "dotnet.*COMP9034"  # 后端
pkill -f "vite"              # 前端

# 重新启动
cd backend && ./scripts/start-backend.sh
cd frontendWebsite && npm run dev
```

### 🌐 各环境访问地址总览

| 环境 | 后端API | 前端应用 | Swagger文档 | 健康检查 |
|------|---------|----------|-------------|----------|
| **开发** | http://localhost:4000 | http://localhost:3000 | http://localhost:4000 | http://localhost:4000/health |
| **生产** | https://localhost:5001 | http://localhost:3000 | https://localhost:5001 | https://localhost:5001/health |
| **Azure** | https://flindersdevops.azurewebsites.net | (根据部署配置) | https://flindersdevops.azurewebsites.net | https://flindersdevops.azurewebsites.net/health |

## 🔍 验证启动

### 1. 后端验证
访问健康检查端点：
```bash
curl http://localhost:4000/health
```

期望响应：
```json
{
  "status": "healthy",
  "timestamp": "2024-08-29T...",
  "version": "1.0.0",
  "environment": "Development"
}
```

### 2. 前端验证
- 打开浏览器访问 http://localhost:3000
- 检查网络标签确认API连接正常
- 查看控制台输出确认配置信息

## ⚙️ 配置说明

### 端口配置
- **后端端口**: 4000 (可在Program.cs或环境变量中修改)
- **前端端口**: 3000 (可在vite.config.ts中修改)

### 数据库配置
- **开发环境**: SQLite (`farmtimems-dev.db`)
- **生产环境**: SQL Server (需配置连接字符串)

### CORS配置
- **开发环境**: 自动允许所有localhost端口
- **生产环境**: 严格域名白名单控制

## 🛠️ 常用命令

### 后端命令
```bash
# 重新构建
dotnet build

# 运行测试
dotnet test

# 清理构建
dotnet clean

# 重置数据库
dotnet ef database drop --force
dotnet ef database update

# 查看迁移状态
dotnet ef migrations list
```

### 前端命令
```bash
# 开发模式
npm run dev

# 构建生产版本
npm run build

# 预览生产构建
npm run preview

# 代码检查
npm run lint

# 代码格式化
npm run format

# 修复lint问题
npm run lint:fix
```

## 🐛 不同环境常见问题

### 开发环境问题

#### 1. 后端启动失败
- **检查.NET版本**: `dotnet --version` (需要8.0+)
- **端口占用**: `lsof -ti:4000 | xargs kill -9` 或更改端口
- **数据库问题**: 删除数据库文件重新创建

#### 2. 前端无法连接后端  
- **确认后端已启动**: 访问 http://localhost:4000/health
- **检查网络标签**: 查看API请求是否成功
- **CORS错误**: 检查后端CORS配置

#### 3. 数据库错误
```bash
# 重置开发数据库
rm farmtimems-dev.db*
dotnet ef database update
```

### 生产环境问题

#### 1. HTTPS证书问题
```bash
# 生成开发证书 (仅开发测试用)
dotnet dev-certs https --trust
```

#### 2. 环境变量未设置
```bash
# 检查环境变量
echo $JWT_SECRET_KEY
echo $ASPNETCORE_ENVIRONMENT

# 设置缺失的环境变量
export JWT_SECRET_KEY="your-production-jwt-key"
```

#### 3. 数据库连接失败
- 检查生产数据库连接字符串
- 确认数据库服务器可访问
- 验证数据库用户权限

### Docker环境问题

#### 1. 容器启动失败
```bash
# 查看容器日志
docker-compose logs backend
docker-compose logs frontend

# 重建容器
docker-compose down
docker-compose up --build
```

#### 2. 端口映射问题
```bash
# 检查端口使用情况
docker-compose ps
netstat -tulpn | grep :4000
```

### Azure部署问题

#### 1. 应用启动失败
- 检查Azure App Service日志
- 确认应用设置中的环境变量
- 验证连接字符串配置

#### 2. CORS问题
- 确认Azure中的CORS设置
- 检查生产域名配置

### 通用解决方案

#### 依赖问题
```bash
# 后端清理重装
dotnet clean
dotnet restore

# 前端清理重装
rm -rf node_modules package-lock.json
npm install
```

#### 网络连接问题
```bash
# 检查防火墙设置
sudo ufw status

# 检查端口监听
netstat -tlnp | grep :4000
netstat -tlnp | grep :3000
```

## 🔒 不同环境安全注意事项

### 开发环境安全配置
- ✅ 使用默认的开发JWT密钥 (已配置)
- ✅ HTTP连接已启用 (开发便利)
- ✅ 详细错误信息已开启 (调试便利)
- ✅ CORS允许所有localhost端口 (开发便利)

### 生产环境安全要求
- ❗ **必须**设置强JWT密钥环境变量 (32字符以上)
- ❗ **必须**使用HTTPS (已配置自动启用)
- ❗ **必须**配置CORS域名白名单 (在appsettings.json中)
- ❗ **必须**使用生产级数据库 (SQL Server/Azure SQL)
- ❗ **必须**设置强数据库密码
- ❗ **建议**使用Azure Key Vault管理密钥

### 测试环境安全配置
- 使用与生产类似的安全配置
- 可以使用测试专用的JWT密钥
- 建议使用独立的测试数据库

### 云端部署安全最佳实践
- 使用Azure Key Vault存储敏感配置
- 启用Application Insights监控
- 配置Azure AD身份验证 (如适用)
- 定期更新依赖包和安全补丁

### 环境变量安全清单
| 环境变量 | 开发环境 | 生产环境 | 说明 |
|----------|----------|----------|------|
| `JWT_SECRET_KEY` | 可选 | **必须** | 32字符以上强密钥 |
| `CONNECTION_STRING_PRODUCTION` | 不需要 | **必须** | 生产数据库连接 |
| `ASPNETCORE_ENVIRONMENT` | Development | Production | 环境标识 |
| `HTTPS_PORT` | 不需要 | **建议** | HTTPS端口配置 |

## 📞 获取帮助

如果遇到问题：
1. 检查控制台输出的错误信息
2. 查看本文档的常见问题部分
3. 检查项目的Issue页面
4. 联系开发团队

## 📝 开发流程

1. **启动开发环境** (按上述步骤)
2. **前端开发**: 修改 `frontendWebsite/src/` 下的文件
3. **后端开发**: 修改 `backend/` 下的文件
4. **数据库更改**: 
   ```bash
   dotnet ef migrations add <MigrationName>
   dotnet ef database update
   ```
5. **测试**: 确保前后端通信正常
6. **提交代码**: 使用Git管理版本

---

🎉 **启动完成！** 现在你可以开始开发Farm Time Management System了。
# Render 部署配置指南

## 🚀 完整的 Render 后端部署步骤

### 1. **Render 服务配置**

#### **Web Service 基本设置**
- **Name**: `comp9034farmsystem-backend`
- **Environment**: `Docker`
- **Region**: 选择最近的区域（如 Singapore）
- **Branch**: `main`
- **Root Directory**: `backend`

#### **构建设置**
- **Build Command**: `dotnet publish -c Release -o out`
- **Start Command**: `dotnet out/COMP9034-Backend.dll`

### 2. **必需的环境变量配置**

请在 Render 控制面板的 Environment 部分添加以下环境变量：

#### **数据库配置**
```bash
# PostgreSQL 数据库连接（Render 会自动提供）
DATABASE_URL=<Render PostgreSQL 连接字符串>
```

#### **JWT 认证配置**
```bash
# JWT 密钥（必须设置，用于用户认证）
JWT_SECRET_KEY=0634178ecb250a5766e4d873595b429f

# JWT 发行者和受众
JWT_ISSUER=COMP9034-FarmTimeMS
JWT_AUDIENCE=COMP9034-FarmTimeMS-Users
```

#### **ASP.NET Core 配置**
```bash
# 运行环境
ASPNETCORE_ENVIRONMENT=Production

# 监听端口（Render 会自动设置）
PORT=10000
```

#### **CORS 配置**
```bash
# 前端域名（GitHub Pages）
CORS_ORIGINS=https://yuan0173.github.io
```

### 3. **PostgreSQL 数据库设置**

#### **创建 PostgreSQL 数据库**
1. 在 Render 控制面板中创建新的 PostgreSQL 数据库
2. **Database Name**: `farmtimems`
3. **User**: 使用 Render 生成的用户名
4. **Region**: 与 Web Service 相同区域

#### **连接数据库到 Web Service**
1. 在 Web Service 的 Environment 部分
2. 添加 `DATABASE_URL` 环境变量
3. 值设置为 PostgreSQL 数据库的 External Database URL

### 4. **部署后验证步骤**

#### **Step 1: 检查服务状态**
```bash
curl https://comp9034farmsystem-backend.onrender.com/health
```
预期响应：
```json
{
  "status": "healthy",
  "timestamp": "2025-09-17T...",
  "version": "1.0.0",
  "environment": "Production"
}
```

#### **Step 2: 检查数据库状态**
```bash
curl https://comp9034farmsystem-backend.onrender.com/api/Diagnostics/database-status
```

#### **Step 3: 强制初始化数据库（如果需要）**
```bash
curl -X POST https://comp9034farmsystem-backend.onrender.com/api/Diagnostics/force-init
```

#### **Step 4: 测试登录**
```bash
curl -X POST "https://comp9034farmsystem-backend.onrender.com/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@farmtimems.com", "password": "admin123"}'
```

### 5. **默认用户账户**

系统会自动创建以下默认用户：

| 角色 | 邮箱 | 密码 | 用途 |
|------|------|------|------|
| Admin | admin@farmtimems.com | admin123 | 系统管理员 |
| Manager | manager@farmtimems.com | manager123 | 农场经理 |
| Staff | worker@farmtimems.com | worker123 | 普通员工 |

### 6. **故障排除**

#### **常见问题及解决方案**

**问题 1: 数据库连接失败**
- 检查 `DATABASE_URL` 环境变量是否正确设置
- 确认 PostgreSQL 数据库在同一区域
- 检查数据库是否已启动

**问题 2: JWT 认证失败**
- 确认 `JWT_SECRET_KEY` 已设置
- 检查 `JWT_ISSUER` 和 `JWT_AUDIENCE` 配置
- 验证前端和后端的 JWT 配置一致

**问题 3: CORS 错误**
- 确认 `CORS_ORIGINS` 包含前端域名
- 检查前端 API 基础 URL 配置

**问题 4: 数据库为空**
- 使用诊断端点检查数据库状态
- 运行强制初始化端点
- 检查迁移是否正确应用

### 7. **监控和日志**

#### **Render 日志查看**
1. 在 Render 控制面板中打开您的 Web Service
2. 点击 "Logs" 标签查看实时日志
3. 查找数据库初始化和错误信息

#### **关键日志信息**
- `✅ Database initialization completed successfully`
- `✅ Created X default users`
- `✅ Created X default devices`

### 8. **安全建议**

#### **生产环境安全**
- 更改默认用户密码
- 使用强 JWT 密钥
- 启用 HTTPS（Render 自动提供）
- 定期备份数据库
- 监控访问日志

### 9. **性能优化**

#### **Render 服务配置**
- 选择适当的实例类型
- 启用自动扩展（如需要）
- 配置健康检查端点
- 设置适当的超时时间

---

## 📋 **配置检查清单**

- [ ] Web Service 已创建并连接到 GitHub
- [ ] PostgreSQL 数据库已创建
- [ ] `DATABASE_URL` 环境变量已设置
- [ ] JWT 相关环境变量已配置
- [ ] `ASPNETCORE_ENVIRONMENT=Production` 已设置
- [ ] CORS 配置已添加
- [ ] 服务部署成功
- [ ] 健康检查通过
- [ ] 数据库初始化成功
- [ ] 默认用户可以登录
- [ ] 前端可以连接后端

完成以上配置后，您的 COMP9034 Farm Time Management System 后端应该可以正常运行！

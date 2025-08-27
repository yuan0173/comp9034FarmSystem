# Assignment Frontend - Attendance & Payroll Management System

A modern, offline-first Progressive Web App (PWA) built with React 18, TypeScript, and Material UI for managing employee attendance, scheduling, and payroll.

## 安装和运行

### 环境要求

- Node.js 18+
- npm 或 yarn

### 安装步骤

1. 克隆项目并安装依赖

```bash
git clone <repository-url>
cd assignment-frontend
npm install
```

2. 启动开发服务器

```bash
npm run dev
```

3. 在浏览器中打开

```
http://localhost:3000
```

### 环境变量配置

复制 `env.example` 文件为 `.env` 并修改配置（可选）：

```bash
cp env.example .env
```

然后编辑 `.env` 文件：

```
VITE_API_BASE_URL=http://localhost:4000
```

如果不配置，默认使用 `http://localhost:4000`

## 登录凭据

| 角色   | 员工ID范围 | PIN         | 示例                |
| ------ | ---------- | ----------- | ------------------- |
| 员工   | 1001-7999  | 任意4位数字 | ID: 1234, PIN: 0000 |
| 经理   | 8001-8999  | 任意4位数字 | ID: 8001, PIN: 1234 |
| 管理员 | 9000+      | 任意4位数字 | ID: 9001, PIN: 9999 |

## 开发命令

```bash
npm run dev        # 启动开发服务器
npm run build      # 构建生产版本
npm run preview    # 预览生产版本
npm run lint       # 代码检查
npm run lint:fix   # 修复代码问题
npm run format     # 格式化代码
```

## 生产部署

1. 构建应用

```bash
npm run build
```

2. 部署 `dist/` 文件夹中的内容到Web服务器

支持的部署平台：

- IIS / Apache / Nginx
- Netlify / Vercel
- AWS S3 + CloudFront
- 任何静态文件托管服务

## 技术栈

- React 18 + TypeScript
- Material UI (MUI v5)
- React Query 数据管理
- Axios HTTP客户端
- Vite 构建工具
- PWA 离线支持

## API连接

后端API地址：`https://flindersdevops.azurewebsites.net/api`

详细的API连接信息请查看 `API连接文档.md`

## 浏览器支持

- Chrome/Edge 88+（推荐）
- Firefox 85+
- Safari 14+
- 支持PWA的移动浏览器

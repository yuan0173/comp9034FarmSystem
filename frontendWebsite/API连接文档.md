# 前端API连接说明

## 使用技术

前端使用 **Axios** 连接后端API

## 配置位置

**文件位置**: `src/api/http.ts`

```javascript
const httpClient = axios.create({
  baseURL: 'http://localhost:4000',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})
```

## 后端API地址

**默认地址**: http://localhost:4000

**环境变量**: 可通过 `VITE_API_BASE_URL` 环境变量修改

## 前端调用的API端点

**文件位置**: `src/api/client.ts`

### 员工管理

- `GET /api/Staffs` - 获取员工列表
- `GET /api/Staffs/{id}` - 获取单个员工
- `POST /api/Staffs` - 创建员工
- `PUT /api/Staffs/{id}` - 更新员工
- `DELETE /api/Staffs/{id}` - 删除员工

### 设备管理

- `GET /api/Devices` - 获取设备列表
- `GET /api/Devices/{id}` - 获取单个设备
- `POST /api/Devices` - 创建设备
- `PUT /api/Devices/{id}` - 更新设备
- `DELETE /api/Devices/{id}` - 删除设备

### 打卡事件

- `GET /api/Events` - 获取事件列表
- `GET /api/Events/{id}` - 获取单个事件
- `POST /api/Events` - 创建事件
- `PUT /api/Events/{id}` - 更新事件
- `DELETE /api/Events/{id}` - 删除事件

### 生物识别

- `GET /api/Biometric` - 获取生物识别数据
- `GET /api/Biometric/{id}` - 获取单个生物识别数据
- `POST /api/Biometric` - 创建生物识别数据
- `PUT /api/Biometric/{id}` - 更新生物识别数据
- `DELETE /api/Biometric/{id}` - 删除生物识别数据

## 请求配置

- **超时时间**: 10秒
- **请求头**: Content-Type: application/json
- **错误处理**: 自动处理网络错误和HTTP状态码错误

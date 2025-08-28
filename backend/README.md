# COMP9034 FarmTimeMS Backend API

Backend API service for Farm Time Management System, developed with ASP.NET Core 8.0.

## 🚀 Quick Start

### Method 1: Using Start Script (Recommended)
```bash
# Give execution permission to script
chmod +x start-backend.sh

# Start backend service
./start-backend.sh
```

### Method 2: Manual Start
```bash
# 1. Restore packages
dotnet restore

# 2. Build project
dotnet build

# 3. Run project
dotnet run --urls="http://localhost:4000"
```

## 📡 API Endpoints

### Health Check
- `GET /health` - Service health status

### Staff Management
- `GET /api/Staffs` - Get staff list
- `GET /api/Staffs/{id}` - Get specific staff
- `POST /api/Staffs` - Create staff
- `PUT /api/Staffs/{id}` - Update staff
- `DELETE /api/Staffs/{id}` - Delete staff
- `POST /api/Staffs/{id}/verify` - PIN verification

### Event Management
- `GET /api/Events` - Get event list
- `GET /api/Events/{id}` - Get specific event
- `POST /api/Events` - Create event
- `PUT /api/Events/{id}` - Update event
- `DELETE /api/Events/{id}` - Delete event
- `POST /api/Events/batch` - Batch create events
- `GET /api/Events/staff/{staffId}/today` - Get today's events

### Device Management
- `GET /api/Devices` - Get device list
- `GET /api/Devices/{id}` - Get specific device
- `POST /api/Devices` - Create device
- `PUT /api/Devices/{id}` - Update device
- `DELETE /api/Devices/{id}` - Delete device
- `PATCH /api/Devices/{id}/status` - Update device status
- `GET /api/Devices/statistics` - Device statistics
- `POST /api/Devices/{id}/test` - Test device connection

### Biometric Management
- `GET /api/Biometric` - Get biometric data
- `GET /api/Biometric/{id}` - Get specific data
- `POST /api/Biometric` - Create biometric data
- `PUT /api/Biometric/{id}` - Update data
- `DELETE /api/Biometric/{id}` - Delete data
- `POST /api/Biometric/verify` - Biometric verification
- `GET /api/Biometric/staff/{staffId}` - Get staff biometric data

## 📊 Data Models

### Staff
```json
{
  "id": 1001,
  "name": "John Doe",
  "role": "staff",
  "pin": "1234",
  "hourlyRate": 25.00,
  "email": "john@example.com",
  "phone": "13800138001",
  "address": "Farm Dormitory #1",
  "isActive": true
}
```

### Event
```json
{
  "id": 1,
  "staffId": 1001,
  "deviceId": 1,
  "eventType": "IN",
  "timestamp": "2025-08-28T08:00:00Z",
  "notes": "Normal clock in"
}
```

### Device
```json
{
  "id": 1,
  "name": "Main Entrance Terminal",
  "type": "terminal",
  "location": "Main Entrance",
  "status": "active",
  "ipAddress": "192.168.1.100"
}
```

## 🔧 Configuration

### Database Configuration
- **Development Environment**: SQLite (`farmtimems-dev.db`)
- **Production Environment**: SQL Server (configured via connection string)

### CORS Configuration
Default allowed origins:
- `http://localhost:3000`
- `http://10.14.12.177:3000`
- `http://192.168.64.1:3000`
- `http://169.254.99.235:3000`

### Logging Configuration
- **Development Environment**: Debug level logging
- **Production Environment**: Information level logging

## 🛠️ Development Tools

### Swagger Documentation
Visit `http://localhost:4000` to view complete API documentation and testing interface.

### Database Management
```bash
# Create new migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# Drop database
dotnet ef database drop
```

## 🔒 Security Features

1. **Input Validation**: All API endpoints perform parameter validation
2. **Error Handling**: Unified error response format
3. **Logging**: Complete request and error logging
4. **Data Encryption**: Biometric template data encrypted storage
5. **Soft Delete**: Important data uses soft delete strategy

## 🧪 Testing

### Sample Requests

#### Get Staff List
```bash
curl -X GET "http://localhost:4000/api/Staffs?limit=10" \
  -H "Content-Type: application/json"
```

#### Create Attendance Event
```bash
curl -X POST "http://localhost:4000/api/Events" \
  -H "Content-Type: application/json" \
  -d '{
    "staffId": 1001,
    "deviceId": 1,
    "eventType": "IN",
    "timestamp": "2025-08-28T08:00:00Z"
  }'
```

#### Staff PIN Verification
```bash
curl -X POST "http://localhost:4000/api/Staffs/1001/verify" \
  -H "Content-Type: application/json" \
  -d '"1001"'
```

## 📈 Performance Optimization

1. **Database Indexing**: Automatically creates optimized indexes
2. **Paginated Queries**: Avoids large data set queries
3. **Caching Strategy**: Entity Framework query optimization
4. **Async Processing**: All database operations are asynchronous

## 🚨 Troubleshooting

### Common Issues

1. **Port Occupied**
   ```bash
   # Find process using port 4000
   lsof -i :4000
   # Kill process
   kill -9 <PID>
   ```

2. **Database Connection Failed**
   - Check connection string configuration
   - Verify database service running status
   - Validate permission settings

3. **CORS Error**
   - Check if frontend access address is in allow list
   - Update CORS configuration in `appsettings.json`

## 📞 Support

If you encounter issues, please check:
1. Console error logs
2. Examples in Swagger documentation
3. Database connection status
4. Network configuration

---

**Development Team**: COMP9034  
**Version**: v1.0.0  
**Last Updated**: 2025-08-28
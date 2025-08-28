import { httpClient } from './http'
import {
  BiometricData,
  BiometricQuery,
  Device,
  DeviceQuery,
  Event,
  EventQuery,
  Staff,
  StaffQuery,
  // NEW - Tan Design interfaces
  WorkSchedule,
  WorkScheduleQuery,
  Salary,
  SalaryQuery,
  BiometricVerification,
  BiometricVerificationQuery,
  StaffScheduleOverview,
  SalarySummary,
  BiometricVerificationStats,
  DevicePerformanceStats,
  SuspiciousActivity
} from '../types/api_updated'

// Existing APIs (unchanged for backward compatibility)

// Biometric API
export const biometricApi = {
  getAll: async (query?: BiometricQuery): Promise<BiometricData[]> => {
    const params = new URLSearchParams()
    if (query?.staffId) params.append('staffId', query.staffId.toString())
    
    const response = await httpClient.get(`/api/Biometric?${params.toString()}`)
    return response.data
  },

  create: async (data: Omit<BiometricData, 'biometricId'>): Promise<BiometricData> => {
    const response = await httpClient.post('/api/Biometric', data)
    return response.data
  },

  getById: async (id: number): Promise<BiometricData> => {
    const response = await httpClient.get(`/api/Biometric/${id}`)
    return response.data
  },

  update: async (id: number, data: Partial<BiometricData>): Promise<void> => {
    await httpClient.put(`/api/Biometric/${id}`, data)
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/api/Biometric/${id}`)
  }
}

// Device API
export const deviceApi = {
  getAll: async (query?: DeviceQuery): Promise<Device[]> => {
    const params = new URLSearchParams()
    if (query?.query) params.append('query', query.query)
    
    const response = await httpClient.get(`/api/Devices?${params.toString()}`)
    return response.data
  },

  create: async (data: Omit<Device, 'deviceId'>): Promise<Device> => {
    const response = await httpClient.post('/api/Devices', data)
    return response.data
  },

  getById: async (id: number): Promise<Device> => {
    const response = await httpClient.get(`/api/Devices/${id}`)
    return response.data
  },

  update: async (id: number, data: Partial<Device>): Promise<void> => {
    await httpClient.put(`/api/Devices/${id}`, data)
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/api/Devices/${id}`)
  }
}

// Event API (UPDATED - supports new event types)
export const eventApi = {
  getAll: async (query?: EventQuery): Promise<Event[]> => {
    const params = new URLSearchParams()
    if (query?.staffId) params.append('staffId', query.staffId.toString())
    if (query?.deviceId) params.append('deviceId', query.deviceId.toString())
    if (query?.adminId) params.append('adminId', query.adminId.toString())
    if (query?.type) params.append('type', query.type)
    if (query?.from) params.append('from', query.from)
    if (query?.to) params.append('to', query.to)
    
    const response = await httpClient.get(`/api/Events?${params.toString()}`)
    return response.data
  },

  create: async (data: Omit<Event, 'eventId'>): Promise<Event> => {
    const response = await httpClient.post('/api/Events', data)
    return response.data
  },

  getById: async (id: number): Promise<Event> => {
    const response = await httpClient.get(`/api/Events/${id}`)
    return response.data
  },

  update: async (id: number, data: Partial<Event>): Promise<void> => {
    await httpClient.put(`/api/Events/${id}`, data)
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/api/Events/${id}`)
  }
}

// Staff API (UPDATED - supports new fields)
export const staffApi = {
  getAll: async (query?: StaffQuery): Promise<Staff[]> => {
    const params = new URLSearchParams()
    if (query?.query) params.append('query', query.query)
    
    const response = await httpClient.get(`/api/Staffs?${params.toString()}`)
    // Add computed fullName property
    return response.data.map((staff: Staff) => ({
      ...staff,
      fullName: staff.firstName && staff.lastName 
        ? `${staff.firstName} ${staff.lastName}`
        : staff.name
    }))
  },

  create: async (data: Omit<Staff, 'id'>): Promise<Staff> => {
    const response = await httpClient.post('/api/Staffs', data)
    return response.data
  },

  getById: async (id: number): Promise<Staff> => {
    const response = await httpClient.get(`/api/Staffs/${id}`)
    const staff = response.data
    // Add computed fullName property
    return {
      ...staff,
      fullName: staff.firstName && staff.lastName 
        ? `${staff.firstName} ${staff.lastName}`
        : staff.name
    }
  },

  update: async (id: number, data: Partial<Staff>): Promise<void> => {
    await httpClient.put(`/api/Staffs/${id}`, data)
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/api/Staffs/${id}`)
  }
}

// NEW - Work Schedule API (Tan Design)
export const workScheduleApi = {
  getAll: async (query?: WorkScheduleQuery): Promise<WorkSchedule[]> => {
    const params = new URLSearchParams()
    if (query?.staffId) params.append('staffId', query.staffId.toString())
    if (query?.fromDate) params.append('fromDate', query.fromDate)
    if (query?.toDate) params.append('toDate', query.toDate)
    if (query?.status) params.append('status', query.status)
    
    const response = await httpClient.get(`/api/WorkSchedules?${params.toString()}`)
    return response.data
  },

  create: async (data: Omit<WorkSchedule, 'id'>): Promise<WorkSchedule> => {
    const response = await httpClient.post('/api/WorkSchedules', data)
    return response.data
  },

  getById: async (id: number): Promise<WorkSchedule> => {
    const response = await httpClient.get(`/api/WorkSchedules/${id}`)
    return response.data
  },

  update: async (id: number, data: Partial<WorkSchedule>): Promise<void> => {
    await httpClient.put(`/api/WorkSchedules/${id}`, data)
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/api/WorkSchedules/${id}`)
  },

  // Get schedule overview for dashboard
  getOverview: async (staffId?: number): Promise<StaffScheduleOverview[]> => {
    const params = new URLSearchParams()
    if (staffId) params.append('staffId', staffId.toString())
    
    const response = await httpClient.get(`/api/WorkSchedules/overview?${params.toString()}`)
    return response.data
  }
}

// NEW - Salary API (Tan Design)
export const salaryApi = {
  getAll: async (query?: SalaryQuery): Promise<Salary[]> => {
    const params = new URLSearchParams()
    if (query?.staffId) params.append('staffId', query.staffId.toString())
    if (query?.payPeriodStart) params.append('payPeriodStart', query.payPeriodStart)
    if (query?.payPeriodEnd) params.append('payPeriodEnd', query.payPeriodEnd)
    if (query?.status) params.append('status', query.status)
    
    const response = await httpClient.get(`/api/Salaries?${params.toString()}`)
    return response.data
  },

  create: async (data: Omit<Salary, 'id'>): Promise<Salary> => {
    const response = await httpClient.post('/api/Salaries', data)
    return response.data
  },

  getById: async (id: number): Promise<Salary> => {
    const response = await httpClient.get(`/api/Salaries/${id}`)
    return response.data
  },

  update: async (id: number, data: Partial<Salary>): Promise<void> => {
    await httpClient.put(`/api/Salaries/${id}`, data)
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/api/Salaries/${id}`)
  },

  // Calculate salary for a staff member
  calculate: async (staffId: number, payPeriodStart: string, payPeriodEnd: string): Promise<Salary> => {
    const response = await httpClient.post('/api/Salaries/calculate', {
      staffId,
      payPeriodStart,
      payPeriodEnd
    })
    return response.data
  },

  // Get salary summary for dashboard
  getSummary: async (staffId?: number): Promise<SalarySummary[]> => {
    const params = new URLSearchParams()
    if (staffId) params.append('staffId', staffId.toString())
    
    const response = await httpClient.get(`/api/Salaries/summary?${params.toString()}`)
    return response.data
  }
}

// NEW - Biometric Verification API (Tan Design Core)
export const biometricVerificationApi = {
  getAll: async (query?: BiometricVerificationQuery): Promise<BiometricVerification[]> => {
    const params = new URLSearchParams()
    if (query?.staffId) params.append('staffId', query.staffId.toString())
    if (query?.deviceId) params.append('deviceId', query.deviceId.toString())
    if (query?.verificationResult) params.append('verificationResult', query.verificationResult)
    if (query?.fromDate) params.append('fromDate', query.fromDate)
    if (query?.toDate) params.append('toDate', query.toDate)
    
    const response = await httpClient.get(`/api/BiometricVerifications?${params.toString()}`)
    return response.data
  },

  getById: async (id: number): Promise<BiometricVerification> => {
    const response = await httpClient.get(`/api/BiometricVerifications/${id}`)
    return response.data
  },

  // Perform biometric verification
  verify: async (deviceId: number, capturedTemplate: string): Promise<BiometricVerification> => {
    const response = await httpClient.post('/api/BiometricVerifications/verify', {
      deviceId,
      capturedTemplate,
      ipAddress: window.location.hostname,
      userAgent: navigator.userAgent
    })
    return response.data
  },

  // Get verification statistics
  getStats: async (staffId?: number): Promise<BiometricVerificationStats[]> => {
    const params = new URLSearchParams()
    if (staffId) params.append('staffId', staffId.toString())
    
    const response = await httpClient.get(`/api/BiometricVerifications/stats?${params.toString()}`)
    return response.data
  },

  // Get device performance statistics
  getDeviceStats: async (deviceId?: number): Promise<DevicePerformanceStats[]> => {
    const params = new URLSearchParams()
    if (deviceId) params.append('deviceId', deviceId.toString())
    
    const response = await httpClient.get(`/api/BiometricVerifications/device-stats?${params.toString()}`)
    return response.data
  },

  // Get suspicious activity alerts
  getSuspiciousActivity: async (): Promise<SuspiciousActivity[]> => {
    const response = await httpClient.get('/api/BiometricVerifications/suspicious-activity')
    return response.data
  }
}

// Auth API (unchanged)
export const authApi = {
  loginWithPin: async (staffId: number, pin: string) => {
    const response = await httpClient.post('/api/Auth/login-pin', {
      staffId,
      pin
    })
    return response.data
  },

  login: async (username: string, password: string) => {
    const response = await httpClient.post('/api/Auth/login', {
      username,
      password
    })
    return response.data
  },

  getCurrentUser: async () => {
    const response = await httpClient.get('/api/Auth/me')
    return response.data
  },

  logout: async () => {
    const response = await httpClient.post('/api/Auth/logout')
    return response.data
  }
}

// NEW - Dashboard API (Tan Design Analytics)
export const dashboardApi = {
  // Get comprehensive dashboard data
  getOverview: async () => {
    const response = await httpClient.get('/api/Dashboard/overview')
    return response.data
  },

  // Get staff attendance summary
  getAttendanceSummary: async (fromDate?: string, toDate?: string) => {
    const params = new URLSearchParams()
    if (fromDate) params.append('fromDate', fromDate)
    if (toDate) params.append('toDate', toDate)
    
    const response = await httpClient.get(`/api/Dashboard/attendance-summary?${params.toString()}`)
    return response.data
  },

  // Get device health status
  getDeviceHealth: async () => {
    const response = await httpClient.get('/api/Dashboard/device-health')
    return response.data
  },

  // Get security alerts
  getSecurityAlerts: async () => {
    const response = await httpClient.get('/api/Dashboard/security-alerts')
    return response.data
  }
}

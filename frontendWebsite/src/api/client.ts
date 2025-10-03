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
  StaffCreateRequest
} from '../types/api'

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

// Event API
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

// WorkSchedule (Roster) API
export const workScheduleApi = {
  getAll: async (query?: { staffId?: number; startDate?: string; endDate?: string; limit?: number; offset?: number }) => {
    const params = new URLSearchParams()
    if (query?.staffId) params.append('staffId', String(query.staffId))
    if (query?.startDate) params.append('startDate', query.startDate)
    if (query?.endDate) params.append('endDate', query.endDate)
    if (query?.limit) params.append('limit', String(query.limit))
    if (query?.offset) params.append('offset', String(query.offset))
    const res = await httpClient.get(`/api/WorkSchedule?${params.toString()}`)
    return res.data
  },
  create: async (data: { staffId: number; date: string; startTime: string; endTime: string }) => {
    const res = await httpClient.post('/api/WorkSchedule', data)
    return res.data
  },
  update: async (id: number, data: { staffId: number; date: string; startTime: string; endTime: string }) => {
    await httpClient.put(`/api/WorkSchedule/${id}`, data)
  },
  delete: async (id: number) => {
    await httpClient.delete(`/api/WorkSchedule/${id}`)
  },
}

// Event overrides (admin only)
export const eventOverridesApi = {
  clockIn: async (staffId: number, reason?: string) => {
    const res = await httpClient.post('/api/Events/override/clock-in', { staffId, reason })
    return res.data
  },
  clockOut: async (staffId: number, reason?: string) => {
    const res = await httpClient.post('/api/Events/override/clock-out', { staffId, reason })
    return res.data
  },
}

// Login History API
export const loginHistoryApi = {
  getAll: async (query?: { limit?: number; offset?: number }): Promise<any[]> => {
    const params = new URLSearchParams()
    if (query?.limit) params.append('limit', query.limit.toString())
    if (query?.offset) params.append('offset', query.offset.toString())
    
    const response = await httpClient.get(`/api/Auth/login-logs?${params.toString()}`)
    return response.data
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/api/Auth/login-logs/${id}`)
  }
}

// Staff API
export const staffApi = {
  getAll: async (query?: StaffQuery): Promise<Staff[]> => {
    const params = new URLSearchParams()
    if (query?.query) params.append('query', query.query)
    
    const response = await httpClient.get(`/api/Staffs?${params.toString()}`)
    return response.data
  },

  create: async (data: StaffCreateRequest): Promise<Staff> => {
    const response = await httpClient.post('/api/Staffs', data)
    return response.data
  },

  getById: async (id: number): Promise<Staff> => {
    const response = await httpClient.get(`/api/Staffs/${id}`)
    return response.data
  },

  update: async (id: number, data: Partial<Staff>): Promise<void> => {
    await httpClient.put(`/api/Staffs/${id}`, data)
  },

  delete: async (id: number): Promise<void> => {
    await httpClient.delete(`/api/Staffs/${id}`)
  },

  // Get inactive (soft-deleted) staff
  getInactive: async (query?: StaffQuery): Promise<Staff[]> => {
    const params = new URLSearchParams()
    if (query?.query) params.append('search', query.query)
    
    const response = await httpClient.get(`/api/Staffs/inactive?${params.toString()}`)
    return response.data
  },

  // Restore a soft-deleted staff member
  restore: async (id: number): Promise<void> => {
    await httpClient.put(`/api/Staffs/${id}/restore`)
  }
}

// Auth API
export const authApi = {
  login: async (email: string, password: string, options?: { timeout?: number }) => {
    const response = await httpClient.post(
      '/api/Auth/login',
      { email, password },
      { timeout: options?.timeout ?? 10000 }
    )
    return response.data
  },

  register: async (firstName: string, lastName: string, email: string, password: string, confirmPassword: string, phone?: string, address?: string) => {
    const response = await httpClient.post('/api/Auth/register', {
      firstName,
      lastName,
      email,
      password,
      confirmPassword,
      phone,
      address
    })
    return response.data
  },

  changePassword: async (currentPassword: string, newPassword: string, confirmNewPassword: string) => {
    const response = await httpClient.put('/api/Auth/change-password', {
      currentPassword,
      newPassword,
      confirmNewPassword
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

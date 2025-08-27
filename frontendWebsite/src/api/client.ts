import { httpClient } from './http'
import {
  BiometricData,
  BiometricQuery,
  Device,
  DeviceQuery,
  Event,
  EventQuery,
  Staff,
  StaffQuery
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

// Staff API
export const staffApi = {
  getAll: async (query?: StaffQuery): Promise<Staff[]> => {
    const params = new URLSearchParams()
    if (query?.query) params.append('query', query.query)
    
    const response = await httpClient.get(`/api/Staffs?${params.toString()}`)
    return response.data
  },

  create: async (data: Omit<Staff, 'staffId'>): Promise<Staff> => {
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
  }
} 
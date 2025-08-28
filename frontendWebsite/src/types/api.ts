export interface BiometricData {
  biometricId: number
  staffId: number
  template: string
}

export interface Device {
  deviceId: number
  location: string
  type: string
  status: string
}

export interface Event {
  eventId: number
  staffId: number
  timeStamp: string
  eventType: string
  reason: string
  deviceId: number
  adminId: number
}

export interface Staff {
  id: number
  name: string
  email: string
  phone?: string
  address?: string
  username?: string
  pin: string
  passwordHash: string
  role: string
  hourlyRate: number
  isActive: boolean
  createdAt: string
  updatedAt: string
  events: Event[]
  biometricData: BiometricData[]
}

// Event types enumeration
export const EVENT_TYPES = {
  IN: 'IN',
  OUT: 'OUT',
  BREAK_START: 'BREAK_START',
  BREAK_END: 'BREAK_END',
  OTHER: 'OTHER'
} as const

export type EventType = typeof EVENT_TYPES[keyof typeof EVENT_TYPES]

// User roles
export type UserRole = 'staff' | 'manager' | 'admin'

// Current user interface for local storage
export interface CurrentUser {
  staffId: number
  firstName: string
  lastName: string
  role: UserRole
  pin: string
}

// API Query parameters
export interface BiometricQuery {
  staffId?: number
}

export interface DeviceQuery {
  query?: string
}

export interface EventQuery {
  staffId?: number
  deviceId?: number
  adminId?: number
  type?: string
  from?: string
  to?: string
}

export interface StaffQuery {
  query?: string
}

// Offline event queue item
export interface OfflineEvent extends Omit<Event, 'eventId'> {
  id?: string // local ID for queue management
  timestamp?: number // local timestamp for ordering
}

// Work hours calculation interfaces
export interface WorkHoursData {
  staffId: number
  workHours: number
  breakHours: number
  netHours: number
  anomalies: string[]
}

export interface PayslipData extends WorkHoursData {
  staffName: string
  grossPay: number
  standardPayRate: number
} 
// Updated API types to match Tan Design database schema

export interface Event {
  eventId: number
  staffId: number
  timeStamp: string // ISO 8601 format
  eventType: string // 'CLOCK_IN' | 'CLOCK_OUT' | 'BREAK_START' | 'BREAK_END' | 'MANUAL_OVERRIDE'
  reason: string
  deviceId: number
  adminId?: number // NEW - for manual overrides
}

// UPDATED - Staff interface with Tan Design fields
export interface Staff {
  id: number
  name: string // kept for backward compatibility
  
  // NEW - Tan Design fields
  firstName?: string
  lastName?: string
  contractType?: string // 'fulltime' | 'parttime' | 'casual'
  standardHoursPerWeek?: number
  overtimePayRate?: number
  
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
  
  // NEW - Computed property for full name
  fullName?: string
}

export interface Device {
  deviceId: number
  location: string
  type: string
  status: string
  
  // NEW - Tan Design fields
  lastHeartbeat?: string
  firmware?: string
  configData?: string
}

// UPDATED - BiometricData with security enhancements
export interface BiometricData {
  biometricId: number
  staffId: number
  template: string
  
  // NEW - Tan Design security fields
  salt?: string
  templateHash?: string
  deviceEnrollment?: number
}

// NEW - Work Schedule interface (Tan Design)
export interface WorkSchedule {
  id: number
  staffId: number
  date: string // ISO date string
  startTime: string // Time string (HH:mm)
  endTime: string // Time string (HH:mm)
  scheduleHours: number
  status: 'scheduled' | 'completed' | 'absent' | 'cancelled'
  notes?: string
  createdBy?: number
  createdAt: string
  updatedAt: string
  
  // Navigation properties
  staff?: Staff
  creator?: Staff
}

// NEW - Salary interface (Tan Design)
export interface Salary {
  id: number
  staffId: number
  payPeriodStart: string // ISO date string
  payPeriodEnd: string // ISO date string
  totalHours: number
  totalOvertimeHours: number
  scheduledHours?: number
  regularPay: number
  overtimePay: number
  grossPay: number
  deductions: number
  netPay: number
  status: 'draft' | 'calculated' | 'approved' | 'paid'
  calculatedBy?: number
  approvedBy?: number
  generatedAt: string
  approvedAt?: string
  
  // Navigation properties
  staff?: Staff
  calculator?: Staff
  approver?: Staff
}

// NEW - Biometric Verification interface (Tan Design Core)
export interface BiometricVerification {
  id: number
  staffId?: number
  biometricId?: number
  deviceId: number
  eventId?: number
  verificationResult: 'success' | 'failed' | 'no_match' | 'poor_quality' | 'timeout'
  confidenceScore?: number // 0.000 to 1.000
  failureReason?: string
  capturedTemplate?: string
  processingTime?: number // milliseconds
  ipAddress?: string
  userAgent?: string
  createdAt: string
  
  // Navigation properties
  staff?: Staff
  biometricData?: BiometricData
  device?: Device
  event?: Event
}

// Event types enumeration (UPDATED)
export const EVENT_TYPES = {
  CLOCK_IN: 'CLOCK_IN',        // Updated from 'IN'
  CLOCK_OUT: 'CLOCK_OUT',      // Updated from 'OUT'
  BREAK_START: 'BREAK_START',
  BREAK_END: 'BREAK_END',
  MANUAL_OVERRIDE: 'MANUAL_OVERRIDE', // Updated from 'OTHER'
  
  // Legacy support
  IN: 'CLOCK_IN',
  OUT: 'CLOCK_OUT',
  OTHER: 'MANUAL_OVERRIDE'
} as const

// Work Schedule Status Types (NEW)
export const SCHEDULE_STATUS = {
  SCHEDULED: 'scheduled',
  COMPLETED: 'completed',
  ABSENT: 'absent',
  CANCELLED: 'cancelled'
} as const

// Salary Status Types (NEW)
export const SALARY_STATUS = {
  DRAFT: 'draft',
  CALCULATED: 'calculated',
  APPROVED: 'approved',
  PAID: 'paid'
} as const

// Contract Types (NEW)
export const CONTRACT_TYPES = {
  FULLTIME: 'fulltime',
  PARTTIME: 'parttime',
  CASUAL: 'casual'
} as const

// Biometric Verification Results (NEW)
export const VERIFICATION_RESULTS = {
  SUCCESS: 'success',
  FAILED: 'failed',
  NO_MATCH: 'no_match',
  POOR_QUALITY: 'poor_quality',
  TIMEOUT: 'timeout'
} as const

// NEW - Query interfaces for API calls
export interface WorkScheduleQuery {
  staffId?: number
  fromDate?: string
  toDate?: string
  status?: string
}

export interface SalaryQuery {
  staffId?: number
  payPeriodStart?: string
  payPeriodEnd?: string
  status?: string
}

export interface BiometricVerificationQuery {
  staffId?: number
  deviceId?: number
  verificationResult?: string
  fromDate?: string
  toDate?: string
}

// Existing query interfaces (kept for compatibility)
export interface StaffQuery {
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

export interface DeviceQuery {
  query?: string
}

export interface BiometricQuery {
  staffId?: number
}

// NEW - Dashboard statistics interfaces
export interface StaffScheduleOverview {
  staffId: number
  fullName: string
  role: string
  date: string
  startTime: string
  endTime: string
  scheduleHours: number
  status: string
  scheduleStatus: string
}

export interface SalarySummary {
  staffId: number
  fullName: string
  role: string
  hourlyRate: number
  payPeriodStart: string
  payPeriodEnd: string
  totalHours: number
  totalOvertimeHours: number
  grossPay: number
  netPay: number
  status: string
  generatedAt: string
}

export interface BiometricVerificationStats {
  staffId: number
  staffName: string
  role: string
  totalVerifications: number
  successfulVerifications: number
  failedVerifications: number
  avgConfidenceScore?: number
  lastVerification?: string
}

export interface DevicePerformanceStats {
  deviceId: number
  deviceName: string
  type: string
  location: string
  totalVerifications: number
  successfulVerifications: number
  avgProcessingTime?: number
  verificationsLast24h: number
  lastActivity?: string
}

// NEW - Suspicious activity detection
export interface SuspiciousActivity {
  alertType: string
  staffId?: number
  staffName?: string
  deviceId: number
  deviceName: string
  eventCount: number
  description: string
  detectedAt: string
}

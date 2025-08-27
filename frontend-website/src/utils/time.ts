import { Event, EVENT_TYPES, WorkHoursData } from '../types/api'

// Parse ISO date string to Date object
export function parseISODate(dateString: string): Date {
  return new Date(dateString)
}

// Format date for display
export function formatDate(date: Date): string {
  return date.toLocaleDateString()
}

// Format time for display
export function formatTime(date: Date): string {
  return date.toLocaleTimeString()
}

// Format datetime for display
export function formatDateTime(date: Date): string {
  return `${formatDate(date)} ${formatTime(date)}`
}

// Calculate duration in hours between two dates
export function calculateHours(start: Date, end: Date): number {
  const diffMs = end.getTime() - start.getTime()
  return diffMs / (1000 * 60 * 60) // Convert to hours
}

// Event pair interface
interface EventPair {
  start: Event
  end?: Event
  duration?: number
  anomaly?: string
}

// Calculate work hours for a single staff member
export function calculateWorkHours(
  staffId: number,
  events: Event[],
  fromDate: Date,
  toDate: Date
): WorkHoursData {
  // Filter events for this staff member and date range
  const staffEvents = events
    .filter(event => event.staffId === staffId)
    .filter(event => {
      const eventDate = parseISODate(event.timeStamp)
      return eventDate >= fromDate && eventDate <= toDate
    })
    .sort((a, b) => parseISODate(a.timeStamp).getTime() - parseISODate(b.timeStamp).getTime())

  let workHours = 0
  let breakHours = 0
  const anomalies: string[] = []

  // Process IN/OUT pairs
  const workPairs = pairEvents(staffEvents, EVENT_TYPES.IN, EVENT_TYPES.OUT)
  for (const pair of workPairs) {
    if (pair.duration) {
      workHours += pair.duration
    }
    if (pair.anomaly) {
      anomalies.push(pair.anomaly)
    }
  }

  // Process BREAK pairs
  const breakPairs = pairEvents(staffEvents, EVENT_TYPES.BREAK_START, EVENT_TYPES.BREAK_END)
  for (const pair of breakPairs) {
    if (pair.duration) {
      breakHours += pair.duration
    }
    if (pair.anomaly) {
      anomalies.push(pair.anomaly)
    }
  }

  const netHours = Math.max(0, workHours - breakHours)

  return {
    staffId,
    workHours: Math.round(workHours * 100) / 100, // Round to 2 decimal places
    breakHours: Math.round(breakHours * 100) / 100,
    netHours: Math.round(netHours * 100) / 100,
    anomalies
  }
}

// Pair start and end events
function pairEvents(events: Event[], startType: string, endType: string): EventPair[] {
  const pairs: EventPair[] = []
  const stack: Event[] = []

  for (const event of events) {
    if (event.eventType === startType) {
      stack.push(event)
    } else if (event.eventType === endType) {
      if (stack.length > 0) {
        const start = stack.pop()!
        const startTime = parseISODate(start.timeStamp)
        const endTime = parseISODate(event.timeStamp)
        const duration = calculateHours(startTime, endTime)

        pairs.push({
          start,
          end: event,
          duration: duration > 0 ? duration : undefined,
          anomaly: duration <= 0 ? `Invalid ${startType}-${endType} pair: negative or zero duration` : undefined
        })
      } else {
        pairs.push({
          start: event, // Using end event as start for anomaly tracking
          anomaly: `Unpaired ${endType} event`
        })
      }
    }
  }

  // Handle unpaired start events
  for (const unpairedStart of stack) {
    pairs.push({
      start: unpairedStart,
      anomaly: `Unpaired ${startType} event`
    })
  }

  return pairs
}

// Calculate work hours for multiple staff members
export function calculateAllWorkHours(
  events: Event[],
  fromDate: Date,
  toDate: Date
): WorkHoursData[] {
  // Get unique staff IDs
  const staffIds = [...new Set(events.map(event => event.staffId))]
  
  return staffIds.map(staffId => 
    calculateWorkHours(staffId, events, fromDate, toDate)
  ).filter(data => data.workHours > 0 || data.breakHours > 0 || data.anomalies.length > 0)
}

// Get currently active staff (clocked in but not out)
export function getActiveStaff(events: Event[], hoursThreshold = 8): number[] {
  const now = new Date()
  const thresholdTime = new Date(now.getTime() - hoursThreshold * 60 * 60 * 1000)
  
  // Get recent events
  const recentEvents = events
    .filter(event => parseISODate(event.timeStamp) >= thresholdTime)
    .sort((a, b) => parseISODate(b.timeStamp).getTime() - parseISODate(a.timeStamp).getTime())

  const staffStatus = new Map<number, string>()

  // Track last event for each staff member
  for (const event of recentEvents) {
    if (!staffStatus.has(event.staffId)) {
      staffStatus.set(event.staffId, event.eventType)
    }
  }

  // Return staff IDs who last clocked IN
  return Array.from(staffStatus.entries())
    .filter(([_, lastEvent]) => lastEvent === EVENT_TYPES.IN)
    .map(([staffId]) => staffId)
}

// Format hours for display (e.g., "8.5" -> "8h 30m")
export function formatHours(hours: number): string {
  const wholeHours = Math.floor(hours)
  const minutes = Math.round((hours - wholeHours) * 60)
  
  if (minutes === 0) {
    return `${wholeHours}h`
  } else {
    return `${wholeHours}h ${minutes}m`
  }
} 
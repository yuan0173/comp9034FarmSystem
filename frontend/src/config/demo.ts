// Demo configuration
// Set to false when connecting to real backend
export const DEMO_MODE = true

// Demo mode messages
export const DEMO_MESSAGES = {
  dashboard:
    '🎯 Demo Mode: Displaying mock dashboard data with simulated devices and staff.',
  station:
    '🎯 Demo Mode: Clock events are stored locally. In production, they sync with the backend.',
  roster:
    '🎯 Demo Mode: Showing sample roster data. In production, this loads from the backend schedule system.',
  attendance:
    '🎯 Demo Mode: Using mock attendance events. In production, this queries real event data from the API.',
  payslips:
    '🎯 Demo Mode: Calculating payslips from mock data. In production, this uses real attendance and payroll data.',
  adminStaffs:
    '🎯 Demo Mode: Displaying mock staff data. In production, this manages real employee records.',
  adminDevices:
    '🎯 Demo Mode: Showing mock device data. In production, this manages real attendance devices.',
  adminEvents:
    '🎯 Demo Mode: Displaying mock event logs. In production, this queries real attendance events.',
}

// Backend connection status
let backendConnected = false
let lastConnectionCheck = 0
const CONNECTION_CHECK_INTERVAL = 30000 // 30 seconds

// Check backend connectivity
export const checkBackendConnection = async (): Promise<boolean> => {
  const now = Date.now()

  // Avoid frequent checks
  if (
    now - lastConnectionCheck < CONNECTION_CHECK_INTERVAL &&
    backendConnected
  ) {
    return backendConnected
  }

  try {
    const baseURL =
      import.meta.env.VITE_API_BASE_URL ||
      'https://flindersdevops.azurewebsites.net'

    // Try to fetch a simple endpoint (like getting staffs with limit)
    const response = await fetch(`${baseURL}/api/Staffs?limit=1`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      // Short timeout for connection check
      signal: AbortSignal.timeout(5000), // 5 second timeout
    })

    // If we get any response (even 404 or 500), the backend is reachable
    const isConnected = response.status !== 0 && response.status < 600

    backendConnected = isConnected
    lastConnectionCheck = now

    console.log(
      `Backend connection check: ${isConnected ? 'Connected' : 'Failed'}`,
      {
        status: response.status,
        url: `${baseURL}/api/Staffs`,
      }
    )

    return isConnected
  } catch (error) {
    console.log('Backend connection check failed:', error)
    backendConnected = false
    lastConnectionCheck = now
    return false
  }
}

// Check if we should use demo mode
export const isDemoMode = async (): Promise<boolean> => {
  // If DEMO_MODE is explicitly set to false, never use demo mode
  if (!DEMO_MODE) {
    return false
  }

  // If DEMO_MODE is true, check backend connectivity
  const connected = await checkBackendConnection()

  // Use demo mode only if backend is not connected
  return !connected
}

// Synchronous version for immediate use (uses cached result)
export const isDemoModeSync = (): boolean => {
  if (!DEMO_MODE) {
    return false
  }

  // Use cached connection status
  return !backendConnected
}

// Get demo message for a specific page
export const getDemoMessage = async (
  page: keyof typeof DEMO_MESSAGES
): Promise<string | null> => {
  const demoMode = await isDemoMode()
  return demoMode ? DEMO_MESSAGES[page] : null
}

// Synchronous version for immediate use
export const getDemoMessageSync = (
  page: keyof typeof DEMO_MESSAGES
): string | null => {
  const demoMode = isDemoModeSync()
  return demoMode ? DEMO_MESSAGES[page] : null
}

// Force a connection check (useful for manual refresh)
export const forceConnectionCheck = async (): Promise<boolean> => {
  lastConnectionCheck = 0 // Reset cache
  return await checkBackendConnection()
}

// Get current backend connection status
export const getBackendStatus = () => ({
  connected: backendConnected,
  lastCheck: lastConnectionCheck,
  nextCheck: lastConnectionCheck + CONNECTION_CHECK_INTERVAL,
})

import { useState, useEffect, useCallback } from 'react'

export interface BackendConnectionState {
  isConnected: boolean
  isChecking: boolean
  lastCheck: number
  error: string | null
}

// Simple backend connection check
const checkBackendConnection = async (): Promise<boolean> => {
  try {
    const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:4000'

    const response = await fetch(`${baseURL}/api/Staffs?limit=1`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      signal: AbortSignal.timeout(5000), // 5 second timeout
    })

    const isConnected = response.status !== 0 && response.status < 600
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
    return false
  }
}

export const useBackendConnection = () => {
  const [state, setState] = useState<BackendConnectionState>({
    isConnected: false,
    isChecking: false,
    lastCheck: 0,
    error: null,
  })

  // Check connection status
  const checkConnection = useCallback(async () => {
    setState(prev => ({ ...prev, isChecking: true, error: null }))

    try {
      const isConnected = await checkBackendConnection()

      setState({
        isConnected,
        isChecking: false,
        lastCheck: Date.now(),
        error: null,
      })

      return isConnected
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : 'Connection check failed'

      setState(prev => ({
        ...prev,
        isChecking: false,
        error: errorMessage,
      }))

      return false
    }
  }, [])

  // Initialize and set up periodic checks
  useEffect(() => {
    // Initial check
    checkConnection()

    // Set up periodic checks every 30 seconds
    const interval = setInterval(() => {
      checkConnection()
    }, 30000)

    // Check when window regains focus (user comes back to app)
    const handleFocus = () => {
      checkConnection()
    }

    // Check when network status changes
    const handleOnline = () => {
      setTimeout(() => checkConnection(), 1000) // Delay to allow network to stabilize
    }

    const handleOffline = () => {
      setState(prev => ({
        ...prev,
        isConnected: false,
      }))
    }

    window.addEventListener('focus', handleFocus)
    window.addEventListener('online', handleOnline)
    window.addEventListener('offline', handleOffline)

    return () => {
      clearInterval(interval)
      window.removeEventListener('focus', handleFocus)
      window.removeEventListener('online', handleOnline)
      window.removeEventListener('offline', handleOffline)
    }
  }, [checkConnection])

  return {
    ...state,
    checkConnection,
  }
}

import { useState, useEffect, useCallback } from 'react'
import {
  checkBackendConnection,
  isDemoModeSync,
  forceConnectionCheck,
  getBackendStatus,
} from '../config/demo'

export interface BackendConnectionState {
  isConnected: boolean
  isDemoMode: boolean
  isChecking: boolean
  lastCheck: number
  error: string | null
}

export const useBackendConnection = () => {
  const [state, setState] = useState<BackendConnectionState>({
    isConnected: false,
    isDemoMode: true,
    isChecking: false,
    lastCheck: 0,
    error: null,
  })

  // Check connection status
  const checkConnection = useCallback(async (force = false) => {
    setState(prev => ({ ...prev, isChecking: true, error: null }))

    try {
      const isConnected = force
        ? await forceConnectionCheck()
        : await checkBackendConnection()

      const isDemoMode = isDemoModeSync()
      const status = getBackendStatus()

      setState({
        isConnected,
        isDemoMode,
        isChecking: false,
        lastCheck: status.lastCheck,
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

  // Force connection check
  const forceCheck = useCallback(() => {
    return checkConnection(true)
  }, [checkConnection])

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
      setTimeout(() => checkConnection(true), 1000) // Delay to allow network to stabilize
    }

    const handleOffline = () => {
      setState(prev => ({
        ...prev,
        isConnected: false,
        isDemoMode: true,
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
    checkConnection: forceCheck,
  }
}

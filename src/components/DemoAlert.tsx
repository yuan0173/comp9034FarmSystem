import React from 'react'
import { Alert, AlertProps, Box, Button } from '@mui/material'
import { Refresh } from '@mui/icons-material'
import { getDemoMessageSync, DEMO_MESSAGES } from '../config/demo'
import { useBackendConnection } from '../hooks/useBackendConnection'

interface DemoAlertProps extends Omit<AlertProps, 'children'> {
  page: keyof typeof DEMO_MESSAGES
  customMessage?: string
}

export function DemoAlert({
  page,
  customMessage,
  ...alertProps
}: DemoAlertProps) {
  const { isDemoMode, isChecking, checkConnection, isConnected } =
    useBackendConnection()

  // If not in demo mode, don't show anything
  if (!isDemoMode) {
    return null
  }

  const message = customMessage || (isDemoMode ? DEMO_MESSAGES[page] : null)

  if (!message) {
    return null
  }

  return (
    <Alert
      severity="info"
      sx={{ mb: 3, ...alertProps.sx }}
      {...alertProps}
      action={
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {!isConnected && (
            <Button
              color="inherit"
              size="small"
              onClick={checkConnection}
              disabled={isChecking}
              startIcon={<Refresh />}
            >
              {isChecking ? 'Checking...' : 'Check Backend'}
            </Button>
          )}
        </Box>
      }
    >
      <Box>
        {message}
        {!isConnected && (
          <Box
            component="span"
            sx={{ display: 'block', mt: 1, fontSize: '0.875em', opacity: 0.8 }}
          >
            💡 The app will automatically switch to production mode when backend
            connection is detected.
          </Box>
        )}
      </Box>
    </Alert>
  )
}

import React from 'react'
import { Box, Chip, Tooltip, CircularProgress } from '@mui/material'
import { CloudDone, CloudOff, Info } from '@mui/icons-material'
import { useBackendConnection } from '../hooks/useBackendConnection'

export function BackendStatus() {
  const { isConnected, isDemoMode, isChecking, checkConnection, error } =
    useBackendConnection()

  const getStatusConfig = () => {
    if (isChecking) {
      return {
        label: 'Checking...',
        color: 'default' as const,
        icon: <CircularProgress size={16} />,
        tooltip: 'Checking backend connection...',
      }
    }

    if (isDemoMode) {
      return {
        label: 'Demo Mode',
        color: 'warning' as const,
        icon: <Info />,
        tooltip: 'Using demo data. Backend will be detected automatically.',
      }
    }

    if (isConnected) {
      return {
        label: 'Backend Connected',
        color: 'success' as const,
        icon: <CloudDone />,
        tooltip: 'Connected to backend API. Using real data.',
      }
    }

    return {
      label: 'Backend Offline',
      color: 'error' as const,
      icon: <CloudOff />,
      tooltip: error || 'Backend API is not reachable. Using demo data.',
    }
  }

  const statusConfig = getStatusConfig()

  return (
    <Tooltip title={statusConfig.tooltip}>
      <Chip
        label={statusConfig.label}
        color={statusConfig.color}
        size="small"
        icon={statusConfig.icon}
        onClick={!isChecking ? checkConnection : undefined}
        sx={{
          cursor: !isChecking ? 'pointer' : 'default',
          '& .MuiChip-icon': {
            fontSize: '16px',
          },
        }}
      />
    </Tooltip>
  )
}

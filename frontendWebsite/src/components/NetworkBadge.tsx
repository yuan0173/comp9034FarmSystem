import { useState, useEffect } from 'react'
import {
  Badge,
  IconButton,
  Tooltip,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
} from '@mui/material'
import { Wifi, WifiOff, CloudQueue, Sync, Warning } from '@mui/icons-material'
import { getSyncStatus, flushQueue, type SyncStatus } from '../offline/sync'
import { formatDateTime } from '../utils/time'
import { useBackendConnection } from '../hooks/useBackendConnection'

export function NetworkBadge() {
  const { isConnected: backendConnected } = useBackendConnection()
  const [syncStatus, setSyncStatus] = useState<SyncStatus>({
    isOnline: navigator.onLine && backendConnected,
    queueLength: 0,
    lastSync: null,
    isSyncing: false,
  })
  const [dialogOpen, setDialogOpen] = useState(false)
  const [isManualSyncing, setIsManualSyncing] = useState(false)

  // Update sync status periodically
  useEffect(() => {
    const updateStatus = async () => {
      const status = await getSyncStatus()
      // Override isOnline with actual backend connection status
      setSyncStatus({
        ...status,
        isOnline: navigator.onLine && backendConnected,
      })
    }

    updateStatus()

    const interval = setInterval(updateStatus, 5000) // Update every 5 seconds

    // Listen for online/offline events
    const handleOnline = () => updateStatus()
    const handleOffline = () => updateStatus()

    window.addEventListener('online', handleOnline)
    window.addEventListener('offline', handleOffline)

    return () => {
      clearInterval(interval)
      window.removeEventListener('online', handleOnline)
      window.removeEventListener('offline', handleOffline)
    }
  }, [backendConnected])

  const handleManualSync = async () => {
    if (!syncStatus.isOnline || syncStatus.isSyncing) return

    setIsManualSyncing(true)
    try {
      await flushQueue()
      // Update status after sync
      const newStatus = await getSyncStatus()
      setSyncStatus(newStatus)
    } catch (error) {
      console.error('Manual sync failed:', error)
    } finally {
      setIsManualSyncing(false)
    }
  }

  const getStatusIcon = () => {
    if (!syncStatus.isOnline) {
      return <WifiOff color="error" />
    }

    if (syncStatus.isSyncing || isManualSyncing) {
      return <Sync className="animate-spin" color="primary" />
    }

    if (syncStatus.queueLength > 0) {
      return <CloudQueue color="warning" />
    }

    return <Wifi color="success" />
  }

  const getStatusText = () => {
    if (!navigator.onLine) {
      return 'No Network'
    }

    if (!backendConnected) {
      return 'Backend Offline'
    }

    if (syncStatus.isSyncing || isManualSyncing) {
      return 'Syncing...'
    }

    if (syncStatus.queueLength > 0) {
      return `${syncStatus.queueLength} pending`
    }

    return 'Online'
  }

  const getStatusColor = (): 'success' | 'warning' | 'error' | 'default' => {
    if (!syncStatus.isOnline) return 'error'
    if (syncStatus.queueLength > 0) return 'warning'
    return 'success'
  }

  return (
    <>
      <Tooltip title="Network & Sync Status">
        <Badge
          badgeContent={
            syncStatus.queueLength > 0 ? syncStatus.queueLength : undefined
          }
          color="warning"
        >
          <IconButton onClick={() => setDialogOpen(true)} color="inherit">
            {getStatusIcon()}
          </IconButton>
        </Badge>
      </Tooltip>

      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)}>
        <DialogTitle>Network & Sync Status</DialogTitle>
        <DialogContent>
          <Box sx={{ minWidth: 300, py: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
              {getStatusIcon()}
              <Chip
                label={getStatusText()}
                color={getStatusColor()}
                variant="outlined"
              />
            </Box>

            <Typography variant="body2" color="textSecondary" sx={{ mb: 1 }}>
              Network: {navigator.onLine ? 'Connected' : 'Disconnected'}
            </Typography>

            <Typography variant="body2" color="textSecondary" sx={{ mb: 1 }}>
              Backend: {backendConnected ? 'Connected' : 'Disconnected'}
            </Typography>

            <Typography variant="body2" color="textSecondary" sx={{ mb: 1 }}>
              Queue: {syncStatus.queueLength} events pending
            </Typography>

            {syncStatus.lastSync && (
              <Typography variant="body2" color="textSecondary" sx={{ mb: 1 }}>
                Last sync: {formatDateTime(syncStatus.lastSync)}
              </Typography>
            )}

            {syncStatus.queueLength > 0 && (
              <Box
                sx={{ mt: 2, p: 2, bgcolor: 'warning.light', borderRadius: 1 }}
              >
                <Box
                  sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}
                >
                  <Warning color="warning" fontSize="small" />
                  <Typography variant="body2" fontWeight="bold">
                    Offline Events Queued
                  </Typography>
                </Box>
                <Typography variant="caption" color="textSecondary">
                  {syncStatus.queueLength} events will be synchronized when
                  connection is restored.
                </Typography>
              </Box>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          {syncStatus.isOnline && syncStatus.queueLength > 0 && (
            <Button
              onClick={handleManualSync}
              disabled={syncStatus.isSyncing || isManualSyncing}
              startIcon={<Sync />}
            >
              {isManualSyncing ? 'Syncing...' : 'Sync Now'}
            </Button>
          )}
          <Button onClick={() => setDialogOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </>
  )
}

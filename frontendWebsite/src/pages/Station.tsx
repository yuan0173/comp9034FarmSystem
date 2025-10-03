import { useState, useEffect } from 'react'
import {
  Box,
  Card,
  CardContent,
  Button,
  Typography,
  Grid,
  Chip,
  Alert,
  Paper,
  Divider,
} from '@mui/material'
import {
  Login,
  Logout,
  Coffee,
  RestaurantMenu,
  Schedule,
  CheckCircle,
  Warning,
} from '@mui/icons-material'
import { CurrentUser, EVENT_TYPES, Event } from '../types/api'
import { enqueueEvent } from '../offline/sync'
import { eventApi, eventOverridesApi } from '../api/client'
import { formatDateTime } from '../utils/time'
import { useQuery } from '@tanstack/react-query'

interface StationProps {
  currentUser: CurrentUser
}

interface WorkStatus {
  status: 'clocked_out' | 'clocked_in' | 'on_break'
  lastEvent?: Event
  lastEventTime?: string
}

export function Station({ currentUser }: StationProps) {
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [message, setMessage] = useState<{
    type: 'success' | 'error' | 'info'
    text: string
  } | null>(null)
  const [workStatus, setWorkStatus] = useState<WorkStatus>({
    status: 'clocked_out',
  })

  // Fetch recent events from API
  const { data: recentEvents = [], refetch: refetchEvents } = useQuery({
    queryKey: ['events', 'recent', currentUser.staffId],
    queryFn: async () => {
      const today = new Date()
      const yesterday = new Date(today.getTime() - 24 * 60 * 60 * 1000)

      return eventApi.getAll({
        staffId: currentUser.staffId,
        from: yesterday.toISOString(),
        to: today.toISOString(),
      })
    },
    retry: 1,
    refetchOnWindowFocus: false,
  })

  // Determine work status from recent events
  useEffect(() => {
    if (recentEvents && recentEvents.length > 0) {
      const sortedEvents = recentEvents.sort(
        (a, b) =>
          new Date(b.timeStamp).getTime() - new Date(a.timeStamp).getTime()
      )
      const lastEvent = sortedEvents[0]

      let status: WorkStatus['status'] = 'clocked_out'

      switch (lastEvent.eventType) {
        case EVENT_TYPES.IN:
          status = 'clocked_in'
          break
        case EVENT_TYPES.OUT:
          status = 'clocked_out'
          break
        case EVENT_TYPES.BREAK_START:
          status = 'on_break'
          break
        case EVENT_TYPES.BREAK_END:
          status = 'clocked_in'
          break
      }

      setWorkStatus({
        status,
        lastEvent,
        lastEventTime: formatDateTime(new Date(lastEvent.timeStamp)),
      })
    }
  }, [recentEvents])

  // Clear message after 5 seconds
  useEffect(() => {
    if (message) {
      const timer = setTimeout(() => setMessage(null), 5000)
      return () => clearTimeout(timer)
    }
  }, [message])

  const createEvent = async (eventType: string, reason = '') => {
    setIsSubmitting(true)
    setMessage(null)

    try {
      const eventDraft = {
        staffId: currentUser.staffId,
        timeStamp: new Date().toISOString(),
        eventType,
        reason,
        deviceId: 0, // Default device ID (could be determined by location/device)
        adminId: 0,
      }

      // Try online first for immediate feedback (use existing offline queue as fallback)
      try {
        await eventApi.create(eventDraft as any)
      } catch (err: any) {
        // If known business error, show message and offer override for admin
        const code = err?.code || err?.error || ''
        const msg = err?.message || 'Failed to record event.'

        // Business codes from backend: NO_ROSTER, DUPLICATE_CLOCK_IN/OUT, NOT_CLOCKED_IN, ALREADY_CLOCKED_IN
        if (code) {
          setMessage({ type: 'error', text: `${msg} (${code})` })

          // Admin override flow for clock-in/out blocks
          const isAdmin = currentUser.role === 'admin'
          const canOverride = isAdmin && (code === 'NO_ROSTER' || code === 'NOT_CLOCKED_IN')
          if (canOverride) {
            const reason = window.prompt('Override reason (required):') || ''
            if (reason.trim()) {
              try {
                if (eventType === 'IN') {
                  await eventOverridesApi.clockIn(currentUser.staffId, reason.trim())
                } else if (eventType === 'OUT') {
                  await eventOverridesApi.clockOut(currentUser.staffId, reason.trim())
                }
                setMessage({ type: 'success', text: 'Override recorded successfully.' })
                refetchEvents()
                setIsSubmitting(false)
                return
              } catch (e: any) {
                setMessage({ type: 'error', text: e?.message || 'Override failed.' })
                setIsSubmitting(false)
                return
              }
            }
          }

          setIsSubmitting(false)
          return
        }

        // If not a known business error, fallback to offline queue
        await enqueueEvent(eventDraft)
      }

      setMessage({
        type: 'success',
        text: navigator.onLine
          ? `${eventType} recorded successfully!`
          : `${eventType} saved offline - will sync when connection restored`,
      })

      // Refresh events after a short delay
      setTimeout(() => {
        refetchEvents()
      }, 1000)
    } catch (error) {
      console.error('Error creating event:', error)
      setMessage({
        type: 'error',
        text: 'Failed to record event. Please try again.',
      })
    } finally {
      setIsSubmitting(false)
    }
  }

  const getStatusDisplay = () => {
    switch (workStatus.status) {
      case 'clocked_in':
        return {
          label: 'CLOCKED IN',
          color: 'success' as const,
          icon: <CheckCircle />,
        }
      case 'on_break':
        return {
          label: 'ON BREAK',
          color: 'warning' as const,
          icon: <Coffee />,
        }
      case 'clocked_out':
      default:
        return {
          label: 'CLOCKED OUT',
          color: 'default' as const,
          icon: <Schedule />,
        }
    }
  }

  const statusDisplay = getStatusDisplay()

  return (
    <Box sx={{ maxWidth: 800, mx: 'auto', p: 2 }}>
      {/* Header */}
      <Paper elevation={2} sx={{ p: 3, mb: 3, textAlign: 'center' }}>
        <Typography variant="h4" gutterBottom>
          Welcome, {currentUser.firstName}!
        </Typography>
        <Typography variant="subtitle1" color="textSecondary">
          Clock Station - Staff ID: {currentUser.staffId}
        </Typography>
      </Paper>

      {/* Current Status */}
      <Card sx={{ mb: 3 }}>
        <CardContent sx={{ textAlign: 'center', py: 4 }}>
          <Box sx={{ mb: 2 }}>{statusDisplay.icon}</Box>
          <Chip
            label={statusDisplay.label}
            color={statusDisplay.color}
            sx={{ fontSize: '1.2rem', py: 3, px: 2, mb: 2 }}
          />
          {workStatus.lastEventTime && (
            <Typography variant="body2" color="textSecondary">
              Last activity: {workStatus.lastEventTime}
            </Typography>
          )}
        </CardContent>
      </Card>

      {/* Message Display */}
      {message && (
        <Alert severity={message.type} sx={{ mb: 3 }}>
          {message.text}
        </Alert>
      )}

      {/* Action Buttons */}
      <Grid container spacing={2}>
        <Grid item xs={12} sm={6}>
          <Button
            fullWidth
            size="large"
            variant="contained"
            color="success"
            startIcon={<Login />}
            onClick={() => createEvent(EVENT_TYPES.IN)}
            disabled={
              isSubmitting ||
              workStatus.status === 'clocked_in' ||
              workStatus.status === 'on_break'
            }
            sx={{ py: 3, fontSize: '1.1rem' }}
          >
            Clock In
          </Button>
        </Grid>

        <Grid item xs={12} sm={6}>
          <Button
            fullWidth
            size="large"
            variant="contained"
            color="error"
            startIcon={<Logout />}
            onClick={() => createEvent(EVENT_TYPES.OUT)}
            disabled={isSubmitting || workStatus.status === 'clocked_out'}
            sx={{ py: 3, fontSize: '1.1rem' }}
          >
            Clock Out
          </Button>
        </Grid>

        <Grid item xs={12} sm={6}>
          <Button
            fullWidth
            size="large"
            variant="outlined"
            color="warning"
            startIcon={<Coffee />}
            onClick={() => createEvent(EVENT_TYPES.BREAK_START)}
            disabled={isSubmitting || workStatus.status !== 'clocked_in'}
            sx={{ py: 3, fontSize: '1.1rem' }}
          >
            Start Break
          </Button>
        </Grid>

        <Grid item xs={12} sm={6}>
          <Button
            fullWidth
            size="large"
            variant="outlined"
            color="info"
            startIcon={<RestaurantMenu />}
            onClick={() => createEvent(EVENT_TYPES.BREAK_END)}
            disabled={isSubmitting || workStatus.status !== 'on_break'}
            sx={{ py: 3, fontSize: '1.1rem' }}
          >
            End Break
          </Button>
        </Grid>
      </Grid>

      {/* Recent Activity */}
      {recentEvents && recentEvents.length > 0 && (
        <Card sx={{ mt: 4 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Recent Activity
            </Typography>
            <Divider sx={{ mb: 2 }} />
            {recentEvents.slice(0, 5).map((event, index) => (
              <Box key={event.eventId} sx={{ mb: 2, pb: 1 }}>
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                  }}
                >
                  <Chip
                    label={event.eventType.replace('_', ' ')}
                    size="small"
                    color={
                      event.eventType === EVENT_TYPES.IN
                        ? 'success'
                        : event.eventType === EVENT_TYPES.OUT
                          ? 'error'
                          : 'warning'
                    }
                  />
                  <Typography variant="body2" color="textSecondary">
                    {formatDateTime(new Date(event.timeStamp))}
                  </Typography>
                </Box>
                {event.reason && (
                  <Typography
                    variant="caption"
                    color="textSecondary"
                    sx={{ mt: 0.5, display: 'block' }}
                  >
                    Reason: {event.reason}
                  </Typography>
                )}
                {index < 4 && <Divider sx={{ mt: 1 }} />}
              </Box>
            ))}
          </CardContent>
        </Card>
      )}

      {/* Offline Warning */}
      {!navigator.onLine && (
        <Alert severity="warning" sx={{ mt: 3 }} icon={<Warning />}>
          You are currently offline. Clock events will be saved locally and
          synchronized when connection is restored.
        </Alert>
      )}
    </Box>
  )
}

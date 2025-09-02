import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { eventApi } from '../api/client'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  IconButton,
  Button,
  TextField,
  InputAdornment,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from '@mui/material'
import {
  Schedule,
  Search,
  Edit,
  Delete,
  Refresh,
  FilterList,
} from '@mui/icons-material'
import { CurrentUser } from '../types/api'
import { formatDateTime } from '../utils/time'

interface AdminEventsProps {
  currentUser: CurrentUser
}

export function AdminEvents({ currentUser: _currentUser }: AdminEventsProps) {
  const [searchQuery, setSearchQuery] = useState('')
  const [eventTypeFilter, setEventTypeFilter] = useState('all')

  // Mock events data for demo
  /* const _mockEvents: Event[] = [
    {
      eventId: 1,
      staffId: 1001,
      timeStamp: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
      eventType: 'IN',
      reason: '',
      deviceId: 1,
      adminId: 0,
    },
    {
      eventId: 2,
      staffId: 1002,
      timeStamp: new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString(),
      eventType: 'OUT',
      reason: '',
      deviceId: 2,
      adminId: 0,
    },
    {
      eventId: 3,
      staffId: 1003,
      timeStamp: new Date(Date.now() - 1 * 60 * 60 * 1000).toISOString(),
      eventType: 'BREAK_START',
      reason: 'Lunch break',
      deviceId: 4,
      adminId: 0,
    },
    {
      eventId: 4,
      staffId: 1003,
      timeStamp: new Date(Date.now() - 30 * 60 * 1000).toISOString(),
      eventType: 'BREAK_END',
      reason: 'Back from lunch',
      deviceId: 4,
      adminId: 0,
    },
    {
      eventId: 5,
      staffId: 1006,
      timeStamp: new Date(Date.now() - 4 * 60 * 60 * 1000).toISOString(),
      eventType: 'IN',
      reason: '',
      deviceId: 1,
      adminId: 0,
    },
    {
      eventId: 6,
      staffId: 1004,
      timeStamp: new Date(Date.now() - 1.5 * 60 * 60 * 1000).toISOString(),
      eventType: 'IN',
      reason: '',
      deviceId: 3,
      adminId: 0,
    },
    {
      eventId: 7,
      staffId: 1001,
      timeStamp: new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString(),
      eventType: 'OUT',
      reason: 'End of shift',
      deviceId: 1,
      adminId: 8001,
    },
    {
      eventId: 8,
      staffId: 1005,
      timeStamp: new Date(Date.now() - 6 * 60 * 60 * 1000).toISOString(),
      eventType: 'IN',
      reason: 'Late arrival',
      deviceId: 2,
      adminId: 8001,
    },
  ] */

  // Mock staff names for display
  const staffNames: { [key: number]: string } = {
    1001: 'John Smith',
    1002: 'Sarah Johnson',
    1003: 'Mike Brown',
    1004: 'Emily Davis',
    1005: 'David Wilson',
    1006: 'Lisa Chen',
  }

  const queryClient = useQueryClient()

  // Fetch events from API
  const { data: events = [], isLoading } = useQuery({
    queryKey: ['events'],
    queryFn: () => eventApi.getAll(),
  })

  // Delete event mutation
  const deleteEventMutation = useMutation({
    mutationFn: (id: number) => eventApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['events'] })
    },
    onError: (error: any) => {
      console.error('Failed to delete event:', error)
    },
  })

  // Filter events based on search query and type filter
  const filteredEvents = events.filter(event => {
    const matchesSearch =
      event.staffId?.toString().includes(searchQuery) ||
      staffNames[event.staffId!]
        ?.toLowerCase()
        .includes(searchQuery.toLowerCase()) ||
      event.eventType?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      event.reason?.toLowerCase().includes(searchQuery.toLowerCase())

    const matchesType =
      eventTypeFilter === 'all' || event.eventType === eventTypeFilter

    return matchesSearch && matchesType
  })

  const getEventTypeColor = (eventType: string) => {
    switch (eventType) {
      case 'IN':
        return 'success'
      case 'OUT':
        return 'error'
      case 'BREAK_START':
        return 'warning'
      case 'BREAK_END':
        return 'info'
      default:
        return 'default'
    }
  }

  const getEventTypeText = (eventType: string) => {
    switch (eventType) {
      case 'IN':
        return 'Clock In'
      case 'OUT':
        return 'Clock Out'
      case 'BREAK_START':
        return 'Break Start'
      case 'BREAK_END':
        return 'Break End'
      default:
        return eventType
    }
  }

  const handleEditEvent = (eventId: number) => {
    // For now, just alert - full edit functionality can be added later
    alert(`Edit Event ${eventId} - Edit functionality would be here`)
  }

  const handleDeleteEvent = (eventId: number) => {
    if (window.confirm('Are you sure you want to delete this event?')) {
      deleteEventMutation.mutate(eventId)
    }
  }

  const handleRefresh = () => {
    queryClient.invalidateQueries({ queryKey: ['events'] })
  }

  const getEventStats = () => {
    const clockIn = events.filter(e => e.eventType === 'IN').length
    const clockOut = events.filter(e => e.eventType === 'OUT').length
    const breaks = events.filter(e => e.eventType.includes('BREAK')).length
    return { clockIn, clockOut, breaks, total: events.length }
  }

  const stats = getEventStats()

  return (
    <Box>
      {/* Header */}
      <Paper elevation={2} sx={{ p: 3, mb: 3, textAlign: 'center' }}>
        <Schedule sx={{ fontSize: 40, color: 'primary.main', mb: 1 }} />
        <Typography variant="h4" gutterBottom>
          Event Management
        </Typography>
        <Typography variant="subtitle1" color="textSecondary">
          Monitor and manage attendance events
        </Typography>
      </Paper>

      {/* Demo Mode Alert */}

      {/* Stats Cards */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <Card sx={{ minWidth: 120, flex: 1 }}>
          <CardContent sx={{ textAlign: 'center', py: 2 }}>
            <Typography variant="h4" color="success.main">
              {stats.clockIn}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Clock In
            </Typography>
          </CardContent>
        </Card>
        <Card sx={{ minWidth: 120, flex: 1 }}>
          <CardContent sx={{ textAlign: 'center', py: 2 }}>
            <Typography variant="h4" color="error.main">
              {stats.clockOut}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Clock Out
            </Typography>
          </CardContent>
        </Card>
        <Card sx={{ minWidth: 120, flex: 1 }}>
          <CardContent sx={{ textAlign: 'center', py: 2 }}>
            <Typography variant="h4" color="warning.main">
              {stats.breaks}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Break Events
            </Typography>
          </CardContent>
        </Card>
        <Card sx={{ minWidth: 120, flex: 1 }}>
          <CardContent sx={{ textAlign: 'center', py: 2 }}>
            <Typography variant="h4" color="primary.main">
              {stats.total}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Total Events
            </Typography>
          </CardContent>
        </Card>
      </Box>

      {/* Controls */}
      <Box
        sx={{
          display: 'flex',
          gap: 2,
          mb: 3,
          alignItems: 'center',
          flexWrap: 'wrap',
        }}
      >
        <TextField
          placeholder="Search events..."
          value={searchQuery}
          onChange={e => setSearchQuery(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search />
              </InputAdornment>
            ),
          }}
          sx={{ flexGrow: 1, minWidth: 200 }}
        />
        <FormControl sx={{ minWidth: 150 }}>
          <InputLabel>Event Type</InputLabel>
          <Select
            value={eventTypeFilter}
            onChange={e => setEventTypeFilter(e.target.value)}
            label="Event Type"
            startAdornment={<FilterList sx={{ mr: 1 }} />}
          >
            <MenuItem value="all">All Types</MenuItem>
            <MenuItem value="IN">Clock In</MenuItem>
            <MenuItem value="OUT">Clock Out</MenuItem>
            <MenuItem value="BREAK_START">Break Start</MenuItem>
            <MenuItem value="BREAK_END">Break End</MenuItem>
          </Select>
        </FormControl>
        <Button
          variant="outlined"
          startIcon={<Refresh />}
          onClick={handleRefresh}
        >
          Refresh
        </Button>
      </Box>

      {/* Events Table */}
      <Card>
        <CardContent>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Event ID</TableCell>
                  <TableCell>Staff</TableCell>
                  <TableCell>Event Type</TableCell>
                  <TableCell>Time</TableCell>
                  <TableCell>Device</TableCell>
                  <TableCell>Reason</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {isLoading ? (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      Loading events...
                    </TableCell>
                  </TableRow>
                ) : filteredEvents.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} align="center">
                      {searchQuery || eventTypeFilter !== 'all'
                        ? 'No events match your filters'
                        : 'No events found'}
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredEvents
                    .sort(
                      (a, b) =>
                        new Date(b.timeStamp).getTime() -
                        new Date(a.timeStamp).getTime()
                    )
                    .map(event => (
                      <TableRow key={event.eventId} hover>
                        <TableCell>
                          <Typography variant="body2" fontWeight="bold">
                            #{event.eventId}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {staffNames[event.staffId] ||
                              `Staff ${event.staffId}`}
                          </Typography>
                          <Typography variant="caption" color="textSecondary">
                            ID: {event.staffId}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={getEventTypeText(event.eventType)}
                            color={getEventTypeColor(event.eventType) as any}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {formatDateTime(new Date(event.timeStamp))}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            Device #{event.deviceId}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2">
                            {event.reason || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">
                          <IconButton
                            size="small"
                            onClick={() => handleEditEvent(event.eventId)}
                            color="primary"
                          >
                            <Edit />
                          </IconButton>
                          <IconButton
                            size="small"
                            onClick={() => handleDeleteEvent(event.eventId)}
                            color="error"
                          >
                            <Delete />
                          </IconButton>
                        </TableCell>
                      </TableRow>
                    ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>
    </Box>
  )
}

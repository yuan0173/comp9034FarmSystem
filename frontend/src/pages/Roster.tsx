import React from 'react'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Alert,
  Paper,
  Grid,
  Chip,
} from '@mui/material'
import { CalendarMonth, Schedule, Info } from '@mui/icons-material'
import { CurrentUser } from '../types/api'
import { DemoAlert } from '../components/DemoAlert'

interface RosterProps {
  currentUser: CurrentUser
}

// Mock roster data for demonstration
const mockRosterData = [
  {
    date: '2024-01-15',
    shift: 'Morning',
    hours: '8:00 AM - 4:00 PM',
    status: 'scheduled',
  },
  {
    date: '2024-01-16',
    shift: 'Morning',
    hours: '8:00 AM - 4:00 PM',
    status: 'scheduled',
  },
  {
    date: '2024-01-17',
    shift: 'Evening',
    hours: '4:00 PM - 12:00 AM',
    status: 'scheduled',
  },
  { date: '2024-01-18', shift: 'Off', hours: 'Day Off', status: 'off' },
  {
    date: '2024-01-19',
    shift: 'Morning',
    hours: '8:00 AM - 4:00 PM',
    status: 'scheduled',
  },
  {
    date: '2024-01-20',
    shift: 'Weekend',
    hours: '9:00 AM - 3:00 PM',
    status: 'scheduled',
  },
  { date: '2024-01-21', shift: 'Off', hours: 'Day Off', status: 'off' },
]

export function Roster({ currentUser }: RosterProps) {
  const getStatusColor = (status: string) => {
    switch (status) {
      case 'scheduled':
        return 'primary'
      case 'off':
        return 'default'
      case 'completed':
        return 'success'
      case 'missed':
        return 'error'
      default:
        return 'default'
    }
  }

  const formatDate = (dateString: string) => {
    const date = new Date(dateString)
    return date.toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    })
  }

  return (
    <Box sx={{ maxWidth: 1000, mx: 'auto' }}>
      {/* Header */}
      <Paper elevation={2} sx={{ p: 3, mb: 3, textAlign: 'center' }}>
        <CalendarMonth sx={{ fontSize: 40, color: 'primary.main', mb: 1 }} />
        <Typography variant="h4" gutterBottom>
          My Work Schedule
        </Typography>
        <Typography variant="subtitle1" color="textSecondary">
          {currentUser.firstName} {currentUser.lastName} - Staff ID:{' '}
          {currentUser.staffId}
        </Typography>
      </Paper>

      {/* Demo Mode Alert */}
      <DemoAlert page="roster" />

      {/* Schedule Cards */}
      <Grid container spacing={2}>
        {mockRosterData.map((item, index) => (
          <Grid item xs={12} sm={6} md={4} key={index}>
            <Card sx={{ height: '100%' }}>
              <CardContent>
                <Box
                  sx={{
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'flex-start',
                    mb: 2,
                  }}
                >
                  <Schedule color="primary" />
                  <Chip
                    label={item.status.toUpperCase()}
                    color={getStatusColor(item.status)}
                    size="small"
                  />
                </Box>

                <Typography variant="h6" gutterBottom>
                  {formatDate(item.date)}
                </Typography>

                <Typography variant="body1" sx={{ fontWeight: 'bold', mb: 1 }}>
                  {item.shift} Shift
                </Typography>

                <Typography variant="body2" color="textSecondary">
                  {item.hours}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Summary */}
      <Card sx={{ mt: 4 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Weekly Summary
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={6} sm={3}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="primary">
                  5
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Scheduled Days
                </Typography>
              </Box>
            </Grid>
            <Grid item xs={6} sm={3}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="secondary">
                  40
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Total Hours
                </Typography>
              </Box>
            </Grid>
            <Grid item xs={6} sm={3}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="success.main">
                  2
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Days Off
                </Typography>
              </Box>
            </Grid>
            <Grid item xs={6} sm={3}>
              <Box sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="warning.main">
                  2
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Different Shifts
                </Typography>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    </Box>
  )
}

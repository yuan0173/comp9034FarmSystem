import React from 'react'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Paper,
  List,
  ListItem,
  ListItemText,
  Chip,
  Avatar,
  Divider,
  CircularProgress,
} from '@mui/material'
import {
  Dashboard,
  Computer,
  ComputerOutlined,
  People,
  PersonOutline,
  TrendingUp,
  AccessTime,
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { CurrentUser } from '../types/api'
import { staffApi, deviceApi, eventApi } from '../api/client'

interface ManagerDashboardProps {
  currentUser: CurrentUser
}

export function ManagerDashboard({ currentUser }: ManagerDashboardProps) {
  console.log('ManagerDashboard rendering with user:', currentUser)

  // Fetch data from APIs
  const { data: devices = [], isLoading: devicesLoading } = useQuery({
    queryKey: ['devices'],
    queryFn: () => deviceApi.getAll(),
    retry: 1,
  })

  const { data: staffs = [], isLoading: staffsLoading } = useQuery({
    queryKey: ['staffs'],
    queryFn: () => staffApi.getAll(),
    retry: 1,
  })

  const { data: recentEvents = [], isLoading: eventsLoading } = useQuery({
    queryKey: ['events', 'recent'],
    queryFn: async () => {
      const today = new Date()
      const eightHoursAgo = new Date(today.getTime() - 8 * 60 * 60 * 1000)

      return eventApi.getAll({
        from: eightHoursAgo.toISOString(),
        to: today.toISOString(),
      })
    },
    retry: 1,
  })

  const isLoading = devicesLoading || staffsLoading || eventsLoading

  // Calculate metrics
  const onlineDevices = devices.filter(
    device => device.status === 'online'
  ).length
  const offlineDevices = devices.filter(
    device => device.status === 'offline'
  ).length

  // Get currently active staff (those who clocked in recently)
  const activeStaffIds = recentEvents
    .filter(event => event.eventType === 'IN')
    .map(event => event.staffId)

  const activeStaffDetails = staffs.filter(
    staff => activeStaffIds.includes(staff.staffId) && staff.isActive
  )

  const stats = [
    {
      title: 'Online Devices',
      value: onlineDevices,
      total: devices.length,
      icon: <Computer />,
      color: 'success.main',
    },
    {
      title: 'Offline Devices',
      value: offlineDevices,
      total: devices.length,
      icon: <ComputerOutlined />,
      color: 'error.main',
    },
    {
      title: 'Active Staff',
      value: activeStaffDetails.length,
      total: staffs.length,
      icon: <People />,
      color: 'primary.main',
    },
    {
      title: 'Total Staff',
      value: staffs.length,
      total: staffs.length,
      icon: <PersonOutline />,
      color: 'secondary.main',
    },
  ]

  if (isLoading) {
    return (
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'center',
          alignItems: 'center',
          minHeight: '50vh',
        }}
      >
        <CircularProgress />
      </Box>
    )
  }

  return (
    <Box>
      {/* Header */}
      <Paper elevation={2} sx={{ p: 3, mb: 3, textAlign: 'center' }}>
        <Dashboard sx={{ fontSize: 40, color: 'primary.main', mb: 1 }} />
        <Typography variant="h4" gutterBottom>
          Manager Dashboard
        </Typography>
        <Typography variant="subtitle1" color="textSecondary">
          Welcome back, {currentUser.firstName} {currentUser.lastName}
        </Typography>
      </Paper>

      {/* Stats Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        {stats.map((stat, index) => (
          <Grid item xs={12} sm={6} md={3} key={index}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <Box sx={{ color: stat.color, mr: 2 }}>{stat.icon}</Box>
                  <Typography variant="h6" component="div">
                    {stat.title}
                  </Typography>
                </Box>
                <Typography variant="h3" color={stat.color} gutterBottom>
                  {stat.value}
                </Typography>
                {stat.total > stat.value && (
                  <Typography variant="body2" color="textSecondary">
                    of {stat.total} total
                  </Typography>
                )}
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Grid container spacing={3}>
        {/* Currently Active Staff */}
        <Grid item xs={12} md={6}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <AccessTime color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">Currently Clocked In</Typography>
              </Box>

              {activeStaffDetails.length === 0 ? (
                <Typography
                  variant="body2"
                  color="textSecondary"
                  sx={{ py: 3 }}
                >
                  No staff currently clocked in
                </Typography>
              ) : (
                <List>
                  {activeStaffDetails.slice(0, 10).map((staff, index) => (
                    <React.Fragment key={staff.staffId}>
                      <ListItem sx={{ px: 0 }}>
                        <Avatar sx={{ mr: 2, bgcolor: 'primary.main' }}>
                          {staff.firstName[0]}
                          {staff.lastName[0]}
                        </Avatar>
                        <ListItemText
                          primary={`${staff.firstName} ${staff.lastName}`}
                          secondary={`ID: ${staff.staffId} • ${staff.role}`}
                        />
                        <Chip label="Active" color="success" size="small" />
                      </ListItem>
                      {index < activeStaffDetails.length - 1 && <Divider />}
                    </React.Fragment>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Device Status */}
        <Grid item xs={12} md={6}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                <Computer color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">Device Status</Typography>
              </Box>

              {devices.length === 0 ? (
                <Typography
                  variant="body2"
                  color="textSecondary"
                  sx={{ py: 3 }}
                >
                  No devices configured
                </Typography>
              ) : (
                <List>
                  {devices.slice(0, 10).map((device, index) => (
                    <React.Fragment key={device.deviceId}>
                      <ListItem sx={{ px: 0 }}>
                        <ListItemText
                          primary={device.location}
                          secondary={`ID: ${device.deviceId} • ${device.type}`}
                        />
                        <Chip
                          label={device.status.toUpperCase()}
                          color={
                            device.status === 'online' ? 'success' : 'error'
                          }
                          size="small"
                        />
                      </ListItem>
                      {index < devices.length - 1 && <Divider />}
                    </React.Fragment>
                  ))}
                </List>
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Quick Statistics */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
                <TrendingUp color="primary" sx={{ mr: 1 }} />
                <Typography variant="h6">Quick Statistics</Typography>
              </Box>

              <Grid container spacing={3}>
                <Grid item xs={6} sm={3}>
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="primary">
                      {recentEvents.length}
                    </Typography>
                    <Typography variant="body2" color="textSecondary">
                      Events (Last 8h)
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="success.main">
                      {onlineDevices}
                    </Typography>
                    <Typography variant="body2" color="textSecondary">
                      Online Devices
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="secondary.main">
                      {activeStaffDetails.length}
                    </Typography>
                    <Typography variant="body2" color="textSecondary">
                      Staff Working
                    </Typography>
                  </Box>
                </Grid>
                <Grid item xs={6} sm={3}>
                  <Box sx={{ textAlign: 'center' }}>
                    <Typography variant="h4" color="warning.main">
                      {Math.round(
                        (onlineDevices / Math.max(devices.length, 1)) * 100
                      )}
                      %
                    </Typography>
                    <Typography variant="body2" color="textSecondary">
                      Device Uptime
                    </Typography>
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}

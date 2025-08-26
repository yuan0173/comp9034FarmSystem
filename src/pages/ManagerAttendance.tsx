import React, { useState, useMemo } from 'react'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Alert,
  Button,
  Grid,
} from '@mui/material'
import { EventNote, Download, Warning } from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { eventApi, staffApi } from '../api/client'
import { calculateAllWorkHours, formatHours } from '../utils/time'
import { DateRangePicker } from '../components/DateRangePicker'
import { ExportCsvButton } from '../components/ExportCsvButton'
import { DemoAlert } from '../components/DemoAlert'
import { CurrentUser, WorkHoursData } from '../types/api'

interface ManagerAttendanceProps {
  currentUser: CurrentUser
}

export function ManagerAttendance({ currentUser }: ManagerAttendanceProps) {
  const [startDate, setStartDate] = useState<Date | null>(() => {
    const date = new Date()
    date.setDate(date.getDate() - 7) // Default to last 7 days
    return date
  })
  const [endDate, setEndDate] = useState<Date | null>(new Date())

  // Mock data for demonstration
  const mockEvents = [
    {
      eventId: 1,
      staffId: 1001,
      timeStamp: new Date(Date.now() - 8 * 60 * 60 * 1000).toISOString(),
      eventType: 'IN',
      reason: '',
      deviceId: 1,
      adminId: 0,
    },
    {
      eventId: 2,
      staffId: 1001,
      timeStamp: new Date(Date.now() - 4 * 60 * 60 * 1000).toISOString(),
      eventType: 'OUT',
      reason: '',
      deviceId: 1,
      adminId: 0,
    },
    {
      eventId: 3,
      staffId: 1002,
      timeStamp: new Date(Date.now() - 7 * 60 * 60 * 1000).toISOString(),
      eventType: 'IN',
      reason: '',
      deviceId: 2,
      adminId: 0,
    },
    {
      eventId: 4,
      staffId: 1002,
      timeStamp: new Date(Date.now() - 3 * 60 * 60 * 1000).toISOString(),
      eventType: 'BREAK_START',
      reason: 'Lunch',
      deviceId: 2,
      adminId: 0,
    },
    {
      eventId: 5,
      staffId: 1003,
      timeStamp: new Date(Date.now() - 6 * 60 * 60 * 1000).toISOString(),
      eventType: 'IN',
      reason: '',
      deviceId: 1,
      adminId: 0,
    },
  ]

  const mockStaffs = [
    {
      staffId: 1001,
      firstName: 'John',
      lastName: 'Smith',
      email: 'john.smith@company.com',
      role: 'Employee',
      standardPayRate: 25.5,
    },
    {
      staffId: 1002,
      firstName: 'Sarah',
      lastName: 'Johnson',
      email: 'sarah.johnson@company.com',
      role: 'Supervisor',
      standardPayRate: 32.0,
    },
    {
      staffId: 1003,
      firstName: 'Mike',
      lastName: 'Brown',
      email: 'mike.brown@company.com',
      role: 'Employee',
      standardPayRate: 24.75,
    },
  ]

  // Use mock data directly (no API calls for demo)
  const events = mockEvents
  const staffs = mockStaffs
  const eventsLoading = false

  // Optional: Keep API calls disabled for demo
  // const { data: events = [], isLoading: eventsLoading } = useQuery({
  //   queryKey: ['events', 'attendance', startDate, endDate],
  //   queryFn: async () => {
  //     if (!startDate || !endDate) return []
  //     return eventApi.getAll({
  //       from: startDate.toISOString(),
  //       to: endDate.toISOString()
  //     })
  //   },
  //   enabled: false
  // })

  // Calculate work hours data
  const workHoursData = useMemo(() => {
    if (!startDate || !endDate || events.length === 0) return []

    const calculatedHours = calculateAllWorkHours(events, startDate, endDate)

    // Enrich with staff details
    return calculatedHours
      .map(hours => {
        const staff = staffs.find(s => s.staffId === hours.staffId)
        return {
          ...hours,
          staffName: staff
            ? `${staff.firstName} ${staff.lastName}`
            : `Unknown (${hours.staffId})`,
          email: staff?.email || '',
          role: staff?.role || '',
        }
      })
      .sort((a, b) => a.staffName.localeCompare(b.staffName))
  }, [events, staffs, startDate, endDate])

  // Prepare CSV export data
  const csvData = workHoursData.map(data => ({
    'Staff ID': data.staffId,
    Name: data.staffName,
    Role: data.role,
    'Work Hours': data.workHours,
    'Break Hours': data.breakHours,
    'Net Hours': data.netHours,
    Anomalies: data.anomalies.join('; ') || 'None',
  }))

  const totalWorkHours = workHoursData.reduce(
    (sum, data) => sum + data.workHours,
    0
  )
  const totalNetHours = workHoursData.reduce(
    (sum, data) => sum + data.netHours,
    0
  )
  const totalAnomalies = workHoursData.reduce(
    (sum, data) => sum + data.anomalies.length,
    0
  )

  return (
    <Box>
      {/* Header */}
      <Paper elevation={2} sx={{ p: 3, mb: 3, textAlign: 'center' }}>
        <EventNote sx={{ fontSize: 40, color: 'primary.main', mb: 1 }} />
        <Typography variant="h4" gutterBottom>
          Attendance Management
        </Typography>
        <Typography variant="subtitle1" color="textSecondary">
          Track and analyze employee attendance data
        </Typography>
      </Paper>

      {/* Demo Mode Alert */}
      <DemoAlert page="attendance" />

      {/* Date Range Selector */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Select Date Range
          </Typography>
          <DateRangePicker
            startDate={startDate}
            endDate={endDate}
            onStartDateChange={setStartDate}
            onEndDateChange={setEndDate}
            label="Attendance Period"
            maxDate={new Date()}
          />
        </CardContent>
      </Card>

      {/* Summary Cards */}
      {workHoursData.length > 0 && (
        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="primary">
                  {workHoursData.length}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Employees
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="secondary">
                  {totalWorkHours.toFixed(1)}h
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Total Work Hours
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="success.main">
                  {totalNetHours.toFixed(1)}h
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Net Hours
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography
                  variant="h4"
                  color={totalAnomalies > 0 ? 'error.main' : 'success.main'}
                >
                  {totalAnomalies}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Anomalies
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Data Table */}
      <Card>
        <CardContent>
          <Box
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              mb: 2,
            }}
          >
            <Typography variant="h6">Attendance Report</Typography>
            {workHoursData.length > 0 && (
              <ExportCsvButton
                data={csvData}
                filename={`attendance-report-${startDate?.toISOString().split('T')[0]}-to-${endDate?.toISOString().split('T')[0]}.csv`}
                variant="contained"
                startIcon={<Download />}
              >
                Export CSV
              </ExportCsvButton>
            )}
          </Box>

          {eventsLoading ? (
            <Typography variant="body1" sx={{ textAlign: 'center', py: 4 }}>
              Loading attendance data...
            </Typography>
          ) : workHoursData.length === 0 ? (
            <Alert severity="info" sx={{ mt: 2 }}>
              No attendance data found for the selected date range.
              {!startDate || !endDate ? ' Please select a date range.' : ''}
            </Alert>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Employee</TableCell>
                    <TableCell>Staff ID</TableCell>
                    <TableCell>Role</TableCell>
                    <TableCell align="right">Work Hours</TableCell>
                    <TableCell align="right">Break Hours</TableCell>
                    <TableCell align="right">Net Hours</TableCell>
                    <TableCell align="center">Anomalies</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {workHoursData.map(data => (
                    <TableRow key={data.staffId}>
                      <TableCell>
                        <Typography variant="body2" fontWeight="bold">
                          {data.staffName}
                        </Typography>
                      </TableCell>
                      <TableCell>{data.staffId}</TableCell>
                      <TableCell>{data.role}</TableCell>
                      <TableCell align="right">
                        <Typography variant="body2" fontWeight="bold">
                          {formatHours(data.workHours)}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        {formatHours(data.breakHours)}
                      </TableCell>
                      <TableCell align="right">
                        <Typography
                          variant="body2"
                          fontWeight="bold"
                          color="primary"
                        >
                          {formatHours(data.netHours)}
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        {data.anomalies.length > 0 ? (
                          <Box
                            sx={{
                              display: 'flex',
                              alignItems: 'center',
                              justifyContent: 'center',
                            }}
                          >
                            <Warning
                              color="error"
                              fontSize="small"
                              sx={{ mr: 1 }}
                            />
                            <Typography variant="caption" color="error">
                              {data.anomalies.length}
                            </Typography>
                          </Box>
                        ) : (
                          <Typography variant="caption" color="success.main">
                            None
                          </Typography>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* Anomalies Details */}
      {totalAnomalies > 0 && (
        <Card sx={{ mt: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Attendance Anomalies
            </Typography>
            {workHoursData
              .filter(data => data.anomalies.length > 0)
              .map(data => (
                <Alert key={data.staffId} severity="warning" sx={{ mb: 1 }}>
                  <Typography variant="body2" fontWeight="bold">
                    {data.staffName} (ID: {data.staffId})
                  </Typography>
                  <ul style={{ margin: '4px 0', paddingLeft: '20px' }}>
                    {data.anomalies.map((anomaly, index) => (
                      <li key={index}>
                        <Typography variant="caption">{anomaly}</Typography>
                      </li>
                    ))}
                  </ul>
                </Alert>
              ))}
          </CardContent>
        </Card>
      )}
    </Box>
  )
}

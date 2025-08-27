import { useState, useMemo } from 'react'
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
  Grid,
} from '@mui/material'
import { Receipt, Download, Info } from '@mui/icons-material'

import { calculateAllWorkHours, formatHours } from '../utils/time'
import { DateRangePicker } from '../components/DateRangePicker'
import { ExportCsvButton } from '../components/ExportCsvButton'

import { CurrentUser } from '../types/api'

interface ManagerPayslipsProps {
  currentUser: CurrentUser
}

export function ManagerPayslips({
  currentUser: _currentUser,
}: ManagerPayslipsProps) {
  const [startDate, setStartDate] = useState<Date | null>(() => {
    const date = new Date()
    date.setDate(date.getDate() - 7)
    return date
  })
  const [endDate, setEndDate] = useState<Date | null>(new Date())

  // Mock data for demonstration (same as attendance page)
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
      eventType: 'OUT',
      reason: '',
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
    {
      eventId: 6,
      staffId: 1003,
      timeStamp: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
      eventType: 'OUT',
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

  // Calculate payroll data
  const payrollData = useMemo(() => {
    if (!startDate || !endDate || events.length === 0) return []

    const workHoursData = calculateAllWorkHours(events, startDate, endDate)

    return workHoursData
      .map(hours => {
        const staff = staffs.find(s => s.staffId === hours.staffId)
        const payRate = staff?.standardPayRate || 25.0 // Default rate
        const grossPay = hours.netHours * payRate

        return {
          ...hours,
          staffName: staff
            ? `${staff.firstName} ${staff.lastName}`
            : `Unknown (${hours.staffId})`,
          email: staff?.email || '',
          role: staff?.role || '',
          payRate,
          grossPay: Math.round(grossPay * 100) / 100,
        }
      })
      .sort((a, b) => a.staffName.localeCompare(b.staffName))
  }, [events, staffs, startDate, endDate])

  // CSV export data
  const csvData = payrollData.map(data => ({
    'Staff ID': data.staffId,
    Name: data.staffName,
    Role: data.role,
    'Net Hours': data.netHours,
    'Pay Rate': `$${data.payRate}`,
    'Gross Pay': `$${data.grossPay}`,
    Anomalies: data.anomalies.length,
  }))

  const totalGrossPay = payrollData.reduce(
    (sum, data) => sum + data.grossPay,
    0
  )
  const totalNetHours = payrollData.reduce(
    (sum, data) => sum + data.netHours,
    0
  )

  return (
    <Box>
      {/* Header */}
      <Paper elevation={2} sx={{ p: 3, mb: 3, textAlign: 'center' }}>
        <Receipt sx={{ fontSize: 40, color: 'primary.main', mb: 1 }} />
        <Typography variant="h4" gutterBottom>
          Payroll Management
        </Typography>
        <Typography variant="subtitle1" color="textSecondary">
          Generate payroll calculations based on attendance data
        </Typography>
      </Paper>

      {/* Demo Mode Alert */}

      {/* Date Range Selector */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Payroll Period
          </Typography>
          <DateRangePicker
            startDate={startDate}
            endDate={endDate}
            onStartDateChange={setStartDate}
            onEndDateChange={setEndDate}
            label="Pay Period"
            maxDate={new Date()}
          />
        </CardContent>
      </Card>

      {/* Summary Cards */}
      {payrollData.length > 0 && (
        <Grid container spacing={3} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="primary">
                  {payrollData.length}
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
                  {totalNetHours.toFixed(1)}h
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Total Hours
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="success.main">
                  ${totalGrossPay.toFixed(2)}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Total Gross Pay
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="warning.main">
                  $
                  {(totalGrossPay / Math.max(payrollData.length, 1)).toFixed(2)}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Avg. Pay/Employee
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      <Alert severity="info" sx={{ mb: 3 }} icon={<Info />}>
        This payroll calculation is simplified for demonstration. In a
        production system, it would include tax calculations, deductions,
        overtime rates, and integration with payroll systems.
      </Alert>

      {/* Payroll Table */}
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
            <Typography variant="h6">Payroll Report</Typography>
            {payrollData.length > 0 && (
              <ExportCsvButton
                data={csvData}
                filename={`payroll-report-${startDate?.toISOString().split('T')[0]}-to-${endDate?.toISOString().split('T')[0]}.csv`}
                variant="contained"
                startIcon={<Download />}
              >
                Export Payroll CSV
              </ExportCsvButton>
            )}
          </Box>

          {eventsLoading ? (
            <Typography variant="body1" sx={{ textAlign: 'center', py: 4 }}>
              Loading payroll data...
            </Typography>
          ) : payrollData.length === 0 ? (
            <Alert severity="info" sx={{ mt: 2 }}>
              No payroll data found for the selected date range.
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
                    <TableCell align="right">Net Hours</TableCell>
                    <TableCell align="right">Pay Rate</TableCell>
                    <TableCell align="right">Gross Pay</TableCell>
                    <TableCell align="center">Issues</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {payrollData.map(data => (
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
                          {formatHours(data.netHours)}
                        </Typography>
                      </TableCell>
                      <TableCell align="right">
                        ${data.payRate.toFixed(2)}/hr
                      </TableCell>
                      <TableCell align="right">
                        <Typography
                          variant="body2"
                          fontWeight="bold"
                          color="success.main"
                        >
                          ${data.grossPay.toFixed(2)}
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        {data.anomalies.length > 0 ? (
                          <Typography variant="caption" color="error">
                            {data.anomalies.length} issues
                          </Typography>
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
    </Box>
  )
}

import React, { useState } from 'react'
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
  Button,
  TextField,
  Grid,
  Chip,
  Alert,
} from '@mui/material'
import { People, Add, Search, Info } from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { staffApi } from '../api/client'
import { CurrentUser } from '../types/api'
import { DemoAlert } from '../components/DemoAlert'

interface AdminStaffsProps {
  currentUser: CurrentUser
}

export function AdminStaffs({ currentUser }: AdminStaffsProps) {
  const [searchQuery, setSearchQuery] = useState('')

  // Mock staff data for demonstration
  const mockStaffs = [
    {
      staffId: 1001,
      firstName: 'John',
      lastName: 'Smith',
      email: 'john.smith@company.com',
      role: 'Employee',
      contractType: 'Full-time',
      standardPayRate: 25.5,
      isActive: true,
    },
    {
      staffId: 1002,
      firstName: 'Sarah',
      lastName: 'Johnson',
      email: 'sarah.johnson@company.com',
      role: 'Supervisor',
      contractType: 'Full-time',
      standardPayRate: 32.0,
      isActive: true,
    },
    {
      staffId: 1003,
      firstName: 'Mike',
      lastName: 'Brown',
      email: 'mike.brown@company.com',
      role: 'Employee',
      contractType: 'Part-time',
      standardPayRate: 22.75,
      isActive: true,
    },
    {
      staffId: 1004,
      firstName: 'Emily',
      lastName: 'Davis',
      email: 'emily.davis@company.com',
      role: 'Team Lead',
      contractType: 'Full-time',
      standardPayRate: 35.0,
      isActive: true,
    },
    {
      staffId: 1005,
      firstName: 'David',
      lastName: 'Wilson',
      email: 'david.wilson@company.com',
      role: 'Employee',
      contractType: 'Full-time',
      standardPayRate: 24.0,
      isActive: false,
    },
    {
      staffId: 1006,
      firstName: 'Lisa',
      lastName: 'Chen',
      email: 'lisa.chen@company.com',
      role: 'Employee',
      contractType: 'Full-time',
      standardPayRate: 26.5,
      isActive: true,
    },
    {
      staffId: 8001,
      firstName: 'Manager',
      lastName: 'Demo',
      email: 'manager@company.com',
      role: 'Manager',
      contractType: 'Full-time',
      standardPayRate: 45.0,
      isActive: true,
    },
    {
      staffId: 9001,
      firstName: 'Admin',
      lastName: 'User',
      email: 'admin@company.com',
      role: 'Administrator',
      contractType: 'Full-time',
      standardPayRate: 50.0,
      isActive: true,
    },
  ]

  // Use mock data directly (no API calls for demo)
  const staffs = mockStaffs
  const isLoading = false

  // Optional: Keep API call disabled for demo
  // const { data: staffs = mockStaffs, isLoading } = useQuery({
  //   queryKey: ['staffs'],
  //   queryFn: () => staffApi.getAll(),
  //   retry: 0,
  //   refetchOnWindowFocus: false,
  //   enabled: false
  // })

  // Filter staffs based on search query
  const filteredStaffs = staffs.filter(
    staff =>
      staff.firstName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      staff.lastName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      staff.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
      staff.staffId.toString().includes(searchQuery)
  )

  const getStatusColor = (isActive: boolean) => {
    return isActive ? 'success' : 'error'
  }

  const getStatusLabel = (isActive: boolean) => {
    return isActive ? 'Active' : 'Inactive'
  }

  return (
    <Box>
      {/* Header */}
      <Paper elevation={2} sx={{ p: 3, mb: 3, textAlign: 'center' }}>
        <People sx={{ fontSize: 40, color: 'primary.main', mb: 1 }} />
        <Typography variant="h4" gutterBottom>
          Staff Management
        </Typography>
        <Typography variant="subtitle1" color="textSecondary">
          Manage employee records and information
        </Typography>
      </Paper>

      {/* Demo Mode Alert */}
      <DemoAlert page="adminStaffs" />

      {/* Controls */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={6}>
              <TextField
                fullWidth
                label="Search Staff"
                placeholder="Search by name, email, or staff ID..."
                value={searchQuery}
                onChange={e => setSearchQuery(e.target.value)}
                InputProps={{
                  startAdornment: <Search sx={{ mr: 1, color: 'grey.500' }} />,
                }}
              />
            </Grid>
            <Grid item xs={12} md={6}>
              <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
                <Button
                  variant="contained"
                  startIcon={<Add />}
                  onClick={() =>
                    alert('Add staff functionality would be implemented here')
                  }
                >
                  Add Staff Member
                </Button>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Staff Table */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Staff Directory ({filteredStaffs.length} employees)
          </Typography>

          {isLoading ? (
            <Typography variant="body1" sx={{ textAlign: 'center', py: 4 }}>
              Loading staff data...
            </Typography>
          ) : filteredStaffs.length === 0 ? (
            <Typography variant="body1" sx={{ textAlign: 'center', py: 4 }}>
              {searchQuery
                ? 'No staff members match your search.'
                : 'No staff members found.'}
            </Typography>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Staff ID</TableCell>
                    <TableCell>Name</TableCell>
                    <TableCell>Email</TableCell>
                    <TableCell>Role</TableCell>
                    <TableCell>Contract Type</TableCell>
                    <TableCell>Pay Rate</TableCell>
                    <TableCell align="center">Status</TableCell>
                    <TableCell align="center">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredStaffs.map(staff => (
                    <TableRow key={staff.staffId}>
                      <TableCell>
                        <Typography variant="body2" fontWeight="bold">
                          {staff.staffId}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {staff.firstName} {staff.lastName}
                        </Typography>
                      </TableCell>
                      <TableCell>{staff.email}</TableCell>
                      <TableCell>
                        <Chip
                          label={staff.role}
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>{staff.contractType}</TableCell>
                      <TableCell>
                        ${staff.standardPayRate.toFixed(2)}/hr
                      </TableCell>
                      <TableCell align="center">
                        <Chip
                          label={getStatusLabel(staff.isActive)}
                          color={getStatusColor(staff.isActive)}
                          size="small"
                        />
                      </TableCell>
                      <TableCell align="center">
                        <Button
                          size="small"
                          variant="outlined"
                          onClick={() =>
                            alert(
                              `Edit functionality for ${staff.firstName} ${staff.lastName} would be implemented here`
                            )
                          }
                        >
                          Edit
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>

      {/* Summary Stats */}
      {!isLoading && staffs.length > 0 && (
        <Grid container spacing={3} sx={{ mt: 2 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="primary">
                  {staffs.length}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Total Staff
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="success.main">
                  {staffs.filter(s => s.isActive).length}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Active Staff
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="secondary.main">
                  {staffs.filter(s => s.contractType === 'Full-time').length}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Full-time
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="warning.main">
                  $
                  {staffs.reduce((sum, s) => sum + s.standardPayRate, 0) /
                    staffs.length}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Avg. Pay Rate
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}
    </Box>
  )
}

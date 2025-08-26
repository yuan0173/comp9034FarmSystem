import React, { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Alert,
  Container,
  Paper,
  InputAdornment
} from '@mui/material'
import { Person, Lock } from '@mui/icons-material'
import { CurrentUser, UserRole } from '../types/api'
import { staffApi } from '../api/client'

interface LoginProps {
  onLogin: (user: CurrentUser) => void
}

export function Login({ onLogin }: LoginProps) {
  const [staffId, setStaffId] = useState('')
  const [pin, setPin] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const navigate = useNavigate()

  // Clear error when inputs change
  useEffect(() => {
    if (error) setError('')
  }, [staffId, pin])

  const validatePin = (pin: string): boolean => {
    // Simple PIN validation: must be exactly 4 digits
    return /^\d{4}$/.test(pin)
  }

  const determineUserRole = (staffId: number): UserRole => {
    // Simple role determination logic (in real app, this would come from backend)
    if (staffId >= 9000) return 'admin'
    if (staffId >= 8000) return 'manager'
    return 'staff'
  }

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)

    try {
      const staffIdNum = parseInt(staffId)
      if (isNaN(staffIdNum) || staffIdNum <= 0) {
        setError('Please enter a valid Staff ID')
        return
      }

      if (!validatePin(pin)) {
        setError('PIN must be exactly 4 digits')
        return
      }

      // Try to fetch staff details from API
      let staffDetails
      try {
        staffDetails = await staffApi.getById(staffIdNum)
      } catch (apiError) {
        // If API fails, create a mock user for demo purposes
        console.warn('API call failed, using mock data:', apiError)
        staffDetails = {
          staffId: staffIdNum,
          firstName: 'Demo',
          lastName: 'User',
          email: `user${staffIdNum}@example.com`,
          phone: '0000000000',
          address: 'Demo Address',
          contractType: 'Full-time',
          isActive: true,
          role: 'employee',
          standardHoursPerWeek: 40,
          standardPayRate: 25.0,
          overtimePayRate: 37.5
        }
      }

      const userRole = determineUserRole(staffIdNum)
      const currentUser: CurrentUser = {
        staffId: staffDetails.staffId,
        firstName: staffDetails.firstName,
        lastName: staffDetails.lastName,
        role: userRole,
        pin: pin
      }

      onLogin(currentUser)
      
      // Navigate based on role
      switch (userRole) {
        case 'admin':
          navigate('/admin/staffs')
          break
        case 'manager':
          navigate('/manager')
          break
        case 'staff':
        default:
          navigate('/station')
          break
      }

    } catch (error) {
      console.error('Login error:', error)
      setError('Login failed. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Container maxWidth="sm" sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center' }}>
      <Paper elevation={8} sx={{ width: '100%', p: 4 }}>
        <Box sx={{ textAlign: 'center', mb: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom>
            Assignment App
          </Typography>
          <Typography variant="subtitle1" color="textSecondary">
            Attendance & Payroll Management
          </Typography>
        </Box>

        <Card>
          <CardContent sx={{ p: 4 }}>
            <form onSubmit={handleLogin}>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                <TextField
                  fullWidth
                  label="Staff ID"
                  variant="outlined"
                  type="number"
                  value={staffId}
                  onChange={(e) => setStaffId(e.target.value)}
                  required
                  disabled={isLoading}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Person />
                      </InputAdornment>
                    ),
                  }}
                  helperText="Enter your employee ID number"
                />

                <TextField
                  fullWidth
                  label="PIN"
                  variant="outlined"
                  type="password"
                  inputProps={{ maxLength: 4, pattern: '[0-9]{4}' }}
                  value={pin}
                  onChange={(e) => setPin(e.target.value.replace(/\D/g, ''))}
                  required
                  disabled={isLoading}
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Lock />
                      </InputAdornment>
                    ),
                  }}
                  helperText="Enter your 4-digit PIN"
                />

                {error && (
                  <Alert severity="error" sx={{ mt: 2 }}>
                    {error}
                  </Alert>
                )}

                <Button
                  type="submit"
                  variant="contained"
                  size="large"
                  disabled={isLoading}
                  sx={{ mt: 2, py: 1.5 }}
                >
                  {isLoading ? 'Signing In...' : 'Sign In'}
                </Button>
              </Box>
            </form>

            <Box sx={{ mt: 4, p: 2, bgcolor: 'grey.100', borderRadius: 1 }}>
              <Typography variant="subtitle2" gutterBottom>
                Demo Credentials:
              </Typography>
              <Typography variant="body2" color="textSecondary">
                Staff: 1001-7999 + any 4-digit PIN<br />
                Manager: 8001-8999 + any 4-digit PIN<br />
                Admin: 9001+ + any 4-digit PIN
              </Typography>
            </Box>
          </CardContent>
        </Card>
      </Paper>
    </Container>
  )
} 
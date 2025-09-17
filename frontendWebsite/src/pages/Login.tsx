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
  InputAdornment,
} from '@mui/material'
import { Person, Lock } from '@mui/icons-material'
import { CurrentUser, UserRole } from '../types/api'
import { authApi } from '../api/client'

interface LoginProps {
  onLogin: (user: CurrentUser) => void
}

export function Login({ onLogin }: LoginProps) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const navigate = useNavigate()

  // Clear error when inputs change
  useEffect(() => {
    if (error) setError('')
  }, [email, password])

  const validateEmail = (email: string): boolean => {
    // Simple email validation
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)
  }


  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setIsLoading(true)

    try {
      if (!validateEmail(email)) {
        setError('Please enter a valid email address')
        return
      }

      if (!password || password.length === 0) {
        setError('Please enter your password')
        return
      }

      // Use the proper authentication API
      const loginResponse = await authApi.login(email, password)
      
      if (loginResponse && loginResponse.staff) {
        const currentUser: CurrentUser = {
          staffId: loginResponse.staff.id,
          firstName: loginResponse.staff.name.split(' ')[0] || '',
          lastName: loginResponse.staff.name.split(' ').slice(1).join(' ') || '',
          role: loginResponse.staff.role.toLowerCase() as UserRole,
          pin: '', // PIN no longer used
        }

        // Store the JWT token for future API calls
        if (loginResponse.token) {
          localStorage.setItem('authToken', loginResponse.token)
        }

        onLogin(currentUser)

        // Navigate based on role
        switch (currentUser.role) {
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
      } else {
        throw new Error('Invalid login response')
      }
    } catch (error) {
      console.error('Login error:', error)
      setError('Invalid email or password. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Container
      maxWidth="sm"
      sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center' }}
    >
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
                  label="Email Address"
                  variant="outlined"
                  type="email"
                  value={email}
                  onChange={e => setEmail(e.target.value)}
                  required
                  disabled={isLoading}
                  autoComplete="email"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Person />
                      </InputAdornment>
                    ),
                  }}
                  helperText="Enter your email address"
                />

                <TextField
                  fullWidth
                  label="Password"
                  variant="outlined"
                  type="password"
                  value={password}
                  onChange={e => setPassword(e.target.value)}
                  required
                  disabled={isLoading}
                  autoComplete="current-password"
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <Lock />
                      </InputAdornment>
                    ),
                  }}
                  helperText="Enter your password"
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
          </CardContent>
        </Card>
      </Paper>
    </Container>
  )
}

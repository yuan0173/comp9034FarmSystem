import { useState, useEffect } from 'react'
import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
} from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ThemeProvider, createTheme } from '@mui/material/styles'
import { CssBaseline } from '@mui/material'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'

// Components
import { AppShell } from './components/AppShell'

// Pages
import { Login } from './pages/Login'
import { Station } from './pages/Station'
import { Roster } from './pages/Roster'
import { ManagerDashboard } from './pages/ManagerDashboard'
import { ManagerAttendance } from './pages/ManagerAttendance'
import { ManagerPayslips } from './pages/ManagerPayslips'
import { AdminStaffs } from './pages/AdminStaffs'
import { AdminDevices } from './pages/AdminDevices'
import { AdminEvents } from './pages/AdminEvents'
// All admin pages are now imported above

// Types
import { CurrentUser } from './types/api'

// Offline functionality
import { initDB } from './offline/db'
import { startAutoSync, stopAutoSync } from './offline/sync'

// Create a client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: failureCount => {
        // Don't retry if we're offline
        if (!navigator.onLine) return false
        // Only retry up to 3 times
        return failureCount < 3
      },
      staleTime: 5 * 60 * 1000, // 5 minutes
    },
  },
})

// Create theme
const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        root: {
          textTransform: 'none',
        },
      },
    },
  },
})

function App() {
  const [currentUser, setCurrentUser] = useState<CurrentUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  // Initialize app on mount
  useEffect(() => {
    const initializeApp = async () => {
      try {
        // Initialize IndexedDB
        await initDB()

        // Start auto sync for offline events
        startAutoSync()

        // Load saved user from localStorage
        const savedUser = localStorage.getItem('currentUser')
        if (savedUser) {
          try {
            setCurrentUser(JSON.parse(savedUser))
          } catch (error) {
            console.error('Failed to parse saved user:', error)
            localStorage.removeItem('currentUser')
          }
        }
      } catch (error) {
        console.error('Failed to initialize app:', error)
      } finally {
        setIsLoading(false)
      }
    }

    initializeApp()

    // Cleanup on unmount
    return () => {
      stopAutoSync()
    }
  }, [])

  const handleLogin = (user: CurrentUser) => {
    setCurrentUser(user)
    localStorage.setItem('currentUser', JSON.stringify(user))
  }

  const handleLogout = () => {
    setCurrentUser(null)
    localStorage.removeItem('currentUser')
  }

  // Show loading screen while initializing
  if (isLoading) {
    return (
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <div
          style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            height: '100vh',
            fontSize: '1.2rem',
            color: theme.palette.primary.main,
          }}
        >
          Loading...
        </div>
      </ThemeProvider>
    )
  }

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <LocalizationProvider dateAdapter={AdapterDayjs}>
        <QueryClientProvider client={queryClient}>
          <Router>
            <Routes>
              {/* Login route */}
              <Route
                path="/login"
                element={
                  currentUser ? (
                    <Navigate
                      to={
                        currentUser.role === 'admin'
                          ? '/admin/staffs'
                          : currentUser.role === 'manager'
                            ? '/manager'
                            : '/station'
                      }
                      replace
                    />
                  ) : (
                    <Login onLogin={handleLogin} />
                  )
                }
              />

              {/* Protected routes with AppShell */}
              <Route
                path="/"
                element={
                  !currentUser ? (
                    <Navigate to="/login" replace />
                  ) : (
                    <AppShell
                      currentUser={currentUser}
                      onLogout={handleLogout}
                    />
                  )
                }
              >
                {/* Staff routes */}
                <Route
                  path="station"
                  element={
                    currentUser?.role === 'staff' ? (
                      <Station currentUser={currentUser} />
                    ) : (
                      <Navigate to="/login" replace />
                    )
                  }
                />
                <Route
                  path="roster"
                  element={
                    currentUser?.role === 'staff' ? (
                      <Roster currentUser={currentUser} />
                    ) : (
                      <Navigate to="/login" replace />
                    )
                  }
                />

                {/* Manager routes */}
                <Route
                  path="manager"
                  element={
                    currentUser &&
                    ['manager', 'admin'].includes(currentUser.role) ? (
                      <ManagerDashboard currentUser={currentUser} />
                    ) : (
                      <Navigate to="/login" replace />
                    )
                  }
                />
                <Route
                  path="manager/attendance"
                  element={
                    currentUser &&
                    ['manager', 'admin'].includes(currentUser.role) ? (
                      <ManagerAttendance currentUser={currentUser} />
                    ) : (
                      <Navigate to="/login" replace />
                    )
                  }
                />

                {/* Manager Payslips */}
                <Route
                  path="manager/payslips"
                  element={
                    currentUser &&
                    ['manager', 'admin'].includes(currentUser.role) ? (
                      <ManagerPayslips currentUser={currentUser} />
                    ) : (
                      <Navigate to="/login" replace />
                    )
                  }
                />

                {/* Admin routes */}
                <Route
                  path="admin/staffs"
                  element={
                    currentUser?.role === 'admin' ? (
                      <AdminStaffs currentUser={currentUser} />
                    ) : (
                      <Navigate to="/login" replace />
                    )
                  }
                />
                <Route
                  path="admin/devices"
                  element={
                    currentUser?.role === 'admin' ? (
                      <AdminDevices currentUser={currentUser} />
                    ) : (
                      <Navigate to="/login" replace />
                    )
                  }
                />
                <Route
                  path="admin/events"
                  element={
                    currentUser?.role === 'admin' ? (
                      <AdminEvents currentUser={currentUser} />
                    ) : (
                      <Navigate to="/login" replace />
                    )
                  }
                />

                {/* Default redirect */}
                <Route
                  index
                  element={
                    <Navigate
                      to={
                        currentUser?.role === 'admin'
                          ? '/admin/staffs'
                          : currentUser?.role === 'manager'
                            ? '/manager'
                            : '/station'
                      }
                      replace
                    />
                  }
                />
              </Route>
            </Routes>
          </Router>
        </QueryClientProvider>
      </LocalizationProvider>
    </ThemeProvider>
  )
}

export default App

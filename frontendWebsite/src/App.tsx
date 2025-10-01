import { useEffect } from 'react'
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

// Stores
import { useUserStore, useAppStore, useTheme } from './stores'

// Components
import { AppShell } from './components/AppShell'
import { LoadingSpinner, ErrorBoundary } from './components/ui'

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
import { AdminLoginHistory } from './pages/AdminLoginHistory'
// All admin pages are now imported above

// Types - imported for type definitions used in components

// Offline functionality
import { initDB } from './offline/db'
import { startAutoSync, stopAutoSync } from './offline/sync'
import { httpClient } from './api/http'

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

function App() {
  // Zustand stores
  const { currentUser, isLoading, isAuthenticated, login, logout, setLoading } = useUserStore()
  const { setOnlineStatus } = useAppStore()
  const themeMode = useTheme()

  // Create theme based on store preference
  const theme = createTheme({
    palette: {
      mode: themeMode,
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

  // Initialize app on mount
  useEffect(() => {
    const initializeApp = async () => {
      try {
        // Initialize IndexedDB
        await initDB()

        // Start auto sync for offline events
        startAutoSync()

        // Set up online/offline listeners
        const handleOnline = () => setOnlineStatus(true)
        const handleOffline = () => setOnlineStatus(false)

        window.addEventListener('online', handleOnline)
        window.addEventListener('offline', handleOffline)

        // Pre-warm backend to reduce first-login latency
        try {
          await httpClient.get('/health', { timeout: 5000 })
        } catch (err) {
          // Ignore pre-warm errors; backend may still be starting
          console.warn('Backend pre-warm failed (will not block app):', err)
        }

        // User state is automatically loaded from localStorage via Zustand persist
        setLoading(false)
      } catch (error) {
        console.error('Failed to initialize app:', error)
        setLoading(false)
      }
    }

    initializeApp()

    // Cleanup on unmount
    return () => {
      stopAutoSync()
      window.removeEventListener('online', () => setOnlineStatus(true))
      window.removeEventListener('offline', () => setOnlineStatus(false))
    }
  }, [])

  // Show loading screen while initializing
  if (isLoading) {
    return (
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <LoadingSpinner
          message="Initializing Farm Time Management System..."
          fullScreen
        />
      </ThemeProvider>
    )
  }

  return (
    <ErrorBoundary>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <LocalizationProvider dateAdapter={AdapterDayjs}>
          <QueryClientProvider client={queryClient}>
            <Router basename="/comp9034FarmSystem">
              <Routes>
              {/* Login route */}
              <Route
                path="/login"
                element={
                  isAuthenticated ? (
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
                  ) : (
                    <Login onLogin={login} />
                  )
                }
              />

              {/* Protected routes with AppShell */}
              <Route
                path="/"
                element={
                  !isAuthenticated ? (
                    <Navigate to="/login" replace />
                  ) : (
                    <AppShell
                      currentUser={currentUser}
                      onLogout={logout}
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
                <Route
                  path="admin/login-history"
                  element={
                    currentUser?.role === 'admin' ? (
                      <AdminLoginHistory />
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
    </ErrorBoundary>
  )
}

export default App

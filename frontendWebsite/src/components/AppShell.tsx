import React, { useState } from 'react'
import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import {
  AppBar,
  Box,
  CssBaseline,
  Drawer,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
  Divider,
  Avatar,
  Chip,
} from '@mui/material'
import {
  Menu as MenuIcon,
  Schedule,
  Dashboard,
  People,
  EventNote,
  Receipt,
  AccessTime,
  Computer,
  ExitToApp,
  CalendarMonth,
} from '@mui/icons-material'
import { CurrentUser } from '../types/api'
import { NetworkBadge } from './NetworkBadge'

const drawerWidth = 240

interface AppShellProps {
  currentUser: CurrentUser | null
  onLogout: () => void
}

interface NavItem {
  text: string
  icon: React.ReactElement
  path: string
  roles: string[]
}

const navigationItems: NavItem[] = [
  // Staff navigation
  {
    text: 'Clock Station',
    icon: <AccessTime />,
    path: '/station',
    roles: ['staff'],
  },
  {
    text: 'My Roster',
    icon: <CalendarMonth />,
    path: '/roster',
    roles: ['staff'],
  },

  // Manager navigation
  {
    text: 'Dashboard',
    icon: <Dashboard />,
    path: '/manager',
    roles: ['manager', 'admin'],
  },
  {
    text: 'Attendance',
    icon: <EventNote />,
    path: '/manager/attendance',
    roles: ['manager', 'admin'],
  },
  {
    text: 'Payslips',
    icon: <Receipt />,
    path: '/manager/payslips',
    roles: ['manager', 'admin'],
  },

  // Admin navigation
  {
    text: 'Staff Management',
    icon: <People />,
    path: '/admin/staffs',
    roles: ['admin'],
  },
  {
    text: 'Device Management',
    icon: <Computer />,
    path: '/admin/devices',
    roles: ['admin'],
  },
  {
    text: 'Event Management',
    icon: <Schedule />,
    path: '/admin/events',
    roles: ['admin'],
  },
]

export function AppShell({ currentUser, onLogout }: AppShellProps) {
  const [mobileOpen, setMobileOpen] = useState(false)
  const navigate = useNavigate()
  const location = useLocation()

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen)
  }

  const handleNavigation = (path: string) => {
    navigate(path)
    setMobileOpen(false)
  }

  const getFilteredNavItems = () => {
    if (!currentUser) return []
    return navigationItems.filter(item => item.roles.includes(currentUser.role))
  }

  const getRoleLabel = (role: string) => {
    const labels = {
      staff: 'Employee',
      manager: 'Manager',
      admin: 'Administrator',
    }
    return labels[role as keyof typeof labels] || role
  }

  const getRoleColor = (role: string) => {
    switch (role) {
      case 'staff':
        return 'primary'
      case 'manager':
        return 'secondary'
      case 'admin':
        return 'error'
      default:
        return 'default'
    }
  }

  const drawer = (
    <div>
      <Toolbar>
        <Typography variant="h6" noWrap component="div">
          Assignment App
        </Typography>
      </Toolbar>
      <Divider />

      {currentUser && (
        <>
          <Box sx={{ p: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
              <Avatar sx={{ bgcolor: 'primary.main' }}>
                {currentUser.firstName[0]}
                {currentUser.lastName[0]}
              </Avatar>
              <Box>
                <Typography variant="subtitle2" noWrap>
                  {currentUser.firstName} {currentUser.lastName}
                </Typography>
                <Chip
                  label={getRoleLabel(currentUser.role)}
                  size="small"
                  color={getRoleColor(currentUser.role)}
                />
              </Box>
            </Box>
          </Box>
          <Divider />
        </>
      )}

      <List>
        {getFilteredNavItems().map(item => (
          <ListItem key={item.text} disablePadding>
            <ListItemButton
              selected={location.pathname === item.path}
              onClick={() => handleNavigation(item.path)}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.text} />
            </ListItemButton>
          </ListItem>
        ))}
      </List>

      <Divider />
      <List>
        <ListItem disablePadding>
          <ListItemButton onClick={onLogout}>
            <ListItemIcon>
              <ExitToApp />
            </ListItemIcon>
            <ListItemText primary="Logout" />
          </ListItemButton>
        </ListItem>
      </List>
    </div>
  )

  if (!currentUser) {
    return (
      <Box sx={{ display: 'flex', minHeight: '100vh' }}>
        <CssBaseline />
        <Box component="main" sx={{ flexGrow: 1 }}>
          <Outlet />
        </Box>
      </Box>
    )
  }

  return (
    <Box sx={{ display: 'flex' }}>
      <CssBaseline />
      <AppBar
        position="fixed"
        sx={{
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          ml: { sm: `${drawerWidth}px` },
        }}
      >
        <Toolbar>
          <IconButton
            color="inherit"
            aria-label="open drawer"
            edge="start"
            onClick={handleDrawerToggle}
            sx={{ mr: 2, display: { sm: 'none' } }}
          >
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" noWrap component="div" sx={{ flexGrow: 1 }}>
            {navigationItems.find(item => item.path === location.pathname)
              ?.text || 'Assignment App'}
          </Typography>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <NetworkBadge />
          </Box>
        </Toolbar>
      </AppBar>
      <Box
        component="nav"
        sx={{ width: { sm: drawerWidth }, flexShrink: { sm: 0 } }}
        aria-label="navigation"
      >
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={handleDrawerToggle}
          ModalProps={{
            keepMounted: true, // Better open performance on mobile.
          }}
          sx={{
            display: { xs: 'block', sm: 'none' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: drawerWidth,
            },
          }}
        >
          {drawer}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: 'none', sm: 'block' },
            '& .MuiDrawer-paper': {
              boxSizing: 'border-box',
              width: drawerWidth,
            },
          }}
          open
        >
          {drawer}
        </Drawer>
      </Box>
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: { sm: `calc(100% - ${drawerWidth}px)` },
          mt: { xs: 7, sm: 8 }, // Account for AppBar height
          minHeight: `calc(100vh - ${64}px)`, // Full height minus AppBar
        }}
      >
        <Outlet />
      </Box>
    </Box>
  )
}

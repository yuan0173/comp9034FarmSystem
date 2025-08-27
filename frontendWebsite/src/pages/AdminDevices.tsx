import { useState } from 'react'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  IconButton,
  Button,
  TextField,
  InputAdornment,
  Fab,
} from '@mui/material'
import {
  Computer,
  Search,
  Edit,
  Delete,
  Add,
  Refresh,
} from '@mui/icons-material'
import { CurrentUser, Device } from '../types/api'

interface AdminDevicesProps {
  currentUser: CurrentUser
}

export function AdminDevices({ currentUser: _currentUser }: AdminDevicesProps) {
  const [searchQuery, setSearchQuery] = useState('')

  // Mock devices data for demo
  const mockDevices: Device[] = [
    {
      deviceId: 1,
      location: 'Main Entrance',
      type: 'Biometric Scanner',
      status: 'online',
    },
    {
      deviceId: 2,
      location: 'Office Floor 2',
      type: 'Card Reader',
      status: 'online',
    },
    {
      deviceId: 3,
      location: 'Warehouse Entry',
      type: 'PIN Pad',
      status: 'offline',
    },
    {
      deviceId: 4,
      location: 'Break Room',
      type: 'Mobile Check-in',
      status: 'online',
    },
    {
      deviceId: 5,
      location: 'Parking Lot',
      type: 'QR Scanner',
      status: 'online',
    },
    {
      deviceId: 6,
      location: 'Conference Room A',
      type: 'Facial Recognition',
      status: 'maintenance',
    },
    {
      deviceId: 7,
      location: 'Loading Dock',
      type: 'RFID Reader',
      status: 'online',
    },
    {
      deviceId: 8,
      location: 'Executive Floor',
      type: 'Biometric Scanner',
      status: 'offline',
    },
  ]

  // Use mock data directly for demo
  const devices = mockDevices
  const isLoading = false

  // Filter devices based on search query
  const filteredDevices = devices.filter(
    device =>
      device.location.toLowerCase().includes(searchQuery.toLowerCase()) ||
      device.type.toLowerCase().includes(searchQuery.toLowerCase()) ||
      device.deviceId.toString().includes(searchQuery)
  )

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'online':
        return 'success'
      case 'offline':
        return 'error'
      case 'maintenance':
        return 'warning'
      default:
        return 'default'
    }
  }

  const getStatusText = (status: string) => {
    return status.charAt(0).toUpperCase() + status.slice(1)
  }

  const handleAddDevice = () => {
    alert('Add Device functionality - would open dialog in full implementation')
  }

  const handleEditDevice = (deviceId: number) => {
    alert(
      `Edit Device ${deviceId} - would open edit dialog in full implementation`
    )
  }

  const handleDeleteDevice = (deviceId: number) => {
    alert(
      `Delete Device ${deviceId} - would show confirmation dialog in full implementation`
    )
  }

  const handleRefresh = () => {
    alert('Refresh - would reload device data from API in full implementation')
  }

  const getDeviceStats = () => {
    const online = devices.filter(d => d.status === 'online').length
    const offline = devices.filter(d => d.status === 'offline').length
    const maintenance = devices.filter(d => d.status === 'maintenance').length
    return { online, offline, maintenance, total: devices.length }
  }

  const stats = getDeviceStats()

  return (
    <Box>
      {/* Header */}
      <Paper elevation={2} sx={{ p: 3, mb: 3, textAlign: 'center' }}>
        <Computer sx={{ fontSize: 40, color: 'primary.main', mb: 1 }} />
        <Typography variant="h4" gutterBottom>
          Device Management
        </Typography>
        <Typography variant="subtitle1" color="textSecondary">
          Manage attendance tracking devices
        </Typography>
      </Paper>

      {/* Demo Mode Alert */}

      {/* Stats Cards */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3, flexWrap: 'wrap' }}>
        <Card sx={{ minWidth: 120, flex: 1 }}>
          <CardContent sx={{ textAlign: 'center', py: 2 }}>
            <Typography variant="h4" color="success.main">
              {stats.online}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Online
            </Typography>
          </CardContent>
        </Card>
        <Card sx={{ minWidth: 120, flex: 1 }}>
          <CardContent sx={{ textAlign: 'center', py: 2 }}>
            <Typography variant="h4" color="error.main">
              {stats.offline}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Offline
            </Typography>
          </CardContent>
        </Card>
        <Card sx={{ minWidth: 120, flex: 1 }}>
          <CardContent sx={{ textAlign: 'center', py: 2 }}>
            <Typography variant="h4" color="warning.main">
              {stats.maintenance}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Maintenance
            </Typography>
          </CardContent>
        </Card>
        <Card sx={{ minWidth: 120, flex: 1 }}>
          <CardContent sx={{ textAlign: 'center', py: 2 }}>
            <Typography variant="h4" color="primary.main">
              {stats.total}
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Total
            </Typography>
          </CardContent>
        </Card>
      </Box>

      {/* Controls */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3, alignItems: 'center' }}>
        <TextField
          placeholder="Search devices..."
          value={searchQuery}
          onChange={e => setSearchQuery(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <Search />
              </InputAdornment>
            ),
          }}
          sx={{ flexGrow: 1 }}
        />
        <Button
          variant="outlined"
          startIcon={<Refresh />}
          onClick={handleRefresh}
        >
          Refresh
        </Button>
        <Button
          variant="contained"
          startIcon={<Add />}
          onClick={handleAddDevice}
        >
          Add Device
        </Button>
      </Box>

      {/* Devices Table */}
      <Card>
        <CardContent>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>Device ID</TableCell>
                  <TableCell>Location</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {isLoading ? (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      Loading devices...
                    </TableCell>
                  </TableRow>
                ) : filteredDevices.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      {searchQuery
                        ? 'No devices match your search'
                        : 'No devices found'}
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredDevices.map(device => (
                    <TableRow key={device.deviceId} hover>
                      <TableCell>
                        <Typography variant="body2" fontWeight="bold">
                          #{device.deviceId}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {device.location}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">{device.type}</Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={getStatusText(device.status)}
                          color={getStatusColor(device.status) as any}
                          size="small"
                        />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton
                          size="small"
                          onClick={() => handleEditDevice(device.deviceId)}
                          color="primary"
                        >
                          <Edit />
                        </IconButton>
                        <IconButton
                          size="small"
                          onClick={() => handleDeleteDevice(device.deviceId)}
                          color="error"
                        >
                          <Delete />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </CardContent>
      </Card>

      {/* Floating Action Button */}
      <Fab
        color="primary"
        aria-label="add device"
        sx={{ position: 'fixed', bottom: 16, right: 16 }}
        onClick={handleAddDevice}
      >
        <Add />
      </Fab>
    </Box>
  )
}

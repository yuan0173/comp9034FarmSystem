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
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  CircularProgress,
} from '@mui/material'
import {
  Computer,
  Search,
  Edit,
  Delete,
  Add,
  Refresh,
  Warning,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { deviceApi } from '../api/client'
import { CurrentUser, Device } from '../types/api'

interface AdminDevicesProps {
  currentUser: CurrentUser
}

export function AdminDevices({ currentUser: _currentUser }: AdminDevicesProps) {
  const [searchQuery, setSearchQuery] = useState('')
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false)
  const [editingDevice, setEditingDevice] = useState<Device | null>(null)
  const [deletingDevice, setDeletingDevice] = useState<Device | null>(null)
  const [formData, setFormData] = useState({
    name: '',
    type: '',
    location: '',
    ipAddress: '',
    status: 'active',
  })
  const [formErrors, setFormErrors] = useState<any>({})

  const queryClient = useQueryClient()

  // Fetch devices from API
  const { data: devices = [], isLoading, refetch } = useQuery({
    queryKey: ['devices'],
    queryFn: () => deviceApi.getAll({ query: searchQuery }),
  })

  // Create device mutation
  const createDeviceMutation = useMutation({
    mutationFn: (data: Omit<Device, 'deviceId'>) => deviceApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['devices'] })
      setIsAddDialogOpen(false)
      resetForm()
    },
    onError: (error: any) => {
      console.error('Failed to create device:', error)
    },
  })

  // Update device mutation
  const updateDeviceMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<Device> }) => 
      deviceApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['devices'] })
      setIsEditDialogOpen(false)
      resetForm()
      setEditingDevice(null)
    },
    onError: (error: any) => {
      console.error('Failed to update device:', error)
    },
  })

  // Delete device mutation
  const deleteDeviceMutation = useMutation({
    mutationFn: (id: number) => deviceApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['devices'] })
      setIsDeleteDialogOpen(false)
      setDeletingDevice(null)
    },
    onError: (error: any) => {
      console.error('Failed to delete device:', error)
    },
  })



  // Helper functions
  const resetForm = () => {
    setFormData({
      name: '',
      type: '',
      location: '',
      ipAddress: '',
      status: 'active',
    })
    setFormErrors({})
  }

  const validateForm = () => {
    const errors: any = {}
    
    if (!formData.name.trim()) {
      errors.name = 'Device name is required'
    }
    if (!formData.type) {
      errors.type = 'Device type is required'
    }
    if (!formData.location.trim()) {
      errors.location = 'Location is required'
    }
    
    setFormErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleSubmit = () => {
    if (!validateForm()) return

    if (editingDevice) {
      // Update device
      const updatedDevice = {
        ...editingDevice,
        ...formData,
      }
      updateDeviceMutation.mutate({ id: editingDevice.id!, data: updatedDevice })
    } else {
      // Create device
      createDeviceMutation.mutate(formData as any)
    }
  }

  const handleAddDevice = () => {
    resetForm()
    setIsAddDialogOpen(true)
  }

  const handleEditDevice = (device: Device) => {
    setFormData({
      name: device.name || '',
      type: device.type || '',
      location: device.location || '',
      ipAddress: device.ipAddress || '',
      status: device.status || 'active',
    })
    setFormErrors({})
    setEditingDevice(device)
    setIsEditDialogOpen(true)
  }

  const handleDeleteDevice = (device: Device) => {
    setDeletingDevice(device)
    setIsDeleteDialogOpen(true)
  }

  const confirmDelete = () => {
    if (deletingDevice?.id) {
      deleteDeviceMutation.mutate(deletingDevice.id)
    }
  }

  const handleRefresh = () => {
    refetch()
  }

  // Filter devices based on search query
  const filteredDevices = devices.filter(
    device =>
      device.name?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      device.location?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      device.type?.toLowerCase().includes(searchQuery.toLowerCase()) ||
      device.id?.toString().includes(searchQuery)
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
                          onClick={() => handleEditDevice(device)}
                          color="primary"
                        >
                          <Edit />
                        </IconButton>
                        <IconButton
                          size="small"
                          onClick={() => handleDeleteDevice(device)}
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

      {/* Add Device Dialog */}
      <Dialog open={isAddDialogOpen} onClose={() => setIsAddDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Add New Device</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              label="Device Name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              error={!!formErrors.name}
              helperText={formErrors.name}
              autoComplete="off"
              sx={{ mb: 2 }}
            />
            
            <FormControl fullWidth sx={{ mb: 2 }} error={!!formErrors.type}>
              <InputLabel>Device Type</InputLabel>
              <Select
                value={formData.type}
                label="Device Type"
                onChange={(e) => setFormData({ ...formData, type: e.target.value })}
              >
                <MenuItem value="biometric">Biometric Scanner</MenuItem>
                <MenuItem value="terminal">Card Reader</MenuItem>
                <MenuItem value="card_reader">PIN Pad</MenuItem>
                <MenuItem value="mobile">Mobile Check-in</MenuItem>
                <MenuItem value="qr">QR Scanner</MenuItem>
                <MenuItem value="facial">Facial Recognition</MenuItem>
                <MenuItem value="rfid">RFID Reader</MenuItem>
              </Select>
              {formErrors.type && <Typography variant="caption" color="error">{formErrors.type}</Typography>}
            </FormControl>

            <TextField
              fullWidth
              label="Location"
              value={formData.location}
              onChange={(e) => setFormData({ ...formData, location: e.target.value })}
              error={!!formErrors.location}
              helperText={formErrors.location}
              autoComplete="off"
              sx={{ mb: 2 }}
            />

            <TextField
              fullWidth
              label="IP Address (Optional)"
              value={formData.ipAddress}
              onChange={(e) => setFormData({ ...formData, ipAddress: e.target.value })}
              autoComplete="off"
              sx={{ mb: 2 }}
            />

            <FormControl fullWidth>
              <InputLabel>Status</InputLabel>
              <Select
                value={formData.status}
                label="Status"
                onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              >
                <MenuItem value="active">Active</MenuItem>
                <MenuItem value="inactive">Inactive</MenuItem>
                <MenuItem value="maintenance">Maintenance</MenuItem>
              </Select>
            </FormControl>

            {createDeviceMutation.isError && (
              <Alert severity="error" sx={{ mt: 2 }}>
                Failed to create device. Please try again.
              </Alert>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setIsAddDialogOpen(false)} disabled={createDeviceMutation.isPending}>
            Cancel
          </Button>
          <Button 
            onClick={handleSubmit}
            variant="contained"
            disabled={createDeviceMutation.isPending}
            startIcon={createDeviceMutation.isPending ? <CircularProgress size={20} /> : <Add />}
          >
            {createDeviceMutation.isPending ? 'Creating...' : 'Create Device'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Edit Device Dialog */}
      <Dialog open={isEditDialogOpen} onClose={() => setIsEditDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Edit Device</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <TextField
              fullWidth
              label="Device Name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              error={!!formErrors.name}
              helperText={formErrors.name}
              autoComplete="off"
              sx={{ mb: 2 }}
            />
            
            <FormControl fullWidth sx={{ mb: 2 }} error={!!formErrors.type}>
              <InputLabel>Device Type</InputLabel>
              <Select
                value={formData.type}
                label="Device Type"
                onChange={(e) => setFormData({ ...formData, type: e.target.value })}
              >
                <MenuItem value="biometric">Biometric Scanner</MenuItem>
                <MenuItem value="terminal">Card Reader</MenuItem>
                <MenuItem value="card_reader">PIN Pad</MenuItem>
                <MenuItem value="mobile">Mobile Check-in</MenuItem>
                <MenuItem value="qr">QR Scanner</MenuItem>
                <MenuItem value="facial">Facial Recognition</MenuItem>
                <MenuItem value="rfid">RFID Reader</MenuItem>
              </Select>
              {formErrors.type && <Typography variant="caption" color="error">{formErrors.type}</Typography>}
            </FormControl>

            <TextField
              fullWidth
              label="Location"
              value={formData.location}
              onChange={(e) => setFormData({ ...formData, location: e.target.value })}
              error={!!formErrors.location}
              helperText={formErrors.location}
              autoComplete="off"
              sx={{ mb: 2 }}
            />

            <TextField
              fullWidth
              label="IP Address"
              value={formData.ipAddress}
              onChange={(e) => setFormData({ ...formData, ipAddress: e.target.value })}
              autoComplete="off"
              sx={{ mb: 2 }}
            />

            <FormControl fullWidth>
              <InputLabel>Status</InputLabel>
              <Select
                value={formData.status}
                label="Status"
                onChange={(e) => setFormData({ ...formData, status: e.target.value })}
              >
                <MenuItem value="active">Active</MenuItem>
                <MenuItem value="inactive">Inactive</MenuItem>
                <MenuItem value="maintenance">Maintenance</MenuItem>
              </Select>
            </FormControl>

            {updateDeviceMutation.isError && (
              <Alert severity="error" sx={{ mt: 2 }}>
                Failed to update device. Please try again.
              </Alert>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setIsEditDialogOpen(false)} disabled={updateDeviceMutation.isPending}>
            Cancel
          </Button>
          <Button 
            onClick={handleSubmit}
            variant="contained"
            disabled={updateDeviceMutation.isPending}
            startIcon={updateDeviceMutation.isPending ? <CircularProgress size={20} /> : <Edit />}
          >
            {updateDeviceMutation.isPending ? 'Updating...' : 'Update Device'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog open={isDeleteDialogOpen} onClose={() => setIsDeleteDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Warning color="error" />
          Confirm Delete
        </DialogTitle>
        <DialogContent>
          <Typography variant="body1" sx={{ mb: 2 }}>
            Are you sure you want to delete this device?
          </Typography>
          
          {deletingDevice && (
            <Box sx={{ 
              p: 2, 
              bgcolor: 'grey.100', 
              borderRadius: 1,
              border: '1px solid',
              borderColor: 'grey.300'
            }}>
              <Typography variant="subtitle2" gutterBottom>
                Device Information:
              </Typography>
              <Typography variant="body2">
                <strong>Name:</strong> {deletingDevice.name}
              </Typography>
              <Typography variant="body2">
                <strong>Type:</strong> {deletingDevice.type}
              </Typography>
              <Typography variant="body2">
                <strong>Location:</strong> {deletingDevice.location}
              </Typography>
            </Box>
          )}
          
          <Alert severity="warning" sx={{ mt: 2 }}>
            <Typography variant="body2">
              <strong>Warning:</strong> This action will permanently remove the device from the system.
            </Typography>
          </Alert>
          
          {deleteDeviceMutation.isError && (
            <Alert severity="error" sx={{ mt: 2 }}>
              Failed to delete device. Please try again.
            </Alert>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setIsDeleteDialogOpen(false)} disabled={deleteDeviceMutation.isPending}>
            Cancel
          </Button>
          <Button 
            onClick={confirmDelete}
            variant="contained"
            color="error"
            disabled={deleteDeviceMutation.isPending}
            startIcon={deleteDeviceMutation.isPending ? <CircularProgress size={20} /> : <Delete />}
          >
            {deleteDeviceMutation.isPending ? 'Deleting...' : 'Delete Device'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

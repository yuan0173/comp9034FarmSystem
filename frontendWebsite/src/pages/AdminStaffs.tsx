import { useState } from 'react'
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
import { People, Add, Search, Edit, Delete, Warning, FilterList, Sort } from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { staffApi } from '../api/client'
import { CurrentUser, Staff } from '../types/api'

interface AdminStaffsProps {
  currentUser: CurrentUser
}

export function AdminStaffs({ currentUser: _currentUser }: AdminStaffsProps) {
  const [searchQuery, setSearchQuery] = useState('')
  const [roleFilter, setRoleFilter] = useState<string>('all')
  const [statusFilter, setStatusFilter] = useState<string>('all')
  const [sortBy, setSortBy] = useState<string>('name')
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc')
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false)
  const [editingStaff, setEditingStaff] = useState<Staff | null>(null)
  const [deletingStaff, setDeletingStaff] = useState<Staff | null>(null)
  const [formData, setFormData] = useState({
    id: '',
    name: '',
    email: '',
    pin: '',
    role: '',
    hourlyRate: '',
  })
  const [formErrors, setFormErrors] = useState<Record<string, string>>({})
  
  const queryClient = useQueryClient()

  // Fetch staff data from API
  const { data: staffs = [], isLoading } = useQuery({
    queryKey: ['staffs'],
    queryFn: () => staffApi.getAll(),
    retry: 1,
    refetchOnWindowFocus: false,
  })

  // Create staff mutation
  const createStaffMutation = useMutation({
    mutationFn: (newStaff: Partial<Staff>) => staffApi.create(newStaff),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['staffs'] })
      setIsAddDialogOpen(false)
      resetForm()
    },
    onError: (error: any) => {
      console.error('Failed to create staff:', error)
    },
  })

  // Update staff mutation
  const updateStaffMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: Partial<Staff> }) => 
      staffApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['staffs'] })
      setIsEditDialogOpen(false)
      resetForm()
      setEditingStaff(null)
    },
    onError: (error: any) => {
      console.error('Failed to update staff:', error)
    },
  })

  // Delete staff mutation
  const deleteStaffMutation = useMutation({
    mutationFn: (id: number) => staffApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['staffs'] })
      setIsDeleteDialogOpen(false)
      setDeletingStaff(null)
    },
    onError: (error: any) => {
      console.error('Failed to delete staff:', error)
    },
  })

  // Helper functions
  const resetForm = () => {
    setFormData({
      id: '',
      name: '',
      email: '',
      pin: '',
      role: '',
      hourlyRate: '',
    })
    setFormErrors({})
  }

  const validateForm = (isEditing = false) => {
    const errors: Record<string, string> = {}
    
    // ID validation (only for new staff)
    if (!isEditing) {
      if (!formData.id) errors.id = 'Staff ID is required'
      else if (!/^\d+$/.test(formData.id)) errors.id = 'Staff ID must be a number'
      else if (staffs.find(s => s.id?.toString() === formData.id)) {
        errors.id = 'Staff ID already exists'
      }
    }
    
    if (!formData.name.trim()) errors.name = 'Name is required'
    if (!formData.email.trim()) errors.email = 'Email is required'
    else if (!/\S+@\S+\.\S+/.test(formData.email)) errors.email = 'Invalid email format'
    
    if (!formData.pin) errors.pin = 'PIN is required'
    else if (formData.pin.length < 4) errors.pin = 'PIN must be at least 4 characters'
    
    if (!formData.role) errors.role = 'Role is required'
    
    if (!formData.hourlyRate) errors.hourlyRate = 'Hourly rate is required'
    else if (isNaN(Number(formData.hourlyRate)) || Number(formData.hourlyRate) <= 0) {
      errors.hourlyRate = 'Hourly rate must be a positive number'
    }
    
    setFormErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleSubmit = () => {
    const isEditing = editingStaff !== null
    if (!validateForm(isEditing)) return
    
    if (isEditing) {
      // Update existing staff
      const updatedStaff = {
        ...editingStaff,
        name: formData.name.trim(),
        email: formData.email.trim(),
        pin: formData.pin,
        role: formData.role,
        hourlyRate: parseFloat(formData.hourlyRate),
      }
      
      updateStaffMutation.mutate({ 
        id: editingStaff.id!, 
        data: updatedStaff 
      })
    } else {
      // Create new staff
      const newStaff = {
        id: parseInt(formData.id),
        name: formData.name.trim(),
        email: formData.email.trim(),
        pin: formData.pin,
        role: formData.role,
        hourlyRate: parseFloat(formData.hourlyRate),
        isActive: true,
      }
      
      createStaffMutation.mutate(newStaff)
    }
  }

  const handleAddStaff = () => {
    resetForm()
    setEditingStaff(null)
    setIsAddDialogOpen(true)
  }

  const handleEditStaff = (staff: Staff) => {
    setFormData({
      id: staff.id?.toString() || '',
      name: staff.name || '',
      email: staff.email || '',
      pin: staff.pin || '',
      role: staff.role || '',
      hourlyRate: staff.hourlyRate?.toString() || '',
    })
    setFormErrors({})
    setEditingStaff(staff)
    setIsEditDialogOpen(true)
  }

  const handleDeleteStaff = (staff: Staff) => {
    setDeletingStaff(staff)
    setIsDeleteDialogOpen(true)
  }

  const confirmDelete = () => {
    if (deletingStaff?.id) {
      deleteStaffMutation.mutate(deletingStaff.id)
    }
  }

  const clearFilters = () => {
    setSearchQuery('')
    setRoleFilter('all')
    setStatusFilter('all')
    setSortBy('name')
    setSortDirection('asc')
  }

  // Filter and sort staffs
  const filteredAndSortedStaffs = staffs
    .filter(staff => {
      // Search query filter
      const matchesSearch = !searchQuery || (
        staff.name?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        staff.email?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        staff.id?.toString().includes(searchQuery) ||
        staff.username?.toLowerCase().includes(searchQuery.toLowerCase()) ||
        staff.phone?.toLowerCase().includes(searchQuery.toLowerCase())
      )

      // Role filter
      const matchesRole = roleFilter === 'all' || staff.role === roleFilter

                    // Status filter (keep the frontend filtering since backend only returns active staff)
              const matchesStatus = statusFilter === 'all' || 
                (statusFilter === 'active' && staff.isActive) ||
                (statusFilter === 'inactive' && !staff.isActive)

      return matchesSearch && matchesRole && matchesStatus
    })
    .sort((a, b) => {
      let aValue: any
      let bValue: any

      switch (sortBy) {
        case 'name':
          aValue = a.name?.toLowerCase() || ''
          bValue = b.name?.toLowerCase() || ''
          break
        case 'id':
          aValue = a.id || 0
          bValue = b.id || 0
          break
        case 'role':
          aValue = a.role?.toLowerCase() || ''
          bValue = b.role?.toLowerCase() || ''
          break
        case 'email':
          aValue = a.email?.toLowerCase() || ''
          bValue = b.email?.toLowerCase() || ''
          break
        case 'hourlyRate':
          aValue = a.hourlyRate || 0
          bValue = b.hourlyRate || 0
          break
        case 'createdAt':
          aValue = new Date(a.createdAt || 0).getTime()
          bValue = new Date(b.createdAt || 0).getTime()
          break
        default:
          aValue = a.name?.toLowerCase() || ''
          bValue = b.name?.toLowerCase() || ''
      }

      if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1
      if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1
      return 0
    })

  // For backward compatibility, keep the original name
  const filteredStaffs = filteredAndSortedStaffs

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

      {/* Controls */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          {/* Search and Filter Controls */}
          <Grid container spacing={2} alignItems="center" sx={{ mb: 2 }}>
            {/* Search Field */}
            <Grid item xs={12} md={4}>
              <TextField
                size="small"
                fullWidth
                label="Search Staff"
                placeholder="Search by name, email, ID, or phone..."
                value={searchQuery}
                onChange={e => setSearchQuery(e.target.value)}
                InputProps={{
                  startAdornment: <Search sx={{ mr: 1, color: 'grey.500' }} />,
                }}
              />
            </Grid>

            {/* Role Filter */}
            <Grid item xs={12} sm={6} md={2}>
              <FormControl size="small" fullWidth>
                <InputLabel>Role</InputLabel>
                <Select
                  value={roleFilter}
                  label="Role"
                  onChange={e => setRoleFilter(e.target.value)}
                  startAdornment={<FilterList sx={{ mr: 1, color: 'grey.500' }} />}
                >
                  <MenuItem value="all">All Roles</MenuItem>
                  <MenuItem value="admin">Admin</MenuItem>
                  <MenuItem value="manager">Manager</MenuItem>
                  <MenuItem value="staff">Staff</MenuItem>
                </Select>
              </FormControl>
            </Grid>

            {/* Status Filter */}
            <Grid item xs={12} sm={6} md={2}>
              <FormControl size="small" fullWidth>
                <InputLabel>Status</InputLabel>
                <Select
                  value={statusFilter}
                  label="Status"
                  onChange={e => setStatusFilter(e.target.value)}
                >
                  <MenuItem value="all">All Status</MenuItem>
                  <MenuItem value="active">Active</MenuItem>
                  <MenuItem value="inactive">Inactive</MenuItem>
                </Select>
              </FormControl>
            </Grid>

            {/* Sort Options */}
            <Grid item xs={12} sm={6} md={2}>
              <FormControl size="small" fullWidth>
                <InputLabel>Sort By</InputLabel>
                <Select
                  value={sortBy}
                  label="Sort By"
                  onChange={e => setSortBy(e.target.value)}
                  startAdornment={<Sort sx={{ mr: 1, color: 'grey.500' }} />}
                >
                  <MenuItem value="name">Name</MenuItem>
                  <MenuItem value="id">Staff ID</MenuItem>
                  <MenuItem value="role">Role</MenuItem>
                  <MenuItem value="email">Email</MenuItem>
                  <MenuItem value="hourlyRate">Pay Rate</MenuItem>
                  <MenuItem value="createdAt">Created Date</MenuItem>
                </Select>
              </FormControl>
            </Grid>

            {/* Action Buttons */}
            <Grid item xs={12} md={2}>
              <Box sx={{ display: 'flex', gap: 1, flexDirection: { xs: 'row', md: 'column' } }}>
                <Button
                  variant="contained"
                  startIcon={<Add />}
                  onClick={handleAddStaff}
                  size="small"
                  fullWidth
                >
                  Add Staff
                </Button>
                <Button
                  variant="outlined"
                  onClick={clearFilters}
                  size="small"
                  fullWidth
                >
                  Clear Filters
                </Button>
              </Box>
            </Grid>
          </Grid>

          {/* Sort Direction Toggle */}
          <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
            <Typography variant="body2" color="text.secondary">
              Sort Direction:
            </Typography>
            <Button
              size="small"
              variant={sortDirection === 'asc' ? 'contained' : 'outlined'}
              onClick={() => setSortDirection('asc')}
            >
              Ascending
            </Button>
            <Button
              size="small"
              variant={sortDirection === 'desc' ? 'contained' : 'outlined'}
              onClick={() => setSortDirection('desc')}
            >
              Descending
            </Button>
          </Box>
        </CardContent>
      </Card>

      {/* Staff Table */}
      <Card>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6">
              Staff Directory ({filteredStaffs.length} of {staffs.length} employees)
            </Typography>
            
            {/* Active Filters Display */}
            {(searchQuery || roleFilter !== 'all' || statusFilter !== 'all' || sortBy !== 'name') && (
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                {searchQuery && (
                  <Chip 
                    label={`Search: "${searchQuery}"`} 
                    size="small" 
                    onDelete={() => setSearchQuery('')}
                    color="primary" 
                    variant="outlined" 
                  />
                )}
                {roleFilter !== 'all' && (
                  <Chip 
                    label={`Role: ${roleFilter}`} 
                    size="small" 
                    onDelete={() => setRoleFilter('all')}
                    color="secondary" 
                    variant="outlined" 
                  />
                )}
                {statusFilter !== 'all' && (
                  <Chip 
                    label={`Status: ${statusFilter}`} 
                    size="small" 
                    onDelete={() => setStatusFilter('all')}
                    color="info" 
                    variant="outlined" 
                  />
                )}
                {sortBy !== 'name' && (
                  <Chip 
                    label={`Sort: ${sortBy} (${sortDirection})`} 
                    size="small" 
                    onDelete={() => { setSortBy('name'); setSortDirection('asc') }}
                    color="success" 
                    variant="outlined" 
                  />
                )}
              </Box>
            )}
          </Box>

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
                    <TableRow key={staff.id}>
                      <TableCell>
                        <Typography variant="body2" fontWeight="bold">
                          {staff.id}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {staff.name}
                        </Typography>
                      </TableCell>
                      <TableCell>{staff.email}</TableCell>
                      <TableCell>
                        <Chip
                          label={staff.role || 'N/A'}
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>Full-time</TableCell>
                      <TableCell>
                        ${staff.hourlyRate?.toFixed(2) || 'N/A'}/hr
                      </TableCell>
                      <TableCell align="center">
                        <Chip
                          label={getStatusLabel(staff.isActive)}
                          color={getStatusColor(staff.isActive)}
                          size="small"
                        />
                      </TableCell>
                      <TableCell align="center">
                        <Box sx={{ display: 'flex', gap: 1, justifyContent: 'center' }}>
                          <Button
                            size="small"
                            variant="outlined"
                            startIcon={<Edit />}
                            onClick={() => handleEditStaff(staff)}
                          >
                            Edit
                          </Button>
                          <Button
                            size="small"
                            variant="outlined"
                            color="error"
                            startIcon={<Delete />}
                            onClick={() => handleDeleteStaff(staff)}
                          >
                            Delete
                          </Button>
                        </Box>
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
                  {staffs.filter(s => s.role === 'admin' || s.role === 'manager').length}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Admin/Manager
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="warning.main">
                  $
                  {(staffs.reduce((sum, s) => sum + (s.hourlyRate || 0), 0) /
                    staffs.length).toFixed(2)}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Avg. Pay Rate
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Add Staff Dialog */}
      <Dialog 
        open={isAddDialogOpen} 
        onClose={() => setIsAddDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Add New Staff Member</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Staff ID"
                  value={formData.id}
                  onChange={(e) => setFormData({ ...formData, id: e.target.value })}
                  error={!!formErrors.id}
                  helperText={formErrors.id || 'e.g., 1001, 8001, 9001'}
                  placeholder="Enter staff ID number"
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="PIN"
                  value={formData.pin}
                  onChange={(e) => setFormData({ ...formData, pin: e.target.value })}
                  error={!!formErrors.pin}
                  helperText={formErrors.pin || 'At least 4 characters'}
                  placeholder="Enter staff PIN"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Full Name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  error={!!formErrors.name}
                  helperText={formErrors.name}
                  placeholder="Enter staff full name"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Email"
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  error={!!formErrors.email}
                  helperText={formErrors.email}
                  placeholder="Enter email address"
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth error={!!formErrors.role}>
                  <InputLabel>Role</InputLabel>
                  <Select
                    value={formData.role}
                    onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                    label="Role"
                  >
                    <MenuItem value="staff">Staff (1000-7999)</MenuItem>
                    <MenuItem value="manager">Manager (8000-8999)</MenuItem>
                    <MenuItem value="admin">Admin (9000+)</MenuItem>
                  </Select>
                  {formErrors.role && (
                    <Typography variant="caption" color="error" sx={{ ml: 2 }}>
                      {formErrors.role}
                    </Typography>
                  )}
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Hourly Rate"
                  value={formData.hourlyRate}
                  onChange={(e) => setFormData({ ...formData, hourlyRate: e.target.value })}
                  error={!!formErrors.hourlyRate}
                  helperText={formErrors.hourlyRate}
                  placeholder="25.00"
                  InputProps={{
                    startAdornment: <Typography sx={{ mr: 1 }}>$</Typography>,
                  }}
                />
              </Grid>
            </Grid>
            
            {createStaffMutation.isError && (
              <Alert severity="error" sx={{ mt: 2 }}>
                Failed to create staff member. Please check all fields and try again.
              </Alert>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={() => setIsAddDialogOpen(false)}
            disabled={createStaffMutation.isPending}
          >
            Cancel
          </Button>
          <Button 
            onClick={handleSubmit}
            variant="contained"
            disabled={createStaffMutation.isPending}
            startIcon={createStaffMutation.isPending ? <CircularProgress size={20} /> : <Add />}
          >
            {createStaffMutation.isPending ? 'Creating...' : 'Create Staff'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Edit Staff Dialog */}
      <Dialog 
        open={isEditDialogOpen} 
        onClose={() => setIsEditDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Edit Staff Member</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Staff ID"
                  value={formData.id}
                  disabled
                  helperText="Staff ID cannot be changed"
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="PIN"
                  value={formData.pin}
                  onChange={(e) => setFormData({ ...formData, pin: e.target.value })}
                  error={!!formErrors.pin}
                  helperText={formErrors.pin || 'At least 4 characters'}
                  placeholder="Enter staff PIN"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Full Name"
                  value={formData.name}
                  onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                  error={!!formErrors.name}
                  helperText={formErrors.name}
                  placeholder="Enter staff full name"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Email"
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  error={!!formErrors.email}
                  helperText={formErrors.email}
                  placeholder="Enter email address"
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <FormControl fullWidth error={!!formErrors.role}>
                  <InputLabel>Role</InputLabel>
                  <Select
                    value={formData.role}
                    onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                    label="Role"
                  >
                    <MenuItem value="staff">Staff (1000-7999)</MenuItem>
                    <MenuItem value="manager">Manager (8000-8999)</MenuItem>
                    <MenuItem value="admin">Admin (9000+)</MenuItem>
                  </Select>
                  {formErrors.role && (
                    <Typography variant="caption" color="error" sx={{ ml: 2 }}>
                      {formErrors.role}
                    </Typography>
                  )}
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Hourly Rate"
                  value={formData.hourlyRate}
                  onChange={(e) => setFormData({ ...formData, hourlyRate: e.target.value })}
                  error={!!formErrors.hourlyRate}
                  helperText={formErrors.hourlyRate}
                  placeholder="25.00"
                  InputProps={{
                    startAdornment: <Typography sx={{ mr: 1 }}>$</Typography>,
                  }}
                />
              </Grid>
            </Grid>
            
            {updateStaffMutation.isError && (
              <Alert severity="error" sx={{ mt: 2 }}>
                Failed to update staff member. Please check all fields and try again.
              </Alert>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={() => setIsEditDialogOpen(false)}
            disabled={updateStaffMutation.isPending}
          >
            Cancel
          </Button>
          <Button 
            onClick={handleSubmit}
            variant="contained"
            disabled={updateStaffMutation.isPending}
            startIcon={updateStaffMutation.isPending ? <CircularProgress size={20} /> : <Edit />}
          >
            {updateStaffMutation.isPending ? 'Updating...' : 'Update Staff'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <Dialog 
        open={isDeleteDialogOpen} 
        onClose={() => setIsDeleteDialogOpen(false)}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Warning color="error" />
          Confirm Delete
        </DialogTitle>
        <DialogContent>
          <Typography variant="body1" sx={{ mb: 2 }}>
            Are you sure you want to delete this staff member?
          </Typography>
          
          {deletingStaff && (
            <Box sx={{ 
              p: 2, 
              bgcolor: 'grey.100', 
              borderRadius: 1,
              border: '1px solid',
              borderColor: 'grey.300'
            }}>
              <Typography variant="subtitle2" gutterBottom>
                Staff Information:
              </Typography>
              <Typography variant="body2">
                <strong>ID:</strong> {deletingStaff.id}
              </Typography>
              <Typography variant="body2">
                <strong>Name:</strong> {deletingStaff.name}
              </Typography>
              <Typography variant="body2">
                <strong>Role:</strong> {deletingStaff.role}
              </Typography>
              <Typography variant="body2">
                <strong>Email:</strong> {deletingStaff.email}
              </Typography>
            </Box>
          )}
          
          <Alert severity="warning" sx={{ mt: 2 }}>
            <Typography variant="body2">
              <strong>Warning:</strong> This action will deactivate the staff member's account. 
              They will no longer be able to clock in/out, but their historical data will be preserved.
            </Typography>
          </Alert>
          
          {deleteStaffMutation.isError && (
            <Alert severity="error" sx={{ mt: 2 }}>
              Failed to delete staff member. Please try again or contact support.
            </Alert>
          )}
        </DialogContent>
        <DialogActions>
          <Button 
            onClick={() => setIsDeleteDialogOpen(false)}
            disabled={deleteStaffMutation.isPending}
          >
            Cancel
          </Button>
          <Button 
            onClick={confirmDelete}
            variant="contained"
            color="error"
            disabled={deleteStaffMutation.isPending}
            startIcon={deleteStaffMutation.isPending ? <CircularProgress size={20} /> : <Delete />}
          >
            {deleteStaffMutation.isPending ? 'Deleting...' : 'Delete Staff'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

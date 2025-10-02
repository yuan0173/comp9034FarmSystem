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
  Tabs,
  Tab,
  Tooltip,
  Switch,
  FormControlLabel,
} from '@mui/material'
import { People, Add, Search, Edit, Delete, Warning, FilterList, Sort, Restore, Archive } from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { staffApi } from '../api/client'
import { CurrentUser, Staff, StaffCreateRequest } from '../types/api'
import { formatDateTimeString } from '../utils/time'

interface AdminStaffsProps {
  currentUser: CurrentUser
}

export function AdminStaffs({ currentUser }: AdminStaffsProps) {
  const [searchQuery, setSearchQuery] = useState('')
  const [roleFilter, setRoleFilter] = useState<string>('all')
  // Status is controlled by tabs; no standalone status filter state
  const [sortBy, setSortBy] = useState<string>('name')
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc')
  const [activeTab, setActiveTab] = useState(0) // 0: Active Staff, 1: Inactive Staff
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false)
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false)
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false)
  const [editingStaff, setEditingStaff] = useState<Staff | null>(null)
  const [deletingStaff, setDeletingStaff] = useState<Staff | null>(null)
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    role: 'Staff',
    phone: '',
    address: '',
    contractType: 'Casual',
    standardPayRate: '',
    overtimePayRate: '',
    standardHoursPerWeek: '40',
    isActive: true,
  })
  const [formErrors, setFormErrors] = useState<Record<string, string>>({})
  
  // TypeScript state variables matching your provided code
  const [emailTimer, setEmailTimer] = useState<NodeJS.Timeout | null>(null)
  const [autoOvertimeEnabled, setAutoOvertimeEnabled] = useState(true)
  const [userTouchedStandardPay, setUserTouchedStandardPay] = useState(false)
  const [, setUserTouchedOvertimePay] = useState(false)
  const [addAnother, setAddAnother] = useState(false)
  
  const queryClient = useQueryClient()

  // Use centralized staffApi + httpClient (env-aware, auth)
  async function searchStaffs(keyword: string, includeInactive: boolean = true): Promise<Staff[]> {
    const results: Staff[] = []
    try {
      const active = await staffApi.getAll({ query: keyword })
      results.push(...active)
    } catch (error) {
      console.warn('Error searching active staff:', error)
    }

    if (includeInactive) {
      try {
        const inactive = await staffApi.getInactive({ query: keyword })
        results.push(...inactive)
      } catch (error) {
        console.warn('Error searching inactive staff:', error)
      }
    }

    return results
  }


  async function validateEmail(email: string): Promise<void> {
    if (emailTimer) clearTimeout(emailTimer)
    
    if (!email) {
      setFormErrors(prev => ({ ...prev, email: '' }))
      return
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    if (!emailRegex.test(email)) {
      setFormErrors(prev => ({ ...prev, email: 'Please enter a valid email address' }))
      return
    }

    const timer = setTimeout(async () => {
      try {
        const existingStaffs = await searchStaffs(email)
        const exists = existingStaffs.some((s: Staff) => 
          (s.email || '').toLowerCase() === email.toLowerCase()
        )
        
        setFormErrors(prev => ({
          ...prev,
          email: exists ? 'Email already exists' : ''
        }))
      } catch (error) {
        setFormErrors(prev => ({ ...prev, email: '' }))
      }
    }, 600)
    setEmailTimer(timer)
  }

  // Form handling functions exactly matching your TypeScript code
  function updateDefaultHours(contractType: string): void {
    let defaultHours: number
    switch (contractType) {
      case 'FullTime':
        defaultHours = 40
        break
      case 'Casual':
        defaultHours = 20
        break
      case 'PartTime':
        defaultHours = 30
        break
      default:
        defaultHours = 40
    }
    
    setFormData(prev => ({ ...prev, standardHoursPerWeek: defaultHours.toString() }))
  }

  function updateOvertimeRate(): void {
    if (!autoOvertimeEnabled) return
    
    const standardRate = parseFloat(formData.standardPayRate)
    if (standardRate && standardRate > 0) {
      setFormData(prev => ({
        ...prev,
        overtimePayRate: (standardRate * 1.5).toFixed(2)
      }))
    }
  }

  // Helper function removed for build optimization

  // Helper function removed for build optimization



  // Guard: check whether a staff can be deleted
  const canDeleteStaff = (staff: Staff) => {
    // 1) Cannot delete current user
    if (staff.id === currentUser.staffId) {
      return false
    }
    
    // 2) Cannot delete the last admin
    if (staff.role === 'admin') {
      const adminCount = staffs.filter(s => s.role === 'admin' && s.isActive).length
      if (adminCount <= 1) {
        return false
      }
    }
    
    return true
  }

  // Get delete button tooltip
  const getDeleteTooltip = (staff: Staff) => {
    if (staff.id === currentUser.staffId) {
      return 'You cannot delete your own account'
    }
    
    if (staff.role === 'admin') {
      const adminCount = staffs.filter(s => s.role === 'admin' && s.isActive).length
      if (adminCount <= 1) {
        return 'You cannot delete the last system administrator'
      }
    }
    
    return 'Delete this staff account'
  }

  // Fetch active staff data from API
  const { data: staffs = [], isLoading } = useQuery({
    queryKey: ['staffs'],
    queryFn: () => staffApi.getAll(),
    retry: 1,
    refetchOnWindowFocus: false,
  })

  // Fetch inactive (soft-deleted) staff data from API (preload for accurate tab count)
  const { data: inactiveStaffs = [], isLoading: isLoadingInactive } = useQuery({
    queryKey: ['inactiveStaffs'],
    queryFn: () => staffApi.getInactive(),
    retry: 1,
    refetchOnWindowFocus: false,
    staleTime: 300000, // 5 minutes to avoid frequent refetching
  })

  // Create staff mutation
  const createStaffMutation = useMutation({
    mutationFn: (newStaff: any) => staffApi.create(newStaff),
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
      queryClient.invalidateQueries({ queryKey: ['inactiveStaffs'] })
      setIsDeleteDialogOpen(false)
      setDeletingStaff(null)
    },
    onError: (error: any) => {
      console.error('Failed to delete staff:', error)
    },
  })

  // Restore staff mutation
  const restoreStaffMutation = useMutation({
    mutationFn: (id: number) => staffApi.restore(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['staffs'] })
      queryClient.invalidateQueries({ queryKey: ['inactiveStaffs'] })
    },
    onError: (error: any) => {
      console.error('Failed to restore staff:', error)
    },
  })

  // Helper functions
  const resetForm = () => {
    setFormData({
      firstName: '',
      lastName: '',
      email: '',
      role: 'Staff',
      phone: '',
      address: '',
      contractType: 'Casual',
      standardPayRate: '',
      overtimePayRate: '',
      standardHoursPerWeek: '40',
      isActive: true,
    })
    setFormErrors({})
    setAutoOvertimeEnabled(true)
    setAddAnother(false)
    
    // Clear timers
    if (emailTimer) {
      clearTimeout(emailTimer)
      setEmailTimer(null)
    }
    
    // Reset user interaction flags
    setUserTouchedStandardPay(false)
    setUserTouchedOvertimePay(false)
  }


  const validateForm = (_isEditing = false) => {
    const errors: Record<string, string> = {}
    
    // Name validation
    if (!formData.firstName.trim()) errors.firstName = 'First name is required'
    if (!formData.lastName.trim()) errors.lastName = 'Last name is required'
    
    // Email validation
    if (!formData.email.trim()) errors.email = 'Email is required'
    else if (!/\S+@\S+\.\S+/.test(formData.email)) errors.email = 'Invalid email format'
    
    // Role validation
    if (!formData.role) errors.role = 'Role is required'
    
    // Contract type validation
    if (!formData.contractType) errors.contractType = 'Contract type is required'
    
    // Standard hours validation
    const hours = Number(formData.standardHoursPerWeek)
    if (!formData.standardHoursPerWeek || hours <= 0 || hours > 60) {
      errors.standardHoursPerWeek = 'Standard hours must be between 1 and 60'
    }
    
    // Pay rate validation (only if provided)
    if (formData.standardPayRate) {
      if (isNaN(Number(formData.standardPayRate)) || Number(formData.standardPayRate) <= 0) {
        errors.standardPayRate = 'Standard pay rate must be a positive number'
      }
    }
    
    if (formData.overtimePayRate) {
      if (isNaN(Number(formData.overtimePayRate)) || Number(formData.overtimePayRate) <= 0) {
        errors.overtimePayRate = 'Overtime pay rate must be a positive number'
      }
    }
    
    // Phone validation (optional but if provided must be valid)
    if (formData.phone && !/^[\+]?[1-9][\d]{0,15}$/.test(formData.phone.replace(/[\s\-\(\)]/g, ''))) {
      errors.phone = 'Please enter a valid phone number'
    }
    
    setFormErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleSubmit = async () => {
    const isEditing = editingStaff !== null
    if (!validateForm(isEditing)) return
    
    if (isEditing) {
      // Update existing staff
      const updatedStaff = {
        ...editingStaff,
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        email: formData.email.trim().toLowerCase(),
        phone: formData.phone?.trim() || undefined,
        address: formData.address?.trim() || undefined,
        standardPayRate: parseFloat(formData.standardPayRate),
        overtimePayRate: parseFloat(formData.overtimePayRate),
        contractType: formData.contractType,
        standardHoursPerWeek: parseFloat(formData.standardHoursPerWeek) || 40,
      }
      
      updateStaffMutation.mutate({ 
        id: editingStaff.id!, 
        data: updatedStaff 
      })
    } else {
      // Create new staff using StaffCreateRequest model
      const newStaff = {
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        email: formData.email.trim().toLowerCase(),
        role: formData.role,
        phone: formData.phone?.trim() || null,
        address: formData.address?.trim() || null,
        contractType: formData.contractType,
        standardPayRate: formData.standardPayRate ? parseFloat(formData.standardPayRate) : null,
        overtimePayRate: formData.overtimePayRate ? parseFloat(formData.overtimePayRate) : null,
        standardHoursPerWeek: formData.standardHoursPerWeek ? parseInt(formData.standardHoursPerWeek) : null,
        isActive: formData.isActive,
      }
      
      try {
        await createStaffMutation.mutateAsync(newStaff as StaffCreateRequest)
        
        // Handle "add another" functionality
        if (addAnother) {
          // Reset form but keep some values for convenience
          const keepValues = {
            role: formData.role,
            contractType: formData.contractType,
            standardHoursPerWeek: formData.standardHoursPerWeek,
            standardPayRate: formData.standardPayRate,
            overtimePayRate: formData.overtimePayRate,
          }
          resetForm()
          setFormData(prev => ({ ...prev, ...keepValues }))
          setIsAddDialogOpen(true)
        }
      } catch (error) {
        console.error('Failed to create staff:', error)
      }
    }
  }

  const handleAddStaff = () => {
    resetForm()
    setEditingStaff(null)
    setIsAddDialogOpen(true)
  }

  const handleEditStaff = (staff: Staff) => {
    const nameParts = (staff.name || '').split(' ')
    const firstName = nameParts[0] || ''
    const lastName = nameParts.slice(1).join(' ') || ''
    
    setFormData({
      firstName: firstName,
      lastName: lastName,
      email: staff.email || '',
      role: staff.role || 'Staff',
      phone: staff.phone || '',
      address: staff.address || '',
      contractType: staff.contractType || 'Casual',
      standardPayRate: staff.standardPayRate?.toString() || staff.hourlyRate?.toString() || '',
      overtimePayRate: staff.overtimePayRate?.toString() || '',
      standardHoursPerWeek: staff.standardHoursPerWeek?.toString() || '40',
      isActive: staff.isActive !== undefined ? staff.isActive : true,
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

  const handleRestoreStaff = (staff: Staff) => {
    if (staff.id) {
      restoreStaffMutation.mutate(staff.id)
    }
  }


  const clearFilters = () => {
    setSearchQuery('')
    setRoleFilter('all')
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

      // Status filter:
      // - Active tab (0): strictly show only active staff (isActive = true)
      //   Ignore statusFilter to avoid showing inactive entries on Active tab
      // - Inactive tab uses a separate dataset (inactiveStaffs), so always allow here
      const matchesStatus = activeTab === 0 ? staff.isActive : true

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

  // Calculate actual counts for tabs
  const activeStaffCount = staffs.filter(staff => staff.isActive).length
  const totalStaffCount = staffs.length

  // Helpers: reuse filtering and sorting logic across tabs
  const filterStaffs = (list: Staff[], search: string, role: string): Staff[] => {
    return list.filter(staff => {
      const matchesSearch = !search || (
        staff.name?.toLowerCase().includes(search.toLowerCase()) ||
        staff.email?.toLowerCase().includes(search.toLowerCase()) ||
        staff.id?.toString().includes(search) ||
        staff.username?.toLowerCase().includes(search.toLowerCase()) ||
        staff.phone?.toLowerCase().includes(search.toLowerCase())
      )
      const matchesRole = role === 'all' || staff.role === role
      return matchesSearch && matchesRole
    })
  }

  const sortStaffs = (list: Staff[], key: string, direction: 'asc' | 'desc'): Staff[] => {
    const copied = [...list]
    copied.sort((a, b) => {
      let aValue: any
      let bValue: any
      switch (key) {
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
      if (aValue < bValue) return direction === 'asc' ? -1 : 1
      if (aValue > bValue) return direction === 'asc' ? 1 : -1
      return 0
    })
    return copied
  }

  // Unified inactive list using helpers
  const filteredSortedInactiveStaffs = sortStaffs(
    filterStaffs(inactiveStaffs, searchQuery, roleFilter),
    sortBy,
    sortDirection
  )

  const getStatusColor = (isActive: boolean) => {
    return isActive ? 'success' : 'error'
  }

  const getStatusLabel = (isActive: boolean) => {
    return isActive ? 'Active' : 'Inactive'
  }

  return (
    <Box>
      {/* Welcome Message */}
      <Paper elevation={1} sx={{ p: 2, mb: 2, bgcolor: 'success.light', color: 'success.contrastText' }}>
        <Typography variant="body1" sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <span>🎉</span>
          Welcome back, <strong>{currentUser.firstName} {currentUser.lastName}</strong>! 
          You are logged in as <strong>{currentUser.role}</strong>.
        </Typography>
      </Paper>

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

      {/* Tabs for Active/Inactive Staff */}
      <Paper elevation={1} sx={{ mb: 3 }}>
        <Tabs 
          value={activeTab} 
          onChange={(_, newValue) => setActiveTab(newValue)}
          variant="fullWidth"
          sx={{
            borderBottom: 1,
            borderColor: 'divider',
            '& .MuiTab-root': {
              textTransform: 'none',
              fontSize: '1rem',
              fontWeight: 500,
            }
          }}
        >
          <Tab
            icon={<People />}
            label={`Active Staff (${activeStaffCount})`}
            iconPosition="start"
          />
          <Tab
            icon={<Archive />}
            label={`Inactive Staff (${inactiveStaffs.length})`}
            iconPosition="start"
          />
        </Tabs>
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

            {/* Status filter removed: controlled by tabs */}

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
              {activeTab === 0
                ? `Active Staff Directory (${filteredStaffs.length} of ${activeStaffCount} employees)`
                : `Inactive Staff Directory (${inactiveStaffs.length} employees)`
              }
            </Typography>
            
            {/* Active Filters Display */}
            {(searchQuery || roleFilter !== 'all' || sortBy !== 'name') && (
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
                {/* Status chip removed: status is tab-controlled */}
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

          {/* Tab Panel Content */}
          {activeTab === 0 ? (
            // Active Staff Tab
            isLoading ? (
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
                      <TableCell>{staff.contractType || 'Casual'}</TableCell>
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
                            disabled={!canDeleteStaff(staff)}
                            title={getDeleteTooltip(staff)}
                            sx={{
                              opacity: canDeleteStaff(staff) ? 1 : 0.5,
                              cursor: canDeleteStaff(staff) ? 'pointer' : 'not-allowed'
                            }}
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
            )
          ) : (
            // Inactive Staff Tab
            isLoadingInactive ? (
              <Typography variant="body1" sx={{ textAlign: 'center', py: 4 }}>
                Loading inactive staff data...
              </Typography>
            ) : inactiveStaffs.length === 0 ? (
              <Typography variant="body1" sx={{ textAlign: 'center', py: 4 }}>
                No inactive staff found.
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
                      <TableCell>Deleted At</TableCell>
                      <TableCell align="center">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {filteredSortedInactiveStaffs.map(staff => (
                      <TableRow key={staff.id} sx={{ backgroundColor: 'grey.50' }}>
                        <TableCell>
                          <Typography variant="body2" fontWeight="bold">
                            {staff.id}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" sx={{ textDecoration: 'line-through', color: 'text.secondary' }}>
                            {staff.name}
                          </Typography>
                        </TableCell>
                        <TableCell>{staff.email}</TableCell>
                        <TableCell>
                          <Chip
                            label={staff.role || 'N/A'}
                            size="small"
                            variant="outlined"
                            color="default"
                          />
                        </TableCell>
                        <TableCell>{staff.contractType || 'Casual'}</TableCell>
                        <TableCell>
                          ${staff.hourlyRate?.toFixed(2) || 'N/A'}/hr
                        </TableCell>
                        <TableCell>
                          <Typography variant="body2" color="text.secondary">
                            {formatDateTimeString(staff.updatedAt || '')}
                          </Typography>
                        </TableCell>
                        <TableCell align="center">
                          <Tooltip title="Restore this staff member">
                            <Button
                              size="small"
                              variant="outlined"
                              color="success"
                              startIcon={restoreStaffMutation.isPending ? <CircularProgress size={16} /> : <Restore />}
                              onClick={() => handleRestoreStaff(staff)}
                              disabled={restoreStaffMutation.isPending}
                              sx={{ minWidth: 'auto' }}
                            >
                              {restoreStaffMutation.isPending ? 'Restoring...' : 'Restore'}
                            </Button>
                          </Tooltip>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )
          )}
        </CardContent>
      </Card>

      {/* Summary Stats */}
      {!isLoading && totalStaffCount > 0 && (
        <Grid container spacing={3} sx={{ mt: 2 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h4" color="primary">
                  {totalStaffCount}
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
                  {activeStaffCount}
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
                  {activeStaffCount > 0
                    ? (staffs.filter(s => s.isActive).reduce((sum, s) => sum + (s.hourlyRate || 0), 0) / activeStaffCount).toFixed(2)
                    : "0.00"}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  Avg. Pay Rate
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Enhanced Add Staff Dialog */}
      <Dialog 
        open={isAddDialogOpen} 
        onClose={() => setIsAddDialogOpen(false)}
        maxWidth="md"
        fullWidth
        PaperProps={{
          sx: {
            borderRadius: 3,
            boxShadow: '0 25px 50px rgba(0, 0, 0, 0.25)',
          }
        }}
      >
        <DialogTitle sx={{ 
          fontSize: '1.5rem', 
          fontWeight: 600, 
          borderBottom: 1, 
          borderColor: 'divider',
          pb: 2
        }}>
          Add New Staff Member
        </DialogTitle>
        
        <DialogContent sx={{ p: 3 }}>
          {/* Profile Information Section */}
          <Paper sx={{ p: 2.5, mb: 3, bgcolor: 'grey.50', border: 1, borderColor: 'grey.200' }}>
            <Typography variant="h6" sx={{ 
              mb: 2, 
              display: 'flex', 
              alignItems: 'center', 
              gap: 1,
              fontWeight: 600
            }}>
              <People /> Profile Information
            </Typography>
            
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <FormControl 
                  fullWidth 
                  required 
                  error={!!formErrors.role}
                  sx={{
                    '& .MuiInputLabel-asterisk': {
                      color: '#d32f2f'
                    }
                  }}
                >
                  <InputLabel>Role</InputLabel>
                  <Select
                    value={formData.role}
                    onChange={(e) => {
                      const role = e.target.value
                      setFormData(prev => ({ ...prev, role }))
                      
                      // Auto-set default pay rates based on role if not touched by user
                      if (!userTouchedStandardPay) {
                        let defaultPay = 25
                        if (role === 'Manager') defaultPay = 35
                        else if (role === 'Admin') defaultPay = 45
                        
                        setFormData(prev => ({ ...prev, standardPayRate: defaultPay.toString() }))
                        
                        if (autoOvertimeEnabled) {
                          setFormData(prev => ({ ...prev, overtimePayRate: (defaultPay * 1.5).toFixed(2) }))
                        }
                      }
                    }}
                    label="Role"
                    sx={{ 
                      '& .MuiOutlinedInput-notchedOutline': {
                        borderRadius: '8px',
                        borderColor: formData.role ? '#1976d2' : '#d32f2f',
                        borderWidth: '1px'
                      },
                      height: '56px',
                      '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#1976d2',
                        borderWidth: '2px'
                      },
                      '&.Mui-error .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#d32f2f',
                        borderWidth: '2px'
                      }
                    }}
                  >
                    <MenuItem value="Staff">Staff (ID: 1000-7999)</MenuItem>
                    <MenuItem value="Manager">Manager (ID: 8000-8999)</MenuItem>
                    <MenuItem value="Admin">Admin (ID: 9000-9999)</MenuItem>
                  </Select>
                  {formErrors.role && (
                    <Typography variant="caption" color="error" sx={{ ml: 2, mt: 0.5 }}>
                      {formErrors.role}
                    </Typography>
                  )}
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Email"
                  required
                  type="email"
                  value={formData.email}
                  onChange={(e) => {
                    const value = e.target.value
                    setFormData(prev => ({ ...prev, email: value }))
                    validateEmail(value)
                  }}
                  error={!!formErrors.email}
                  helperText={formErrors.email}
                  placeholder="john.doe@company.com"
                  autoComplete="email"
                  sx={{ 
                    '& .MuiOutlinedInput-root': {
                      borderRadius: '8px',
                      height: '56px',
                      '& .MuiOutlinedInput-notchedOutline': {
                        borderColor: formData.email ? '#1976d2' : '#d32f2f',
                        borderWidth: '1px'
                      },
                      '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#1976d2',
                        borderWidth: '2px'
                      },
                      '&.Mui-error .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#d32f2f',
                        borderWidth: '2px'
                      }
                    },
                    '& .MuiInputLabel-asterisk': {
                      color: '#d32f2f'
                    }
                  }}
                />
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="First Name"
                  required
                  value={formData.firstName}
                  onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                  error={!!formErrors.firstName}
                  helperText={formErrors.firstName}
                  placeholder="John"
                  autoComplete="given-name"
                  sx={{ 
                    '& .MuiOutlinedInput-root': {
                      borderRadius: '8px',
                      height: '56px',
                      '& .MuiOutlinedInput-notchedOutline': {
                        borderColor: formData.firstName ? '#1976d2' : '#d32f2f',
                        borderWidth: '1px'
                      },
                      '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#1976d2',
                        borderWidth: '2px'
                      },
                      '&.Mui-error .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#d32f2f',
                        borderWidth: '2px'
                      }
                    },
                    '& .MuiInputLabel-asterisk': {
                      color: '#d32f2f'
                    }
                  }}
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Last Name"
                  required
                  value={formData.lastName}
                  onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                  error={!!formErrors.lastName}
                  helperText={formErrors.lastName}
                  placeholder="Doe"
                  autoComplete="family-name"
                  sx={{ 
                    '& .MuiOutlinedInput-root': {
                      borderRadius: '8px',
                      height: '56px',
                      '& .MuiOutlinedInput-notchedOutline': {
                        borderColor: formData.lastName ? '#1976d2' : '#d32f2f',
                        borderWidth: '1px'
                      },
                      '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#1976d2',
                        borderWidth: '2px'
                      },
                      '&.Mui-error .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#d32f2f',
                        borderWidth: '2px'
                      }
                    },
                    '& .MuiInputLabel-asterisk': {
                      color: '#d32f2f'
                    }
                  }}
                />
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Phone (Optional)"
                  type="tel"
                  value={formData.phone}
                  onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                  error={!!formErrors.phone}
                  helperText={formErrors.phone}
                  placeholder="+61 400 000 000"
                  autoComplete="tel"
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Address (Optional)"
                  value={formData.address}
                  onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                  error={!!formErrors.address}
                  helperText={formErrors.address}
                  placeholder="123 Main St, Adelaide SA"
                  autoComplete="address-line1"
                />
              </Grid>
              
            </Grid>
          </Paper>

          {/* Pay & Contract Details Section */}
          <Paper sx={{ p: 2.5, mb: 3, bgcolor: 'grey.50', border: 1, borderColor: 'grey.200' }}>
            <Typography variant="h6" sx={{ 
              mb: 2, 
              display: 'flex', 
              alignItems: 'center', 
              gap: 1,
              fontWeight: 600
            }}>
              <svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
                <path d="M4 4a2 2 0 00-2 2v1h16V6a2 2 0 00-2-2H4zM18 9H2v5a2 2 0 002 2h12a2 2 0 002-2V9zM4 13a1 1 0 011-1h1a1 1 0 110 2H5a1 1 0 01-1-1zm5-1a1 1 0 100 2h1a1 1 0 100-2H9z"/>
              </svg>
              Pay & Contract Details
            </Typography>
            
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6}>
                <FormControl 
                  fullWidth 
                  required 
                  error={!!formErrors.contractType}
                  sx={{
                    '& .MuiInputLabel-asterisk': {
                      color: '#d32f2f'
                    }
                  }}
                >
                  <InputLabel>Contract Type</InputLabel>
                  <Select
                    value={formData.contractType}
                    onChange={(e) => {
                      const contractType = e.target.value
                      setFormData({ ...formData, contractType })
                      updateDefaultHours(contractType)
                    }}
                    label="Contract Type"
                    sx={{ 
                      '& .MuiOutlinedInput-notchedOutline': {
                        borderRadius: '8px',
                        borderColor: formData.contractType ? '#1976d2' : '#d32f2f',
                        borderWidth: '1px'
                      },
                      height: '56px',
                      '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#1976d2',
                        borderWidth: '2px'
                      },
                      '&.Mui-error .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#d32f2f',
                        borderWidth: '2px'
                      }
                    }}
                  >
                    <MenuItem value="Casual">Casual</MenuItem>
                    <MenuItem value="PartTime">Part Time</MenuItem>
                    <MenuItem value="FullTime">Full Time</MenuItem>
                  </Select>
                  {formErrors.contractType && (
                    <Typography variant="caption" color="error" sx={{ ml: 2 }}>
                      {formErrors.contractType}
                    </Typography>
                  )}
                </FormControl>
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Standard Hours/Week (Optional)"
                  type="number"
                  value={formData.standardHoursPerWeek}
                  onChange={(e) => setFormData({ ...formData, standardHoursPerWeek: e.target.value })}
                  error={!!formErrors.standardHoursPerWeek}
                  helperText={formErrors.standardHoursPerWeek || 'Leave empty to use role defaults'}
                  placeholder="40"
                  inputProps={{ min: 1, max: 60, step: 0.5 }}
                  sx={{ 
                    '& .MuiOutlinedInput-root': {
                      borderRadius: '8px',
                      height: '56px',
                      '& .MuiOutlinedInput-notchedOutline': {
                        borderColor: formData.standardHoursPerWeek ? '#1976d2' : 'rgba(0, 0, 0, 0.23)',
                        borderWidth: '1px'
                      },
                      '&.Mui-focused .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#1976d2',
                        borderWidth: '2px'
                      },
                      '&.Mui-error .MuiOutlinedInput-notchedOutline': {
                        borderColor: '#d32f2f',
                        borderWidth: '2px'
                      }
                    }
                  }}
                />
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Standard Pay Rate (Optional)"
                  type="number"
                  value={formData.standardPayRate}
                  onChange={(e) => {
                    const value = e.target.value
                    setUserTouchedStandardPay(true)
                    setFormData({ ...formData, standardPayRate: value })
                    // Auto-update overtime rate if enabled
                    if (autoOvertimeEnabled && value) {
                      const overtimeRate = (parseFloat(value) * 1.5).toFixed(2)
                      setFormData(prev => ({ ...prev, standardPayRate: value, overtimePayRate: overtimeRate }))
                    }
                  }}
                  error={!!formErrors.standardPayRate}
                  helperText={formErrors.standardPayRate || 'Leave empty to use role defaults'}
                  placeholder="25.00"
                  inputProps={{ min: 0.01, step: 0.01 }}
                  InputProps={{
                    startAdornment: <Typography sx={{ mr: 1 }}>$</Typography>,
                  }}
                />
              </Grid>
              
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Overtime Pay Rate (Optional)"
                  type="number"
                  value={formData.overtimePayRate}
                  onChange={(e) => {
                    setUserTouchedOvertimePay(true)
                    setFormData({ ...formData, overtimePayRate: e.target.value })
                  }}
                  error={!!formErrors.overtimePayRate}
                  helperText={formErrors.overtimePayRate || 'Leave empty to use 1.5× standard rate'}
                  placeholder="37.50"
                  inputProps={{ min: 0.01, step: 0.01 }}
                  disabled={autoOvertimeEnabled}
                  InputProps={{
                    startAdornment: <Typography sx={{ mr: 1 }}>$</Typography>,
                  }}
                />
                <Box sx={{ mt: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
                  <input 
                    type="checkbox" 
                    id="autoOvertimeCalc"
                    checked={autoOvertimeEnabled}
                    onChange={(e) => {
                      setAutoOvertimeEnabled(e.target.checked)
                      if (e.target.checked) {
                        setTimeout(updateOvertimeRate, 100)
                      }
                    }}
                    style={{ accentColor: '#3b82f6' }}
                  />
                  <Typography variant="caption" color="textSecondary">
                    Auto 1.5× calculation
                  </Typography>
                </Box>
              </Grid>
              
              
              <Grid item xs={12} sm={6}>
                <Paper sx={{ p: 2, bgcolor: 'grey.25', border: 1, borderColor: 'grey.200' }}>
                  <Typography variant="h6" sx={{ 
                    mb: 1, 
                    display: 'flex', 
                    alignItems: 'center', 
                    gap: 1,
                    fontWeight: 600
                  }}>
                    <svg width="20" height="20" viewBox="0 0 20 20" fill="currentColor">
                      <path d="M10 18a8 8 0 100-16 8 8 0 000 16zM9.555 7.168A1 1 0 008 8v4a1 1 0 001.555.832L12 11.202a1 1 0 000-1.404l-2.445-1.63z"/>
                    </svg>
                    Status
                  </Typography>
                  <Typography variant="body2" color="textSecondary" sx={{ mb: 2 }}>
                    Account Status
                  </Typography>
                  <FormControlLabel
                    control={
                      <Switch
                        checked={formData.isActive}
                        onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                        color="primary"
                      />
                    }
                    label={
                      <Typography variant="body1" sx={{ fontWeight: formData.isActive ? 600 : 400 }}>
                        {formData.isActive ? 'Active' : 'Inactive'}
                      </Typography>
                    }
                  />
                </Paper>
              </Grid>
            </Grid>
          </Paper>
            
          {createStaffMutation.isError && (
            <Alert severity="error" sx={{ mt: 2 }}>
              Failed to create staff member. Please check all fields and try again.
            </Alert>
          )}
        </DialogContent>
        
        <DialogActions sx={{ 
          p: 3, 
          borderTop: 1, 
          borderColor: 'divider',
          bgcolor: 'grey.50',
          display: 'flex',
          gap: 2
        }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mr: 'auto' }}>
            <input 
              type="checkbox" 
              id="addAnother"
              checked={addAnother}
              onChange={(e) => setAddAnother(e.target.checked)}
              style={{ accentColor: '#3b82f6' }}
            />
            <Typography variant="body2" color="textSecondary">
              Create and add another
            </Typography>
          </Box>
          
          <Button 
            onClick={() => setIsAddDialogOpen(false)}
            disabled={createStaffMutation.isPending}
            variant="outlined"
          >
            Cancel
          </Button>
          <Button 
            onClick={handleSubmit}
            variant="contained"
            disabled={createStaffMutation.isPending}
            startIcon={createStaffMutation.isPending ? <CircularProgress size={20} /> : <Add />}
            sx={{ minWidth: 140 }}
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
                  type="number"
                  value={""}
                  disabled
                  helperText="Staff ID cannot be changed"
                  autoComplete="off"
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Full Name"
                  value={`${formData.firstName} ${formData.lastName}`}
                  onChange={(e) => {
                    const names = e.target.value.split(' ');
                    setFormData({ ...formData, firstName: names[0] || '', lastName: names.slice(1).join(' ') || '' });
                  }}
                  error={!!formErrors.name}
                  helperText={formErrors.name}
                  placeholder="Enter staff full name"
                  autoComplete="name"
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
                  autoComplete="email"
                />
              </Grid>
              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  label="Hourly Rate"
                  type="number"
                  value={formData.standardPayRate}
                  onChange={(e) => setFormData({ ...formData, standardPayRate: e.target.value })}
                  error={!!formErrors.hourlyRate}
                  helperText={formErrors.hourlyRate}
                  placeholder="25.00"
                  autoComplete="off"
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

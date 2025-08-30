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
  TextField,
  Button,
  Chip,
  IconButton,
  Tooltip,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Alert,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  DialogContentText
} from '@mui/material'
import {
  History,
  Search,
  Delete,

  Clear,
  Warning,
  CheckCircle,
  Cancel
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { loginHistoryApi } from '../api/client'
import { LoginHistory } from '../types/api'

export function AdminLoginHistory() {
  // State management
  const [searchQuery, setSearchQuery] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [deletingRecord, setDeletingRecord] = useState<LoginHistory | null>(null)
  const [limit] = useState(50)
  const [offset] = useState(0)

  const queryClient = useQueryClient()

  // Fetch login history data
  const { data: loginHistory = [], isLoading, error } = useQuery({
    queryKey: ['loginHistory', limit, offset],
    queryFn: () => loginHistoryApi.getAll({ limit, offset }),
    retry: 1,
    refetchOnWindowFocus: false,
  })

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (id: number) => loginHistoryApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['loginHistory'] })
      setDeleteDialogOpen(false)
      setDeletingRecord(null)
    },
    onError: (error: any) => {
      console.error('Failed to delete login record:', error)
    },
  })

  // Filter and search logic
  const filteredHistory = loginHistory.filter((record: LoginHistory) => {
    const matchesSearch = searchQuery === '' || 
      record.username.toLowerCase().includes(searchQuery.toLowerCase()) ||
      record.ipAddress.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (record.staffName && record.staffName.toLowerCase().includes(searchQuery.toLowerCase()))

    const matchesStatus = statusFilter === 'all' || 
      (statusFilter === 'success' && record.success) ||
      (statusFilter === 'failed' && !record.success)

    return matchesSearch && matchesStatus
  })

  // Handle delete confirmation
  const handleDelete = (record: LoginHistory) => {
    setDeletingRecord(record)
    setDeleteDialogOpen(true)
  }

  const confirmDelete = () => {
    if (deletingRecord) {
      deleteMutation.mutate(deletingRecord.id)
    }
  }

  // Format timestamp
  const formatTimestamp = (timestamp: string) => {
    try {
      const date = new Date(timestamp)
      return date.toLocaleString('zh-CN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false
      })
    } catch (error) {
      return timestamp
    }
  }

  // Clear filters
  const clearFilters = () => {
    setSearchQuery('')
    setStatusFilter('all')
  }

  return (
    <Box>
      {/* Header */}
      <Paper elevation={2} sx={{ p: 3, mb: 3, textAlign: 'center' }}>
        <History sx={{ fontSize: 40, color: 'primary.main', mb: 1 }} />
        <Typography variant="h4" gutterBottom>
          Login History
        </Typography>
        <Typography variant="subtitle1" color="textSecondary">
          Monitor and manage system login attempts
        </Typography>
      </Paper>

      {/* Controls */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Search"
                placeholder="Search by username, IP, or staff name..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                InputProps={{
                  startAdornment: <Search sx={{ mr: 1, color: 'text.secondary' }} />
                }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth>
                <InputLabel>Status</InputLabel>
                <Select
                  value={statusFilter}
                  label="Status"
                  onChange={(e) => setStatusFilter(e.target.value)}
                >
                  <MenuItem value="all">All</MenuItem>
                  <MenuItem value="success">Success</MenuItem>
                  <MenuItem value="failed">Failed</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={3}>
              <Button
                variant="outlined"
                onClick={clearFilters}
                startIcon={<Clear />}
                fullWidth
              >
                Clear Filters
              </Button>
            </Grid>
            <Grid item xs={12} md={2}>
              <Typography variant="body2" color="text.secondary" align="center">
                {filteredHistory.length} records
              </Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Login History Table */}
      <Card>
        <CardContent>
          {isLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : error ? (
            <Alert severity="error" sx={{ mb: 2 }}>
              Failed to load login history data
            </Alert>
          ) : filteredHistory.length === 0 ? (
            <Typography variant="body1" sx={{ textAlign: 'center', py: 4 }}>
              {searchQuery || statusFilter !== 'all'
                ? 'No login records match your search criteria.'
                : 'No login records found.'}
            </Typography>
          ) : (
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>History ID</TableCell>
                    <TableCell>Timestamp</TableCell>
                    <TableCell>Actor</TableCell>
                    <TableCell>IP Address</TableCell>
                    <TableCell>Action</TableCell>
                    <TableCell>Result</TableCell>
                    <TableCell>Details</TableCell>
                    <TableCell align="center">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {filteredHistory.map((record: LoginHistory) => (
                    <TableRow key={record.id}>
                      <TableCell>
                        <Typography variant="body2" fontWeight="bold">
                          {record.id}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {formatTimestamp(record.timestamp)}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {record.username}
                        </Typography>
                        {record.staffName && (
                          <Typography variant="caption" color="text.secondary" display="block">
                            {record.staffName}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" fontFamily="monospace">
                          {record.ipAddress}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label="Login"
                          size="small"
                          variant="outlined"
                          color="primary"
                        />
                      </TableCell>
                      <TableCell>
                        <Chip
                          icon={record.success ? <CheckCircle /> : <Cancel />}
                          label={record.success ? 'Succeed' : 'Failed'}
                          color={record.success ? 'success' : 'error'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        {record.failureReason ? (
                          <Typography variant="body2" color="error.main">
                            {record.failureReason}
                          </Typography>
                        ) : (
                          <Typography variant="body2" color="text.secondary">
                            -
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell align="center">
                        <Tooltip title="Delete this record">
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => handleDelete(record)}
                            disabled={deleteMutation.isPending}
                          >
                            <Delete />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}

          {/* Error Alert */}
          {deleteMutation.isError && (
            <Alert severity="error" sx={{ mt: 2 }}>
              Failed to delete login record
            </Alert>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={() => setDeleteDialogOpen(false)}
      >
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <Warning color="warning" />
            Confirm Delete
          </Box>
        </DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete this login record?
            <br />
            <strong>ID:</strong> {deletingRecord?.id}
            <br />
            <strong>User:</strong> {deletingRecord?.username}
            <br />
            <strong>Time:</strong> {deletingRecord ? formatTimestamp(deletingRecord.timestamp) : ''}
            <br />
            <br />
            This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>
            Cancel
          </Button>
          <Button
            onClick={confirmDelete}
            color="error"
            variant="contained"
            disabled={deleteMutation.isPending}
            startIcon={deleteMutation.isPending ? <CircularProgress size={16} /> : <Delete />}
          >
            {deleteMutation.isPending ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

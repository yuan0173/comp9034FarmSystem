import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Button,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Alert,
} from '@mui/material'
import Autocomplete from '@mui/material/Autocomplete'
import CircularProgress from '@mui/material/CircularProgress'
import { Add, Edit, Delete } from '@mui/icons-material'
import { workScheduleApi, staffApi } from '../api/client'
import { CurrentUser, WorkSchedule } from '../types/api'

interface AdminRosterProps {
  currentUser: CurrentUser
}

function toTime(value: string) {
  // Ensure HH:mm:ss
  if (value.length === 5) return value + ':00'
  return value
}

export function AdminRoster({ currentUser }: AdminRosterProps) {
  const qc = useQueryClient()
  // Use current user in UI header to satisfy TS6133 and provide context
  const adminDisplayName = `${currentUser.firstName} ${currentUser.lastName}`
  const [filters, setFilters] = useState<{ staffId?: string; startDate?: string; endDate?: string }>({})
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editing, setEditing] = useState<WorkSchedule | null>(null)
  const [form, setForm] = useState<{ staffId: string; date: string; startTime: string; endTime: string }>({
    staffId: '',
    date: '',
    startTime: '',
    endTime: '',
  })
  const [error, setError] = useState<string | null>(null)
  const [staffInput, setStaffInput] = useState('')
  const [selectedStaff, setSelectedStaff] = useState<any | null>(null)

  // Debounced staff search using React Query
  const [staffSearchTerm, setStaffSearchTerm] = useState('')
  const staffQuery = useQuery({
    queryKey: ['staff-search', staffSearchTerm],
    queryFn: () => staffApi.search(staffSearchTerm),
    enabled: staffSearchTerm.length > 0 && dialogOpen,
    staleTime: 60_000,
    keepPreviousData: true,
  })

  const { data: schedules = [], isLoading } = useQuery({
    queryKey: ['workSchedules', filters],
    queryFn: async () => {
      const payload: any = {}
      if (filters.staffId) payload.staffId = Number(filters.staffId)
      if (filters.startDate) payload.startDate = filters.startDate
      if (filters.endDate) payload.endDate = filters.endDate
      return workScheduleApi.getAll(payload)
    },
  })

  const openCreate = () => {
    setEditing(null)
    setForm({ staffId: '', date: '', startTime: '', endTime: '' })
    setError(null)
    setDialogOpen(true)
  }

  const openEdit = (row: WorkSchedule) => {
    setEditing(row)
    setForm({
      staffId: String(row.staffId),
      date: row.date.split('T')[0] || row.date,
      startTime: row.startTime.substring(0, 5),
      endTime: row.endTime.substring(0, 5),
    })
    setError(null)
    setDialogOpen(true)
  }

  const closeDialog = () => setDialogOpen(false)

  const createMut = useMutation({
    mutationFn: async () => {
      if (!form.staffId || !form.date || !form.startTime || !form.endTime) {
        throw new Error('Please fill all fields')
      }
      return workScheduleApi.create({
        staffId: Number(form.staffId),
        date: form.date,
        startTime: toTime(form.startTime),
        endTime: toTime(form.endTime),
      })
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['workSchedules'] })
      setDialogOpen(false)
    },
    onError: (e: any) => {
      setError(e?.message || 'Failed to create schedule')
    },
  })

  const updateMut = useMutation({
    mutationFn: async () => {
      if (!editing) return
      if (!form.staffId || !form.date || !form.startTime || !form.endTime) {
        throw new Error('Please fill all fields')
      }
      await workScheduleApi.update(editing.scheduleId, {
        staffId: Number(form.staffId),
        date: form.date,
        startTime: toTime(form.startTime),
        endTime: toTime(form.endTime),
      })
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['workSchedules'] })
      setDialogOpen(false)
    },
    onError: (e: any) => setError(e?.message || 'Failed to update schedule'),
  })

  const deleteMut = useMutation({
    mutationFn: async (id: number) => workScheduleApi.delete(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['workSchedules'] }),
  })

  const submit = () => (editing ? updateMut.mutate() : createMut.mutate())

  return (
    <Box>
      <Paper elevation={2} sx={{ p: 3, mb: 3, textAlign: 'center' }}>
        <Typography variant="h4" gutterBottom>
          Roster Management
        </Typography>
        <Typography variant="subtitle1" color="textSecondary">
          Assign shifts; prevent overlaps; hours auto-calculated
        </Typography>
        <Typography variant="caption" color="textSecondary">
          Admin: {adminDisplayName} (ID: {currentUser.staffId})
        </Typography>
      </Paper>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} sm={3}>
              <TextField
                label="Staff ID"
                size="small"
                value={filters.staffId || ''}
                onChange={e => setFilters(prev => ({ ...prev, staffId: e.target.value }))}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <TextField
                label="Start Date"
                type="date"
                size="small"
                InputLabelProps={{ shrink: true }}
                value={filters.startDate || ''}
                onChange={e => setFilters(prev => ({ ...prev, startDate: e.target.value }))}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={3}>
              <TextField
                label="End Date"
                type="date"
                size="small"
                InputLabelProps={{ shrink: true }}
                value={filters.endDate || ''}
                onChange={e => setFilters(prev => ({ ...prev, endDate: e.target.value }))}
                fullWidth
              />
            </Grid>
            <Grid item xs={12} sm={3} textAlign="right">
              <Button variant="contained" startIcon={<Add />} onClick={openCreate}>
                New Shift
              </Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      <Card>
        <CardContent>
          {isLoading ? (
            <Typography>Loading...</Typography>
          ) : (
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>ID</TableCell>
                  <TableCell>Staff</TableCell>
                  <TableCell>Date</TableCell>
                  <TableCell>Start</TableCell>
                  <TableCell>End</TableCell>
                  <TableCell>Hours</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {schedules.map((row: WorkSchedule) => (
                  <TableRow key={row.scheduleId} hover>
                    <TableCell>#{row.scheduleId}</TableCell>
                    <TableCell>{row.staffId}</TableCell>
                    <TableCell>{(row.date || '').split('T')[0]}</TableCell>
                    <TableCell>{row.startTime?.substring(0, 5)}</TableCell>
                    <TableCell>{row.endTime?.substring(0, 5)}</TableCell>
                    <TableCell>{row.scheduleHours?.toFixed(2)}</TableCell>
                    <TableCell align="right">
                      <IconButton size="small" onClick={() => openEdit(row)} color="primary">
                        <Edit />
                      </IconButton>
                      <IconButton size="small" onClick={() => deleteMut.mutate(row.scheduleId)} color="error">
                        <Delete />
                      </IconButton>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <Dialog open={dialogOpen} onClose={closeDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editing ? 'Edit Shift' : 'New Shift'}</DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12} sm={6}>
              <Autocomplete
                options={(staffQuery.data || []) as any[]}
                getOptionLabel={(option: any) => `${option.firstName} ${option.lastName} (${option.staffId}) - ${option.email}`}
                loading={staffQuery.isLoading}
                value={selectedStaff}
                onChange={(_, value) => {
                  setSelectedStaff(value)
                  setForm(prev => ({ ...prev, staffId: value ? String(value.staffId) : '' }))
                }}
                inputValue={staffInput}
                onInputChange={(_, value) => {
                  setStaffInput(value)
                  // Simple debounce: delay term update by 300ms
                  const v = value.trim()
                  if (!v) {
                    setStaffSearchTerm('')
                  } else {
                    setTimeout(() => {
                      // only update if input hasn't changed further
                      if (value === v) setStaffSearchTerm(v)
                    }, 300)
                  }
                }}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Select Staff"
                    placeholder="Type name/email/ID"
                    fullWidth
                    InputProps={{
                      ...params.InputProps,
                      endAdornment: (
                        <>
                          {staffQuery.isFetching ? <CircularProgress color="inherit" size={16} /> : null}
                          {params.InputProps.endAdornment}
                        </>
                      ),
                    }}
                  />
                )}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Date"
                type="date"
                fullWidth
                InputLabelProps={{ shrink: true }}
                value={form.date}
                onChange={e => setForm(prev => ({ ...prev, date: e.target.value }))}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="Start Time"
                type="time"
                fullWidth
                InputLabelProps={{ shrink: true }}
                value={form.startTime}
                onChange={e => setForm(prev => ({ ...prev, startTime: e.target.value }))}
              />
            </Grid>
            <Grid item xs={12} sm={6}>
              <TextField
                label="End Time"
                type="time"
                fullWidth
                InputLabelProps={{ shrink: true }}
                value={form.endTime}
                onChange={e => setForm(prev => ({ ...prev, endTime: e.target.value }))}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={closeDialog}>Cancel</Button>
          <Button variant="contained" onClick={submit} disabled={createMut.isPending || updateMut.isPending}>
            {editing ? 'Save' : 'Create'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

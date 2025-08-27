import { Box, Grid } from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs'
import dayjs, { Dayjs } from 'dayjs'

interface DateRangePickerProps {
  startDate: Date | null
  endDate: Date | null
  onStartDateChange: (date: Date | null) => void
  onEndDateChange: (date: Date | null) => void
  label?: string
  maxDate?: Date
  minDate?: Date
}

export function DateRangePicker({
  startDate,
  endDate,
  onStartDateChange,
  onEndDateChange,
  label,
  maxDate,
  minDate,
}: DateRangePickerProps) {
  const handleStartDateChange = (date: Dayjs | null) => {
    onStartDateChange(date ? date.toDate() : null)
  }

  const handleEndDateChange = (date: Dayjs | null) => {
    onEndDateChange(date ? date.toDate() : null)
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <Box>
        <Grid container spacing={2}>
          <Grid item xs={6}>
            <DatePicker
              label={`${label || 'Date Range'} - From`}
              value={startDate ? dayjs(startDate) : null}
              onChange={handleStartDateChange}
              maxDate={
                endDate ? dayjs(endDate) : maxDate ? dayjs(maxDate) : undefined
              }
              minDate={minDate ? dayjs(minDate) : undefined}
              slotProps={{
                textField: {
                  fullWidth: true,
                  size: 'small',
                },
              }}
            />
          </Grid>
          <Grid item xs={6}>
            <DatePicker
              label={`${label || 'Date Range'} - To`}
              value={endDate ? dayjs(endDate) : null}
              onChange={handleEndDateChange}
              minDate={
                startDate
                  ? dayjs(startDate)
                  : minDate
                    ? dayjs(minDate)
                    : undefined
              }
              maxDate={maxDate ? dayjs(maxDate) : undefined}
              slotProps={{
                textField: {
                  fullWidth: true,
                  size: 'small',
                },
              }}
            />
          </Grid>
        </Grid>
      </Box>
    </LocalizationProvider>
  )
}

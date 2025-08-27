import React from 'react'
import { Button, ButtonProps } from '@mui/material'
import { Download } from '@mui/icons-material'
import { exportToCSV } from '../utils/csv'

interface ExportCsvButtonProps extends Omit<ButtonProps, 'onClick'> {
  data: Record<string, any>[] | string[][]
  filename: string
  children?: React.ReactNode
}

export function ExportCsvButton({ 
  data, 
  filename, 
  children = 'Export CSV',
  ...buttonProps 
}: ExportCsvButtonProps) {
  const handleExport = () => {
    if (data.length === 0) {
      alert('No data to export')
      return
    }

    try {
      exportToCSV(data, filename)
    } catch (error) {
      console.error('Export failed:', error)
      alert('Export failed. Please try again.')
    }
  }

  return (
    <Button
      {...buttonProps}
      onClick={handleExport}
      startIcon={<Download />}
      disabled={data.length === 0}
    >
      {children}
    </Button>
  )
} 
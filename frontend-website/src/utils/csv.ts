// Convert array of objects to CSV string
export function arrayToCSV(data: Record<string, any>[]): string {
  if (data.length === 0) return ''

  const headers = Object.keys(data[0])
  const csvHeaders = headers.join(',')
  
  const csvRows = data.map(row => 
    headers.map(header => {
      const value = row[header]
      // Escape quotes and wrap in quotes if contains comma, quote, or newline
      if (typeof value === 'string' && (value.includes(',') || value.includes('"') || value.includes('\n'))) {
        return `"${value.replace(/"/g, '""')}"`
      }
      return value?.toString() || ''
    }).join(',')
  )

  return [csvHeaders, ...csvRows].join('\n')
}

// Convert 2D array to CSV string
export function matrixToCSV(data: string[][]): string {
  return data.map(row => 
    row.map(cell => {
      // Escape quotes and wrap in quotes if contains comma, quote, or newline
      if (cell.includes(',') || cell.includes('"') || cell.includes('\n')) {
        return `"${cell.replace(/"/g, '""')}"`
      }
      return cell
    }).join(',')
  ).join('\n')
}

// Download CSV file
export function downloadCSV(csvContent: string, filename: string): void {
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  
  if (link.download !== undefined) {
    const url = URL.createObjectURL(blob)
    link.setAttribute('href', url)
    link.setAttribute('download', filename)
    link.style.visibility = 'hidden'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
  }
}

// Main export function
export function exportToCSV(data: Record<string, any>[] | string[][], filename: string): void {
  let csvContent: string
  
  if (Array.isArray(data[0])) {
    // 2D array
    csvContent = matrixToCSV(data as string[][])
  } else {
    // Array of objects
    csvContent = arrayToCSV(data as Record<string, any>[])
  }
  
  downloadCSV(csvContent, filename)
} 
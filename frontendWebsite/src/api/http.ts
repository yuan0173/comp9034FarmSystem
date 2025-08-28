import axios from 'axios'

// Create axios instance with base configuration
const httpClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:4000',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor for adding auth headers if needed
httpClient.interceptors.request.use(
  config => {
    // Add JWT token to requests if available
    const token = localStorage.getItem('authToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  error => {
    return Promise.reject(error)
  }
)

// Response interceptor for handling errors
httpClient.interceptors.response.use(
  response => {
    return response
  },
  error => {
    if (!navigator.onLine) {
      console.warn('Network request failed - device is offline')
      return Promise.reject(new Error('Device is offline'))
    }

    if (error.response) {
      // The request was made and the server responded with a status code
      // that falls out of the range of 2xx
      console.error('API Error:', error.response.data)
      return Promise.reject(error.response.data)
    } else if (error.request) {
      // The request was made but no response was received
      console.error('Network Error:', error.request)
      return Promise.reject(
        new Error('Network error - please check your connection')
      )
    } else {
      // Something happened in setting up the request that triggered an Error
      console.error('Request Error:', error.message)
      return Promise.reject(error)
    }
  }
)

export { httpClient }

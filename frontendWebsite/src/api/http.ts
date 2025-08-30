import axios from 'axios'

// 🌟 行业标准：多层配置策略
const getApiBaseUrl = () => {
  // 1. 优先使用环境变量
  if (import.meta.env.VITE_API_BASE_URL) {
    return import.meta.env.VITE_API_BASE_URL
  }
  
  // 2. 检测当前前端运行的协议和主机
  const currentHost = window.location.hostname
  const currentProtocol = window.location.protocol
  
  // 3. 智能端口检测：根据环境动态选择
  if (import.meta.env.DEV) {
    // 开发环境：直接使用4000端口
    return `${currentProtocol}//${currentHost}:4000`
  }
  
  // 4. 生产环境：通常后端和前端在同一域名
  return `${currentProtocol}//${currentHost}/api`
}

// Create axios instance with base configuration
const httpClient = axios.create({
  baseURL: getApiBaseUrl(),
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// 🔧 开发模式下打印配置信息
if (import.meta.env.DEV) {
  console.log('🔗 API Base URL:', getApiBaseUrl())
  console.log('🌍 Environment:', import.meta.env.MODE)
}

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

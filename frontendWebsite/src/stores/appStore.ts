import { create } from 'zustand'
import { subscribeWithSelector } from 'zustand/middleware'

interface AppState {
  // UI State
  sidebarOpen: boolean
  theme: 'light' | 'dark'

  // Network State
  isOnline: boolean
  isBackendConnected: boolean

  // Global Loading States
  globalLoading: boolean
  loadingMessage: string

  // Error State
  error: string | null

  // Actions
  setSidebarOpen: (open: boolean) => void
  toggleSidebar: () => void
  setTheme: (theme: 'light' | 'dark') => void
  toggleTheme: () => void
  setOnlineStatus: (online: boolean) => void
  setBackendConnected: (connected: boolean) => void
  setGlobalLoading: (loading: boolean, message?: string) => void
  setError: (error: string | null) => void
  clearError: () => void
}

export const useAppStore = create<AppState>()(
  subscribeWithSelector((set, get) => ({
    // Initial state
    sidebarOpen: false,
    theme: (localStorage.getItem('theme') as 'light' | 'dark') || 'light',
    isOnline: navigator.onLine,
    isBackendConnected: false,
    globalLoading: false,
    loadingMessage: '',
    error: null,

    // Actions
    setSidebarOpen: (open: boolean) => {
      set({ sidebarOpen: open })
    },

    toggleSidebar: () => {
      set(state => ({ sidebarOpen: !state.sidebarOpen }))
    },

    setTheme: (theme: 'light' | 'dark') => {
      localStorage.setItem('theme', theme)
      set({ theme })
    },

    toggleTheme: () => {
      const { theme } = get()
      const newTheme = theme === 'light' ? 'dark' : 'light'
      localStorage.setItem('theme', newTheme)
      set({ theme: newTheme })
    },

    setOnlineStatus: (online: boolean) => {
      set({ isOnline: online })
    },

    setBackendConnected: (connected: boolean) => {
      set({ isBackendConnected: connected })
    },

    setGlobalLoading: (loading: boolean, message = '') => {
      set({ globalLoading: loading, loadingMessage: message })
    },

    setError: (error: string | null) => {
      set({ error })
    },

    clearError: () => {
      set({ error: null })
    }
  }))
)

// Selectors
export const useSidebarOpen = () => useAppStore(state => state.sidebarOpen)
export const useTheme = () => useAppStore(state => state.theme)
export const useOnlineStatus = () => useAppStore(state => state.isOnline)
export const useBackendConnected = () => useAppStore(state => state.isBackendConnected)
export const useGlobalLoading = () => useAppStore(state => ({
  loading: state.globalLoading,
  message: state.loadingMessage
}))
export const useError = () => useAppStore(state => state.error)
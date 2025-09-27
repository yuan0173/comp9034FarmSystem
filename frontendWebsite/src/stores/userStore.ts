import { create } from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'
import { CurrentUser } from '../types/api'

interface UserState {
  currentUser: CurrentUser | null
  isLoading: boolean
  isAuthenticated: boolean

  // Actions
  login: (user: CurrentUser) => void
  logout: () => void
  setLoading: (loading: boolean) => void
  updateUser: (updates: Partial<CurrentUser>) => void
  clearUser: () => void
}

export const useUserStore = create<UserState>()(
  persist(
    (set, get) => ({
      currentUser: null,
      isLoading: true,
      isAuthenticated: false,

      login: (user: CurrentUser) => {
        set({
          currentUser: user,
          isAuthenticated: true,
          isLoading: false
        })
      },

      logout: () => {
        set({
          currentUser: null,
          isAuthenticated: false,
          isLoading: false
        })
      },

      setLoading: (loading: boolean) => {
        set({ isLoading: loading })
      },

      updateUser: (updates: Partial<CurrentUser>) => {
        const { currentUser } = get()
        if (currentUser) {
          set({
            currentUser: { ...currentUser, ...updates }
          })
        }
      },

      clearUser: () => {
        set({
          currentUser: null,
          isAuthenticated: false,
          isLoading: false
        })
      }
    }),
    {
      name: 'farm-time-ms-user',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        currentUser: state.currentUser,
        isAuthenticated: state.isAuthenticated
      })
    }
  )
)

// Selectors for better performance
export const useCurrentUser = () => useUserStore(state => state.currentUser)
export const useIsAuthenticated = () => useUserStore(state => state.isAuthenticated)
export const useIsLoading = () => useUserStore(state => state.isLoading)
export const useUserRole = () => useUserStore(state => state.currentUser?.role)
import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  // Load environment variables
  const env = loadEnv(mode, process.cwd(), '')

  // Get API base URL from environment or use default
  const apiBaseUrl = env.VITE_API_BASE_URL || 'http://localhost:4000'

  // Create dynamic URL pattern for PWA caching based on environment
  const createApiUrlPattern = (baseUrl: string) => {
    try {
      const url = new URL(baseUrl)
      // Escape special regex characters and create pattern
      const escapedHost = url.host.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
      const protocol = url.protocol.replace(':', '')
      return new RegExp(`^${protocol}:\\/\\/${escapedHost}\\/api\\/`)
    } catch {
      // Fallback pattern for localhost
      return /^https?:\/\/localhost:\d+\/api\//
    }
  }

  return {
    base: '/comp9034FarmSystem/',
    build: {
      outDir: 'build/dist',
    },
    plugins: [
      react(),
      VitePWA({
        registerType: 'autoUpdate',
        workbox: {
          globPatterns: ['**/*.{js,css,html,ico,png,svg}'],
          runtimeCaching: [
            {
              urlPattern: createApiUrlPattern(apiBaseUrl),
              handler: 'NetworkFirst',
              options: {
                cacheName: 'api-cache',
                expiration: {
                  maxEntries: 50,
                  maxAgeSeconds: 60 * 60 * 24, // 24 hours
                },
                cacheableResponse: {
                  statuses: [0, 200],
                },
              },
            },
          ],
        },
        manifest: {
          name: 'Assignment Frontend',
          short_name: 'Assignment',
          description: 'Attendance & Payroll Management System',
          theme_color: '#1976d2',
          background_color: '#ffffff',
          display: 'standalone',
          icons: [],
        },
      }),
    ],
    resolve: {
      alias: {
        '@': './src',
      },
    },
    server: {
      host: true,
      port: 3000,
    },
  }
})

import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import netlify from '@netlify/vite-plugin'

const useNetlifyDemo = process.env.VITE_USE_NETLIFY_DEMO === 'true'

export default defineConfig({
  plugins: [react(), ...(useNetlifyDemo ? [netlify()] : [])],
  optimizeDeps: {
    exclude: ['maplibre-gl'],
  },
  server: {
    port: 5173,
    proxy: useNetlifyDemo
      ? {}
      : {
          '/api': {
            target: 'http://localhost:5000',
            changeOrigin: true,
          },
        },
  },
})
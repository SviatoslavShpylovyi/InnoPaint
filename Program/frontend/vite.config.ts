import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// The ASP.NET Core API owns all data. Proxying /api to Kestrel keeps the
// browser on one origin in dev, so the API needs no CORS policy.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': { target: 'http://localhost:5247', changeOrigin: true },
      '/health': { target: 'http://localhost:5247', changeOrigin: true },
    },
  },
})

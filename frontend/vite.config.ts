import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      // Proxy API requests
      '/api': {
        target: 'http://localhost:5082', // Your backend address
        changeOrigin: true,
        secure: false, // If your backend is not using HTTPS locally
        ws: true, // IMPORTANT: Enable WebSocket proxying for SignalR
        rewrite: (path) => path.replace(/^\/api/, ''), // Rewrite /api/foo to /foo
      },
    },
    host: '0.0.0.0', // Allow access from network (e.g. for testing on other devices)
    port: 5173, // The port your frontend runs on, matching docker-compose
    watch: {
      usePolling: true, // Required for Docker on Windows/Mac
      interval: 100,
    },
    hmr: {
      host: 'localhost', // HMR will work through nginx proxy
      clientPort: 80,
    },
  },
})

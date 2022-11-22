import { defineConfig } from 'vite';

export default defineConfig({
  clearScreen: false,
  server: {
    port: 8080,
    proxy: {
        // Redirect requests that start with /api/ to the server on port 8085
        '/api/': {
            target: 'http://localhost:8085',
            changeOrigin: true
        },
        // redirect websocket requests that start with /socket/ to the server on the port 8085
        '/socket/': {
            target: 'http://localhost:8085',
            ws: true
        }        
    }
  }
});
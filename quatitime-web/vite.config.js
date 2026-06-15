import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      '/api': {
        target: 'https://quatitime.uon.pt',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})

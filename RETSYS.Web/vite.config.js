import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import path from 'path';

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './Frontend'),
    },
  },
  build: {
    rollupOptions: {
      input: 'Frontend/app.js', // Ponto de entrada definitivo do app
    },
    outDir: 'wwwroot',
    emptyOutDir: false,
    manifest: true, // Essencial para o InertiaCore injetar os scripts no C# em produção
  },
});
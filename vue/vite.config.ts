import ui from '@nuxt/ui/vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vueDevTools from 'vite-plugin-vue-devtools'
// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueDevTools(),
    ui({
      autoImport: {
        imports: ['vue', 'vue-router'],
      },
      ui: {
        theme: {
          radius: 0.25,
          blackAsPrimary: true,
        },
        colors: {},
        modal: {
          slots: {
            header: 'border-b-0',
            body: 'border-b-0 px-4 sm:py-0 mb-4',
            footer: 'flex flex-row justify-end',
          },
        },
        toast: {},
        // dropdownMenu: {
        //   slots: {
        //     content: 'min-w-48',
        //   },
        // },
      },
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5777,
  },
})

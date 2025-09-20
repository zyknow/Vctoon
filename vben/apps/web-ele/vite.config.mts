import { defineConfig } from '@vben/vite-config'

import AutoImport from 'unplugin-auto-import/vite'
import ElementPlus from 'unplugin-element-plus/vite'
import { ElementPlusResolver } from 'unplugin-vue-components/resolvers'
import Components from 'unplugin-vue-components/vite'

export default defineConfig(async () => {
  return {
    application: {},
    vite: {
      plugins: [
        ElementPlus({
          format: 'esm',
        }),
        AutoImport({
          resolvers: [ElementPlusResolver()],
        }),
        Components({
          resolvers: [ElementPlusResolver()],
        }),
      ],
      // server: {
      //   proxy: {
      //     '/api': {
      //       changeOrigin: true,
      //       rewrite: (path) => path.replace(/^\/api/, ''),
      //       // mock代理目标地址
      //       target: 'http://localhost:5320/api',
      //       ws: true,
      //     },
      //   },
      // },
    },
  }
})

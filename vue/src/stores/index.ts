import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import type { App } from 'vue'

export const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)
export function setupStores(app: App) {
  app.use(pinia)
}

export * from './abp'
export * from './medium'
export * from './preferences'
export * from './user'

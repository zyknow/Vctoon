import './assets/css/main.css'
import '@/utils/string-extensions'

import { createApp } from 'vue'
import ui from '@nuxt/ui/vue-plugin'

import { router, setupRouter } from '@/router'
import { setupStores } from '@/stores'

import App from './App.vue'
import { setupI18n } from './locales/setup'

const app = createApp(App)

app.use(ui)

setupStores(app)
setupI18n(app)
setupRouter(app)

router.isReady().then(() => {
  app.mount('#app')
})

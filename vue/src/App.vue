<script setup lang="ts">
import { appI18nStartup, appThemeStartup } from './app-startup'

appThemeStartup()

const locale = appI18nStartup()
</script>

<template>
  <Suspense>
    <UApp
      :locale="locale"
      :toaster="{ position: 'top-center', progress: false }"
    >
      <RouterView />
    </UApp>
  </Suspense>
</template>

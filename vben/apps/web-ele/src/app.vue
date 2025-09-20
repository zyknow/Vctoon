<script lang="ts" setup>
import { useElementPlusDesignTokens } from '@vben/hooks'

import { ElConfigProvider } from 'element-plus'

import { elementLocale } from '#/locales'

defineOptions({ name: 'App' })

useElementPlusDesignTokens()
</script>

<template>
  <ElConfigProvider :locale="elementLocale">
    <Suspense>
      <RouterView />
    </Suspense>
  </ElConfigProvider>
</template>

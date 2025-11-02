<script setup lang="ts">
import { usePreferenceStore } from '@/stores/preferences'
const appConfig = useAppConfig()
const preferences = usePreferenceStore()

function toggleCollapsed(): void {
  preferences.setCollapsed(!preferences.collapsed)
}
</script>

<template>
  <UButton
    variant="ghost"
    color="neutral"
    size="md"
    square
    :icon="
      preferences.collapsed
        ? appConfig.ui.icons.panelOpen
        : appConfig.ui.icons.panelClose
    "
    @click="toggleCollapsed"
  />
</template>

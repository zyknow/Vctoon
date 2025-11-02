<script setup lang="ts">
import { usePreferenceStore } from '@/stores/preferences'
import { useUserStore } from '@/stores/user'

const preferences = usePreferenceStore()
const userStore = useUserStore()
</script>

<template>
  <UDashboardSidebar
    v-model:collapsed="preferences.collapsed"
    resizable
    collapsible
    :ui="{
      root: 'relative hidden lg:flex flex-col h-full max-h-full min-h-0 min-w-16 w-(--width) shrink-0',
      body: 'flex flex-col gap-4 flex-1 overflow-y-auto px-4 py-2 min-h-0',
    }"
  >
    <template #default="{ collapsed }">
      <div class="flex h-full min-h-0 flex-col">
        <UNavigationMenu
          :items="userStore.accessMenus"
          orientation="vertical"
          tooltip
          collapsible
          :collapsed="collapsed"
          popover
          class="shrink-0"
        >
          <template #item-trailing="{ item }">
            <component
              :is="item.trailingComponent"
              v-if="item.trailingComponent"
              :menu="item"
            />
          </template>
        </UNavigationMenu>
      </div>
    </template>
  </UDashboardSidebar>
</template>
<style lang="scss" scoped></style>

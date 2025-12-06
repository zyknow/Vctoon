<script setup lang="ts">
import { computed } from 'vue'
import type { DropdownMenuItem } from '@nuxt/ui'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'

import { useOidcManager } from '@/api/oidc'
import { useIsMobile } from '@/hooks/useIsMobile'
import { useUserStore } from '@/stores'

withDefaults(
  defineProps<{
    collapsed?: boolean
    showName?: boolean
  }>(),
  {},
)

const { t } = useI18n()

const userStore = useUserStore()
const router = useRouter()
const { isMobile } = useIsMobile()
const isDrawerOpen = ref(false)

const menuGroups = computed<DropdownMenuItem[][]>(() => [
  [
    {
      label: t('preferences.userMenu.profile'),
      icon: 'i-lucide-user',
      click: async () => {
        await router.push({ name: 'Profile' })
      },
    },
  ],
])

const logoutItem = computed<DropdownMenuItem>(() => ({
  label: t('preferences.userMenu.logout'),
  icon: 'i-lucide-log-out',
  click: async () => {
    const oidcManager = useOidcManager()
    await oidcManager.signoutRedirectWithState()
  },
}))

const items = computed<DropdownMenuItem[][]>(() => [
  ...menuGroups.value,
  [logoutItem.value],
])

const handleOpen = () => {
  if (isMobile.value) {
    isDrawerOpen.value = true
  }
}
</script>

<template>
  <div v-if="isMobile">
    <UButton
      :label="collapsed ? undefined : userStore.info?.name"
      :avatar="{ src: 'https://github.com/nuxt.png', size: 'lg' }"
      color="neutral"
      variant="ghost"
      block
      :square="collapsed"
      @click="handleOpen"
    />
    <UserDrawer
      v-model:open="isDrawerOpen"
      :menu-groups="menuGroups"
      :logout-item="logoutItem"
    />
  </div>

  <UDropdownMenu
    v-else
    :items="items"
    :content="{ align: 'center', collisionPadding: 12 }"
    :ui="{
      content: collapsed ? 'w-48' : 'w-(--reka-dropdown-menu-trigger-width)',
    }"
  >
    <UButton
      v-bind="{
        label: collapsed ? undefined : userStore.info?.name,
      }"
      :avatar="{
        src: 'https://github.com/nuxt.png',
      }"
      color="neutral"
      variant="ghost"
      block
      :square="collapsed"
      class="data-[state=open]:bg-elevated"
      :ui="{
        trailingIcon: 'text-dimmed',
      }"
    />

    <template #chip-leading="{ item }">
      <div class="inline-flex size-5 shrink-0 items-center justify-center">
        <span
          class="ring-bg size-2 rounded-full bg-(--chip-light) ring dark:bg-(--chip-dark)"
          :style="{
            '--chip-light': `var(--color-${(item as any).chip}-500)`,
            '--chip-dark': `var(--color-${(item as any).chip}-400)`,
          }"
        />
      </div>
    </template>
  </UDropdownMenu>
</template>

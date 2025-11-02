<script setup lang="ts">
import { computed } from 'vue'
import type { DropdownMenuItem } from '@nuxt/ui'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'

import { useOidcManager } from '@/api/oidc'
import { useUserStore } from '@/stores'
defineProps<{
  collapsed?: boolean
}>()

const { t } = useI18n()

const userStore = useUserStore()
const router = useRouter()

const items = computed<DropdownMenuItem[][]>(() => {
  const baseItems: DropdownMenuItem[][] = [
    [
      {
        label: t('preferences.userMenu.profile'),
        icon: 'i-lucide-user',
        onSelect: async () => {
          await router.push({ name: 'Profile' })
        },
      },
    ],
    [
      {
        label: t('preferences.userMenu.logout'),
        icon: 'i-lucide-log-out',
        onSelect: async () => {
          const oidcManager = useOidcManager()
          await oidcManager.manager.signoutRedirect()
        },
      },
    ],
  ]

  return baseItems
})
</script>

<template>
  <UDropdownMenu
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

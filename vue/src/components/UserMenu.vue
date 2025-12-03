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
    <UDrawer v-model:open="isDrawerOpen" direction="right" :handle="false">
      <template #header>
        <div class="flex w-full items-start justify-between px-2 pt-4">
          <div class="flex flex-col gap-3">
            <UAvatar
              src="https://github.com/nuxt.png"
              :alt="userStore.info?.name"
              size="2xl"
            />
            <div class="flex flex-col">
              <span class="text-lg font-bold text-gray-900 dark:text-white">
                {{ userStore.info?.name }}
              </span>
              <span class="text-xs text-gray-500 dark:text-gray-400">
                {{ userStore.info?.email }}
              </span>
            </div>
          </div>
          <UButton
            color="gray"
            variant="ghost"
            icon="i-heroicons-x-mark-20-solid"
            @click="isDrawerOpen = false"
          />
        </div>
      </template>
      <template #body>
        <div class="flex h-full min-w-64 flex-col gap-4">
          <div
            v-for="(group, groupIndex) in menuGroups"
            :key="groupIndex"
            class="flex flex-col gap-2"
          >
            <UButton
              v-for="(item, itemIndex) in group"
              :key="itemIndex"
              block
              color="gray"
              variant="ghost"
              :label="item.label"
              :icon="item.icon"
              class="justify-start"
              size="xl"
              @click="
                () => {
                  item.click?.()
                  isDrawerOpen = false
                }
              "
            />
            <USeparator v-if="groupIndex < menuGroups.length - 1" />
          </div>
        </div>
      </template>
      <template #footer>
        <div class="flex flex-col gap-6">
          <div class="flex items-center justify-between px-2">
            <span class="text-sm font-medium text-gray-700 dark:text-gray-200">
              {{ t('preferences.userMenu.appearance') }}
            </span>
            <PreferenceMenu :min="true" />
          </div>
          <UButton
            block
            color="red"
            variant="soft"
            :label="logoutItem.label"
            :icon="logoutItem.icon"
            size="lg"
            @click="logoutItem.click"
          />
        </div>
      </template>
    </UDrawer>
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

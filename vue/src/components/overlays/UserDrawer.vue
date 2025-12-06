<script setup lang="ts">
import { computed } from 'vue'
import type { DropdownMenuItem } from '@nuxt/ui'
import { useI18n } from 'vue-i18n'

import { useUserStore } from '@/stores'

const props = defineProps<{
  open: boolean
  menuGroups: DropdownMenuItem[][]
  logoutItem: DropdownMenuItem
}>()

const emit = defineEmits<{
  'update:open': [value: boolean]
}>()

const userStore = useUserStore()
const { t } = useI18n()

const isOpen = computed({
  get: () => props.open,
  set: (value) => emit('update:open', value),
})
</script>

<template>
  <UDrawer v-model:open="isOpen" direction="right" :handle="false">
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
          @click="isOpen = false"
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
                isOpen = false
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
</template>

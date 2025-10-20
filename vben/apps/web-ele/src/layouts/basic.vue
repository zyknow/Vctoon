<script lang="ts" setup>
import type { NotificationItem } from '@vben/layouts'

import { computed, ref, watch } from 'vue'

import { useWatermark } from '@vben/hooks'
import {
  BasicLayout,
  LockScreen,
  Notification,
  UserDropdown,
} from '@vben/layouts'
import { preferences } from '@vben/preferences'
import { useUserStore } from '@vben/stores'

import MediumSearchInput from '#/components/headers/medium-search-input.vue'
import { useAuthStore } from '#/store'

const notifications = ref<NotificationItem[]>([])

const userStore = useUserStore()
const authStore = useAuthStore()
const { destroyWatermark, updateWatermark } = useWatermark()
const showDot = computed(() => notifications.value.some((item) => !item.isRead))
const menus = computed(() => [
  // {
  //   handler: () => {
  //     openWindow(`${VBEN_GITHUB_URL}/issues`, {
  //       target: '_blank',
  //     })
  //   },
  //   icon: CircleHelp,
  //   text: $t('ui.widgets.qa'),
  // },
])

const avatar = computed(() => {
  return preferences.app.defaultAvatar
})

async function handleLogout() {
  await authStore.logout(false)
}

function handleNoticeClear() {
  notifications.value = []
}

function handleMakeAll() {
  notifications.value.forEach((item) => (item.isRead = true))
}
watch(
  () => preferences.app.watermark,
  async (enable) => {
    if (enable) {
      await updateWatermark({
        content: `${userStore.userInfo?.userName} - ${userStore.userInfo?.surName}`,
      })
    } else {
      destroyWatermark()
    }
  },
  {
    immediate: true,
  },
)
</script>

<template>
  <BasicLayout @clear-preferences-and-logout="handleLogout">
    <template #header-left-100>
      <div class="flex items-center gap-3">
        <!-- TODO: medium搜索 -->
        <MediumSearchInput />
      </div>
    </template>
    <template #header-right-50>
      <div>
        <LibraryScanButton />
      </div>
    </template>
    <template #user-dropdown>
      <UserDropdown
        :avatar
        :menus
        :text="
          userStore.userInfo?.surName ||
          userStore.userInfo?.userName ||
          userStore.userInfo?.name
        "
        :description="userStore.userInfo?.email"
        :tag-text="userStore.userInfo?.roles?.join(', ')"
        @logout="handleLogout"
      />
    </template>
    <template #notification>
      <Notification
        :dot="showDot"
        :notifications="notifications"
        @clear="handleNoticeClear"
        @make-all="handleMakeAll"
      />
    </template>
    <template #lock-screen>
      <LockScreen :avatar @to-login="handleLogout" />
    </template>
  </BasicLayout>
</template>

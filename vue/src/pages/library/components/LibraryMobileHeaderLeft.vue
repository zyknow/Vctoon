<script setup lang="ts">
import { Library } from '@/api/http/library'
import { useIsMobile } from '@/hooks/useIsMobile'
import mainRoutes from '@/router/modules/main'
import { useUserStore } from '@/stores'

import { LibraryMobileHeaderLeftState } from './LibraryMobileHeaderLeft'

const props = defineProps<{
  library?: Library
}>()

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()
const { isMobile } = useIsMobile()

const icon = mainRoutes.find((x) => x.name === 'Library')?.meta?.icon

const isDrawerOpen = ref(false)
const libraryMenuItems = computed(() => {
  const libraryMenu = userStore.accessMenus.find(
    (m: any) => m.name === 'Library',
  )
  return libraryMenu?.children || []
})

const handleLibraryClick = (menu: any) => {
  isDrawerOpen.value = false
  LibraryMobileHeaderLeftState.currentLibraryPath = menu.to
  router.push(menu.to)
}

if (
  !props?.library &&
  libraryMenuItems.value.length &&
  !LibraryMobileHeaderLeftState.currentLibraryPath
) {
  handleLibraryClick(libraryMenuItems.value[0])
}

if (LibraryMobileHeaderLeftState.currentLibraryPath) {
  router.push(LibraryMobileHeaderLeftState.currentLibraryPath)
}
</script>

<template>
  <MainLayoutProvider v-if="isMobile">
    <template #header-left>
      <div
        class="flex cursor-pointer items-center gap-1 text-xl font-bold"
        @click="isDrawerOpen = true"
      >
        {{ library?.name }}
        <UIcon name="i-heroicons-chevron-down-20-solid" class="h-5 w-5" />
      </div>
    </template>
  </MainLayoutProvider>

  <UDrawer v-model:open="isDrawerOpen" :handle="false">
    <template #header>
      <div class="flex flex-row items-center gap-2">
        <UIcon :name="icon" class="h-6 w-6" />
        <div class="text-lg font-bold">{{ $t('page.library.title') }}</div>
      </div>
    </template>
    <template #body>
      <div class="flex flex-col gap-2">
        <div
          v-for="item in libraryMenuItems"
          :key="item.path || item.name"
          class="hover:bg-muted flex cursor-pointer items-center gap-3 rounded-md p-3 transition-colors"
          :class="{ 'bg-muted': item.path === route.path }"
          @click="handleLibraryClick(item)"
        >
          <UIcon :name="item.icon || 'i-heroicons-book-open'" class="h-5 w-5" />
          <span>{{ item.label }}</span>
        </div>
      </div>
    </template>
  </UDrawer>
</template>
<style lang="scss" scoped></style>

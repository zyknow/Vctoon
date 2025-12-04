<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { useIsMobile } from '@/hooks/useIsMobile'
import { useRefresh } from '@/hooks/useRefresh'
import { getKeepAliveKey } from '@/router/keepAlive'
import { useLayoutStore } from '@/stores/layout'
import { useUserStore } from '@/stores/user'

import MainLayoutSider from './MainLayoutSider.vue'

const userStore = useUserStore()
const layoutStore = useLayoutStore()
const { isMobile } = useIsMobile()
const router = useRouter()
const route = useRoute()
const { isRouteAlive, refreshCurrentRoute } = useRefresh()

const isMobileMenuOpen = ref(false)

const mobilePrimaryMenus = computed(() => {
  return userStore.accessMenus.filter((item: any) => {
    const route = router.getRoutes().find((r) => r.name === item.name)
    return route?.meta?.isMobileBottomNav
  })
})

const mobileSecondaryMenus = computed(() => {
  return userStore.accessMenus.filter((item: any) => {
    const route = router.getRoutes().find((r) => r.name === item.name)
    return !route?.meta?.isMobileBottomNav
  })
})

const isMenuActive = (item: any) => {
  const path = item.to
  if (!path) return false
  return route.path === path || route.path.startsWith(path + '/')
}

// 刷新功能

// 收集所有需要缓存的路由名称
const keepAliveRoutes = computed(() => {
  const routes: string[] = []

  const collectRoutes = (routeList: any[]) => {
    routeList.forEach((r) => {
      if (r.meta?.keepAlive && r.name) {
        routes.push(r.name as string)
      }
      if (r.children) {
        collectRoutes(r.children)
      }
    })
  }

  collectRoutes(router.getRoutes())
  return routes
})
</script>

<template>
  <div
    v-if="isMobile"
    class="bg-background flex h-screen w-full flex-col overflow-hidden"
  >
    <header
      v-if="layoutStore.customHeaderCount > 0"
      id="layout-header"
      class="layout-header h-14 shrink-0 px-4"
    />
    <header
      v-else
      class="layout-header flex h-14 shrink-0 items-center justify-between px-4"
    >
      <div class="flex items-center gap-2">
        <div id="layout-header-left" />
        <slot name="header-left">
          <Logo
            v-if="
              layoutStore.customHeaderLeftCount === 0 &&
              !route.meta.hideMobileHeaderLogo
            "
            size="lg"
          />
        </slot>
      </div>
      <div class="flex items-center gap-2">
        <div id="layout-header-right" />
        <div
          v-if="layoutStore.customHeaderRightCount === 0"
          class="flex items-center"
        >
          <UButton
            icon="i-lucide-search"
            variant="ghost"
            color="neutral"
            to="/search"
          />
          <UserMenu collapsed :show-name="false" />
        </div>
      </div>
    </header>

    <div class="flex-1 overflow-y-auto">
      <RouterView v-slot="{ Component, route }">
        <KeepAlive :include="keepAliveRoutes" max="10">
          <component :is="Component" :key="getKeepAliveKey(route)" />
        </KeepAlive>
      </RouterView>
    </div>

    <div
      v-if="layoutStore.customFooterCount > 0"
      id="layout-footer"
      class="layout-footer shrink-0"
    />
    <nav
      v-else
      class="bg-background pb-safe mobile-nav-footer layout-footer flex h-16 shrink-0"
    >
      <RouterLink
        v-for="item in mobilePrimaryMenus"
        :key="item.name"
        :to="item.to || ''"
        class="flex flex-1 flex-col items-center justify-center gap-1 transition-colors"
        :class="[
          isMenuActive(item)
            ? 'text-primary'
            : 'text-muted-foreground hover:text-primary',
        ]"
      >
        <UIcon :name="item.icon || 'i-lucide-circle'" class="text-2xl" />
        <span class="text-[10px]">{{ item.label }}</span>
      </RouterLink>

      <button
        v-if="mobileSecondaryMenus.length > 0"
        class="text-muted-foreground hover:text-primary flex flex-1 flex-col items-center justify-center gap-1 transition-colors"
        @click="isMobileMenuOpen = true"
      >
        <UIcon name="i-lucide-menu" class="text-2xl" />
        <span class="text-[10px]">{{ $t('common.more') || '更多' }}</span>
      </button>
    </nav>

    <UDrawer v-model:open="isMobileMenuOpen" direction="bottom" :handle="false">
      <template #header>
        <div>
          <div class="flex items-center justify-between p-4">
            <h2 class="text-lg font-semibold">
              {{ $t('common.menu') }}
            </h2>
            <UButton
              color="gray"
              variant="ghost"
              icon="i-lucide-x"
              @click="isMobileMenuOpen = false"
            />
          </div>
        </div>
      </template>
      <template #body>
        <div class="bg-background flex h-full flex-col">
          <div class="flex-1 overflow-y-auto p-2">
            <RouterLink
              v-for="item in mobileSecondaryMenus"
              :key="item.name"
              :to="item.to || ''"
              class="text-foreground flex items-center gap-3 rounded-md p-3 hover:bg-gray-100 dark:hover:bg-gray-800"
              active-class="bg-primary/10 text-primary"
              @click="isMobileMenuOpen = false"
            >
              <UIcon :name="item.icon || 'i-lucide-circle'" class="text-xl" />
              <span>{{ item.label }}</span>
            </RouterLink>
          </div>
        </div>
      </template>
    </UDrawer>
  </div>
  <div v-else class="flex h-screen w-full flex-col overflow-hidden">
    <div
      v-if="layoutStore.customHeaderCount > 0"
      id="layout-header"
      class="layout-header shrink-0"
    />
    <header
      v-else
      class="border-primary layout-header h-12 w-full shrink-0 border-b-2 px-2"
    >
      <div class="flex h-full flex-row items-center justify-between">
        <div class="flex flex-row items-center gap-4 px-4">
          <div id="layout-header-left" />
          <template v-if="layoutStore.customHeaderLeftCount === 0">
            <SiderCollapsed />
            <Logo size="md" />
            <UButton
              icon="i-lucide-refresh-cw"
              variant="ghost"
              color="neutral"
              size="sm"
              :title="$t('common.refresh')"
              @click="refreshCurrentRoute"
            />
            <MediumSearchInput />
          </template>
        </div>
        <div class="flex flex-row items-center gap-1">
          <div id="layout-header-right" />
          <template v-if="layoutStore.customHeaderRightCount === 0">
            <LibraryScanButton />
            <PreferenceMenu :min="true" />
            <UserMenu class="max-w-40" />
          </template>
        </div>
      </div>
    </header>

    <div class="flex min-h-0 flex-1 flex-row overflow-hidden">
      <MainLayoutSider />

      <div class="min-h-0 flex-1 overflow-auto">
        <RouterView v-if="isRouteAlive" v-slot="{ Component, route }">
          <KeepAlive :include="keepAliveRoutes" max="10">
            <component :is="Component" :key="getKeepAliveKey(route)" />
          </KeepAlive>
        </RouterView>
      </div>
    </div>

    <div
      v-if="layoutStore.customFooterCount > 0"
      id="layout-footer"
      class="layout-footer shrink-0"
    />
  </div>
</template>

<style scoped></style>

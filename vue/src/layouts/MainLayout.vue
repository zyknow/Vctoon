<script setup lang="ts">
import { computed, nextTick, ref } from 'vue'
import { useRouter } from 'vue-router'

import { useIsMobile } from '@/hooks/useIsMobile'
import { getKeepAliveKey } from '@/router/keepAlive'

import MainLayoutSider from './MainLayoutSider.vue'

const { isMobile } = useIsMobile()
const router = useRouter()

// 刷新功能
const isRouteAlive = ref(true)

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

// 刷新当前路由页面
const refreshCurrentRoute = async () => {
  isRouteAlive.value = false
  await nextTick()
  isRouteAlive.value = true
}
</script>

<template>
  <div class="flex h-screen w-full flex-col overflow-hidden">
    <header class="border-primary h-12 w-full shrink-0 border-b-2 px-2">
      <div class="flex h-full flex-row items-center justify-between">
        <div class="flex flex-row items-center gap-4 px-4">
          <SiderCollapsed v-if="!isMobile" />
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
        </div>
        <div class="flex flex-row items-center gap-1">
          <LibraryScanButton />
          <PreferenceMenu v-if="!isMobile" :min="true" />
          <UserMenu class="max-w-40" />
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
  </div>
</template>

<style scoped></style>

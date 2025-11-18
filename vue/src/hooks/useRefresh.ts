import { nextTick, ref } from 'vue'
import { useRoute } from 'vue-router'

import { clearLibraryMediumProviderCache } from './useLibraryMediumProvider'

type RouteRefreshGlobal = typeof globalThis & { __routeAlive__?: Ref<boolean> }

const globalWithRefresh = globalThis as RouteRefreshGlobal

function ensureAliveRef() {
  if (!globalWithRefresh.__routeAlive__) {
    globalWithRefresh.__routeAlive__ = ref(true)
  }
  return globalWithRefresh.__routeAlive__
}

export function useRefresh() {
  const route = useRoute()
  const isRouteAlive = ensureAliveRef()

  const refreshCurrentRoute = async () => {
    if (route.name === 'LibraryMedium') {
      const id = String(route.params.id || '')
      if (id) {
        clearLibraryMediumProviderCache(id)
      }
    }
    isRouteAlive.value = false
    await nextTick()
    isRouteAlive.value = true
  }

  return { isRouteAlive, refreshCurrentRoute }
}
<script setup lang="ts">
import {
  computed,
  nextTick,
  onActivated,
  onBeforeUnmount,
  onDeactivated,
  onMounted,
  ref,
  watch,
} from 'vue'
import { onBeforeRouteLeave, useRoute } from 'vue-router'

import { ElMessage } from 'element-plus'

import useMediumDialogService from '#/hooks/useMediumDialogService'
import {
  useInjectedMediumItemProvider,
  useInjectedMediumProvider,
} from '#/hooks/useMediumProvider'
import { useMediumStore } from '#/store'
import { ItemDisplayMode } from '#/store/typing'

import MediumGridItem from './medium-grid-item.vue'

type MediumScrollCacheGlobal = typeof globalThis & {
  __mediumRouteScrollCache__?: Map<string, number>
  __mediumScrollCache__?: Map<number, number>
}

defineProps<{
  modelValue?: boolean
}>()

const globalWithCache = globalThis as MediumScrollCacheGlobal
if (!globalWithCache.__mediumScrollCache__) {
  globalWithCache.__mediumScrollCache__ = new Map<number, number>()
}
const scrollPositionCache = globalWithCache.__mediumScrollCache__
if (!globalWithCache.__mediumRouteScrollCache__) {
  globalWithCache.__mediumRouteScrollCache__ = new Map<string, number>()
}
const routeScrollCache = globalWithCache.__mediumRouteScrollCache__

const { loadType } = useInjectedMediumProvider()
const { items } = useInjectedMediumItemProvider()
const mediumStore = useMediumStore()
const { openEdit } = useMediumDialogService()
const route = useRoute()
const routeKey = computed(() => route.fullPath)
const activeRouteKey = ref(route.fullPath)

const containerClass = computed(() =>
  mediumStore.itemDisplayMode === ItemDisplayMode.Grid
    ? 'flex flex-wrap gap-6'
    : 'flex flex-col gap-2',
)

// 使用已注入的 provider
const injected = useInjectedMediumProvider()

const historyPosition = ref<null | number>(null)
const hasResolvedHistory = ref(false)
const pendingScrollTop = ref<null | number>(null)
const lastKnownScrollTop = ref(0)

const resolveHistoryPosition = (): null | number => {
  if (typeof window === 'undefined') return null
  const position = window.history.state?.position
  return typeof position === 'number' ? position : null
}

const scrollWrapRef = ref<HTMLElement | null>(null)

const markResolved = () => {
  pendingScrollTop.value = null
  hasResolvedHistory.value = true
}

const scheduleRestore = (top: number | undefined) => {
  if (top === undefined) {
    markResolved()
    return
  }
  const el = scrollWrapRef.value
  const current = el?.scrollTop ?? lastKnownScrollTop.value
  if (Math.abs(current - top) <= 1) {
    lastKnownScrollTop.value = current
    markResolved()
    return
  }
  pendingScrollTop.value = top
  hasResolvedHistory.value = false
  void applyScrollPosition({ loading: injected.loading.value })
}

const saveScrollPosition = (
  targetKey?: null | string,
  explicitTop?: null | number,
) => {
  if (historyPosition.value === null) return
  const el = scrollWrapRef.value
  const topSource =
    explicitTop ??
    el?.scrollTop ??
    (Number.isFinite(lastKnownScrollTop.value) ? lastKnownScrollTop.value : 0)
  lastKnownScrollTop.value = typeof topSource === 'number' ? topSource : 0
  if (!el && explicitTop === null) return
  scrollPositionCache.set(historyPosition.value, lastKnownScrollTop.value)
  const cacheKey = targetKey ?? activeRouteKey.value
  if (cacheKey) {
    routeScrollCache.set(cacheKey, lastKnownScrollTop.value)
  }
}

type RestoreContext = {
  loading: boolean
}

const waitForAnimationFrame = () =>
  new Promise<void>((resolve) => {
    requestAnimationFrame(() => resolve())
  })

const applyScrollPosition = async (
  { loading }: RestoreContext,
  attempt = 0,
) => {
  if (pendingScrollTop.value === null) return
  await nextTick()
  const el = scrollWrapRef.value
  if (!el) return

  const targetTop = pendingScrollTop.value
  const maxScrollable = el.scrollHeight - el.clientHeight

  if (maxScrollable <= 0) {
    if (!loading) {
      markResolved()
    }
    return
  }

  const nextTop = Math.min(targetTop, maxScrollable)
  el.scrollTop = nextTop
  lastKnownScrollTop.value = nextTop
  await waitForAnimationFrame()
  const currentTop = el.scrollTop
  if (Math.abs(currentTop - nextTop) > 5 && attempt < 1) {
    pendingScrollTop.value = targetTop
    await applyScrollPosition({ loading }, attempt + 1)
    return
  }

  if (maxScrollable >= targetTop || !loading) {
    markResolved()
  }
}

const initScrollRestore = () => {
  if (hasResolvedHistory.value) return

  const savedPosition = historyPosition.value
  let cachedTop: number | undefined
  if (savedPosition !== null) {
    cachedTop = scrollPositionCache.get(savedPosition)
  }

  if (cachedTop === undefined && routeKey.value) {
    cachedTop = routeScrollCache.get(routeKey.value)
  }

  scheduleRestore(cachedTop)
}

// 仅在 medium-content 内部滚动触发加载更多
const onInnerScroll = () => {
  const el = scrollWrapRef.value
  if (!el) return
  const threshold = 120 // px 提前量
  const scrollBottom = el.scrollHeight - (el.scrollTop + el.clientHeight)
  if (scrollBottom <= threshold) {
    void providerLoadNext()
  }
  saveScrollPosition(undefined, el.scrollTop)
}

const providerLoadNext = async () => {
  await injected.loadNext()
}

// 编辑弹窗处理（Promise 化）
const handleEdit = async (medium: any) => {
  try {
    const updated = await openEdit({
      mediumId: medium.id,
      mediumType: medium.mediumType,
      closeOnClickModal: false,
    })
    if (updated) {
      const index = items.value.findIndex((item) => item.id === updated.id)
      if (index !== -1) {
        items.value[index] = updated
      }
    }
  } catch (error) {
    // reject 场景（目前未使用）
    console.error('Error creating dialog:', error)
    ElMessage.error('创建弹窗失败，请查看控制台')
  }
}

onMounted(() => {
  historyPosition.value = resolveHistoryPosition()
  initScrollRestore()
  const el = scrollWrapRef.value
  if (el) el.addEventListener('scroll', onInnerScroll, { passive: true })
  activeRouteKey.value = routeKey.value
  if (el) lastKnownScrollTop.value = el.scrollTop
})

onActivated(() => {
  historyPosition.value = resolveHistoryPosition()
  initScrollRestore()
  activeRouteKey.value = routeKey.value
  const el = scrollWrapRef.value
  if (el) lastKnownScrollTop.value = el.scrollTop
})

onBeforeUnmount(() => {
  saveScrollPosition(activeRouteKey.value, lastKnownScrollTop.value)
  const el = scrollWrapRef.value
  if (el) el.removeEventListener('scroll', onInnerScroll)
})

onDeactivated(() => {
  saveScrollPosition(activeRouteKey.value, lastKnownScrollTop.value)
})

onBeforeRouteLeave(() => {
  const el = scrollWrapRef.value
  const top = el ? el.scrollTop : lastKnownScrollTop.value
  saveScrollPosition(activeRouteKey.value, top)
})

watch(
  () => [items.value.length, injected.loading.value],
  () => {
    if (hasResolvedHistory.value) return
    if (pendingScrollTop.value === null) return
    void applyScrollPosition({ loading: injected.loading.value })
  },
  { flush: 'post' },
)

watch(
  routeKey,
  (current, previous) => {
    if (current === previous) return
    historyPosition.value = resolveHistoryPosition()
    const cacheKey = current ?? activeRouteKey.value
    if (!cacheKey) {
      markResolved()
      return
    }
    const cachedTop = routeScrollCache.get(cacheKey)
    scheduleRestore(cachedTop)
  },
  { flush: 'post' },
)
</script>

<template>
  <div ref="scrollWrapRef" class="medium-content">
    <!-- Grid 模式动画容器 -->
    <TransitionGroup
      v-if="mediumStore.itemDisplayMode === ItemDisplayMode.Grid"
      name="fade-slide"
      tag="div"
      :class="containerClass"
      appear
    >
      <MediumGridItem
        :medium-type="loadType"
        v-for="item in items"
        :key="`grid-${item.id}`"
        :model-value="item"
        @edit="handleEdit"
      />
    </TransitionGroup>
  </div>
</template>

<style scoped>
/* 动画（参考 medium-recommendation-section.vue） */
.fade-slide-enter-active {
  transition: all 0.5s cubic-bezier(0.4, 0, 0.2, 1);
}

.fade-slide-enter-from {
  opacity: 0;
  transform: translateX(30px) scale(0.9);
}

.fade-slide-enter-to {
  opacity: 1;
  transform: translateX(0) scale(1);
}

.fade-slide-move {
  transition: transform 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

.fade-slide-enter-active,
.fade-slide-move {
  will-change: transform, opacity;
}
</style>

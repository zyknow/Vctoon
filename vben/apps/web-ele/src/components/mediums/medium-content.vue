<script setup lang="ts">
import type { MediumGetListOutput } from '@vben/api'

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
import { $t } from '#/locales'
import { useMediumStore } from '#/store'
import { ItemDisplayMode } from '#/store/typing'

import MediumGridItem from './medium-grid-item.vue'
import MediumListItem from './medium-list-item.vue'

defineProps<{
  modelValue?: boolean
}>()

const { loadType, loading, hasMore, loadItems, loadNext } =
  useInjectedMediumProvider()
const { items } = useInjectedMediumItemProvider()
const mediumStore = useMediumStore()
const { openEdit } = useMediumDialogService()

const containerClass = computed(() =>
  mediumStore.itemDisplayMode === ItemDisplayMode.Grid
    ? 'flex flex-wrap gap-6'
    : 'flex flex-col gap-2',
)

const scrollWrapRef = ref<HTMLElement | null>(null)
const loadMoreTriggerRef = ref<HTMLElement | null>(null)
const scrollContainerRef = ref<HTMLElement | null>(null)
const savedScrollTop = ref(0)
const scrollRestored = ref(false)
let boundScrollHandler: ((e: Event) => void) | null = null

const route = useRoute()
const routeKey = computed<string>(() => route.fullPath)

interface ScrollRecord {
  top: number
  ts: number
}

function saveScrollPosition(top: number): void {
  try {
    const record: ScrollRecord = { top, ts: Date.now() }
    sessionStorage.setItem(`scroll:${routeKey.value}`, JSON.stringify(record))
  } catch {
    // 忽略存储错误（Safari 隐私模式等）
  }
}

function loadScrollPosition(): number {
  try {
    const raw = sessionStorage.getItem(`scroll:${routeKey.value}`)
    if (!raw) return 0
    const rec = JSON.parse(raw) as ScrollRecord
    return typeof rec.top === 'number' ? rec.top : 0
  } catch {
    return 0
  }
}

let loadObserver: IntersectionObserver | null = null

function getMainContentContainer(): HTMLElement | null {
  const main = document.querySelector(
    '#__vben_main_content',
  ) as HTMLElement | null
  if (!main) return null
  // Page 内容容器：具有 h-full 和 p-4 的 div
  const candidate = main.querySelector('div.h-full.p-4') as HTMLElement | null
  if (!candidate) return null
  const style = window.getComputedStyle(candidate)
  const oy = style.overflowY
  const o = style.overflow
  const canScroll = candidate.scrollHeight > candidate.clientHeight + 1
  const overflowMatches =
    oy === 'auto' ||
    oy === 'scroll' ||
    o === 'auto' ||
    o === 'scroll' ||
    o.includes('auto') ||
    o.includes('scroll')
  if (canScroll && overflowMatches) {
    return candidate
  }
  return null
}

function findNearestPageContent(el: HTMLElement | null): HTMLElement | null {
  let parent = el?.parentElement || null
  while (parent) {
    if (
      parent.classList.contains('h-full') &&
      parent.classList.contains('p-4')
    ) {
      const style = window.getComputedStyle(parent)
      const oy = style.overflowY
      const o = style.overflow
      if (oy === 'auto' || oy === 'scroll' || o === 'auto' || o === 'scroll') {
        return parent
      }
    }
    parent = parent.parentElement
  }
  return null
}

function findScrollContainer(el: HTMLElement | null): HTMLElement | null {
  // 优先检查自身是否可滚动（medium-content 是实际容器）
  if (el) {
    const style = window.getComputedStyle(el)
    const oy = style.overflowY
    const o = style.overflow
    const canScroll = el.scrollHeight > el.clientHeight + 1
    const overflowMatches =
      oy === 'auto' ||
      oy === 'scroll' ||
      o === 'auto' ||
      o === 'scroll' ||
      o.includes('auto') ||
      o.includes('scroll')
    if (canScroll && overflowMatches) {
      return el
    }
  }
  // 其次尝试 Page 主内容容器（有时也承载滚动）
  const mainContent = getMainContentContainer()
  if (mainContent) return mainContent
  const nearest = findNearestPageContent(el)
  if (nearest) return nearest
  let parent = el?.parentElement || null
  while (parent) {
    const style = window.getComputedStyle(parent)
    const oy = style.overflowY
    const o = style.overflow
    if (
      oy === 'auto' ||
      oy === 'scroll' ||
      o === 'auto' ||
      o === 'scroll' ||
      o.includes('auto') ||
      o.includes('scroll')
    ) {
      return parent
    }
    parent = parent.parentElement
  }
  return (document.scrollingElement || document.documentElement) as HTMLElement
}

const setupLoadMoreObserver = () => {
  if (typeof IntersectionObserver === 'undefined') return
  const trigger = loadMoreTriggerRef.value
  // 识别实际滚动容器（Page content）或回退到视口
  const root = scrollContainerRef.value ?? null
  if (!trigger) return
  if (loadObserver) {
    loadObserver.disconnect()
    loadObserver = null
  }
  loadObserver = new IntersectionObserver(
    (entries) => {
      const entry = entries[0]
      if (entry && entry.isIntersecting && !loading.value && hasMore.value) {
        void loadNext().catch((error) => {
          console.error('滚动加载失败', error)
        })
      }
    },
    { root, threshold: [0, 0.25, 0.5, 0.75, 1] },
  )
  loadObserver.observe(trigger)
}

const teardownLoadMoreObserver = () => {
  if (loadObserver) {
    loadObserver.disconnect()
    loadObserver = null
  }
}

function setupScrollListener(): void {
  const c = scrollContainerRef.value
  if (!c) return
  // 避免重复绑定
  if (boundScrollHandler) return
  const handler = () => {
    savedScrollTop.value = c.scrollTop
    // 持续保存，确保路由切换前已有最新值
    saveScrollPosition(savedScrollTop.value)
  }
  c.addEventListener('scroll', handler, { passive: true })
  boundScrollHandler = handler
}

function teardownScrollListener(): void {
  const c = scrollContainerRef.value
  if (c && boundScrollHandler) {
    c.removeEventListener('scroll', boundScrollHandler)
  }
  boundScrollHandler = null
}

const handleEdit = async (medium: MediumGetListOutput) => {
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
    console.error('Error creating dialog:', error)
    ElMessage.error('创建弹窗失败，请查看控制台')
  }
}

const ensureInitialLoad = async () => {
  if (items.value.length === 0) {
    await loadItems()
  }
  await nextTick()
  // Page 的 autoContentHeight 需等待溢出样式应用后再观察与识别容器
  setTimeout(() => {
    // 识别并缓存滚动容器（在 Page 溢出样式应用后）
    scrollContainerRef.value = findScrollContainer(scrollWrapRef.value)
    setupScrollListener()
    setupLoadMoreObserver()
    // 恢复滚动位置（KeepAlive 或重新挂载场景）
    savedScrollTop.value = savedScrollTop.value || loadScrollPosition()
    void restoreScrollWithRetries(24, 80)
  }, 100)
}

onMounted(() => {
  void ensureInitialLoad()
})

onActivated(() => {
  void ensureInitialLoad()
})

onDeactivated(() => {
  // 记录滚动位置，便于返回时恢复
  const container =
    scrollContainerRef.value ||
    findScrollContainer(scrollWrapRef.value) ||
    ((document.scrollingElement || document.documentElement) as HTMLElement)
  if (container) {
    savedScrollTop.value = container.scrollTop
    saveScrollPosition(savedScrollTop.value)
  }
  scrollRestored.value = false
  teardownLoadMoreObserver()
  teardownScrollListener()
})

onBeforeUnmount(() => {
  // 非 KeepAlive 场景也保存滚动位置
  const container =
    scrollContainerRef.value ||
    findScrollContainer(scrollWrapRef.value) ||
    ((document.scrollingElement || document.documentElement) as HTMLElement)
  if (container) {
    savedScrollTop.value = container.scrollTop
    saveScrollPosition(savedScrollTop.value)
  }
  scrollRestored.value = false
  teardownLoadMoreObserver()
  teardownScrollListener()
})

// 在路由离开前保存滚动位置，避免路由滚动行为覆盖
onBeforeRouteLeave((_to, _from, next) => {
  const container =
    scrollContainerRef.value ||
    findScrollContainer(scrollWrapRef.value) ||
    getMainContentContainer() ||
    ((document.scrollingElement || document.documentElement) as HTMLElement)
  if (container) {
    savedScrollTop.value = container.scrollTop
    saveScrollPosition(savedScrollTop.value)
  }
  next()
})

// hasMore 变化时动态管理观察器，避免无意义的触发
watch(
  () => hasMore.value,
  (val) => {
    if (val) {
      setupLoadMoreObserver()
    } else {
      teardownLoadMoreObserver()
    }
  },
)

async function restoreScrollWithRetries(
  maxTries = 8,
  delay = 50,
): Promise<void> {
  const container =
    scrollContainerRef.value ||
    ((document.scrollingElement || document.documentElement) as HTMLElement)
  const targetTop = savedScrollTop.value || loadScrollPosition()
  if (!container || targetTop <= 0 || scrollRestored.value) return

  let tries = 0
  const attempt = () => {
    if (!scrollContainerRef.value) {
      // 尝试重新识别滚动容器
      scrollContainerRef.value = findScrollContainer(scrollWrapRef.value)
      if (!scrollContainerRef.value) return
    }
    const c = scrollContainerRef.value
    const canScroll = c.scrollHeight > c.clientHeight + 1
    if (canScroll) {
      c.scrollTo({ top: targetTop })
      // 验证是否已生效
      if (Math.abs(c.scrollTop - targetTop) < 2) {
        scrollRestored.value = true
        return
      }
    }
    tries += 1
    if (tries < maxTries) {
      // 使用 rAF + 定时器，提高在异步渲染中的恢复成功率
      requestAnimationFrame(() => setTimeout(attempt, delay))
    }
  }
  attempt()
}

// 数据加载完成或列表长度变化后再次尝试恢复滚动
watch(
  () => loading.value,
  (isLoading) => {
    if (!isLoading) void restoreScrollWithRetries(10, 60)
  },
)

watch(
  () => items.value.length,
  () => {
    if (!loading.value) void restoreScrollWithRetries(6, 60)
  },
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
    <!-- List 模式渲染容器 -->
    <div v-else :class="containerClass">
      <MediumListItem
        v-for="item in items"
        :key="`list-${item.id}`"
        :model-value="item"
        @edit="handleEdit"
      />
    </div>
    <div
      v-if="!loading && items.length === 0"
      class="text-muted-foreground py-12 text-center text-sm"
    >
      {{ $t('common.noData') }}
    </div>
    <div
      v-else-if="loading && items.length === 0"
      class="text-muted-foreground py-12 text-center text-sm"
    >
      {{ $t('common.loading') }}
    </div>
    <!-- 滚动加载触发器与状态 -->
    <div ref="loadMoreTriggerRef" class="flex justify-center py-4">
      <div class="text-muted-foreground text-sm" v-if="loading">
        {{ $t('common.loading') }}
      </div>
      <div class="text-muted-foreground text-sm" v-else-if="!hasMore">
        {{ $t('page.mediums.list.noMore') }}
      </div>
    </div>
  </div>
</template>

<style scoped>
.medium-content__pagination {
  margin-top: 1.5rem;
}

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

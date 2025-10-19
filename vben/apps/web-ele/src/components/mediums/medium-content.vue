<script setup lang="ts">
import type { MediumGetListOutput } from '@vben/api'

import {
  computed,
  nextTick,
  onActivated,
  onBeforeUnmount,
  onMounted,
  ref,
  watch,
} from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { ElMessage } from 'element-plus'

import useMediumDialogService from '#/hooks/useMediumDialogService'
import {
  useInjectedMediumItemProvider,
  useInjectedMediumProvider,
} from '#/hooks/useMediumProvider'
import { $t } from '#/locales'
import { useMediumStore } from '#/store'
import { ItemDisplayMode } from '#/store/typing'

import { getMediumAnchorId, MEDIUM_ANCHOR_PREFIX } from './anchor'
import MediumGridItem from './medium-grid-item.vue'

defineProps<{
  modelValue?: boolean
}>()

const DEFAULT_PAGE_SIZE = 50
const PAGE_SIZE_OPTIONS = [20, 50, 100, 200] as const

const route = useRoute()
const router = useRouter()

const injected = useInjectedMediumProvider()
const { loadType } = injected
const { items } = useInjectedMediumItemProvider()
const mediumStore = useMediumStore()
const { openEdit } = useMediumDialogService()

const itemIndexMap = computed(() => {
  const map = new Map<string, number>()
  items.value.forEach((item, index) => {
    map.set(item.id, index)
  })
  return map
})

const containerClass = computed(() =>
  mediumStore.itemDisplayMode === ItemDisplayMode.Grid
    ? 'flex flex-wrap gap-6'
    : 'flex flex-col gap-2',
)

const scrollWrapRef = ref<HTMLElement | null>(null)

const pageSize = computed({
  get: () => {
    const value = Number(
      injected.pageRequest.maxResultCount ?? DEFAULT_PAGE_SIZE,
    )
    return Number.isFinite(value) && value > 0
      ? Math.floor(value)
      : DEFAULT_PAGE_SIZE
  },
  set: (size: number) => {
    const safeSize = Math.max(1, Math.floor(size || DEFAULT_PAGE_SIZE))
    injected.pageRequest.maxResultCount = safeSize
  },
})
const pageSizeOptions = PAGE_SIZE_OPTIONS.map((option) => option)

const totalItems = computed(() => injected.totalCount.value ?? 0)
const totalPages = computed(() =>
  Math.max(1, Math.ceil(totalItems.value / pageSize.value)),
)

const getQueryString = (value: unknown): null | string => {
  if (Array.isArray(value)) {
    return typeof value[0] === 'string' ? value[0] : null
  }
  return typeof value === 'string' ? value : null
}

const parsePositiveInteger = (value: unknown): null | number => {
  const text = getQueryString(value)
  if (!text) return null
  const parsed = Number.parseInt(text, 10)
  if (!Number.isFinite(parsed) || parsed <= 0) return null
  return parsed
}

const targetHighlightId = computed(() => getQueryString(route.query.highlight))
const targetPage = computed(() => parsePositiveInteger(route.query.page))

const currentPage = ref(targetPage.value ?? 1)
const lastLoadedPage = ref<null | number>(null)

const highlightResolved = ref(false)
const ensureTaskRunning = ref(false)
const paginationDisabled = computed(
  () => injected.loading.value || ensureTaskRunning.value,
)
const skipNextRouteSync = ref(false)
const skipNextRouteHighlight = ref<null | string>(null)
const currentVisibleHighlightId = ref<null | string>(null)

let intersectionObserver: IntersectionObserver | null = null
const visibleEntryMap = new Map<string, IntersectionObserverEntry>()

const waitForLoading = async () => {
  if (!injected.loading.value) return
  await new Promise<void>((resolve) => {
    const stop = watch(
      () => injected.loading.value,
      (loading) => {
        if (!loading) {
          stop()
          resolve()
        }
      },
      { immediate: false },
    )
  })
}

const extractMediumIdFromAnchor = (anchorId: string): null | string => {
  if (!anchorId || !anchorId.startsWith(MEDIUM_ANCHOR_PREFIX)) {
    return null
  }
  return anchorId.slice(MEDIUM_ANCHOR_PREFIX.length)
}

const getTopVisibleMediumId = (): null | string => {
  let bestId: null | string = null
  let bestTop = Number.POSITIVE_INFINITY
  let bestIndex = Number.POSITIVE_INFINITY
  let bestLeft = Number.POSITIVE_INFINITY
  visibleEntryMap.forEach((entry, id) => {
    const index = itemIndexMap.value.get(id) ?? Number.POSITIVE_INFINITY
    const { top, left } = entry.boundingClientRect
    if (top < bestTop - 1) {
      bestTop = top
      bestIndex = index
      bestLeft = left
      bestId = id
      return
    }
    if (Math.abs(top - bestTop) <= 1) {
      if (index < bestIndex) {
        bestTop = top
        bestIndex = index
        bestLeft = left
        bestId = id
        return
      }
      if (index === bestIndex && left < bestLeft - 1) {
        bestTop = top
        bestIndex = index
        bestLeft = left
        bestId = id
      }
    }
  })
  return bestId
}

const updateRouteHighlight = (mediumId: null | string) => {
  const current = targetHighlightId.value
  if (current === mediumId) return
  const nextQuery = { ...route.query }
  if (mediumId) {
    nextQuery.highlight = mediumId
  } else {
    delete nextQuery.highlight
  }
  skipNextRouteSync.value = true
  skipNextRouteHighlight.value = mediumId
  void router.replace({ query: nextQuery }).catch(() => {
    skipNextRouteSync.value = false
    skipNextRouteHighlight.value = null
  })
}

const handleIntersection: IntersectionObserverCallback = (entries) => {
  let changed = false
  entries.forEach((entry) => {
    const element = entry.target as HTMLElement
    const mediumId = extractMediumIdFromAnchor(element.id)
    if (!mediumId) return
    if (entry.isIntersecting && entry.intersectionRatio > 0) {
      visibleEntryMap.set(mediumId, entry)
    } else {
      visibleEntryMap.delete(mediumId)
    }
    changed = true
  })
  if (!changed) return
  if (!highlightResolved.value) return
  const nextHighlight = getTopVisibleMediumId()
  if (!nextHighlight || nextHighlight === currentVisibleHighlightId.value) {
    return
  }
  currentVisibleHighlightId.value = nextHighlight
  updateRouteHighlight(nextHighlight)
}

const teardownIntersectionObserver = () => {
  if (intersectionObserver) {
    intersectionObserver.disconnect()
    intersectionObserver = null
  }
  visibleEntryMap.clear()
}

const setupIntersectionObserver = () => {
  if (typeof IntersectionObserver === 'undefined') return
  const root = scrollWrapRef.value
  if (!root) return
  teardownIntersectionObserver()
  intersectionObserver = new IntersectionObserver(handleIntersection, {
    root,
    threshold: [0, 0.25, 0.5, 0.75, 1],
  })
  const anchors = root.querySelectorAll<HTMLElement>(
    `[id^="${MEDIUM_ANCHOR_PREFIX}"]`,
  )
  anchors.forEach((anchor) => {
    intersectionObserver?.observe(anchor)
  })
}

const loadPageData = async (page: number) => {
  const safePage = Math.max(1, Math.floor(page || 1))
  if (typeof injected.loadPage === 'function') {
    await injected.loadPage(safePage)
  } else {
    const skip = (safePage - 1) * pageSize.value
    injected.pageRequest.skipCount = skip
    await injected.loadItems()
  }
  await waitForLoading()
  lastLoadedPage.value = safePage
}

const scrollToTop = () => {
  const el = scrollWrapRef.value
  if (!el) return
  el.scrollTo({ top: 0, behavior: 'auto' })
}

const scrollToHighlight = async (mediumId: null | string) => {
  if (!mediumId) return
  if (typeof document === 'undefined') return
  await nextTick()
  const anchor = document.querySelector<HTMLElement>(
    `#${getMediumAnchorId(mediumId)}`,
  )
  if (!anchor) return
  anchor.scrollIntoView({ block: 'center', behavior: 'auto' })
}

const ensureInitialState = async () => {
  if (ensureTaskRunning.value) return
  ensureTaskRunning.value = true
  try {
    currentVisibleHighlightId.value = null
    const desiredPage = targetPage.value ?? currentPage.value ?? 1
    if (currentPage.value !== desiredPage) {
      currentPage.value = desiredPage
    }
    if (lastLoadedPage.value !== desiredPage) {
      await loadPageData(desiredPage)
      scrollToTop()
    }
    await nextTick()
    const highlightId = targetHighlightId.value
    if (highlightId) {
      await scrollToHighlight(highlightId)
    }
    currentVisibleHighlightId.value = highlightId
    highlightResolved.value = true
    void nextTick(() => {
      setupIntersectionObserver()
    })
  } finally {
    ensureTaskRunning.value = false
  }
}

const syncRoutePage = (page: number) => {
  const nextQuery = { ...route.query }
  const pageText = page > 1 ? String(page) : null
  const current = getQueryString(route.query.page)
  let changed = false
  if (pageText) {
    if (current !== pageText) {
      nextQuery.page = pageText
      changed = true
    }
  } else if (current) {
    delete nextQuery.page
    changed = true
  }
  if ('highlight' in nextQuery) {
    delete nextQuery.highlight
    changed = true
  }
  if (changed) {
    skipNextRouteSync.value = true
    skipNextRouteHighlight.value = null
    void router.replace({ query: nextQuery }).catch(() => {
      skipNextRouteSync.value = false
      skipNextRouteHighlight.value = null
    })
  }
}

const handlePageChange = async (page: number) => {
  const safePage = Math.max(1, Math.floor(page || 1))
  currentPage.value = safePage
  highlightResolved.value = false
  currentVisibleHighlightId.value = null
  await loadPageData(safePage)
  scrollToTop()
  await nextTick()
  highlightResolved.value = true
  void nextTick(() => {
    setupIntersectionObserver()
  })
  syncRoutePage(safePage)
}

const handlePageSizeChange = async (size: number) => {
  const safeSize = Math.max(1, Math.floor(size || DEFAULT_PAGE_SIZE))
  if (pageSize.value === safeSize) return
  pageSize.value = safeSize
  injected.pageRequest.skipCount = 0
  currentPage.value = 1
  highlightResolved.value = false
  currentVisibleHighlightId.value = null
  await loadPageData(1)
  scrollToTop()
  await nextTick()
  highlightResolved.value = true
  void nextTick(() => {
    setupIntersectionObserver()
  })
  syncRoutePage(1)
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

onMounted(() => {
  void ensureInitialState()
})

onActivated(() => {
  void ensureInitialState()
})

watch(
  () => [route.query.page, route.query.highlight],
  () => {
    if (skipNextRouteSync.value) {
      const highlight = targetHighlightId.value
      if (highlight === skipNextRouteHighlight.value) {
        skipNextRouteSync.value = false
        skipNextRouteHighlight.value = null
        return
      }
      skipNextRouteSync.value = false
      skipNextRouteHighlight.value = null
    }
    highlightResolved.value = false
    void ensureInitialState()
  },
)

watch(
  () => totalPages.value,
  (pages) => {
    if (currentPage.value > pages) {
      void handlePageChange(pages)
    }
  },
)

watch(
  () => items.value.length,
  () => {
    if (targetHighlightId.value && !highlightResolved.value) {
      void scrollToHighlight(targetHighlightId.value)
    }
    if (items.value.length === 0) {
      currentVisibleHighlightId.value = null
      updateRouteHighlight(null)
    }
    void nextTick(() => {
      setupIntersectionObserver()
    })
  },
)

watch(
  () => items.value,
  () => {
    void nextTick(() => {
      setupIntersectionObserver()
    })
  },
  { flush: 'post' },
)

watch(
  () => mediumStore.itemDisplayMode,
  () => {
    void nextTick(() => {
      setupIntersectionObserver()
    })
  },
)

onBeforeUnmount(() => {
  teardownIntersectionObserver()
})
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
    <div
      v-if="!injected.loading && items.length === 0"
      class="text-muted-foreground py-12 text-center text-sm"
    >
      {{ $t('common.noData') }}
    </div>
    <div
      v-else-if="injected.loading && items.length === 0"
      class="text-muted-foreground py-12 text-center text-sm"
    >
      {{ $t('common.loading') }}
    </div>
    <div class="medium-content__pagination flex justify-center py-4">
      <el-pagination
        :background="true"
        :current-page="currentPage"
        :disabled="paginationDisabled"
        :hide-on-single-page="false"
        layout="sizes, prev, pager, next, jumper"
        :page-size="pageSize"
        :page-sizes="pageSizeOptions"
        :total="totalItems"
        @current-change="handlePageChange"
        @size-change="handlePageSizeChange"
      />
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

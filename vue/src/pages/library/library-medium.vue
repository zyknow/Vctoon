<script lang="ts" setup>
import type { Ref } from 'vue'

import type { MediumGetListOutput } from '@/api/http/medium/typing'
import { dataChangedHub } from '@/api/signalr/data-changed'
import { libraryHub } from '@/api/signalr/library'
import type { SortField } from '@/components/mediums/types'
import UScrollbar from '@/components/nuxt-ui-extensions/UScrollbar.vue'
import Page from '@/components/Page.vue'
import { useIsMobile } from '@/hooks/useIsMobile'
import { createLibraryMediumProvider } from '@/hooks/useLibraryMediumProvider'
import { useMediumFilterBinding } from '@/hooks/useMediumFilterBinding'
import type { MediumViewTab } from '@/hooks/useMediumProvider'
import {
  createMediumProvider,
  provideMediumAllItemProvider,
  provideMediumItemProvider,
  provideMediumProvider,
} from '@/hooks/useMediumProvider'
import { useRefresh } from '@/hooks/useRefresh'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores'

import LibraryRecommend from './components/library-recommend.vue'
import LibrarySeries from './components/library-series.vue'
import LibraryMediumToolbar from './components/LibraryMediumToolbar.vue'
import LibraryMobileHeaderLeft from './components/LibraryMobileHeaderLeft.vue'
import LibraryUpdateNotice from './components/LibraryUpdateNotice.vue'

const route = useRoute()
const router = useRouter()
const refresh = useRefresh()
const { isMobile } = useIsMobile()
const userStore = useUserStore()

const libraryId = route.params.id as string

const library = userStore.libraries.find((i) => i.id === libraryId)
if (!library) {
  throw new Error('Library not found')
}
const state = createLibraryMediumProvider(library)

state.currentTab.value = (route.query.tab as MediumViewTab) || 'recommend'

const seriesState = createMediumProvider({
  loadType: library.mediumType,
  title: library.name,
  pageRequest: {
    libraryId: library.id,
    includeSeries: true,
    includeMediums: false,
    sorting: 'Title asc',
  },
  autoLoad: true,
})

const mediumContentRef = ref<{
  onEndReached: (direction: 'top' | 'bottom' | 'left' | 'right') => void
} | null>(null)
const seriesRef = ref<{
  onEndReached: (direction: 'top' | 'bottom' | 'left' | 'right') => void
} | null>(null)

const onScrollEnd = (direction: 'top' | 'bottom' | 'left' | 'right') => {
  if (state.currentTab.value === 'library') {
    mediumContentRef.value?.onEndReached(direction)
  } else if (state.currentTab.value === 'series') {
    seriesRef.value?.onEndReached(direction)
  }
}

const normalizeQueryString = (value: unknown): null | string => {
  if (Array.isArray(value)) {
    return typeof value[0] === 'string' ? value[0] : null
  }
  return typeof value === 'string' ? value : null
}

watch(
  () => state.currentTab.value,
  (tab) => {
    const currentTab = normalizeQueryString(route.query.tab)
    if (currentTab === tab) {
      return
    }
    const nextQuery = { ...route.query, tab }
    void router.replace({ query: nextQuery })
  },
  { immediate: true },
)

provideMediumProvider(state)

const allItemsMap = reactive<Record<string, Ref<MediumGetListOutput[]>>>({
  main: state.items,
  series: seriesState.items,
})

provideMediumAllItemProvider({
  itemsMap: allItemsMap,
})

const activeSelectedMediumIds = computed<string[]>({
  get: () => {
    return state.currentTab.value === 'series'
      ? seriesState.selectedMediumIds.value
      : state.selectedMediumIds.value
  },
  set: (value) => {
    if (state.currentTab.value === 'series') {
      seriesState.selectedMediumIds.value = value
      return
    }
    state.selectedMediumIds.value = value
  },
})

const activeItems = computed<MediumGetListOutput[]>(() => {
  if (state.currentTab.value === 'series') {
    return seriesState.items.value
  }
  if (state.currentTab.value === 'recommend') {
    const merged = [
      ...(allItemsMap.recommend?.value ?? []),
      ...(allItemsMap.newest?.value ?? []),
      ...(allItemsMap.mostViewed?.value ?? []),
    ]
    const seen = new Set<string>()
    return merged.filter((item) => {
      if (!item?.id) return false
      if (seen.has(item.id)) return false
      seen.add(item.id)
      return true
    })
  }
  return state.items.value
})

provideMediumItemProvider({
  items: activeItems as unknown as Ref<MediumGetListOutput[]>,
  selectedMediumIds: activeSelectedMediumIds as unknown as Ref<string[]>,
})

const hasActiveSelection = computed(() => activeSelectedMediumIds.value.length)
const tabs = computed(
  (): Array<{ label: string; value: MediumViewTab }> => [
    { label: $t('page.library.tabs.recommend'), value: 'recommend' },
    { label: $t('page.library.tabs.library'), value: 'library' },
    { label: $t('page.library.tabs.series'), value: 'series' },
  ],
)

const { filterValue: mediumFilterValue } = useMediumFilterBinding(state)
const { filterValue: seriesFilterValue } = useMediumFilterBinding(seriesState)

const additionalSorts: SortField[] = [
  { label: $t('page.mediums.sort.title'), value: 'title', listSort: 1 },
]

const hasUpdate = ref(false)

const updateToastId = ref<string | number | null>(null)

const showUpdateToast = () => {
  const id = `library-update-${libraryId}`
  if (updateToastId.value === id || hasUpdate.value) return
  updateToastId.value = id
  hasUpdate.value = true
}

libraryHub.start()

// const onScanned = (id: string) => {
//   if (id === libraryId) {
//     hasUpdate.value = true
//     showUpdateToast()
//   }
// }

const onScanning = (item: {
  libraryId: string
  message: string
  title: string
  updated: boolean
}) => {
  if (item.libraryId === libraryId && item.updated) {
    hasUpdate.value = true
    showUpdateToast()
  }
}

// libraryHub.on('OnScanned', onScanned)
libraryHub.on('OnScanning', onScanning)

const onDeleted = (entityName: 'artist' | 'library' | 'tag', ids: any[]) => {
  if (entityName === 'library' && ids.includes(libraryId)) {
    void router.replace('/home')
  }
}

dataChangedHub.on('OnDeleted', onDeleted)

onUnmounted(() => {
  // libraryHub.off('OnScanned', onScanned)
  libraryHub.off('OnScanning', onScanning)
  dataChangedHub.off('OnDeleted', onDeleted)
})

// 使用 KeepAlive 按 libraryId 分键后，组件会在切换库时重建并自动加载，无需额外监听
</script>

<template>
  <LibraryMobileHeaderLeft :library="library" />

  <Page auto-content-height>
    <component
      :is="isMobile ? UScrollbar : 'div'"
      class="flex h-full min-h-0 flex-col"
      :class="[isMobile ? 'gap-2' : 'gap-6', !isMobile && 'overflow-hidden']"
      :aria-orientation="'vertical'"
      remember
      @end-reached="onScrollEnd"
    >
      <LibraryMediumToolbar
        :is-mobile="isMobile"
        :title="state.title.value"
        :tabs="tabs"
        :model-value="state.currentTab.value"
        :has-active-selection="hasActiveSelection"
        :library-state="state"
        :series-state="seriesState"
        :medium-filter-value="mediumFilterValue"
        :series-filter-value="seriesFilterValue"
        :library-sorting="state.pageRequest.sorting || ''"
        :series-sorting="seriesState.pageRequest.sorting || ''"
        :additional-sorts="additionalSorts"
        @update:model-value="state.currentTab.value = $event"
        @update:medium-filter-value="mediumFilterValue = $event"
        @update:series-filter-value="seriesFilterValue = $event"
        @update:library-sorting="state.pageRequest.sorting = $event"
        @update:series-sorting="seriesState.pageRequest.sorting = $event"
        @library-sort-change="state.updateSorting"
        @series-sort-change="seriesState.updateSorting"
      />
      <MediumContent
        v-show="state.currentTab.value === 'library'"
        ref="mediumContentRef"
        :use-scrollbar="!isMobile"
      />
      <LibraryRecommend v-show="state.currentTab.value === 'recommend'" />
      <LibrarySeries
        v-show="state.currentTab.value === 'series'"
        ref="seriesRef"
        :state="seriesState"
      />
    </component>
  </Page>
  <LibraryUpdateNotice
    :visible="hasUpdate"
    @refresh="refresh.refreshCurrentRoute()"
    @dismiss="hasUpdate = false"
  />
</template>

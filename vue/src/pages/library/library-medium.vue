<script lang="ts" setup>
import { dataChangedHub } from '@/api/signalr/data-changed'
import { libraryHub } from '@/api/signalr/library'
import type { SortField } from '@/components/mediums/types'
import Page from '@/components/Page.vue'
import { createLibraryMediumProvider } from '@/hooks/useLibraryMediumProvider'
import { useMediumFilterBinding } from '@/hooks/useMediumFilterBinding'
import {
  provideMediumAllItemProvider,
  provideMediumItemProvider,
  provideMediumProvider,
} from '@/hooks/useMediumProvider'
import { useRefresh } from '@/hooks/useRefresh'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores'

import LibraryRecommend from './library-recommend.vue'
const route = useRoute()
const router = useRouter()
const refresh = useRefresh()

const libraryId = route.params.id as string

const userStore = useUserStore()
const library = userStore.libraries.find((i) => i.id === libraryId)
if (!library) {
  throw new Error('Library not found')
}
const state = createLibraryMediumProvider(library)

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
provideMediumItemProvider({
  items: state.items,
  selectedMediumIds: state.selectedMediumIds,
})

provideMediumAllItemProvider({
  itemsMap: {
    main: state.items,
  },
})
const tabs = computed(() => [
  { label: $t('page.library.tabs.recommend'), value: 'recommend' },
  { label: $t('page.library.tabs.library'), value: 'library' },
  { label: $t('page.library.tabs.collection'), value: 'collection' },
])

const { filterValue: mediumFilterValue } = useMediumFilterBinding(state)

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
  <Page auto-content-height content-class="flex flex-col overflow-hidden gap-6">
    <div class="medium-toolbar gap-4">
      <medium-toolbar-first :title="state.title.value">
        <template #center>
          <UTabs
            v-model="state.currentTab.value"
            size="xs"
            class="min-w-80"
            :items="tabs"
          />
        </template>
      </medium-toolbar-first>
      <div class="w-full">
        <medium-toolbar-second
          v-if="
            state.currentTab.value === 'library' &&
            state.selectedMediumIds.value.length === 0
          "
        >
          <template #left>
            <div class="flex flex-row items-center gap-4">
              <medium-filter-dropdown
                v-model:model-value="mediumFilterValue"
                :disabled="state.loading.value"
              />

              <medium-sort-dropdown
                v-model:model-value="state.pageRequest.sorting"
                :additional-sort-field-list="additionalSorts"
                @change="state.updateSorting"
              />
              <UBadge variant="subtle">{{ state.totalCount || 0 }}</UBadge>
            </div>
          </template>
        </medium-toolbar-second>
        <medium-toolbar-second-select
          v-else-if="state.selectedMediumIds.value.length > 0"
          :show-selected-all-btn="true"
        />
      </div>
    </div>
    <MediumContent v-show="state.currentTab.value === 'library'" />
    <LibraryRecommend v-show="state.currentTab.value === 'recommend'" />
  </Page>
  <div v-if="hasUpdate" class="fixed right-4 bottom-4 z-50">
    <UCard>
      <div class="flex items-start gap-3">
        <UIcon name="i-lucide-refresh-cw" class="text-info text-lg" />
        <div class="flex-1">
          <div class="text-sm font-medium">
            {{ $t('page.library.messages.updatedTitle') }}
          </div>
          <div class="text-muted-foreground text-sm">
            {{ $t('page.library.messages.updatedDescription') }}
          </div>
        </div>
        <div class="flex gap-2">
          <UButton
            variant="ghost"
            size="sm"
            @click="refresh.refreshCurrentRoute()"
          >
            <template #leading>
              <UIcon name="i-lucide-refresh-cw" />
            </template>
            {{ $t('common.refresh') }}
          </UButton>
          <UButton variant="ghost" size="sm" @click="hasUpdate = false">
            <template #leading>
              <UIcon name="i-lucide-x" />
            </template>
            {{ $t('common.cancel') }}
          </UButton>
        </div>
      </div>
    </UCard>
  </div>
</template>

<script lang="ts" setup>
import type { SortField } from '@/components/mediums/types'
import Page from '@/components/Page.vue'
import { createLibraryMediumProvider } from '@/hooks/useLibraryMediumProvider'
import { useMediumFilterBinding } from '@/hooks/useMediumFilterBinding'
import {
  provideMediumAllItemProvider,
  provideMediumItemProvider,
  provideMediumProvider,
} from '@/hooks/useMediumProvider'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores'

import LibraryRecommend from './library-recommend.vue'
const route = useRoute()
const router = useRouter()

const libraryId = route.params.id

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
</template>

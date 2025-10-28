<script lang="ts" setup>
import type { SortField } from '#/components/mediums/types'

import { computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { Page } from '@vben/common-ui'
import { useUserStore } from '@vben/stores'

import { createLibraryMediumProvider } from '#/hooks/useLibraryMediumProvider'
import { useMediumFilterBinding } from '#/hooks/useMediumFilterBinding'
import {
  provideMediumAllItemProvider,
  provideMediumItemProvider,
  provideMediumProvider,
} from '#/hooks/useMediumProvider'
import { $t } from '#/locales'

import LibraryRecommend from './library-recommend.vue'

interface SegmentedOption {
  label: string
  value: string
  disabled?: boolean
}

const route = useRoute()
const router = useRouter()

const MEDIUM_VIEW_TABS = ['collection', 'library', 'recommend'] as const
type MediumViewTabValue = (typeof MEDIUM_VIEW_TABS)[number]

const resolveLibraryId = () => {
  if (typeof route.name === 'string' && route.name.startsWith('library_')) {
    return route.name.slice('library_'.length)
  }
  return route.path.replace('/library/', '')
}

const libraryId = resolveLibraryId()
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

const resolveMediumTab = (value: unknown): MediumViewTabValue | null => {
  const text = normalizeQueryString(value)
  if (!text) return null
  return MEDIUM_VIEW_TABS.includes(text as MediumViewTabValue)
    ? (text as MediumViewTabValue)
    : null
}

const initialTab = (() => {
  const tab = resolveMediumTab(route.query.tab)
  if (tab) return tab
  return 'library'
})()

state.currentTab.value = initialTab

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

watch(
  () => route.query.tab,
  (value) => {
    const normalized = resolveMediumTab(value)
    if (normalized && state.currentTab.value !== normalized) {
      state.currentTab.value = normalized
    }
  },
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
const tabs = computed<SegmentedOption[]>(() => [
  { label: $t('page.library.tabs.recommend'), value: 'recommend' },
  { label: $t('page.library.tabs.library'), value: 'library' },
  { label: $t('page.library.tabs.collection'), value: 'collection' },
])

const { filterValue: mediumFilterValue } = useMediumFilterBinding(state)

const additionalSorts: SortField[] = [
  { label: $t('page.mediums.sort.title'), value: 'title', listSort: 1 },
]
</script>

<template>
  <Page auto-content-height content-class="flex flex-col overflow-hidden gap-6">
    <div class="medium-toolbar gap-4">
      <medium-toolbar-first :title="state.title.value">
        <template #center>
          <el-segmented
            class="min-w-80"
            v-model="state.currentTab.value"
            :options="tabs"
            block
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
                @change="state.updateSorting"
                :additional-sort-field-list="additionalSorts"
              />
              <el-tag type="primary">{{ state.totalCount || 0 }}</el-tag>
            </div>
          </template>
        </medium-toolbar-second>
        <medium-toolbar-second-select
          :show-selected-all-btn="true"
          v-else-if="state.selectedMediumIds.value.length > 0"
        />
      </div>
    </div>
    <MediumContent v-show="state.currentTab.value === 'library'" />
    <LibraryRecommend v-show="state.currentTab.value === 'recommend'" />
  </Page>
</template>

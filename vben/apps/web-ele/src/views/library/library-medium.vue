<script lang="ts" setup>
import { watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { Page } from '@vben/common-ui'
import { useUserStore } from '@vben/stores'

import { createLibraryMediumProvider } from '#/hooks/useLibraryMediumProvider'
import {
  provideMediumAllItemProvider,
  provideMediumItemProvider,
  provideMediumProvider,
} from '#/hooks/useMediumProvider'
import { $t } from '#/locales'
import { useMediumStore } from '#/store'

import LibraryRecommend from './library-recommend.vue'

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
const mediumStore = useMediumStore()
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
  return mediumStore.libraryTabs[library.id] ?? 'library'
})()

state.currentTab.value = initialTab

watch(
  () => state.currentTab.value,
  (tab) => {
    mediumStore.setLibraryTab(library.id, tab)
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
</script>

<template>
  <Page auto-content-height content-class="flex flex-col overflow-hidden gap-6">
    <div class="medium-toolbar">
      <medium-toolbar-first :title="state.title.value">
        <template #center>
          <el-tabs v-model="state.currentTab.value">
            <el-tab-pane name="recommend">
              <template #label>
                <div class="tab-pane">
                  {{ $t('page.library.tabs.recommend') }}
                </div>
              </template>
            </el-tab-pane>
            <el-tab-pane name="library">
              <template #label>
                <div class="tab-pane">
                  {{ $t('page.library.tabs.library') }}
                </div>
              </template>
            </el-tab-pane>
            <el-tab-pane name="collection">
              <template #label>
                <div class="tab-pane">
                  {{ $t('page.library.tabs.collection') }}
                </div>
              </template>
            </el-tab-pane>
          </el-tabs>
        </template>
      </medium-toolbar-first>
      <div class="w-full">
        <medium-toolbar-second
          v-if="
            state.currentTab.value === 'library' &&
            state.selectedMediumIds.value.length === 0
          "
        />
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

<style scoped>
.tab-pane {
  @apply w-20 text-center;
}
</style>

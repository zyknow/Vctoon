<script lang="ts" setup>
import { Page } from '@vben/common-ui'
import { useUserStore } from '@vben/stores'

import { createLibraryMediumProvider } from '#/hooks/useLibraryMediumProvider'
import {
  provideMediumAllItemProvider,
  provideMediumItemProvider,
  provideMediumProvider,
} from '#/hooks/useMediumProvider'
import { $t } from '#/locales'

import LibraryRecommend from './library-recommend.vue'

// 当前库 ID（从路径中解析）
const libraryId = location.pathname.replace('/library/', '')
const userStore = useUserStore()
const library = userStore.libraries.find((i) => i.id === libraryId)
if (!library) {
  throw new Error('Library not found')
}
const state = createLibraryMediumProvider(library)
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

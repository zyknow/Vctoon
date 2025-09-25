<script lang="ts" setup>
import { Page } from '@vben/common-ui'
import { useUserStore } from '@vben/stores'

import { createLibraryMediumProvider } from '#/hooks/useLibraryMediumProvider'
import {
  provideMediumItemProvider,
  provideMediumProvider,
} from '#/hooks/useMediumProvider'

import LibraryRecommend from './library-recommend.vue'

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
</script>

<template>
  <Page auto-content-height content-class="flex flex-col overflow-hidden gap-6">
    <div class="medium-toolbar">
      <medium-toolbar-first :title="state.title.value">
        <template #center>
          <el-tabs v-model="state.currentTab.value">
            <el-tab-pane label="Recommend" name="recommend" />
            <el-tab-pane label="Library" name="library" />
            <el-tab-pane label="Collection" name="collection" />
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
          v-else-if="state.selectedMediumIds.value.length > 0"
        />
      </div>
    </div>
    <MediumContent v-show="state.currentTab.value === 'library'" />
    <LibraryRecommend v-show="state.currentTab.value === 'recommend'" />
  </Page>
</template>

<style scoped></style>

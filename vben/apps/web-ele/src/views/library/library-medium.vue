<script lang="ts" setup>
import { libraryApi } from '@vben/api'
import { Page } from '@vben/common-ui'

import { createLibraryMediumProvider } from '#/hooks/useLibraryMediumProvider'
import {
  provideMediumItemProvider,
  provideMediumProvider,
} from '#/hooks/useMediumProvider'

import LibraryRecommend from './library-recommend.vue'

const libraryId = location.pathname.replace('/library/', '')
const library = await libraryApi.getById(libraryId)
const state = createLibraryMediumProvider(library)
provideMediumProvider(state)
provideMediumItemProvider({
  items: state.items,
  selectedMediumIds: state.selectedMediumIds,
})
</script>

<template>
  <Page auto-content-height content-class="flex flex-col overflow-hidden gap-6">
    <MediumToolbar />
    <MediumContent v-show="state.currentTab.value === 'library'" />
    <LibraryRecommend v-show="state.currentTab.value === 'recommend'" />
  </Page>
</template>

<style scoped></style>

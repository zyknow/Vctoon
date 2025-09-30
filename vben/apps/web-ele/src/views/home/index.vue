<script setup lang="ts">
import type { Ref } from 'vue'

import type { MediumGetListOutput } from '@vben/api'

import type { UseRecommendMediumProviderOptions } from '#/hooks/useRecommendProvider'

import { ref } from 'vue'

import { MediumType, ReadingProgressType } from '@vben/api'
import { Page } from '@vben/common-ui'
import { $t } from '@vben/locales'

import {
  provideMediumAllItemProvider,
  provideMediumItemProvider,
} from '#/hooks/useMediumProvider'
import { createRecommendMediumProvider } from '#/hooks/useRecommendProvider'

const commonOptions: UseRecommendMediumProviderOptions = {
  mediumTypes: [MediumType.Comic, MediumType.Video],
  pageRequest: {},
  autoLoad: true,
}

const items: Ref<MediumGetListOutput[]> = ref([])
const selectedMediumIds: Ref<string[]> = ref([])

provideMediumItemProvider({
  items,
  selectedMediumIds,
})

const lastReading = createRecommendMediumProvider({
  title: $t('page.home.continueWatching'),
  ...commonOptions,
  pageRequest: {
    ...commonOptions.pageRequest,
    sorting: 'readingLastTime desc',
    readingProgressType: ReadingProgressType.InProgress,
  },
})

const newest = createRecommendMediumProvider({
  title: $t('page.home.newest'),
  ...commonOptions,
  pageRequest: {
    ...commonOptions.pageRequest,
    sorting: 'creationTime desc',
    createdInDays: 30,
  },
})

const mostViewed = createRecommendMediumProvider({
  title: $t('page.home.mostViewed'),
  ...commonOptions,
  pageRequest: {
    ...commonOptions.pageRequest,
    sorting: 'readCount desc',
    hasReadCount: true,
  },
})

provideMediumAllItemProvider({
  itemsMap: {
    main: items,
    lastReading: lastReading.items,
    newest: newest.items,
    mostViewed: mostViewed.items,
  },
})
</script>

<template>
  <Page auto-content-height content-class="flex flex-col overflow-hidden gap-6">
    <medium-toolbar-first :title="$t('page.home.title')" />
    <MediumToolbarSecondSelect v-if="selectedMediumIds.length > 0" />
    <div class="library-recommend">
      <MediumRecommendationSection
        v-if="lastReading.items.value.length > 0"
        :data="lastReading"
      />
      <MediumRecommendationSection
        v-if="newest.items.value.length > 0"
        :data="newest"
      />
      <MediumRecommendationSection
        v-if="mostViewed.items.value.length > 0"
        :data="mostViewed"
      />
    </div>
  </Page>
</template>
<style lang="scss" scoped></style>

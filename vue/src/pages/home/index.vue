<script setup lang="ts">
import { ref } from 'vue'
import type { Ref } from 'vue'

import { ReadingProgressType } from '@/api/http/base/medium-base'
import { MediumType } from '@/api/http/library'
import { MediumGetListOutput } from '@/api/http/typing'
import {
  provideMediumAllItemProvider,
  provideMediumItemProvider,
} from '@/hooks/useMediumProvider'
import {
  createRecommendMediumProvider,
  UseRecommendMediumProviderOptions,
} from '@/hooks/useRecommendProvider'
import { $t } from '@/locales/i18n'

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
  <Page auto-content-height content-class="flex flex-col overflow-hidden gap-4">
    <medium-toolbar-first :title="$t('page.home.title')" />
    <MediumToolbarSecondSelect v-if="selectedMediumIds.length > 0" />
    <UScrollbar
      class="min-h-0 flex-1"
      aria-orientation="vertical"
      view-class="flex flex-col gap-6 pb-10"
    >
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
    </UScrollbar>
  </Page>
</template>
<style lang="scss" scoped></style>

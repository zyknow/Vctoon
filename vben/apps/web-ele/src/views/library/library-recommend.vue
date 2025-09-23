<script setup lang="ts">
import type { LibraryMediumProvider } from '#/hooks/useLibraryMediumProvider'
import type { UseRecommendMediumProviderOptions } from '#/hooks/useRecommendProvider'

import { MediumType } from '@vben/api'
import { $t } from '@vben/locales'

import { useInjectedMediumProvider } from '#/hooks/useMediumProvider'
import { createRecommendMediumProvider } from '#/hooks/useRecommendProvider'

const { library } = useInjectedMediumProvider() as LibraryMediumProvider

const commonOptions: UseRecommendMediumProviderOptions = {
  mediumTypes: [library.mediumType],
  pageRequest: {
    libraryId: library.id,
  },
  autoLoad: true,
}

const mediumTypeTitleMap: Record<MediumType, string> = {
  [MediumType.Comic]: $t('page.library.mediumType.comic'),
  [MediumType.Video]: $t('page.library.mediumType.video'),
}

const lastReading = createRecommendMediumProvider({
  title: $t('page.library.recommend.continueWatching', {
    mediumType: mediumTypeTitleMap[library.mediumType],
  }),
  ...commonOptions,
  pageRequest: {
    ...commonOptions.pageRequest,
    sorting: 'readingLastTime desc',
    hasReadingProgress: true,
  },
})

const newest = createRecommendMediumProvider({
  title: $t('page.library.recommend.newest', {
    mediumType: mediumTypeTitleMap[library.mediumType],
  }),
  ...commonOptions,
  pageRequest: {
    ...commonOptions.pageRequest,
    sorting: 'creationTime desc',
    createdInDays: 30,
  },
})

const mostViewed = createRecommendMediumProvider({
  title: $t('page.library.recommend.mostViewed', {
    mediumType: mediumTypeTitleMap[library.mediumType],
  }),
  ...commonOptions,
  pageRequest: {
    ...commonOptions.pageRequest,
    sorting: 'readCount desc',
    hasReadCount: true,
  },
})
</script>

<template>
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
</template>
<style lang="scss" scoped></style>

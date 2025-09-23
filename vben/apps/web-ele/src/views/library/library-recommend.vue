<script setup lang="ts">
import type { LibraryMediumProvider } from '#/hooks/useLibraryMediumProvider'
import type { UseRecommendMediumProviderOptions } from '#/hooks/useRecommendProvider'

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

const lastReading = createRecommendMediumProvider({
  title: '继续观看',
  ...commonOptions,
  pageRequest: {
    ...commonOptions.pageRequest,
    sorting: 'readingLastTime desc',
  },
})

const newest = createRecommendMediumProvider({
  title: '最新添加',
  ...commonOptions,
  pageRequest: {
    ...commonOptions.pageRequest,
    sorting: 'creationTime desc',
  },
})

const mostViewed = createRecommendMediumProvider({
  title: '最多观看',
  ...commonOptions,
  pageRequest: {
    ...commonOptions.pageRequest,
    sorting: 'readCount desc',
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

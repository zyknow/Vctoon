<script setup lang="ts">
import type { Comic, Video } from '@vben/api'

import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'

import { comicApi, MediumType, videoApi } from '@vben/api'
import { Page } from '@vben/common-ui'

import { $t } from '#/locales'

import MediumComicDetail from './components/comic-detail.vue'
import MediumVideoDetail from './components/video-detail.vue'

const route = useRoute()

const mediumId = computed(() => route.params.mediumId as string)
const typeParam = computed(() => (route.params.type as string) ?? 'comic')

const mediumType = computed<MediumType>(() =>
  typeParam.value === 'video' ? MediumType.Video : MediumType.Comic,
)

const mediumLoading = ref(false)
const mediumData = ref<Comic | null | Video>(null)

const loadMediumDetail = async () => {
  if (!mediumId.value) {
    mediumData.value = null
    return
  }
  mediumLoading.value = true
  mediumData.value = null
  try {
    mediumData.value =
      mediumType.value === MediumType.Comic
        ? await comicApi.getById(mediumId.value)
        : await videoApi.getById(mediumId.value)
  } finally {
    mediumLoading.value = false
  }
}

watch(
  [mediumId, mediumType],
  () => {
    void loadMediumDetail()
  },
  { immediate: true },
)

const comicData = computed(() => {
  if (mediumType.value !== MediumType.Comic) {
    return null
  }
  return (mediumData.value as Comic | null) ?? null
})

const videoData = computed(() => {
  if (mediumType.value !== MediumType.Video) {
    return null
  }
  return (mediumData.value as null | Video) ?? null
})
</script>

<template>
  <Page auto-content-height content-class="flex flex-col gap-8 pb-12">
    <template v-if="mediumType === MediumType.Comic">
      <MediumComicDetail
        v-if="comicData"
        :loading="mediumLoading"
        :medium="comicData"
        :medium-id="mediumId"
      />
      <div
        v-else-if="mediumLoading"
        class="text-muted-foreground py-20 text-center"
      >
        {{ $t('common.loading') }}
      </div>
      <div v-else class="text-muted-foreground py-20 text-center">
        {{ $t('common.noData') }}
      </div>
    </template>

    <template v-else-if="mediumType === MediumType.Video">
      <MediumVideoDetail
        v-if="videoData"
        :loading="mediumLoading"
        :medium="videoData"
      />
      <div
        v-else-if="mediumLoading"
        class="text-muted-foreground py-20 text-center"
      >
        {{ $t('common.loading') }}
      </div>
      <div v-else class="text-muted-foreground py-20 text-center">
        {{ $t('common.noData') }}
      </div>
    </template>
  </Page>
</template>

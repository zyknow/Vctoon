<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'

import { comicApi } from '@/api/http/comic'
import type { Comic } from '@/api/http/comic/typing'
import { MediumType } from '@/api/http/library/typing'
import { videoApi } from '@/api/http/video'
import type { Video } from '@/api/http/video/typing'
import Page from '@/components/Page.vue'
import { $t } from '@/locales/i18n'

import MediumComicDetail from './components/ComicDetail.vue'
import MediumVideoDetail from './components/VideoDetail.vue'

const route = useRoute()

const mediumId = computed(() => route.params.mediumId as string)
const typeParam = computed(() => (route.params.type as string) ?? 'comic')

const mediumType = computed<MediumType>(() =>
  typeParam.value === 'video' ? MediumType.Video : MediumType.Comic,
)

const mediumLoading = ref(false)
const mediumData = ref<Comic | null | Video>(null)
const loadError = ref<string | null>(null)

const loadMediumDetail = async () => {
  if (!mediumId.value) {
    mediumData.value = null
    return
  }
  mediumLoading.value = true
  mediumData.value = null
  loadError.value = null
  try {
    mediumData.value =
      mediumType.value === MediumType.Comic
        ? await comicApi.getById(mediumId.value)
        : await videoApi.getById(mediumId.value)
  } catch (error) {
    const message = (error as Error)?.message || $t('common.operationFailed')
    loadError.value = message
    const toast = useToast()
    toast.add({
      title: $t('common.error'),
      description: message,
      color: 'error',
      icon: 'i-heroicons-exclamation-circle',
    })
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
  <MainLayoutProvider mobile-only>
    <template #header-left>
      <UButton color="neutral" variant="ghost" @click="$router.back()">
        <template #leading>
          <UIcon name="i-heroicons-arrow-left" />
        </template>
        {{ $t('page.mediums.detail.back') }}
      </UButton>
    </template>

    <template #footer></template>
  </MainLayoutProvider>

  <Page content-class="flex flex-col gap-8 pb-12">
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
      <div v-else-if="loadError" class="text-destructive py-20 text-center">
        {{ loadError }}
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
      <div v-else-if="loadError" class="text-destructive py-20 text-center">
        {{ loadError }}
      </div>
      <div v-else class="text-muted-foreground py-20 text-center">
        {{ $t('common.noData') }}
      </div>
    </template>
  </Page>
</template>

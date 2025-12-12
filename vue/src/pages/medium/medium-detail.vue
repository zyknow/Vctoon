<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'

import { MediumType } from '@/api/http/library/typing'
import { mediumApi } from '@/api/http/medium'
import type { Medium } from '@/api/http/medium/typing'
import Page from '@/components/Page.vue'
import { $t } from '@/locales/i18n'

import MediumComicDetail from './components/ComicDetail.vue'
import MediumSeriesDetail from './components/SeriesDetail.vue'
import MediumVideoDetail from './components/VideoDetail.vue'

const route = useRoute()

const mediumId = computed(() => route.params.mediumId as string)

const mediumLoading = ref(false)
const mediumData = ref<Medium | null>(null)
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
    mediumData.value = await mediumApi.getById(mediumId.value)
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
  [mediumId],
  () => {
    void loadMediumDetail()
  },
  { immediate: true },
)
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
    <template v-if="mediumData?.isSeries">
      <MediumSeriesDetail
        v-if="mediumData"
        :loading="mediumLoading"
        :medium="mediumData"
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

    <template v-else-if="mediumData?.mediumType === MediumType.Comic">
      <MediumComicDetail
        v-if="mediumData"
        :loading="mediumLoading"
        :medium="mediumData"
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

    <template v-else-if="mediumData?.mediumType === MediumType.Video">
      <MediumVideoDetail
        v-if="mediumData"
        :loading="mediumLoading"
        :medium="mediumData"
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

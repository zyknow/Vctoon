<script setup lang="ts">
import { useRouter } from 'vue-router'

import type { Video } from '@/api/http/video/typing'
import MediumToolbarFirst from '@/components/mediums/MediumToolbarFirst.vue'
import { $t } from '@/locales/i18n'
import { formatMediumDateTime, formatMediumProgress } from '@/utils/medium'

const props = defineProps<{
  loading: boolean
  medium: Video
}>()

const router = useRouter()

const medium = toRef(props, 'medium')
const loading = toRef(props, 'loading')

const mediumTitle = computed(() => medium.value.title ?? '')
const creationTimeText = computed(() => {
  const value = medium.value.creationTime
  if (!value) return '--'
  return formatMediumDateTime(value)
})

const descriptionText = computed(() => medium.value.description?.trim() ?? '')
const hasTags = computed(() => (medium.value.tags?.length ?? 0) > 0)
const hasArtists = computed(() => (medium.value.artists?.length ?? 0) > 0)

const detailTopGridClasses = computed(() => {
  const classes = ['detail-top', 'grid', 'gap-8', 'items-start']
  classes.push('grid-cols-1')
  return classes
})

const readCountText = computed(() => medium.value.readCount ?? 0)
const progressText = computed(() =>
  formatMediumProgress(medium.value.readingProgress),
)

const resolutionText = computed(() => {
  if (!medium.value.width || !medium.value.height) return undefined
  return `${medium.value.width} × ${medium.value.height}`
})

const hasTechnicalInfo = computed(() => {
  return (
    Boolean(resolutionText.value) ||
    Boolean(medium.value.framerate) ||
    Boolean(medium.value.codec) ||
    Boolean(medium.value.bitrate)
  )
})

const isWide = ref(false)

const goBack = () => {
  router.back()
}
</script>

<template>
  <MediumToolbarFirst :title="mediumTitle">
    <template #left>
      <div class="flex items-center gap-3">
        <UButton size="sm" variant="outline" @click="goBack">
          <template #leading>
            <UIcon name="i-heroicons-arrow-left" />
          </template>
          {{ $t('page.mediums.detail.back') }}
        </UButton>
      </div>
    </template>
  </MediumToolbarFirst>

  <div class="flex min-h-0 flex-1 flex-col overflow-auto">
    <div v-if="loading" class="text-muted-foreground py-20 text-center">
      {{ $t('common.loading') }}
    </div>
    <div v-else class="flex flex-1 flex-col">
      <div :class="detailTopGridClasses">
        <div class="flex flex-col gap-8">
          <div
            class="relative aspect-[16/9] w-full overflow-hidden rounded-lg bg-black shadow-lg"
            :class="isWide ? '2xl:aspect-[18/9]' : ''"
          >
            <VideoBrowser
              :video="medium"
              prefer-web
              :on-toggle-wide="(v) => (isWide = v)"
            />
          </div>

          <div class="space-y-3">
            <h1 class="text-3xl leading-tight font-bold lg:text-4xl">
              {{ mediumTitle || $t('page.mediums.detail.untitled') }}
            </h1>
            <div class="flex flex-wrap items-center gap-3 text-sm">
              <UBadge color="success">
                {{ $t('page.mediums.info.video') }}
              </UBadge>
              <span class="text-muted-foreground">
                {{ creationTimeText.split(' ')[0] }}
              </span>
              <span class="text-muted-foreground">
                {{ medium.duration }}
              </span>
              <div class="flex items-center gap-2">
                <div class="text-muted-foreground">|</div>
                <div class="text-muted-foreground">
                  {{ $t('page.mediums.info.readCount') }}:
                  {{ readCountText }}
                </div>
              </div>
            </div>
          </div>

          <div class="space-y-4">
            <div>
              <div
                class="text-muted-foreground mb-2 text-sm font-medium tracking-wide uppercase"
              >
                {{ $t('page.mediums.info.readingProgress') }}
              </div>
              <div class="text-2xl font-bold">
                {{ progressText }}
              </div>
            </div>

            <div v-if="descriptionText" class="space-y-2">
              <div
                class="text-muted-foreground text-sm font-medium tracking-wide uppercase"
              >
                {{ $t('page.mediums.info.description') }}
              </div>
              <p class="leading-relaxed">
                {{ descriptionText }}
              </p>
            </div>
          </div>

          <div class="space-y-4">
            <div v-if="hasTags">
              <div
                class="text-muted-foreground mb-2 text-sm font-medium tracking-wide uppercase"
              >
                {{ $t('page.mediums.info.tags') }}
              </div>
              <div class="flex flex-wrap gap-2">
                <UBadge v-for="tag in medium.tags" :key="tag.id" size="sm">
                  {{ tag.name }}
                </UBadge>
              </div>
            </div>

            <div v-if="hasArtists">
              <div
                class="text-muted-foreground mb-2 text-sm font-medium tracking-wide uppercase"
              >
                {{ $t('page.mediums.info.artists') }}
              </div>
              <div class="flex flex-wrap gap-2">
                <UBadge
                  v-for="artist in medium.artists"
                  :key="artist.id"
                  size="sm"
                  color="success"
                >
                  {{ artist.name }}
                </UBadge>
              </div>
            </div>

            <div
              v-if="hasTechnicalInfo"
              class="border-border space-y-3 rounded-lg border p-4"
            >
              <div
                class="text-muted-foreground text-sm font-medium tracking-wide uppercase"
              >
                {{ $t('page.mediums.info.technicalInfo') }}
              </div>
              <div class="grid gap-3 text-sm sm:grid-cols-2">
                <div v-if="resolutionText">
                  <div class="text-muted-foreground text-xs">
                    {{ $t('page.mediums.info.resolution') }}
                  </div>
                  <div class="mt-1 font-medium">
                    {{ resolutionText }}
                  </div>
                </div>
                <div v-if="medium.framerate">
                  <div class="text-muted-foreground text-xs">
                    {{ $t('page.mediums.info.framerate') }}
                  </div>
                  <div class="mt-1 font-medium">{{ medium.framerate }} fps</div>
                </div>
                <div v-if="medium.codec">
                  <div class="text-muted-foreground text-xs">
                    {{ $t('page.mediums.info.codec') }}
                  </div>
                  <div class="mt-1 font-medium">
                    {{ medium.codec }}
                  </div>
                </div>
                <div v-if="medium.bitrate">
                  <div class="text-muted-foreground text-xs">
                    {{ $t('page.mediums.info.bitrate') }}
                  </div>
                  <div class="mt-1 font-medium">
                    {{ medium.bitrate }}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

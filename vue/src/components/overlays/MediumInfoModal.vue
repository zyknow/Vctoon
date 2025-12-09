<script setup lang="ts">
import { MediumType } from '@/api/http/library/typing'
import { Medium, mediumApi } from '@/api/http/medium'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores'
import { formatMediumDateTime } from '@/utils/medium'

defineOptions({
  name: 'MediumInfoModal',
})

interface Props {
  mediumId: string
  mediumType: MediumType
}

const props = defineProps<Props>()

defineEmits<{
  close: []
}>()

const { isMobile } = useIsMobile()
const userStore = useUserStore()

const loading = ref(true)
const medium = ref<Medium | undefined>()
const comicImagesCount = ref<number | undefined>()
const numberFormatter = new Intl.NumberFormat()

const coverUrl = computed(() => {
  if (!medium.value?.cover) return undefined
  return mediumApi.getCoverUrl(medium.value.cover)
})
const descriptionText = computed(() => medium.value?.description?.trim())

// 类型守卫
const isComic = computed(() => props.mediumType === MediumType.Comic)
const isVideo = computed(() => props.mediumType === MediumType.Video)
const comicData = computed(() =>
  isComic.value ? medium.value : undefined,
)
const videoData = computed(() =>
  isVideo.value ? medium.value : undefined,
)

const readingProgressPercent = computed<null | number>(() => {
  const progress = medium.value?.readingProgress
  if (typeof progress !== 'number') return null
  const clamped = Math.max(0, Math.min(1, progress))
  return Math.round(clamped * 100)
})

const readingProgressStatus = computed<'success' | 'warning' | 'primary'>(
  () => {
    const percent = readingProgressPercent.value

    if (percent === null) {
      return 'primary'
    }

    if (percent >= 100) {
      return 'success'
    }

    if (percent > 0) {
      return 'warning'
    }

    return 'primary'
  },
)

const readCountText = computed(() =>
  numberFormatter.format(medium.value?.readCount ?? 0),
)

const libraryName = computed(() => {
  if (!medium.value?.libraryId) return undefined
  return userStore.libraries.find((lib) => lib.id === medium.value?.libraryId)
    ?.name
})

const tags = computed(() => medium.value?.tags ?? [])
const artists = computed(() => medium.value?.artists ?? [])
const hasTags = computed(() => tags.value.length > 0)
const hasArtists = computed(() => artists.value.length > 0)

const hasBasicInfo = computed(() =>
  Boolean(
    medium.value?.creationTime ||
      medium.value?.lastModificationTime ||
      libraryName.value,
  ),
)

const hasVideoInfo = computed(() =>
  Boolean(
    isVideo.value &&
      (videoData.value?.videoDetail?.duration ||
        (videoData.value?.videoDetail?.width && videoData.value?.videoDetail?.height) ||
        videoData.value?.videoDetail?.framerate ||
        videoData.value?.videoDetail?.bitrate ||
        videoData.value?.videoDetail?.codec ||
        videoData.value?.videoDetail?.ratio ||
        videoData.value?.videoDetail?.path),
  ),
)

const hasComicInfo = computed(() =>
  Boolean(isComic.value),
)

const hasReadingInfo = computed(() =>
  Boolean(
    medium.value &&
      (typeof medium.value.readCount === 'number' ||
        medium.value.readingLastTime),
  ),
)

const showReadingSummary = computed(() =>
  Boolean(
    medium.value &&
      (readingProgressPercent.value !== null ||
        typeof medium.value.readCount === 'number' ||
        medium.value.readingLastTime),
  ),
)

onMounted(async () => {
  loading.value = true
  try {
    await userStore.reloadLibraries()
    medium.value = await mediumApi.getById(props.mediumId)
    if (medium.value && props.mediumType === MediumType.Comic) {
      const images = await mediumApi.getComicImageList(props.mediumId)
      comicImagesCount.value = images.length
    }
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <UModal :fullscreen="isMobile">
    <template #body>
      <div
        class="max-h-[80vh] space-y-4 overflow-auto p-4"
        :class="isMobile ? 'text-sm' : 'text-base'"
      >
        <div v-if="loading" class="text-muted-foreground py-8 text-center">
          <div>{{ $t('common.loading') }}</div>
        </div>

        <template v-else-if="medium">
          <section class="flex flex-col gap-4 sm:flex-row sm:items-start">
            <div class="cover-wrapper">
              <img
                v-if="coverUrl"
                :src="coverUrl"
                alt="cover"
                class="h-full w-full object-cover"
                loading="lazy"
              />
              <div
                v-else
                class="empty-cover flex h-full w-full items-center justify-center"
              >
                <span class="text-4xl">📄</span>
              </div>
            </div>

            <div class="flex-1 space-y-4">
              <div class="space-y-2">
                <h3 class="line-clamp-2 text-lg font-semibold sm:text-xl">
                  {{ medium.title || $t('page.mediums.info.title') }}
                </h3>
                <div
                  class="text-muted-foreground flex flex-wrap items-center gap-2 text-xs sm:text-sm"
                >
                  <UBadge size="sm" color="primary">
                    {{
                      isComic
                        ? $t('page.mediums.info.comic')
                        : $t('page.mediums.info.video')
                    }}
                  </UBadge>
                  <span v-if="typeof medium.readCount === 'number'">
                    {{ $t('page.mediums.info.readCount') }} ·
                    {{ readCountText }}
                  </span>
                  <span v-if="medium.readingLastTime">
                    {{ $t('page.mediums.info.readingLastTime') }} ·
                    {{ formatMediumDateTime(medium.readingLastTime) }}
                  </span>
                  <span v-if="medium.creationTime">
                    {{ $t('page.mediums.info.creationTime') }} ·
                    {{ formatMediumDateTime(medium.creationTime) }}
                  </span>
                </div>
              </div>

              <div v-if="showReadingSummary" class="reading-overview">
                <div
                  class="flex items-center justify-between text-sm font-medium"
                >
                  <span>{{ $t('page.mediums.info.readingProgress') }}</span>
                </div>
                <UProgress
                  v-if="readingProgressPercent !== null"
                  v-model="readingProgressPercent"
                  status
                  :color="readingProgressStatus"
                  size="md"
                />
                <div
                  v-else
                  class="text-muted-foreground pt-1 text-xs sm:text-sm"
                >
                  {{ $t('page.mediums.info.never') }}
                </div>
              </div>

              <div v-if="descriptionText" class="description-block">
                <div class="text-sm font-medium">
                  {{ $t('page.mediums.info.description') }}
                </div>
                <p class="text-muted-foreground whitespace-pre-line">
                  {{ descriptionText }}
                </p>
              </div>
            </div>
          </section>

          <div class="space-y-4">
            <UCard v-if="hasBasicInfo">
              <template #header>
                <div class="card-title">
                  {{ $t('page.mediums.info.basicInfo') }}
                </div>
              </template>
              <div class="space-y-2 text-sm">
                <div
                  v-if="medium.creationTime"
                  class="flex justify-between border-b pb-2"
                >
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.creationTime')
                  }}</span>
                  <span>{{ formatMediumDateTime(medium.creationTime) }}</span>
                </div>
                <div
                  v-if="medium.lastModificationTime"
                  class="flex justify-between border-b pb-2"
                >
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.lastModificationTime')
                  }}</span>
                  <span>{{
                    formatMediumDateTime(medium.lastModificationTime)
                  }}</span>
                </div>
                <div v-if="libraryName" class="flex justify-between pb-2">
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.library')
                  }}</span>
                  <span>{{ libraryName }}</span>
                </div>
              </div>
            </UCard>

            <UCard v-if="hasVideoInfo && videoData">
              <template #header>
                <div class="card-title">
                  {{ $t('page.mediums.info.technicalInfo') }}
                </div>
              </template>
              <div class="space-y-2 text-sm">
                <div
                  v-if="videoData.videoDetail?.duration"
                  class="flex justify-between border-b pb-2"
                >
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.duration')
                  }}</span>
                  <span>{{ videoData.videoDetail?.duration }}</span>
                </div>
                <div
                  v-if="videoData.videoDetail?.width && videoData.videoDetail?.height"
                  class="flex justify-between border-b pb-2"
                >
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.resolution')
                  }}</span>
                  <span>{{ videoData.videoDetail?.width }} × {{ videoData.videoDetail?.height }}</span>
                </div>
                <div
                  v-if="videoData.videoDetail?.framerate"
                  class="flex justify-between border-b pb-2"
                >
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.framerate')
                  }}</span>
                  <span>{{ videoData.videoDetail?.framerate }} fps</span>
                </div>
                <div
                  v-if="videoData.videoDetail?.bitrate"
                  class="flex justify-between border-b pb-2"
                >
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.bitrate')
                  }}</span>
                  <span>{{ Math.round(videoData.videoDetail?.bitrate / 1000) }} kbps</span>
                </div>
                <div
                  v-if="videoData.videoDetail?.codec"
                  class="flex justify-between border-b pb-2"
                >
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.codec')
                  }}</span>
                  <span>{{ videoData.videoDetail?.codec }}</span>
                </div>
                <div
                  v-if="videoData.videoDetail?.ratio"
                  class="flex justify-between border-b pb-2"
                >
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.ratio')
                  }}</span>
                  <span>{{ videoData.videoDetail?.ratio }}</span>
                </div>
                <div v-if="videoData.videoDetail?.path" class="flex flex-col gap-1 pb-2">
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.path')
                  }}</span>
                  <span class="path-text" :title="videoData.videoDetail?.path">
                    {{ videoData.videoDetail?.path }}
                  </span>
                </div>
              </div>
            </UCard>

            <UCard v-if="hasComicInfo && comicData">
              <template #header>
                <div class="card-title">
                  {{ $t('page.mediums.info.technicalInfo') }}
                </div>
              </template>
              <div class="space-y-2 text-sm">
                <div
                  v-if="typeof comicImagesCount === 'number'"
                  class="flex justify-between pb-2"
                >
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.comicImages')
                  }}</span>
                  <span>{{ comicImagesCount }}</span>
                </div>
              </div>
            </UCard>

            <UCard v-if="hasReadingInfo">
              <template #header>
                <div class="card-title">
                  {{ $t('page.mediums.info.readingInfo') }}
                </div>
              </template>
              <div class="space-y-2 text-sm">
                <div
                  v-if="typeof medium.readCount === 'number'"
                  class="flex justify-between border-b pb-2"
                >
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.readCount')
                  }}</span>
                  <span>{{ readCountText }}</span>
                </div>
                <div class="flex justify-between pb-2">
                  <span class="text-muted-foreground">{{
                    $t('page.mediums.info.readingLastTime')
                  }}</span>
                  <span v-if="medium.readingLastTime">
                    {{ formatMediumDateTime(medium.readingLastTime) }}
                  </span>
                  <span v-else class="text-muted-foreground">
                    {{ $t('page.mediums.info.never') }}
                  </span>
                </div>
              </div>
            </UCard>

            <UCard v-if="hasTags || hasArtists">
              <template #header>
                <div class="card-title">
                  {{ $t('page.mediums.info.relatedInfo') }}
                </div>
              </template>
              <div class="grid gap-4 sm:grid-cols-2">
                <div>
                  <div class="section-label">
                    {{ $t('page.mediums.info.tags') }}
                  </div>
                  <div v-if="hasTags" class="flex flex-wrap gap-2">
                    <UBadge v-for="tag in tags" :key="tag.id" size="sm">
                      {{ tag.name }}
                    </UBadge>
                  </div>
                  <div v-else class="empty-text">
                    {{ $t('page.mediums.info.noTags') }}
                  </div>
                </div>
                <div>
                  <div class="section-label">
                    {{ $t('page.mediums.info.artists') }}
                  </div>
                  <div v-if="hasArtists" class="flex flex-wrap gap-2">
                    <UBadge
                      v-for="artist in artists"
                      :key="artist.id"
                      size="sm"
                      color="success"
                    >
                      {{ artist.name }}
                    </UBadge>
                  </div>
                  <div v-else class="empty-text">
                    {{ $t('page.mediums.info.noArtists') }}
                  </div>
                </div>
              </div>
            </UCard>
          </div>
        </template>

        <div v-else class="text-muted-foreground py-8 text-center">
          {{ $t('common.noData') }}
        </div>
      </div>
    </template>
  </UModal>
</template>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  overflow: hidden;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
}

.cover-wrapper {
  width: 8rem;
  height: 11rem;
  overflow: hidden;
  background-color: hsl(var(--muted));
  border: 1px solid hsl(var(--border));
  border-radius: var(--el-border-radius-base);
}

@media (min-width: 640px) {
  .cover-wrapper {
    width: 9rem;
    height: 13rem;
  }
}

.empty-cover {
  color: hsl(var(--muted-foreground));
}

.reading-overview {
  padding: 1rem;
  background-color: hsl(var(--card));
  border: 1px solid hsl(var(--border));
  border-radius: var(--el-border-radius-base);
}

.description-block {
  padding: 1rem;
  background-color: hsl(var(--muted));
  border: 1px solid hsl(var(--border));
  border-radius: var(--el-border-radius-base);
}

.info-card :deep(.el-card__header) {
  padding: 0.75rem 1rem;
  border-bottom: 1px solid hsl(var(--border));
}

.info-card :deep(.el-descriptions__label) {
  font-weight: 500;
  color: hsl(var(--muted-foreground));
}

.info-card :deep(.el-descriptions__content) {
  color: hsl(var(--foreground));
}

.path-text {
  display: block;
  font-family: var(--el-font-mono);
  font-size: 0.75rem;
  color: hsl(var(--muted-foreground));
  word-break: break-all;
}

.card-title {
  font-weight: 600;
}

.section-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: hsl(var(--muted-foreground));
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.empty-text {
  font-size: 0.875rem;
  color: hsl(var(--muted-foreground));
}
</style>

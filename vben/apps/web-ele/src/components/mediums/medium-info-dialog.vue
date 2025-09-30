<script setup lang="ts">
import type { Comic, MediumDto, Video } from '@vben/api'

import { computed, onMounted, ref } from 'vue'

import { comicApi, mediumResourceApi, MediumType, videoApi } from '@vben/api'
import { useAppConfig, useIsMobile } from '@vben/hooks'
import { useUserStore } from '@vben/stores'

import { $t } from '#/locales'
import {
  formatMediumDateTime,
  formatMediumDuration,
  formatMediumProgress,
} from '#/utils/medium'

interface Props {
  mediumId: string
  mediumType: MediumType
}

const props = defineProps<Props>()
const { isMobile } = useIsMobile()
const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
const userStore = useUserStore()

const loading = ref(true)
const medium = ref<MediumDto | undefined>()
const numberFormatter = new Intl.NumberFormat()

const coverUrl = computed(() => {
  if (!medium.value?.cover) return undefined
  const url = mediumResourceApi.url.getCover.format({
    cover: medium.value.cover,
  })
  return `${apiURL}${url}`
})

const descriptionsColumn = computed(() => (isMobile.value ? 1 : 2))
const descriptionText = computed(() => medium.value?.description?.trim())

// 类型守卫
const isComic = computed(() => props.mediumType === MediumType.Comic)
const isVideo = computed(() => props.mediumType === MediumType.Video)
const comicData = computed(() =>
  isComic.value ? (medium.value as Comic) : undefined,
)
const videoData = computed(() =>
  isVideo.value ? (medium.value as Video) : undefined,
)

const readingProgressPercent = computed<null | number>(() => {
  const progress = medium.value?.readingProgress
  if (typeof progress !== 'number') return null
  const clamped = Math.max(0, Math.min(1, progress))
  return Math.round(clamped * 100)
})

const readingProgressStatus = computed<'success' | 'warning' | undefined>(
  () => {
    const percent = readingProgressPercent.value

    if (percent === null) {
      return undefined
    }

    if (percent >= 100) {
      return 'success'
    }

    if (percent > 0) {
      return 'warning'
    }

    return undefined
  },
)

const readingProgressLabel = computed(() =>
  medium.value ? formatMediumProgress(medium.value.readingProgress) : '',
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
      (videoData.value?.duration ||
        (videoData.value?.width && videoData.value?.height) ||
        videoData.value?.framerate ||
        videoData.value?.bitrate ||
        videoData.value?.codec ||
        videoData.value?.ratio ||
        videoData.value?.path),
  ),
)

const hasComicInfo = computed(() =>
  Boolean(isComic.value && comicData.value?.comicImages?.length),
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
    medium.value =
      props.mediumType === MediumType.Comic
        ? await comicApi.getById(props.mediumId)
        : await videoApi.getById(props.mediumId)
  } finally {
    loading.value = false
  }
})
</script>

<template>
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
              <el-tag size="small" type="primary">
                {{
                  isComic
                    ? $t('page.mediums.info.comic')
                    : $t('page.mediums.info.video')
                }}
              </el-tag>
              <span v-if="typeof medium.readCount === 'number'">
                {{ $t('page.mediums.info.readCount') }} · {{ readCountText }}
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
            <div class="flex items-center justify-between text-sm font-medium">
              <span>{{ $t('page.mediums.info.readingProgress') }}</span>
              <el-tag
                v-if="readingProgressPercent !== null"
                size="small"
                :type="readingProgressStatus"
              >
                {{ readingProgressLabel }}
              </el-tag>
            </div>
            <el-progress
              v-if="readingProgressPercent !== null"
              :percentage="readingProgressPercent"
              :status="readingProgressStatus"
              :stroke-width="8"
              striped
              striped-flow
            />
            <div v-else class="text-muted-foreground pt-1 text-xs sm:text-sm">
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
        <el-card
          v-if="hasBasicInfo"
          shadow="never"
          class="info-card"
          body-class="!p-0"
        >
          <template #header>
            <div class="card-title">
              {{ $t('page.mediums.info.basicInfo') }}
            </div>
          </template>
          <el-descriptions :column="descriptionsColumn" border>
            <el-descriptions-item
              v-if="medium.creationTime"
              :label="$t('page.mediums.info.creationTime')"
            >
              {{ formatMediumDateTime(medium.creationTime) }}
            </el-descriptions-item>
            <el-descriptions-item
              v-if="medium.lastModificationTime"
              :label="$t('page.mediums.info.lastModificationTime')"
            >
              {{ formatMediumDateTime(medium.lastModificationTime) }}
            </el-descriptions-item>
            <el-descriptions-item
              v-if="libraryName"
              :label="$t('page.mediums.info.library')"
            >
              {{ libraryName }}
            </el-descriptions-item>
          </el-descriptions>
        </el-card>

        <el-card
          v-if="hasVideoInfo && videoData"
          shadow="never"
          class="info-card"
          body-class="!p-0"
        >
          <template #header>
            <div class="card-title">
              {{ $t('page.mediums.info.technicalInfo') }}
            </div>
          </template>
          <el-descriptions :column="descriptionsColumn" border>
            <el-descriptions-item
              v-if="videoData.duration"
              :label="$t('page.mediums.info.duration')"
            >
              {{ formatMediumDuration(videoData.duration) }}
            </el-descriptions-item>
            <el-descriptions-item
              v-if="videoData.width && videoData.height"
              :label="$t('page.mediums.info.resolution')"
            >
              {{ videoData.width }} × {{ videoData.height }}
            </el-descriptions-item>
            <el-descriptions-item
              v-if="videoData.framerate"
              :label="$t('page.mediums.info.framerate')"
            >
              {{ videoData.framerate }} fps
            </el-descriptions-item>
            <el-descriptions-item
              v-if="videoData.bitrate"
              :label="$t('page.mediums.info.bitrate')"
            >
              {{ Math.round(videoData.bitrate / 1000) }} kbps
            </el-descriptions-item>
            <el-descriptions-item
              v-if="videoData.codec"
              :label="$t('page.mediums.info.codec')"
            >
              {{ videoData.codec }}
            </el-descriptions-item>
            <el-descriptions-item
              v-if="videoData.ratio"
              :label="$t('page.mediums.info.ratio')"
            >
              {{ videoData.ratio }}
            </el-descriptions-item>
            <el-descriptions-item
              v-if="videoData.path"
              :label="$t('page.mediums.info.path')"
              :span="descriptionsColumn"
            >
              <span class="path-text" :title="videoData.path">
                {{ videoData.path }}
              </span>
            </el-descriptions-item>
          </el-descriptions>
        </el-card>

        <el-card
          v-if="hasComicInfo && comicData"
          shadow="never"
          class="info-card"
          body-class="!p-0"
        >
          <template #header>
            <div class="card-title">
              {{ $t('page.mediums.info.technicalInfo') }}
            </div>
          </template>
          <el-descriptions :column="descriptionsColumn" border>
            <el-descriptions-item
              v-if="comicData.comicImages?.length"
              :label="$t('page.mediums.info.comicImages')"
            >
              {{ comicData.comicImages.length }}
            </el-descriptions-item>
          </el-descriptions>
        </el-card>

        <el-card
          v-if="hasReadingInfo"
          shadow="never"
          class="info-card"
          body-class="!p-0"
        >
          <template #header>
            <div class="card-title">
              {{ $t('page.mediums.info.readingInfo') }}
            </div>
          </template>
          <el-descriptions :column="descriptionsColumn" border>
            <el-descriptions-item
              v-if="typeof medium.readCount === 'number'"
              :label="$t('page.mediums.info.readCount')"
            >
              {{ readCountText }}
            </el-descriptions-item>
            <el-descriptions-item
              :label="$t('page.mediums.info.readingLastTime')"
            >
              <span v-if="medium.readingLastTime">
                {{ formatMediumDateTime(medium.readingLastTime) }}
              </span>
              <span v-else class="text-muted-foreground">
                {{ $t('page.mediums.info.never') }}
              </span>
            </el-descriptions-item>
          </el-descriptions>
        </el-card>

        <el-card
          v-if="hasTags || hasArtists"
          shadow="never"
          class="info-card"
          body-class="!p-4"
        >
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
                <el-tag v-for="tag in tags" :key="tag.id" size="small">
                  {{ tag.name }}
                </el-tag>
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
                <el-tag
                  v-for="artist in artists"
                  :key="artist.id"
                  size="small"
                  type="success"
                >
                  {{ artist.name }}
                </el-tag>
              </div>
              <div v-else class="empty-text">
                {{ $t('page.mediums.info.noArtists') }}
              </div>
            </div>
          </div>
        </el-card>
      </div>
    </template>

    <div v-else class="text-muted-foreground py-8 text-center">
      {{ $t('common.noData') }}
    </div>
  </div>
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

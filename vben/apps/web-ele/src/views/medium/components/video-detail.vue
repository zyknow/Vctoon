<script setup lang="ts">
import type { Video } from '@vben/api'

import { computed, toRef } from 'vue'
import { useRouter } from 'vue-router'

import { mediumResourceApi } from '@vben/api'
import { useAppConfig, useIsMobile } from '@vben/hooks'
import { CiStar, MdiPlayCircle } from '@vben/icons'

import MediumToolbarFirst from '#/components/mediums/medium-toolbar-first.vue'
import { $t } from '#/locales'
import {
  formatMediumDateTime,
  formatMediumDuration,
  formatMediumProgress,
} from '#/utils/medium'

const props = defineProps<{
  loading: boolean
  medium: Video
}>()

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
const { isMobile } = useIsMobile()
const router = useRouter()

const medium = toRef(props, 'medium')
const loading = toRef(props, 'loading')

const coverUrl = computed(() => {
  if (!medium.value.cover) return undefined
  const url = mediumResourceApi.url.getCover.format({
    cover: medium.value.cover,
  })
  return `${apiURL}${url}`
})

const mediumTitle = computed(() => medium.value.title ?? '')
const creationTimeText = computed(() => {
  const value = medium.value.creationTime
  if (!value) return '--'
  return formatMediumDateTime(value)
})

const durationText = computed(() => {
  const duration = medium.value.duration
  if (!duration) return '--'
  return formatMediumDuration(duration)
})

const descriptionText = computed(() => medium.value.description?.trim() ?? '')
const hasTags = computed(() => (medium.value.tags?.length ?? 0) > 0)
const hasArtists = computed(() => (medium.value.artists?.length ?? 0) > 0)

const detailTopGridClasses = computed(() => {
  const classes = ['detail-top', 'grid', 'gap-8', 'items-start']
  if (isMobile.value) {
    classes.push('grid-cols-1')
  } else {
    classes.push(
      'lg:grid-cols-[220px_1fr]',
      'xl:grid-cols-[240px_1fr]',
      '2xl:grid-cols-[260px_1fr]',
    )
  }
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

const goBack = () => {
  router.back()
}

const handlePlay = () => {
  // TODO: 跳转到视频播放页面
}
</script>

<template>
  <MediumToolbarFirst :title="mediumTitle">
    <template #left>
      <div class="flex items-center gap-3">
        <el-button size="small" type="default" @click="goBack">
          {{ $t('page.mediums.detail.back') }}
        </el-button>
      </div>
    </template>
  </MediumToolbarFirst>

  <div class="flex min-h-0 flex-1 flex-col overflow-auto">
    <div v-if="loading" class="text-muted-foreground py-20 text-center">
      {{ $t('common.loading') }}
    </div>
    <div v-else class="flex flex-1 flex-col">
      <div :class="detailTopGridClasses">
        <div
          class="relative flex aspect-[3/4] w-full items-center justify-center overflow-hidden rounded-lg shadow-lg"
          :class="coverUrl ? 'bg-black' : 'bg-muted'"
        >
          <img
            v-if="coverUrl"
            :src="coverUrl"
            alt="cover"
            class="h-full w-full object-cover"
            loading="lazy"
          />
          <div
            v-else
            class="text-muted-foreground flex h-full w-full items-center justify-center text-7xl"
          >
            🎬
          </div>
        </div>

        <div class="flex flex-col gap-8">
          <div class="space-y-3">
            <h1 class="text-3xl font-bold leading-tight lg:text-4xl">
              {{ mediumTitle || $t('page.mediums.detail.untitled') }}
            </h1>
            <div class="flex flex-wrap items-center gap-3 text-sm">
              <el-tag type="success" size="default">
                {{ $t('page.mediums.info.video') }}
              </el-tag>
              <span class="text-muted-foreground">
                {{ creationTimeText.split(' ')[0] }}
              </span>
              <span class="text-muted-foreground">
                {{ durationText }}
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

          <div class="flex flex-wrap gap-3">
            <el-button
              size="large"
              type="primary"
              :icon="MdiPlayCircle"
              disabled
              @click="handlePlay"
            >
              {{ $t('page.mediums.actions.startPlaying') }}
            </el-button>
            <el-button size="large" type="default" :icon="CiStar" disabled>
              {{ $t('page.mediums.actions.rate') }}
            </el-button>
          </div>

          <el-alert
            type="info"
            :closable="false"
            class="border-border text-sm"
            show-icon
          >
            {{ $t('page.mediums.detail.videoComingSoon') }}
          </el-alert>

          <div class="space-y-4">
            <div>
              <div
                class="text-muted-foreground mb-2 text-sm font-medium uppercase tracking-wide"
              >
                {{ $t('page.mediums.info.readingProgress') }}
              </div>
              <div class="text-2xl font-bold">
                {{ progressText }}
              </div>
            </div>

            <div v-if="descriptionText" class="space-y-2">
              <div
                class="text-muted-foreground text-sm font-medium uppercase tracking-wide"
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
                class="text-muted-foreground mb-2 text-sm font-medium uppercase tracking-wide"
              >
                {{ $t('page.mediums.info.tags') }}
              </div>
              <div class="flex flex-wrap gap-2">
                <el-tag v-for="tag in medium.tags" :key="tag.id" size="default">
                  {{ tag.name }}
                </el-tag>
              </div>
            </div>

            <div v-if="hasArtists">
              <div
                class="text-muted-foreground mb-2 text-sm font-medium uppercase tracking-wide"
              >
                {{ $t('page.mediums.info.artists') }}
              </div>
              <div class="flex flex-wrap gap-2">
                <el-tag
                  v-for="artist in medium.artists"
                  :key="artist.id"
                  size="default"
                  type="success"
                >
                  {{ artist.name }}
                </el-tag>
              </div>
            </div>
          </div>

          <div
            v-if="hasTechnicalInfo"
            class="border-border space-y-3 rounded-lg border p-4"
          >
            <div
              class="text-muted-foreground text-sm font-medium uppercase tracking-wide"
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
</template>

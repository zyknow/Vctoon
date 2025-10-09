<script setup lang="ts">
import type { Comic, ComicImage, Video } from '@vben/api'

import type {
  MediumProvider,
  MediumViewTab,
  PageRequest,
} from '#/hooks/useMediumProvider'

import {
  computed,
  nextTick,
  onBeforeUnmount,
  reactive,
  ref,
  watch,
  watchEffect,
} from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { comicApi, mediumResourceApi, MediumType, videoApi } from '@vben/api'
import { Page } from '@vben/common-ui'
import { useAppConfig, useIsMobile } from '@vben/hooks'
import { CiStar, MdiIncognito, MdiPlayCircle, MdiRefresh } from '@vben/icons'

import MediumCoverCard from '#/components/mediums/medium-cover-card.vue'
import MediumSelectionIndicator from '#/components/mediums/medium-selection-indicator.vue'
import MediumToolbarFirst from '#/components/mediums/medium-toolbar-first.vue'
import MediumToolbarSecond from '#/components/mediums/medium-toolbar-second.vue'
import { useDialogService } from '#/hooks/useDialogService'
import { provideMediumProvider } from '#/hooks/useMediumProvider'
import { $t } from '#/locales'
import { useMediumStore } from '#/store'
import {
  formatMediumDateTime,
  formatMediumDuration,
  formatMediumProgress,
} from '#/utils/medium'

const route = useRoute()
const router = useRouter()
const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
const { confirm } = useDialogService()
const { isMobile } = useIsMobile()
const mediumStore = useMediumStore()
const numberFormatter = new Intl.NumberFormat()
const SUMMARY_PLACEHOLDER = '--'

const mediumId = computed(() => route.params.mediumId as string)
const typeParam = computed(() => (route.params.type as string) ?? 'comic')

const mediumType = computed<MediumType>(() =>
  typeParam.value === 'video' ? MediumType.Video : MediumType.Comic,
)

const mediumLoading = ref(false)
const mediumData = ref<(Comic | Video) | undefined>()
const mediumTitle = ref('')

const comicImages = ref<Array<ComicImage & { index: number }>>([])
const comicImagesLoading = ref(false)
const deletingImages = ref(false)
const selectedImageIds = ref<string[]>([])
const lastClickedIndex = ref<null | number>(null)
const sortState = reactive<{
  field: 'index' | 'name' | 'size'
  order: 'asc' | 'desc'
}>({
  field: 'index',
  order: 'asc',
})
const COMIC_IMAGE_BATCH_SIZE = 60
const comicScrollContainer = ref<HTMLElement | null>(null)
const visibleComicCount = ref(COMIC_IMAGE_BATCH_SIZE)

const isSupportedSortField = (
  value: string,
): value is 'index' | 'name' | 'size' =>
  value === 'index' || value === 'name' || value === 'size'

const providerTitle = ref('')
const providerTotalCount = ref(0)
const providerPageRequest = reactive<PageRequest>({
  sorting: 'index asc',
  maxResultCount: 1000,
})
const providerLoadType = ref<MediumType>(mediumType.value)

const mediumProvider: MediumProvider = {
  currentTab: ref<MediumViewTab>('library'),
  hasMore: ref(false),
  items: ref([]),
  loading: ref(false),
  async loadItems() {},
  async loadNext() {},
  loadType: providerLoadType,
  pageRequest: providerPageRequest,
  selectedMediumIds: ref([]),
  title: providerTitle,
  totalCount: providerTotalCount,
  async updateSorting(sorting: string) {
    providerPageRequest.sorting = sorting
    applySortFromString(sorting)
  },
}

provideMediumProvider(mediumProvider)

const isComic = computed(() => mediumType.value === MediumType.Comic)
const isVideo = computed(() => mediumType.value === MediumType.Video)

const coverUrl = computed(() => {
  if (!mediumData.value?.cover) return undefined
  const url = mediumResourceApi.url.getCover.format({
    cover: mediumData.value.cover,
  })
  return `${apiURL}${url}`
})

const sortedComicImages = computed(() => {
  const images = [...comicImages.value]
  const { field, order } = sortState
  images.sort((a, b) => {
    if (field === 'name') {
      return (a.name ?? '').localeCompare(b.name ?? '', 'zh-CN')
    }
    if (field === 'size') {
      return a.size - b.size
    }
    return a.index - b.index
  })
  return order === 'asc' ? images : images.reverse()
})

const imageZoom = computed(() => mediumStore.itemZoom ?? 1)
const comicGridStyle = computed(() => ({
  '--cover-base-width': '10rem',
  '--cover-base-height': '14rem',
  '--cover-zoom': `${imageZoom.value}`,
}))
const comicImageMaxWidth = computed(() =>
  Math.round((isMobile.value ? 800 : 1600) * imageZoom.value),
)
const visibleComicImages = computed(() =>
  sortedComicImages.value.slice(0, visibleComicCount.value),
)

const totalComicPages = computed(() =>
  isComic.value ? sortedComicImages.value.length : 0,
)

const resumePage = computed(() => {
  const total = totalComicPages.value
  if (total <= 0) {
    return 1
  }
  const progress = mediumData.value?.readingProgress ?? 0
  if (!progress || progress <= 0) {
    return 1
  }
  const page = Math.ceil(progress * total)
  return Math.min(Math.max(page, 1), total)
})

const descriptionText = computed(
  () => mediumData.value?.description?.trim() ?? '',
)

const hasReadingProgress = computed(() => {
  const progress = mediumData.value?.readingProgress
  return typeof progress === 'number' && progress > 0
})

const hasComicImages = computed(() => sortedComicImages.value.length > 0)
const canReadComic = computed(() => isComic.value && hasComicImages.value)
const canResumeComic = computed(
  () => canReadComic.value && hasReadingProgress.value,
)
const startReadingDisabled = computed(
  () => !canReadComic.value || comicImagesLoading.value,
)
const resumeReadingDisabled = computed(
  () => !canResumeComic.value || comicImagesLoading.value,
)

const creationTimeText = computed(() => {
  const value = mediumData.value?.creationTime
  if (!value) return SUMMARY_PLACEHOLDER
  return formatMediumDateTime(value)
})

const hasTags = computed(() => (mediumData.value?.tags?.length ?? 0) > 0)
const hasArtists = computed(() => (mediumData.value?.artists?.length ?? 0) > 0)

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

const selectedCount = computed(() => selectedImageIds.value.length)
const hasSelection = computed(() => selectedCount.value > 0)
const sortDescription = computed(() => {
  let fieldLabel = $t('page.mediums.detail.sortFieldIndex')
  if (sortState.field === 'name') {
    fieldLabel = $t('page.mediums.detail.sortFieldName')
  } else if (sortState.field === 'size') {
    fieldLabel = $t('page.mediums.detail.sortFieldSize')
  }
  const orderLabel =
    sortState.order === 'asc'
      ? $t('page.mediums.detail.sortAsc')
      : $t('page.mediums.detail.sortDesc')
  return $t('page.mediums.detail.sortLabel', {
    field: fieldLabel,
    order: orderLabel,
  })
})

watch(mediumType, (value) => {
  providerLoadType.value = value
  selectedImageIds.value = []
  visibleComicCount.value = COMIC_IMAGE_BATCH_SIZE
})

watchEffect(() => {
  providerTitle.value = mediumTitle.value
})

watch(
  () => sortedComicImages.value.length,
  (count) => {
    providerTotalCount.value = count
  },
)

watch(
  () => sortedComicImages.value,
  (list) => {
    const baseCount = Math.min(list.length, COMIC_IMAGE_BATCH_SIZE)
    visibleComicCount.value = baseCount || COMIC_IMAGE_BATCH_SIZE
  },
  { immediate: true },
)

const loadMoreVisibleComicImages = () => {
  if (visibleComicCount.value >= sortedComicImages.value.length) return
  visibleComicCount.value = Math.min(
    visibleComicCount.value + COMIC_IMAGE_BATCH_SIZE,
    sortedComicImages.value.length,
  )
}

const handleComicScroll = () => {
  const el = comicScrollContainer.value
  if (!el || comicImagesLoading.value) return
  if (visibleComicCount.value >= sortedComicImages.value.length) return
  const threshold = el.scrollHeight - el.clientHeight - 120
  if (el.scrollTop >= threshold) {
    loadMoreVisibleComicImages()
  }
}

let detachComicScroll: (() => void) | undefined

const attachComicScrollListener = () => {
  if (detachComicScroll) {
    detachComicScroll()
    detachComicScroll = undefined
  }
  const el = comicScrollContainer.value
  if (!el) return
  const listener = () => handleComicScroll()
  el.addEventListener('scroll', listener, { passive: true })
  detachComicScroll = () => el.removeEventListener('scroll', listener)
}

const ensureComicImagesFillContainer = () => {
  const el = comicScrollContainer.value
  if (!el) return
  if (
    el.scrollHeight <= el.clientHeight &&
    visibleComicCount.value < sortedComicImages.value.length
  ) {
    loadMoreVisibleComicImages()
    nextTick(() => ensureComicImagesFillContainer())
  }
}

watch(
  () => comicScrollContainer.value,
  () => {
    attachComicScrollListener()
    nextTick(() => ensureComicImagesFillContainer())
  },
)

watch(isComic, (value) => {
  if (!value && detachComicScroll) {
    detachComicScroll()
    detachComicScroll = undefined
  }
})

watch(
  () => comicImagesLoading.value,
  (loading) => {
    if (!loading) {
      nextTick(() => ensureComicImagesFillContainer())
    }
  },
)

onBeforeUnmount(() => {
  if (detachComicScroll) {
    detachComicScroll()
  }
})

const applySortFromString = (sorting: string) => {
  const [field, order] = sorting.split(' ') as [string, string]
  sortState.field = isSupportedSortField(field) ? field : 'index'
  sortState.order = order === 'desc' ? 'desc' : 'asc'
}

const updateSort = (field: 'index' | 'name' | 'size') => {
  if (sortState.field === field) {
    sortState.order = sortState.order === 'asc' ? 'desc' : 'asc'
  } else {
    sortState.field = field
    sortState.order = 'asc'
  }
  providerPageRequest.sorting = `${sortState.field} ${sortState.order}`
  visibleComicCount.value = COMIC_IMAGE_BATCH_SIZE
}

const handleSortCommand = (command: unknown) => {
  if (isSupportedSortField(String(command))) {
    updateSort(command as 'index' | 'name' | 'size')
  }
}

const isImageSelected = (id: string) => selectedImageIds.value.includes(id)

const toggleImageSelection = (id: string, event?: MouseEvent) => {
  const currentIndex = sortedComicImages.value.findIndex((img) => img.id === id)
  if (currentIndex === -1) return

  // Shift 键多选
  if (event?.shiftKey && lastClickedIndex.value !== null) {
    const start = Math.min(lastClickedIndex.value, currentIndex)
    const end = Math.max(lastClickedIndex.value, currentIndex)
    const rangeIds = sortedComicImages.value
      .slice(start, end + 1)
      .map((img) => img.id)

    const next = new Set(selectedImageIds.value)
    rangeIds.forEach((rangeId) => next.add(rangeId))
    selectedImageIds.value = [...next]
  } else {
    // 普通单选/取消选择
    const next = new Set(selectedImageIds.value)
    if (next.has(id)) {
      next.delete(id)
    } else {
      next.add(id)
    }
    selectedImageIds.value = [...next]
  }

  lastClickedIndex.value = currentIndex
}

const selectAllImages = () => {
  selectedImageIds.value = sortedComicImages.value.map((item) => item.id)
}

const clearSelection = () => {
  selectedImageIds.value = []
}

const deleteSelectedImages = async () => {
  if (selectedImageIds.value.length === 0) return
  const ok = await confirm({
    danger: true,
    message: $t('page.mediums.detail.deleteImagesConfirm', {
      count: selectedImageIds.value.length,
    }),
    title: $t('page.mediums.detail.deleteImagesTitle'),
  })
  if (!ok) return
  deletingImages.value = true
  try {
    await Promise.all(
      selectedImageIds.value.map((imageId) =>
        comicApi.deleteComicImage(imageId),
      ),
    )
    await loadComicImages()
    selectedImageIds.value = []
  } finally {
    deletingImages.value = false
  }
}

const loadComicImages = async () => {
  if (!isComic.value) return
  comicImagesLoading.value = true
  try {
    const images = await comicApi.getImagesByComicId(mediumId.value)
    comicImages.value = images.map((item, index) => ({ ...item, index }))
    selectedImageIds.value = []
    providerPageRequest.sorting = `${sortState.field} ${sortState.order}`
    const initialVisibleCount =
      Math.min(images.length, COMIC_IMAGE_BATCH_SIZE) || COMIC_IMAGE_BATCH_SIZE
    visibleComicCount.value = initialVisibleCount
  } finally {
    comicImagesLoading.value = false
  }
}

const loadMediumDetail = async () => {
  mediumLoading.value = true
  try {
    const data =
      mediumType.value === MediumType.Comic
        ? await comicApi.getById(mediumId.value)
        : await videoApi.getById(mediumId.value)
    mediumData.value = data
    mediumTitle.value = data.title ?? ''
    if (mediumType.value === MediumType.Comic) {
      await loadComicImages()
    } else {
      comicImages.value = []
    }
  } finally {
    mediumLoading.value = false
  }
}

watch(
  [mediumId, mediumType],
  async () => {
    await loadMediumDetail()
  },
  { immediate: true },
)

type ComicReaderMode = 'restart' | 'resume'

const navigateToComicReader = (
  mode: ComicReaderMode,
  options: { incognito?: boolean; page?: number } = {},
) => {
  if (!canReadComic.value) {
    return
  }
  const id = mediumId.value
  if (!id) {
    return
  }
  const query: Record<string, string> = {}
  if (mode === 'resume') {
    query.mode = mode
  }
  if (options.page && options.page > 0) {
    query.page = String(Math.max(1, Math.floor(options.page)))
  }
  if (options.incognito) {
    query.incognito = '1'
  }
  void router.push({
    name: 'ComicReader',
    params: { comicId: id },
    query,
  })
}

const handleContinueReading = () => {
  navigateToComicReader('resume', { page: resumePage.value })
}

const handleRestartReading = () => {
  navigateToComicReader('restart', { page: 1 })
}

const handleStartReading = () => {
  navigateToComicReader('restart', { page: 1 })
}

const handleIncognitoReading = () => {
  if (startReadingDisabled.value) {
    return
  }
  const mode: ComicReaderMode = hasReadingProgress.value ? 'resume' : 'restart'
  const page = hasReadingProgress.value ? resumePage.value : 1
  navigateToComicReader(mode, { incognito: true, page })
}

const goBack = () => {
  router.back()
}

const formatImageSize = (size: number) => {
  if (size < 1024) return `${size} B`
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`
  if (size < 1024 * 1024 * 1024) {
    return `${(size / (1024 * 1024)).toFixed(1)} MB`
  }
  return `${(size / (1024 * 1024 * 1024)).toFixed(1)} GB`
}

const resolveComicImageUrl = (imageId: string) => {
  const url = comicApi.url.getComicImage.format({
    comicImageId: imageId,
    maxWidth: comicImageMaxWidth.value,
  })
  return `${apiURL}${url}`
}
</script>

<template>
  <Page auto-content-height content-class="flex flex-col gap-8 pb-12">
    <MediumToolbarFirst :title="mediumTitle">
      <template #left>
        <div class="flex items-center gap-3">
          <el-button size="small" type="default" @click="goBack">
            {{ $t('page.mediums.detail.back') }}
          </el-button>
        </div>
      </template>
    </MediumToolbarFirst>

    <MediumToolbarSecond v-if="isComic && hasSelection">
      <template #left>
        <div class="flex items-center gap-2">
          <el-tag size="small" type="success">
            {{
              $t('page.mediums.selection.selectedCount', {
                count: selectedCount,
              })
            }}
          </el-tag>
        </div>
      </template>

      <template #right>
        <div class="flex items-center gap-2">
          <el-button
            size="small"
            type="default"
            :disabled="comicImagesLoading"
            @click="selectAllImages"
          >
            {{ $t('page.mediums.detail.selectAllImages') }}
          </el-button>
          <el-button size="small" type="default" @click="clearSelection">
            {{ $t('page.mediums.detail.clearSelection') }}
          </el-button>
          <el-button
            size="small"
            type="danger"
            :loading="deletingImages"
            @click="deleteSelectedImages"
          >
            {{ $t('page.mediums.detail.deleteSelectedImages') }}
          </el-button>
        </div>
      </template>
    </MediumToolbarSecond>

    <div class="flex min-h-0 flex-1 flex-col overflow-auto">
      <div v-if="mediumLoading" class="text-muted-foreground py-20 text-center">
        {{ $t('common.loading') }}
      </div>
      <div v-else-if="mediumData" class="flex flex-1 flex-col">
        <div :class="detailTopGridClasses">
          <!-- 左侧封面 -->
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
              📄
            </div>
          </div>

          <!-- 右侧信息 -->
          <div class="flex flex-col gap-8">
            <!-- 标题区域 -->
            <div class="space-y-3">
              <h1 class="text-3xl font-bold leading-tight lg:text-4xl">
                {{ mediumTitle || $t('page.mediums.detail.untitled') }}
              </h1>
              <div class="flex flex-wrap items-center gap-3 text-sm">
                <el-tag v-if="isComic" type="primary" size="default">
                  {{ $t('page.mediums.info.comic') }}
                </el-tag>
                <el-tag v-else type="success" size="default">
                  {{ $t('page.mediums.info.video') }}
                </el-tag>
                <span class="text-muted-foreground">{{
                  creationTimeText.split(' ')[0]
                }}</span>
                <span
                  v-if="isVideo && (mediumData as Video).duration"
                  class="text-muted-foreground"
                >
                  {{ formatMediumDuration((mediumData as Video).duration) }}
                </span>
                <div class="flex items-center gap-2">
                  <div class="text-muted-foreground">|</div>
                  <div class="text-muted-foreground">
                    {{ $t('page.mediums.info.readCount') }}:
                    {{ numberFormatter.format(mediumData?.readCount ?? 0) }}
                  </div>
                </div>
              </div>
            </div>

            <!-- 操作按钮 -->
            <div class="flex flex-wrap gap-3">
              <template v-if="hasReadingProgress">
                <el-button
                  size="large"
                  type="primary"
                  :icon="MdiPlayCircle"
                  :disabled="resumeReadingDisabled"
                  @click="handleContinueReading"
                >
                  {{ $t('page.mediums.actions.continueReading') }}
                </el-button>
                <el-button
                  size="large"
                  type="default"
                  :icon="MdiRefresh"
                  :disabled="resumeReadingDisabled"
                  @click="handleRestartReading"
                >
                  {{ $t('page.mediums.actions.restartReading') }}
                </el-button>
                <el-button
                  size="large"
                  type="info"
                  plain
                  :icon="MdiIncognito"
                  :disabled="startReadingDisabled"
                  @click="handleIncognitoReading"
                >
                  {{
                    isComic
                      ? $t('page.mediums.actions.incognitoRead')
                      : $t('page.mediums.actions.incognitoPlay')
                  }}
                </el-button>
              </template>
              <el-button
                v-else
                size="large"
                type="primary"
                :icon="MdiPlayCircle"
                :disabled="startReadingDisabled"
                @click="handleStartReading"
              >
                {{ $t('page.mediums.actions.startReading') }}
              </el-button>
              <el-button
                v-if="!hasReadingProgress"
                size="large"
                type="info"
                plain
                :icon="MdiIncognito"
                :disabled="startReadingDisabled"
                @click="handleIncognitoReading"
              >
                {{
                  isComic
                    ? $t('page.mediums.actions.incognitoRead')
                    : $t('page.mediums.actions.incognitoPlay')
                }}
              </el-button>
              <el-button size="large" type="default" :icon="CiStar" disabled>
                {{ $t('page.mediums.actions.rate') }}
              </el-button>
            </div>

            <!-- 简化的进度信息 -->
            <div class="space-y-4">
              <div>
                <div
                  class="text-muted-foreground mb-2 text-sm font-medium uppercase tracking-wide"
                >
                  {{ $t('page.mediums.info.readingProgress') }}
                </div>
                <div class="text-2xl font-bold">
                  {{
                    mediumData
                      ? formatMediumProgress(mediumData.readingProgress)
                      : SUMMARY_PLACEHOLDER
                  }}
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

            <!-- 标签和艺术家 -->
            <div class="space-y-4">
              <div v-if="hasTags">
                <div
                  class="text-muted-foreground mb-2 text-sm font-medium uppercase tracking-wide"
                >
                  {{ $t('page.mediums.info.tags') }}
                </div>
                <div class="flex flex-wrap gap-2">
                  <el-tag
                    v-for="tag in mediumData.tags"
                    :key="tag.id"
                    size="default"
                  >
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
                    v-for="artist in mediumData.artists"
                    :key="artist.id"
                    size="default"
                    type="success"
                  >
                    {{ artist.name }}
                  </el-tag>
                </div>
              </div>
            </div>

            <!-- 技术信息（仅视频） -->
            <div
              v-if="isVideo"
              class="border-border space-y-3 rounded-lg border p-4"
            >
              <div
                class="text-muted-foreground text-sm font-medium uppercase tracking-wide"
              >
                {{ $t('page.mediums.info.technicalInfo') }}
              </div>
              <div class="grid gap-3 text-sm sm:grid-cols-2">
                <div
                  v-if="
                    (mediumData as Video).width && (mediumData as Video).height
                  "
                >
                  <div class="text-muted-foreground text-xs">
                    {{ $t('page.mediums.info.resolution') }}
                  </div>
                  <div class="mt-1 font-medium">
                    {{ (mediumData as Video).width }} ×
                    {{ (mediumData as Video).height }}
                  </div>
                </div>
                <div v-if="(mediumData as Video).framerate">
                  <div class="text-muted-foreground text-xs">
                    {{ $t('page.mediums.info.framerate') }}
                  </div>
                  <div class="mt-1 font-medium">
                    {{ (mediumData as Video).framerate }} fps
                  </div>
                </div>
                <div v-if="(mediumData as Video).codec">
                  <div class="text-muted-foreground text-xs">
                    {{ $t('page.mediums.info.codec') }}
                  </div>
                  <div class="mt-1 font-medium">
                    {{ (mediumData as Video).codec }}
                  </div>
                </div>
                <div v-if="(mediumData as Video).bitrate">
                  <div class="text-muted-foreground text-xs">
                    {{ $t('page.mediums.info.bitrate') }}
                  </div>
                  <div class="mt-1 font-medium">
                    {{ Math.round((mediumData as Video).bitrate / 1000) }} kbps
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div
          v-if="isComic"
          class="comic-section mt-16 flex flex-1 flex-col gap-6 overflow-hidden"
        >
          <div class="flex items-center justify-between">
            <h2 class="text-2xl font-bold">
              {{ $t('page.mediums.detail.comicImages') }}
              <span class="text-muted-foreground ml-2 text-lg font-normal">
                ({{ providerTotalCount }})
              </span>
            </h2>
            <el-dropdown trigger="click" @command="handleSortCommand">
              <el-button size="default" type="default">
                {{ sortDescription }}
                <i class="i-mdi-chevron-down ml-1 text-base"></i>
              </el-button>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="index">
                    {{ $t('page.mediums.detail.sortFieldIndex') }}
                  </el-dropdown-item>
                  <el-dropdown-item command="name">
                    {{ $t('page.mediums.detail.sortFieldName') }}
                  </el-dropdown-item>
                  <el-dropdown-item command="size">
                    {{ $t('page.mediums.detail.sortFieldSize') }}
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>

          <div
            ref="comicScrollContainer"
            class="comic-scroll-container flex-1 overflow-auto"
          >
            <div
              v-if="comicImagesLoading"
              class="text-muted-foreground py-20 text-center"
            >
              {{ $t('common.loading') }}
            </div>
            <div
              v-else-if="sortedComicImages.length === 0"
              class="text-muted-foreground py-20 text-center"
            >
              {{ $t('page.mediums.detail.noComicImages') }}
            </div>
            <div v-else class="flex flex-wrap gap-6" :style="comicGridStyle">
              <MediumCoverCard
                v-for="image in visibleComicImages"
                :key="image.id"
                :alt="image.name || image.id"
                :preserve-ratio="true"
                :src="resolveComicImageUrl(image.id)"
                class="group relative cursor-pointer select-none border transition-all duration-200"
                fit="contain"
                :class="
                  isImageSelected(image.id)
                    ? 'border-primary border-4 shadow-lg'
                    : 'border-border hover:shadow-md'
                "
                @click="toggleImageSelection(image.id, $event)"
              >
                <template #placeholder>
                  <div
                    class="text-muted-foreground flex h-full w-full items-center justify-center text-4xl"
                  >
                    📄
                  </div>
                </template>
                <MediumSelectionIndicator
                  class="absolute left-2 top-2"
                  :selected="isImageSelected(image.id)"
                  size="sm"
                  @click.stop="toggleImageSelection(image.id)"
                />
                <div
                  class="absolute right-2 top-2 rounded-full bg-black/70 px-2.5 py-1 text-xs font-medium text-white backdrop-blur-sm"
                >
                  #{{ image.index + 1 }}
                </div>
                <div
                  class="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black/80 to-transparent px-3 py-2 text-white"
                >
                  <div class="flex items-center justify-between text-xs">
                    <span
                      class="flex-1 truncate font-medium"
                      :title="image.name || image.id"
                    >
                      {{ image.name || image.id }}
                    </span>
                    <span class="text-muted-foreground ml-2 shrink-0">
                      {{ formatImageSize(image.size) }}
                    </span>
                  </div>
                </div>
              </MediumCoverCard>
            </div>
          </div>
        </div>

        <div
          v-else
          class="border-border text-muted-foreground rounded-md border border-dashed p-6 text-center text-sm"
        >
          {{ $t('page.mediums.detail.videoNoSelection') }}
        </div>
      </div>
      <div v-else class="text-muted-foreground py-10 text-center">
        {{ $t('common.noData') }}
      </div>
    </div>
  </Page>
</template>

<style scoped>
.comic-scroll-container {
  min-height: 0;
}
</style>

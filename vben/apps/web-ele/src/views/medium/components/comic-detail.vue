<script setup lang="ts">
import type { Comic, ComicImage } from '@vben/api'

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
  toRef,
  watch,
  watchEffect,
} from 'vue'
import { useRouter } from 'vue-router'

import { comicApi, mediumResourceApi, MediumType } from '@vben/api'
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
import { formatMediumDateTime, formatMediumProgress } from '#/utils/medium'

const props = defineProps<{
  loading: boolean
  medium: Comic
  mediumId: string
}>()

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
const { confirm } = useDialogService()
const { isMobile } = useIsMobile()
const mediumStore = useMediumStore()
const router = useRouter()
const numberFormatter = new Intl.NumberFormat()

const medium = toRef(props, 'medium')
const mediumId = toRef(props, 'mediumId')
const loading = toRef(props, 'loading')

const mediumTitle = computed(() => medium.value.title ?? '')
const coverUrl = computed(() => {
  if (!medium.value.cover) return undefined
  const url = mediumResourceApi.url.getCover.format({
    cover: medium.value.cover,
  })
  return `${apiURL}${url}`
})

const creationTimeText = computed(() => {
  const value = medium.value.creationTime
  if (!value) return '--'
  return formatMediumDateTime(value)
})

const descriptionText = computed(() => medium.value.description?.trim() ?? '')
const hasTags = computed(() => (medium.value.tags?.length ?? 0) > 0)
const hasArtists = computed(() => (medium.value.artists?.length ?? 0) > 0)

const comicImages = ref<Array<ComicImage & { index: number }>>([])
const comicImagesLoading = ref(false)
const deletingImages = ref(false)
const selectedImageIds = ref<string[]>([])
const lastClickedIndex = ref<null | number>(null)
const comicScrollContainer = ref<HTMLElement | null>(null)

const sortState = reactive<{
  field: 'index' | 'name' | 'size'
  order: 'asc' | 'desc'
}>({
  field: 'index',
  order: 'asc',
})

const COMIC_IMAGE_BATCH_SIZE = 60
const COMIC_PREVIEW_WIDTH = 480
const visibleComicCount = ref(COMIC_IMAGE_BATCH_SIZE)

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

const visibleComicImages = computed(() =>
  sortedComicImages.value.slice(0, visibleComicCount.value),
)

const totalComicPages = computed(() => sortedComicImages.value.length)

const resumePage = computed(() => {
  const total = totalComicPages.value
  if (total <= 0) return 1
  const progress = medium.value.readingProgress ?? 0
  if (!progress || progress <= 0) return 1
  const page = Math.ceil(progress * total)
  return Math.min(Math.max(page, 1), total)
})

const hasReadingProgress = computed(() => {
  const progress = medium.value.readingProgress
  return typeof progress === 'number' && progress > 0
})

const isCompletedReading = computed(() => {
  const progress = medium.value.readingProgress ?? 0
  return progress >= 0.999
})

const canReadComic = computed(() => sortedComicImages.value.length > 0)
const canResumeComic = computed(() => {
  if (!canReadComic.value) return false
  if (!hasReadingProgress.value) return false
  return !isCompletedReading.value
})

const startReadingDisabled = computed(
  () => comicImagesLoading.value || !canReadComic.value,
)

const resumeReadingDisabled = computed(
  () => comicImagesLoading.value || !canResumeComic.value,
)

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

const comicCardStyleVars = computed(() => ({
  '--cover-base-height': '14rem',
  '--cover-base-width': '10rem',
  '--cover-zoom': String(mediumStore.itemZoom || 1),
}))

const selectedCount = computed(() => selectedImageIds.value.length)
const hasSelection = computed(() => selectedCount.value > 0)
const selectedImageSet = computed(() => new Set(selectedImageIds.value))

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

const readCountText = computed(() =>
  numberFormatter.format(medium.value.readCount ?? 0),
)

const readingProgressText = computed(() =>
  formatMediumProgress(medium.value.readingProgress),
)

const providerTitle = ref('')
const providerTotalCount = ref(0)
const providerPageRequest = reactive<PageRequest>({
  sorting: 'index asc',
  maxResultCount: 1000,
})
const mediumProvider: MediumProvider = {
  currentTab: ref<MediumViewTab>('library'),
  hasMore: ref(false),
  items: ref([]),
  loading: ref(false),
  async loadItems() {},
  async loadNext() {},
  loadType: ref(MediumType.Comic),
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

watch(
  () => comicImagesLoading.value,
  (value) => {
    if (!value) {
      nextTick(() => ensureComicImagesFillContainer())
    }
  },
)

watch(
  mediumId,
  () => {
    selectedImageIds.value = []
    visibleComicCount.value = COMIC_IMAGE_BATCH_SIZE
    if (!mediumId.value) {
      comicImages.value = []
      providerTotalCount.value = 0
      return
    }
    void loadComicImages()
  },
  { immediate: true },
)

onBeforeUnmount(() => {
  if (detachComicScroll) {
    detachComicScroll()
  }
})

const isSupportedSortField = (
  value: string,
): value is 'index' | 'name' | 'size' =>
  value === 'index' || value === 'name' || value === 'size'

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

async function loadComicImages() {
  if (!mediumId.value) return
  comicImagesLoading.value = true
  try {
    const images = await comicApi.getImagesByComicId(mediumId.value)
    comicImages.value = images.map((item, index) => ({ ...item, index }))
    providerPageRequest.sorting = `${sortState.field} ${sortState.order}`
    const initialVisibleCount =
      Math.min(images.length, COMIC_IMAGE_BATCH_SIZE) || COMIC_IMAGE_BATCH_SIZE
    visibleComicCount.value = initialVisibleCount
  } finally {
    comicImagesLoading.value = false
  }
}

const isImageSelected = (id: string) => selectedImageSet.value.has(id)

const toggleImageSelection = (id: string, event?: MouseEvent) => {
  const currentIndex = sortedComicImages.value.findIndex((img) => img.id === id)
  if (currentIndex === -1) return

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

const goBack = () => {
  router.back()
}

const navigateToComicReader = (
  mode: 'restart' | 'resume',
  options: { incognito?: boolean; page?: number } = {},
) => {
  if (!mediumId.value) return
  const query: Record<string, string> = {}
  if (mode === 'resume') {
    query.mode = mode
  }
  const resolvedPage = (() => {
    if (options.page && options.page > 0) {
      return options.page
    }
    if (mode === 'resume') {
      return resumePage.value
    }
    return 1
  })()
  query.page = String(Math.max(1, Math.floor(resolvedPage || 1)))
  if (options.incognito) {
    query.incognito = '1'
  }
  void router.push({
    name: 'ComicReader',
    params: { comicId: mediumId.value },
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
  if (startReadingDisabled.value) return
  const hasProgress = hasReadingProgress.value && !isCompletedReading.value
  const mode: 'restart' | 'resume' = hasProgress ? 'resume' : 'restart'
  const page = hasProgress ? resumePage.value : 1
  navigateToComicReader(mode, { incognito: true, page })
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
    maxWidth: COMIC_PREVIEW_WIDTH,
  })
  return `${apiURL}${url}`
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

  <MediumToolbarSecond v-if="hasSelection">
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
            📄
          </div>
        </div>

        <div class="flex flex-col gap-8">
          <div class="space-y-3">
            <h1 class="text-3xl font-bold leading-tight lg:text-4xl">
              {{ mediumTitle || $t('page.mediums.detail.untitled') }}
            </h1>
            <div class="flex flex-wrap items-center gap-3 text-sm">
              <el-tag size="default" type="primary">
                {{ $t('page.mediums.info.comic') }}
              </el-tag>
              <span class="text-muted-foreground">
                {{ creationTimeText.split(' ')[0] }}
              </span>
              <div class="flex items-center gap-2">
                <div class="text-muted-foreground">|</div>
                <div class="text-muted-foreground">
                  {{ $t('page.mediums.info.readCount') }}: {{ readCountText }}
                </div>
              </div>
            </div>
          </div>

          <div class="flex flex-wrap gap-3">
            <template v-if="canResumeComic">
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
                {{ $t('page.mediums.actions.incognitoRead') }}
              </el-button>
            </template>
            <template v-else>
              <el-button
                size="large"
                type="primary"
                :icon="MdiPlayCircle"
                :disabled="startReadingDisabled"
                @click="handleStartReading"
              >
                {{ $t('page.mediums.actions.startReading') }}
              </el-button>
              <el-button
                v-if="hasReadingProgress"
                size="large"
                type="default"
                :icon="MdiRefresh"
                :disabled="startReadingDisabled"
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
                {{ $t('page.mediums.actions.incognitoRead') }}
              </el-button>
            </template>
            <el-button size="large" type="default" :icon="CiStar" disabled>
              {{ $t('page.mediums.actions.rate') }}
            </el-button>
          </div>

          <div class="space-y-4">
            <div>
              <div
                class="text-muted-foreground mb-2 text-sm font-medium uppercase tracking-wide"
              >
                {{ $t('page.mediums.info.readingProgress') }}
              </div>
              <div class="text-2xl font-bold">
                {{ readingProgressText }}
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
        </div>
      </div>

      <div
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
          <div v-else class="flex flex-wrap gap-6" :style="comicCardStyleVars">
            <div
              v-for="image in visibleComicImages"
              :key="image.id"
              v-memo="[image.id, selectedImageSet.has(image.id)]"
              class="card-wrapper"
            >
              <MediumCoverCard
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
          v-if="visibleComicImages.length < sortedComicImages.length"
          class="text-muted-foreground pb-8 text-center text-sm"
        >
          {{ $t('page.mediums.detail.scrollToLoadMore') }}
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card-wrapper {
  display: block;
}

.comic-scroll-container {
  min-height: 0;
}
</style>

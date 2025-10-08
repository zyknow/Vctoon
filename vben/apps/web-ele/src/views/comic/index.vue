<script setup lang="ts">
import type { Comic, ComicImage } from '@vben/api'

import type { ComicViewerSettings } from './types'

import { computed, nextTick, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { comicApi } from '@vben/api'
import { useAppConfig } from '@vben/hooks'
import {
  MdiArrowLeft,
  MdiCog,
  MdiFullscreen,
  MdiFullscreenExit,
  MdiHelpCircle,
  MdiPageFirst,
  MdiPageLast,
} from '@vben/icons'

import { useEventListener, useFullscreen, useLocalStorage } from '@vueuse/core'
import { ElMessage } from 'element-plus'

import { useDialogService } from '#/hooks/useDialogService'
import { $t } from '#/locales'

import ComicSettingsDialog from './components/comic-settings-dialog.vue'
import {
  COMIC_VIEWER_STORAGE_KEY,
  DEFAULT_COMIC_VIEWER_SETTINGS,
  isReverseDirection,
  isVerticalDirection,
} from './types'

defineOptions({ name: 'ComicReaderPage' })

type ViewerImage = ComicImage & { order: number }

const route = useRoute()
const router = useRouter()
const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
const { open } = useDialogService()

const viewerContainerRef = ref<HTMLElement | null>(null)
const stageContainerRef = ref<HTMLElement | null>(null)

const storedSettings = useLocalStorage<ComicViewerSettings>(
  COMIC_VIEWER_STORAGE_KEY,
  DEFAULT_COMIC_VIEWER_SETTINGS,
  { mergeDefaults: true },
)

const settings = reactive<ComicViewerSettings>({ ...storedSettings.value })

watch(
  settings,
  (value) => {
    storedSettings.value = { ...value }
  },
  { deep: true },
)

watch(
  () => settings.customQualityWidth,
  (value) => {
    if (value < 256) {
      settings.customQualityWidth = 256
    }
  },
)

const comicDetail = ref<Comic | null>(null)
const images = ref<ViewerImage[]>([])
const isLoading = ref(false)
const loadError = ref<null | string>(null)
const isUiVisible = ref(true)
const currentStep = ref(0)

const comicId = computed(() => route.params.comicId as string | undefined)

const orientation = computed<'horizontal' | 'vertical'>(() =>
  isVerticalDirection(settings.readingDirection) ? 'vertical' : 'horizontal',
)

const isReverseFlow = computed(() =>
  isReverseDirection(settings.readingDirection),
)

const isDoubleMode = computed(() => settings.displayMode === 'double')
const isScrollMode = computed(() => settings.displayMode === 'scroll')

const orderedImages = computed<ViewerImage[]>(() => {
  const list = [...images.value]
  if (isReverseFlow.value) {
    list.reverse()
  }
  return list
})

const totalSteps = computed(() => {
  const count = orderedImages.value.length
  if (count === 0) {
    return 0
  }
  if (isScrollMode.value) {
    return count
  }
  if (isDoubleMode.value) {
    return Math.ceil(count / 2)
  }
  return count
})

const totalPages = computed(() => orderedImages.value.length)

const clampStep = (value: number) => {
  if (totalSteps.value === 0) {
    return 0
  }
  return Math.min(Math.max(value, 0), totalSteps.value - 1)
}

watch(totalSteps, () => {
  currentStep.value = clampStep(currentStep.value)
})

watch(
  () => settings.displayMode,
  (mode, previous) => {
    if (totalSteps.value === 0) {
      currentStep.value = 0
      return
    }
    if (previous === 'double' && mode !== 'double') {
      currentStep.value = clampStep(currentStep.value * 2)
    }
    if (mode === 'double' && previous !== 'double') {
      currentStep.value = clampStep(Math.floor(currentStep.value / 2))
    }
    if (mode === 'scroll') {
      void scrollToStep(currentStep.value)
    }
  },
)

watch(orderedImages, () => {
  currentStep.value = clampStep(currentStep.value)
})

const currentImages = computed<ViewerImage[]>(() => {
  if (isScrollMode.value) {
    return []
  }
  if (orderedImages.value.length === 0) {
    return []
  }
  if (isDoubleMode.value) {
    const start = currentStep.value * 2
    return orderedImages.value.slice(start, start + 2)
  }
  const image = orderedImages.value[currentStep.value]
  return image ? [image] : []
})

const resolvedQualityWidth = computed(() => {
  switch (settings.qualityPreset) {
    case '720p': {
      return 1280
    }
    case '1080p': {
      return 1920
    }
    case 'custom': {
      return Math.max(256, settings.customQualityWidth)
    }
    default: {
      return null
    }
  }
})

const viewerStyle = computed(() => ({
  backgroundColor: settings.backgroundColor,
}))

const imageClass = computed(() => {
  const classes: string[] = [
    'select-none',
    'object-contain',
    'transition-transform',
    'duration-200',
  ]
  if (isScrollMode.value) {
    classes.push('w-full')
  } else {
    classes.push('max-h-[85vh]')
  }
  if (settings.zoomMode === 'fit-height') {
    classes.push('h-full', 'w-auto')
  } else if (settings.zoomMode === 'original') {
    classes.push('h-auto', 'w-auto')
  } else {
    classes.push('max-w-full')
  }
  return classes.join(' ')
})

const pageWrapperClass = computed(() => {
  const classes: string[] = [
    'viewer-page',
    'flex',
    'items-center',
    'justify-center',
  ]
  if (isScrollMode.value || orientation.value === 'vertical') {
    classes.push('w-full')
  }
  if (
    !isScrollMode.value &&
    isDoubleMode.value &&
    orientation.value === 'horizontal'
  ) {
    classes.push('flex-1')
  }
  return classes.join(' ')
})

const stageClass = computed(() => {
  const classes: string[] = [
    'relative',
    'flex',
    'h-full',
    'w-full',
    'items-center',
    'justify-center',
    'gap-6',
  ]
  if (isScrollMode.value) {
    classes.push('overflow-auto', 'p-6')
    if (orientation.value === 'vertical') {
      classes.push(isReverseFlow.value ? 'flex-col-reverse' : 'flex-col')
    } else {
      classes.push(isReverseFlow.value ? 'flex-row-reverse' : 'flex-row')
    }
  } else if (isDoubleMode.value) {
    if (orientation.value === 'vertical') {
      classes.push('flex-col')
    } else {
      classes.push('flex-row')
    }
  } else {
    classes.push('flex-col')
  }
  return classes.join(' ')
})

const pageDisplayText = computed(() => {
  if (totalPages.value === 0) {
    return '0 / 0'
  }
  if (!isScrollMode.value && isDoubleMode.value) {
    const start = currentStep.value * 2 + 1
    const end = Math.min(start + 1, totalPages.value)
    return start === end
      ? `${start} / ${totalPages.value}`
      : `${start}-${end} / ${totalPages.value}`
  }
  return `${Math.min(currentStep.value + 1, totalPages.value)} / ${totalPages.value}`
})

const sliderMax = computed(() => Math.max(totalSteps.value - 1, 0))

const resolveImageUrl = (imageId: string) => {
  const width = resolvedQualityWidth.value ?? 0
  const requestUrl = comicApi.url.getComicImage.format({
    comicImageId: imageId,
    maxWidth: width,
  })
  return `${apiURL}${requestUrl}`
}

const imageElementMap = new Map<string, HTMLElement>()

const assignImageRef = (imageId: string, el: HTMLElement | null) => {
  if (isScrollMode.value && el) {
    imageElementMap.set(imageId, el)
    return
  }

  if (!el) {
    imageElementMap.delete(imageId)
  }
}

const scrollToStep = async (step: number) => {
  if (!isScrollMode.value) {
    return
  }
  await nextTick()
  const image = orderedImages.value[step]
  if (!image) {
    return
  }
  const target = imageElementMap.get(image.id)
  if (!target) {
    return
  }
  target.scrollIntoView({
    behavior: 'smooth',
    block: 'center',
    inline: 'center',
  })
}

watch(currentStep, (step) => {
  if (isScrollMode.value) {
    void scrollToStep(step)
  }
})

const {
  isFullscreen,
  enter: enterFullscreen,
  exit: exitFullscreen,
  toggle,
} = useFullscreen(viewerContainerRef)

watch(
  () => settings.alwaysFullscreen,
  (value) => {
    if (value) {
      void enterFullscreen()
    } else if (isFullscreen.value) {
      void exitFullscreen()
    }
  },
  { immediate: true },
)

const comicTitle = computed(() => {
  return (
    comicDetail.value?.title?.trim() ?? $t('page.comic.placeholder.untitled')
  )
})

const goToPrevious = () => {
  currentStep.value = clampStep(currentStep.value - 1)
}

const goToNext = () => {
  currentStep.value = clampStep(currentStep.value + 1)
}

const goToFirst = () => {
  if (totalSteps.value === 0) {
    return
  }
  currentStep.value = 0
}

const goToLast = () => {
  if (totalSteps.value === 0) {
    return
  }
  currentStep.value = totalSteps.value - 1
}

const toggleUi = () => {
  isUiVisible.value = !isUiVisible.value
}

const handleRegionClick = (
  region: 'bottom' | 'center' | 'left' | 'right' | 'top',
) => {
  if (region === 'center') {
    toggleUi()
    return
  }

  const isHorizontal = orientation.value === 'horizontal'

  if (isHorizontal) {
    if (region === 'left') {
      if (isReverseFlow.value) {
        goToNext()
      } else {
        goToPrevious()
      }
      return
    }
    if (region === 'right') {
      if (isReverseFlow.value) {
        goToPrevious()
      } else {
        goToNext()
      }
      return
    }
  } else {
    if (region === 'top') {
      if (isReverseFlow.value) {
        goToNext()
      } else {
        goToPrevious()
      }
      return
    }
    if (region === 'bottom') {
      if (isReverseFlow.value) {
        goToPrevious()
      } else {
        goToNext()
      }
      return
    }
  }

  toggleUi()
}

const handleBack = () => {
  router.back()
}

const handleOpenSettings = async () => {
  const result = await open(ComicSettingsDialog, {
    props: { initialSettings: { ...settings } },
    title: $t('page.comic.settings.dialogTitle'),
    width: 520,
    dialog: {
      closeOnClickModal: false,
    },
  })
  if (result) {
    Object.assign(settings, result)
  }
}

const handleToggleFullscreen = async () => {
  await toggle()
}

const handleHelp = () => {
  ElMessage.info($t('page.comic.messages.help'))
}

const fetchComic = async () => {
  const id = comicId.value
  if (!id) {
    loadError.value = $t('page.comic.messages.loadFailed')
    return
  }
  isLoading.value = true
  loadError.value = null
  try {
    const [detail, imageList] = await Promise.all([
      comicApi.getById(id),
      comicApi.getImagesByComicId(id),
    ])
    comicDetail.value = detail
    images.value = (imageList ?? []).map((item, index) => ({
      ...item,
      order: index,
    }))
    currentStep.value = 0
    if (isScrollMode.value) {
      await nextTick()
      void scrollToStep(0)
    }
  } catch (error) {
    const message =
      error instanceof Error ? error.message : String(error ?? 'Unknown error')
    loadError.value = message
    ElMessage.error($t('page.comic.messages.loadFailed'))
  } finally {
    isLoading.value = false
  }
}

watch(
  comicId,
  async () => {
    await fetchComic()
  },
  { immediate: true },
)

useEventListener(window, 'keydown', (event: KeyboardEvent) => {
  if (totalSteps.value === 0) {
    return
  }
  if (event.key === ' ') {
    event.preventDefault()
    toggleUi()
    return
  }
  const isHorizontal = orientation.value === 'horizontal'
  if (isHorizontal) {
    if (event.key === 'ArrowRight') {
      event.preventDefault()
      if (isReverseFlow.value) {
        goToPrevious()
      } else {
        goToNext()
      }
    }
    if (event.key === 'ArrowLeft') {
      event.preventDefault()
      if (isReverseFlow.value) {
        goToNext()
      } else {
        goToPrevious()
      }
    }
  } else {
    if (event.key === 'ArrowDown') {
      event.preventDefault()
      if (isReverseFlow.value) {
        goToPrevious()
      } else {
        goToNext()
      }
    }
    if (event.key === 'ArrowUp') {
      event.preventDefault()
      if (isReverseFlow.value) {
        goToNext()
      } else {
        goToPrevious()
      }
    }
  }
})

const retry = async () => {
  await fetchComic()
}
</script>

<template>
  <div
    ref="viewerContainerRef"
    class="comic-viewer relative flex h-screen min-h-screen flex-col text-white"
    :style="viewerStyle"
  >
    <transition name="fade-fast">
      <header
        v-if="isUiVisible"
        class="viewer-header pointer-events-auto flex items-center justify-between bg-black/70 px-6 py-4"
      >
        <div class="flex items-center gap-4">
          <el-button
            class="text-white"
            link
            :icon="MdiArrowLeft"
            @click="handleBack"
          >
            {{ $t('page.comic.actions.back') }}
          </el-button>
          <div class="text-lg font-semibold">
            {{ comicTitle }}
          </div>
        </div>
        <div class="flex items-center gap-2">
          <el-tooltip :content="$t('page.comic.actions.settings')">
            <el-button
              circle
              class="text-white"
              :icon="MdiCog"
              @click="handleOpenSettings"
            />
          </el-tooltip>
          <el-tooltip
            :content="
              isFullscreen
                ? $t('page.comic.actions.exitFullscreen')
                : $t('page.comic.actions.fullscreen')
            "
          >
            <el-button
              circle
              class="text-white"
              :icon="isFullscreen ? MdiFullscreenExit : MdiFullscreen"
              @click="handleToggleFullscreen"
            />
          </el-tooltip>
          <el-tooltip :content="$t('page.comic.actions.help')">
            <el-button
              circle
              class="text-white"
              :icon="MdiHelpCircle"
              @click="handleHelp"
            />
          </el-tooltip>
        </div>
      </header>
    </transition>

    <div class="viewer-main relative flex-1 overflow-hidden">
      <div
        v-if="isLoading"
        class="state flex h-full items-center justify-center text-base"
      >
        {{ $t('page.comic.states.loading') }}
      </div>
      <div
        v-else-if="loadError"
        class="state flex h-full flex-col items-center justify-center gap-4 text-base"
      >
        <div>{{ $t('page.comic.states.error') }}</div>
        <el-button type="primary" @click="retry">
          {{ $t('page.comic.actions.retry') }}
        </el-button>
      </div>
      <div
        v-else-if="totalPages === 0"
        class="state flex h-full items-center justify-center text-base"
      >
        {{ $t('page.comic.states.empty') }}
      </div>
      <div v-else ref="stageContainerRef" :class="stageClass">
        <template v-if="isScrollMode">
          <figure
            v-for="image in orderedImages"
            :key="image.id"
            :ref="(el) => assignImageRef(image.id, el as HTMLElement | null)"
            :class="pageWrapperClass"
          >
            <img
              :alt="image.name ?? image.id"
              :class="imageClass"
              :src="resolveImageUrl(image.id)"
              loading="lazy"
            />
          </figure>
        </template>
        <template v-else>
          <figure
            v-for="image in currentImages"
            :key="image.id"
            :class="pageWrapperClass"
          >
            <img
              :alt="image.name ?? image.id"
              :class="imageClass"
              :src="resolveImageUrl(image.id)"
              loading="lazy"
            />
          </figure>
        </template>

        <div
          class="interaction-layer pointer-events-none absolute inset-0 grid grid-cols-3 grid-rows-3"
        >
          <button
            class="pointer-events-auto border-none bg-transparent"
            @click="handleRegionClick('top')"
          ></button>
          <button
            class="pointer-events-auto border-none bg-transparent"
            @click="handleRegionClick('top')"
          ></button>
          <button
            class="pointer-events-auto border-none bg-transparent"
            @click="handleRegionClick('top')"
          ></button>
          <button
            class="pointer-events-auto border-none bg-transparent"
            @click="handleRegionClick('left')"
          ></button>
          <button
            class="pointer-events-auto border-none bg-transparent"
            @click="handleRegionClick('center')"
          ></button>
          <button
            class="pointer-events-auto border-none bg-transparent"
            @click="handleRegionClick('right')"
          ></button>
          <button
            class="pointer-events-auto border-none bg-transparent"
            @click="handleRegionClick('bottom')"
          ></button>
          <button
            class="pointer-events-auto border-none bg-transparent"
            @click="handleRegionClick('bottom')"
          ></button>
          <button
            class="pointer-events-auto border-none bg-transparent"
            @click="handleRegionClick('bottom')"
          ></button>
        </div>
      </div>
    </div>

    <transition name="fade-fast">
      <footer
        v-if="isUiVisible"
        class="viewer-footer pointer-events-auto flex items-center justify-between bg-black/70 px-6 py-4"
      >
        <div class="flex items-center gap-2">
          <el-tooltip :content="$t('page.comic.actions.firstPage')">
            <el-button
              circle
              class="text-white"
              :disabled="totalSteps === 0 || currentStep === 0"
              :icon="MdiPageFirst"
              @click="goToFirst"
            />
          </el-tooltip>
        </div>
        <div class="flex flex-1 flex-col items-center gap-2 px-6">
          <el-slider
            v-model="currentStep"
            :disabled="totalSteps === 0"
            :max="sliderMax"
            :min="0"
            :step="1"
            :show-tooltip="false"
            class="w-full max-w-3xl"
          />
          <div class="text-xs text-white/80">
            {{ pageDisplayText }}
          </div>
        </div>
        <div class="flex items-center gap-2">
          <el-tooltip :content="$t('page.comic.actions.lastPage')">
            <el-button
              circle
              class="text-white"
              :disabled="totalSteps === 0 || currentStep === sliderMax"
              :icon="MdiPageLast"
              @click="goToLast"
            />
          </el-tooltip>
        </div>
      </footer>
    </transition>
  </div>
</template>

<style scoped>
.fade-fast-enter-active,
.fade-fast-leave-active {
  transition: opacity 0.2s ease;
}

.fade-fast-enter-from,
.fade-fast-leave-to {
  opacity: 0;
}

.viewer-header,
.viewer-footer {
  backdrop-filter: blur(12px);
}

.viewer-page img {
  border-radius: 0.75rem;
  box-shadow: 0 20px 45px rgb(0 0 0 / 30%);
}

@media (max-width: 768px) {
  .viewer-header,
  .viewer-footer {
    padding-right: 1rem;
    padding-left: 1rem;
  }
}
</style>

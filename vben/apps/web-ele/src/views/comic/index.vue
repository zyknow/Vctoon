<script setup lang="ts">
import type { CSSProperties } from 'vue'
import type { LocationQueryRaw } from 'vue-router'

import type { Comic, ComicImage } from '@vben/api'

import { computed, nextTick, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { comicApi, mediumResourceApi, MediumType } from '@vben/api'
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

import { useEventListener, useFullscreen } from '@vueuse/core'
import { ElMessage } from 'element-plus'

import { $t } from '#/locales'
import { useComicStore } from '#/store'

import ComicSettingsDialog from './components/comic-settings-dialog.vue'
import { isReverseDirection, isVerticalDirection } from './types'

defineOptions({ name: 'ComicReaderPage' })

type ViewerImage = ComicImage & { order: number }

const route = useRoute()
const router = useRouter()
const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)

const viewerContainerRef = ref<HTMLElement | null>(null)
const stageContainerRef = ref<HTMLElement | null>(null)
const settingsDrawerVisible = ref(false)

const showHelpOverlay = ref(false)
const helpOverlayKey = ref(0)
const pendingPageFromRoute = ref<null | number>(null)
const lastSyncedPage = ref<null | number>(null)

const parseBooleanQuery = (value: unknown) => {
  if (Array.isArray(value)) {
    return value.some((item) => item === '1' || item === 'true')
  }
  return value === '1' || value === 'true'
}

const isIncognito = computed(() => parseBooleanQuery(route.query.incognito))

const shouldTrackProgress = computed(() => !isIncognito.value)

const comicStore = useComicStore()
const settings = comicStore.settings

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

const imageMaxHeight = computed(() => {
  return '100vh'
})

const stageStyle = computed<CSSProperties>(() => {
  const spacing = Math.max(0, settings.imageSpacing)

  return {
    gap: `${spacing}px`,
    padding: '0px',
  }
})

const isSyncingFromScroll = ref(false)
const pendingScrollStep = ref<null | number>(null)
let scrollSyncTimer: number | undefined

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

const parsePageQueryValue = (value: unknown) => {
  const raw = Array.isArray(value) ? value[value.length - 1] : value
  if (typeof raw !== 'string') {
    return null
  }
  const parsed = Number.parseInt(raw, 10)
  if (Number.isNaN(parsed) || parsed <= 0) {
    return null
  }
  return parsed
}

const clampPage = (page: number) => {
  if (totalPages.value <= 0) {
    return 1
  }
  return Math.min(Math.max(page, 1), totalPages.value)
}

const pageToStep = (page: number) => {
  const bounded = clampPage(page)
  if (isScrollMode.value) {
    return clampStep(bounded - 1)
  }
  if (isDoubleMode.value) {
    return clampStep(Math.floor((bounded - 1) / 2))
  }
  return clampStep(bounded - 1)
}

const clampStep = (value: number) => {
  if (totalSteps.value === 0) {
    return 0
  }
  return Math.min(Math.max(value, 0), totalSteps.value - 1)
}

const getPageForStep = (step: number) => {
  if (totalPages.value === 0) {
    return 1
  }
  if (isScrollMode.value) {
    return clampPage(step + 1)
  }
  if (isDoubleMode.value) {
    return clampPage(step * 2 + 1)
  }
  return clampPage(step + 1)
}

let updatingFromRoute = false
let updatingRoute = false

const applyPageFromRoute = (page: number) => {
  const bounded = clampPage(page)
  updatingFromRoute = true
  currentStep.value = pageToStep(bounded)
  lastSyncedPage.value = bounded
  void nextTick(() => {
    updatingFromRoute = false
  })
}

const tryApplyPendingPage = () => {
  if (pendingPageFromRoute.value === null) {
    return
  }
  if (totalPages.value === 0) {
    return
  }
  const page = clampPage(pendingPageFromRoute.value)
  if (page === lastSyncedPage.value && !updatingFromRoute) {
    pendingPageFromRoute.value = null
    return
  }
  pendingPageFromRoute.value = null
  applyPageFromRoute(page)
}

watch(
  () => route.query.page,
  (pageQuery) => {
    if (updatingRoute) {
      return
    }
    const parsed = parsePageQueryValue(pageQuery)
    if (parsed === null) {
      pendingPageFromRoute.value = null
      return
    }
    pendingPageFromRoute.value = parsed
    tryApplyPendingPage()
  },
  { immediate: true },
)

watch([totalPages, () => pendingPageFromRoute.value], () => {
  tryApplyPendingPage()
})

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
    case '480p': {
      return 854
    }
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
    'transition-transform',
    'duration-200',
    'viewer-image',
  ]

  if (isScrollMode.value) {
    if (orientation.value === 'vertical') {
      classes.push('w-full')
    } else {
      classes.push('h-full')
    }
  }

  return classes.join(' ')
})

const imageStyle = computed<CSSProperties>(() => {
  const style: CSSProperties = {
    objectFit: 'contain',
    maxHeight: imageMaxHeight.value,
    maxWidth: '100%',
  }

  if (isScrollMode.value) {
    if (orientation.value === 'vertical') {
      style.width = '100%'
      style.height = 'auto'
      style.maxWidth = '100%'
      style.maxHeight = 'none'
    } else {
      style.height = imageMaxHeight.value
      style.width = 'auto'
      style.maxHeight = imageMaxHeight.value
      style.maxWidth = 'none'
    }
  } else {
    style.maxWidth = '100%'
    style.height = 'auto'
    style.width = 'auto'
  }

  switch (settings.zoomMode) {
    case 'fit-height': {
      style.height = imageMaxHeight.value
      style.maxHeight = imageMaxHeight.value
      style.width = 'auto'
      style.maxWidth = 'none'
      break
    }
    case 'fit-width': {
      style.width = '100%'
      style.maxWidth = '100%'
      style.height = 'auto'
      style.maxHeight = 'none'
      break
    }
    case 'original': {
      style.objectFit = 'initial'
      style.maxWidth = 'none'
      style.maxHeight = 'none'
      style.width = 'auto'
      style.height = 'auto'
      break
    }
    default: {
      style.maxWidth = '100%'
      style.maxHeight = imageMaxHeight.value
      break
    }
  }

  return style
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
    'stage-container',
  ]
  if (isScrollMode.value) {
    classes.push('items-start', 'justify-start', 'overflow-auto')
    if (orientation.value === 'vertical') {
      classes.push(isReverseFlow.value ? 'flex-col-reverse' : 'flex-col')
    } else {
      classes.push(isReverseFlow.value ? 'flex-row-reverse' : 'flex-row')
    }
  } else {
    classes.push('items-center', 'justify-center')
    if (settings.zoomMode === 'original') {
      classes.push('overflow-auto')
    } else {
      classes.push('overflow-hidden')
    }

    if (isDoubleMode.value) {
      if (orientation.value === 'vertical') {
        classes.push('flex-col')
      } else {
        classes.push('flex-row')
      }
    } else {
      classes.push('flex-col')
    }
  }
  return classes.join(' ')
})

const isHorizontalOrientation = computed(
  () => orientation.value === 'horizontal',
)

const helpOverlaySegments = computed(() => {
  const previousLabel = $t('page.comic.hints.previous')
  const nextLabel = $t('page.comic.hints.next')
  const toggleLabel = $t('page.comic.hints.toggleUi')

  if (isHorizontalOrientation.value) {
    return [
      {
        key: 'left',
        label: isReverseFlow.value ? nextLabel : previousLabel,
        position: 'left',
      },
      {
        key: 'center',
        label: toggleLabel,
        position: 'center',
      },
      {
        key: 'right',
        label: isReverseFlow.value ? previousLabel : nextLabel,
        position: 'right',
      },
    ] as const
  }

  return [
    {
      key: 'top',
      label: isReverseFlow.value ? nextLabel : previousLabel,
      position: 'top',
    },
    {
      key: 'center',
      label: toggleLabel,
      position: 'center',
    },
    {
      key: 'bottom',
      label: isReverseFlow.value ? previousLabel : nextLabel,
      position: 'bottom',
    },
  ] as const
})

const helpOverlayDismissText = computed(() => {
  const text = $t('page.comic.messages.helpDismiss')
  if (text && text !== 'page.comic.messages.helpDismiss') {
    return text
  }
  return '点击任意位置关闭'
})

const currentPageRange = computed(() => {
  if (totalPages.value === 0) {
    return { end: 0, start: 0 }
  }
  if (!isScrollMode.value && isDoubleMode.value) {
    const start = currentStep.value * 2 + 1
    const end = Math.min(start + 1, totalPages.value)
    return { start, end }
  }
  const page = Math.min(currentStep.value + 1, totalPages.value)
  return { start: page, end: page }
})

const currentPageLabel = computed(() => {
  const { start, end } = currentPageRange.value
  if (start === 0 && end === 0) {
    return '0'
  }
  return start === end ? `${start}` : `${start}-${end}`
})

const totalPagesLabel = computed(() => `${totalPages.value}`)

const currentProgress = computed(() => {
  if (totalPages.value === 0) {
    return 0
  }
  const { end } = currentPageRange.value
  return Math.min(Math.max(end / totalPages.value, 0), 1)
})

let lastSavedProgress = -1
let progressDirty = false

const saveProgress = async () => {
  if (!shouldTrackProgress.value) {
    progressDirty = false
    return
  }
  if (!progressDirty) {
    return
  }
  const id = comicId.value
  if (!id || totalPages.value === 0) {
    return
  }
  const progress = Number.isFinite(currentProgress.value)
    ? Number.parseFloat(currentProgress.value.toFixed(4))
    : 0
  if (!Number.isFinite(progress)) {
    return
  }
  if (
    progress >= 0 &&
    lastSavedProgress >= 0 &&
    Math.abs(progress - lastSavedProgress) < 0.001
  ) {
    return
  }
  try {
    await mediumResourceApi.updateReadingProcess([
      {
        mediumId: id,
        mediumType: MediumType.Comic,
        progress,
        readingLastTime: new Date().toISOString(),
      },
    ])
    lastSavedProgress = progress
    progressDirty = false
  } catch (error) {
    console.error('Failed to update reading progress', error)
  }
}

const sliderMax = computed(() => Math.max(totalSteps.value - 1, 0))

const sliderValue = computed<number>({
  get() {
    if (sliderMax.value === 0) {
      return 0
    }
    return isReverseFlow.value
      ? sliderMax.value - currentStep.value
      : currentStep.value
  },
  set(value) {
    const normalized = Number.isFinite(value) ? Math.round(value) : 0
    const bounded = clampStep(normalized)
    const reversedTarget = isReverseFlow.value
      ? sliderMax.value - bounded
      : bounded
    const targetStep = clampStep(reversedTarget)
    pendingScrollStep.value = isScrollMode.value ? targetStep : null
    currentStep.value = targetStep
  },
})

const resolveImageUrl = (imageId: string) => {
  const width = resolvedQualityWidth.value

  if (!width) {
    const template = comicApi.url.getComicImage.replace(
      '?maxWidth={maxWidth}',
      '',
    )
    const requestUrl = template.format({ comicImageId: imageId })
    return `${apiURL}${requestUrl}`
  }

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
  isSyncingFromScroll.value = true
  await nextTick()
  const image = orderedImages.value[step]
  if (!image) {
    isSyncingFromScroll.value = false
    return
  }
  const target = imageElementMap.get(image.id)
  if (!target) {
    isSyncingFromScroll.value = false
    return
  }
  const behavior: ScrollBehavior = settings.pageTransition ? 'smooth' : 'auto'
  target.scrollIntoView({
    behavior,
    block: 'center',
    inline: 'center',
  })

  if (scrollSyncTimer) {
    window.clearTimeout(scrollSyncTimer)
  }
  scrollSyncTimer = window.setTimeout(
    () => {
      isSyncingFromScroll.value = false
      scrollSyncTimer = undefined
    },
    behavior === 'smooth' ? 360 : 0,
  )
}

const updateStepFromScroll = () => {
  const container = stageContainerRef.value
  if (
    !container ||
    !isScrollMode.value ||
    isSyncingFromScroll.value ||
    orderedImages.value.length === 0
  ) {
    return
  }

  const containerRect = container.getBoundingClientRect()
  const isVertical = orientation.value === 'vertical'
  const centerPosition = isVertical
    ? containerRect.top + containerRect.height / 2
    : containerRect.left + containerRect.width / 2

  const edgeThreshold = isVertical
    ? Math.max(containerRect.height * 0.08, 32)
    : Math.max(containerRect.width * 0.08, 32)

  let closestIndex = currentStep.value
  let closestDistance = Number.POSITIVE_INFINITY

  const firstImage = orderedImages.value[0]
  const lastIndex = orderedImages.value.length - 1
  const lastImage = lastIndex >= 0 ? orderedImages.value[lastIndex] : undefined

  if (firstImage) {
    const element = imageElementMap.get(firstImage.id)
    if (element) {
      const rect = element.getBoundingClientRect()
      let distanceToStart: number
      if (isVertical) {
        distanceToStart = isReverseFlow.value
          ? Math.abs(containerRect.bottom - rect.bottom)
          : Math.abs(rect.top - containerRect.top)
      } else {
        distanceToStart = isReverseFlow.value
          ? Math.abs(containerRect.right - rect.right)
          : Math.abs(rect.left - containerRect.left)
      }
      if (distanceToStart <= edgeThreshold) {
        closestIndex = 0
        closestDistance = 0
      }
    }
  }

  if (lastImage) {
    const element = imageElementMap.get(lastImage.id)
    if (element) {
      const rect = element.getBoundingClientRect()
      let distanceToEnd: number
      if (isVertical) {
        distanceToEnd = isReverseFlow.value
          ? Math.abs(rect.top - containerRect.top)
          : Math.abs(containerRect.bottom - rect.bottom)
      } else {
        distanceToEnd = isReverseFlow.value
          ? Math.abs(rect.left - containerRect.left)
          : Math.abs(containerRect.right - rect.right)
      }
      if (distanceToEnd <= edgeThreshold) {
        closestIndex = lastIndex
        closestDistance = 0
      }
    }
  }

  for (let index = 0; index < orderedImages.value.length; index += 1) {
    if (closestDistance === 0 && (index === 0 || index === lastIndex)) {
      continue
    }
    const item = orderedImages.value[index]
    if (!item) {
      continue
    }
    const element = imageElementMap.get(item.id)
    if (!element) {
      continue
    }

    const rect = element.getBoundingClientRect()
    const elementCenter = isVertical
      ? rect.top + rect.height / 2
      : rect.left + rect.width / 2

    const distance = Math.abs(elementCenter - centerPosition)

    if (distance < closestDistance) {
      closestDistance = distance
      closestIndex = index
    }
  }

  if (closestIndex !== currentStep.value) {
    const previousStep = currentStep.value
    const nextStep = clampStep(closestIndex)
    if (totalSteps.value > 0) {
      if (nextStep === 0 && previousStep !== 0) {
        showBoundaryNotice('first')
      } else if (
        nextStep === totalSteps.value - 1 &&
        previousStep !== totalSteps.value - 1
      ) {
        showBoundaryNotice('last')
      }
    }
    isSyncingFromScroll.value = true
    currentStep.value = nextStep
    requestAnimationFrame(() => {
      isSyncingFromScroll.value = false
    })
  }
}

watch(currentStep, (step) => {
  if (!isScrollMode.value) {
    return
  }
  if (isSyncingFromScroll.value) {
    return
  }
  void scrollToStep(step)
})

watch(isScrollMode, (value) => {
  imageElementMap.clear()
  if (value) {
    requestAnimationFrame(updateStepFromScroll)
  }
})

useEventListener(stageContainerRef, 'scroll', () => {
  if (!isScrollMode.value) {
    return
  }
  requestAnimationFrame(updateStepFromScroll)
})

useEventListener(
  stageContainerRef,
  'wheel',
  (event: WheelEvent) => {
    const container = stageContainerRef.value
    if (!container) {
      return
    }

    if (isScrollMode.value) {
      if (orientation.value !== 'horizontal') {
        return
      }
      event.preventDefault()
      const behavior: ScrollBehavior = settings.pageTransition
        ? 'smooth'
        : 'auto'
      container.scrollBy({ left: event.deltaY, behavior })
      return
    }

    const primaryDelta =
      Math.abs(event.deltaY) >= Math.abs(event.deltaX)
        ? event.deltaY
        : event.deltaX

    if (!primaryDelta) {
      return
    }

    event.preventDefault()

    const forward = primaryDelta > 0
    if (isReverseFlow.value) {
      if (forward) {
        goToPrevious()
      } else {
        goToNext()
      }
    } else if (forward) {
      goToNext()
    } else {
      goToPrevious()
    }
  },
  { passive: false },
)

watch(
  [isScrollMode, orientation],
  () => {
    if (!isScrollMode.value) {
      return
    }
    requestAnimationFrame(updateStepFromScroll)
  },
  { flush: 'post' },
)

watch(
  currentStep,
  (step) => {
    if (totalPages.value === 0) {
      return
    }
    if (pendingPageFromRoute.value !== null) {
      return
    }
    if (isScrollMode.value) {
      const hasPending = pendingScrollStep.value !== null
      if (hasPending || !isSyncingFromScroll.value) {
        const targetStep = clampStep(
          hasPending && pendingScrollStep.value !== null
            ? pendingScrollStep.value
            : step,
        )
        pendingScrollStep.value = null
        void scrollToStep(targetStep)
      }
    } else {
      pendingScrollStep.value = null
    }
    if (!updatingFromRoute && shouldTrackProgress.value) {
      progressDirty = true
    }
    const canonicalPage = getPageForStep(step)
    if (updatingFromRoute) {
      lastSyncedPage.value = canonicalPage
      return
    }
    if (lastSyncedPage.value === canonicalPage) {
      return
    }
    lastSyncedPage.value = canonicalPage
    const currentRoutePage = parsePageQueryValue(route.query.page)
    const nextQuery: LocationQueryRaw = {
      ...route.query,
    }
    if (canonicalPage <= 1) {
      if (!('page' in route.query)) {
        return
      }
      delete nextQuery.page
    } else {
      if (currentRoutePage === canonicalPage) {
        return
      }
      nextQuery.page = String(canonicalPage)
    }
    updatingRoute = true
    void router.replace({ query: nextQuery }).finally(() => {
      updatingRoute = false
    })
  },
  { flush: 'post' },
)

useEventListener(window, 'visibilitychange', () => {
  if (document.visibilityState === 'hidden') {
    void saveProgress()
  }
})

useEventListener(window, 'pagehide', () => {
  void saveProgress()
})

useEventListener(window, 'beforeunload', () => {
  void saveProgress()
})

useEventListener(window, 'blur', () => {
  void saveProgress()
})

const {
  isFullscreen,
  enter: enterFullscreen,
  exit: exitFullscreen,
  toggle,
} = useFullscreen(viewerContainerRef)

const forcedFullscreen = ref(false)

watch(
  () => settings.alwaysFullscreen,
  async (value) => {
    if (value) {
      if (isFullscreen.value) {
        forcedFullscreen.value = false
        return
      }
      forcedFullscreen.value = true
      await enterFullscreen()
    } else if (forcedFullscreen.value) {
      if (isFullscreen.value) {
        await exitFullscreen()
      }
      forcedFullscreen.value = false
    }
  },
  { immediate: true },
)

watch(
  isFullscreen,
  async (value) => {
    if (!settings.alwaysFullscreen) {
      forcedFullscreen.value = false
      return
    }
    if (value) {
      forcedFullscreen.value = false
      return
    }
    forcedFullscreen.value = true
    await enterFullscreen()
  },
  { flush: 'post' },
)

const comicTitle = computed(() => {
  return (
    comicDetail.value?.title?.trim() ?? $t('page.comic.placeholder.untitled')
  )
})

let boundaryResetTimer: number | undefined
let lastBoundaryNotice: 'first' | 'last' | null = null

const showBoundaryNotice = (type: 'first' | 'last') => {
  if (lastBoundaryNotice === type) {
    return
  }
  if (boundaryResetTimer) {
    window.clearTimeout(boundaryResetTimer)
    boundaryResetTimer = undefined
  }
  lastBoundaryNotice = type
  const messageKey =
    type === 'first'
      ? 'page.comic.messages.firstPage'
      : 'page.comic.messages.lastPage'
  ElMessage.warning({
    message: $t(messageKey),
    duration: 1800,
    grouping: true,
  })
  boundaryResetTimer = window.setTimeout(() => {
    lastBoundaryNotice = null
    boundaryResetTimer = undefined
  }, 1500)
}

const goToPrevious = () => {
  if (currentStep.value <= 0) {
    showBoundaryNotice('first')
    return
  }
  currentStep.value = clampStep(currentStep.value - 1)
}

const goToNext = () => {
  if (totalSteps.value === 0 || currentStep.value >= totalSteps.value - 1) {
    showBoundaryNotice('last')
    return
  }
  currentStep.value = clampStep(currentStep.value + 1)
}

const goToFirst = () => {
  if (totalSteps.value === 0) {
    return
  }
  if (currentStep.value === 0) {
    showBoundaryNotice('first')
    return
  }
  currentStep.value = 0
}

const goToLast = () => {
  if (totalSteps.value === 0) {
    return
  }
  if (currentStep.value === totalSteps.value - 1) {
    showBoundaryNotice('last')
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

const handleStageClick = (event: MouseEvent) => {
  const container = stageContainerRef.value
  if (!container) {
    return
  }

  const rect = container.getBoundingClientRect()
  if (rect.width === 0 || rect.height === 0) {
    handleRegionClick('center')
    return
  }

  const x = (event.clientX - rect.left) / rect.width
  const y = (event.clientY - rect.top) / rect.height

  if (x < 1 / 3) {
    handleRegionClick('left')
    return
  }
  if (x > 2 / 3) {
    handleRegionClick('right')
    return
  }
  if (y < 1 / 3) {
    handleRegionClick('top')
    return
  }
  if (y > 2 / 3) {
    handleRegionClick('bottom')
    return
  }

  handleRegionClick('center')
}

const handleBack = async () => {
  await saveProgress()
  router.back()
}

const handleOpenSettings = () => {
  settingsDrawerVisible.value = true
}

const handleToggleFullscreen = async () => {
  await toggle()
}

const handleHelp = async () => {
  ElMessage.info($t('page.comic.messages.help'))
  if (showHelpOverlay.value) {
    showHelpOverlay.value = false
    await nextTick()
  }

  helpOverlayKey.value += 1
  showHelpOverlay.value = true
}

const handleHelpOverlayClose = () => {
  showHelpOverlay.value = false
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
    lastSavedProgress = detail.readingProgress ?? 0
    progressDirty = false
    imageElementMap.clear()
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

onBeforeUnmount(() => {
  if (boundaryResetTimer) {
    window.clearTimeout(boundaryResetTimer)
  }
  void saveProgress()
})
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

    <transition name="fade-fast">
      <div
        v-if="showHelpOverlay"
        :key="helpOverlayKey"
        class="touch-overlay absolute inset-0 z-40 flex flex-col"
        role="presentation"
        @click.stop="handleHelpOverlayClose"
      >
        <div
          class="touch-overlay__grid grid flex-1 gap-2"
          :class="isHorizontalOrientation ? 'grid-cols-3' : 'grid-rows-3'"
        >
          <div
            v-for="segment in helpOverlaySegments"
            :key="segment.key"
            class="touch-overlay__segment flex flex-col items-center justify-center px-4 py-8 text-base font-medium tracking-wide"
          >
            <span>{{ segment.label }}</span>
          </div>
        </div>
        <div
          class="touch-overlay__footer px-4 pb-6 text-center text-sm text-white/80"
        >
          {{ helpOverlayDismissText }}
        </div>
      </div>
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
      <div
        v-else
        ref="stageContainerRef"
        :class="stageClass"
        :style="stageStyle"
        @click="handleStageClick"
      >
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
              :style="imageStyle"
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
              :style="imageStyle"
              :src="resolveImageUrl(image.id)"
              loading="lazy"
            />
          </figure>
        </template>
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
        <div class="flex flex-1 flex-row items-center gap-8 px-6">
          <el-slider
            v-model="sliderValue"
            :disabled="totalSteps === 0"
            :max="sliderMax"
            :min="0"
            :step="1"
            :show-tooltip="false"
            class="w-full"
          />
          <div class="flex items-center gap-2 text-xs text-white/80">
            <span class="font-mono">{{ currentPageLabel }}</span>
            <span class="text-white/50">/</span>
            <span class="font-mono">{{ totalPagesLabel }}</span>
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

  <ComicSettingsDialog v-model="settingsDrawerVisible" />
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

.viewer-header {
  position: absolute;
  top: 0;
  right: 0;
  left: 0;
  z-index: 20;
}

.viewer-footer {
  position: absolute;
  right: 0;
  bottom: 0;
  left: 0;
  z-index: 20;
}

.stage-container {
  scrollbar-width: none;
  -ms-overflow-style: none;
}

.stage-container::-webkit-scrollbar {
  display: none;
}

.touch-overlay {
  display: flex;
  cursor: pointer;
  background-color: hsl(var(--background) / 80%);
  backdrop-filter: blur(8px);
}

.touch-overlay__grid,
.touch-overlay__footer {
  pointer-events: none;
}

.touch-overlay__segment {
  color: hsl(var(--foreground) / 92%);
  background-color: hsl(var(--background) / 60%);
  border: 1px solid hsl(var(--border) / 60%);
  border-radius: 0.75rem;
}

@media (max-width: 768px) {
  .viewer-header,
  .viewer-footer {
    padding-right: 1rem;
    padding-left: 1rem;
  }
}
</style>

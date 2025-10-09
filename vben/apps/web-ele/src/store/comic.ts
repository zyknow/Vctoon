import type { ComicViewerSettings } from '#/views/comic/types'

import { reactive, watch } from 'vue'

import { useLocalStorage } from '@vueuse/core'
import { defineStore } from 'pinia'

import {
  COMIC_VIEWER_STORAGE_KEY,
  DEFAULT_COMIC_VIEWER_SETTINGS,
  isVerticalDirection,
} from '#/views/comic/types'

export const useComicStore = defineStore('comic', () => {
  const settings = reactive<ComicViewerSettings>({
    ...DEFAULT_COMIC_VIEWER_SETTINGS,
  })

  const normalizeSettings = (
    value: ComicViewerSettings,
  ): ComicViewerSettings => {
    const next: ComicViewerSettings = { ...value }
    const shouldForceFitHeight =
      next.displayMode === 'scroll' &&
      !isVerticalDirection(next.readingDirection)
    if (shouldForceFitHeight) {
      next.zoomMode = 'fit-height'
    }
    return next
  }

  const storedSettings = useLocalStorage<ComicViewerSettings>(
    COMIC_VIEWER_STORAGE_KEY,
    DEFAULT_COMIC_VIEWER_SETTINGS,
    { mergeDefaults: true },
  )

  let syncingFromStorage = false

  const applyStoredSettings = (
    value: ComicViewerSettings | null | undefined,
  ) => {
    if (!value) {
      return
    }
    syncingFromStorage = true
    Object.assign(settings, normalizeSettings(value))
    syncingFromStorage = false
  }

  applyStoredSettings(storedSettings.value)

  watch(
    () => storedSettings.value,
    (value) => {
      applyStoredSettings(value)
    },
    { deep: true },
  )

  watch(
    settings,
    (value) => {
      if (syncingFromStorage) {
        return
      }
      storedSettings.value = normalizeSettings({ ...value })
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

  watch(
    () => settings.imageSpacing,
    (value) => {
      if (!Number.isFinite(value)) {
        settings.imageSpacing = 0
        return
      }
      if (value < 0) {
        settings.imageSpacing = 0
        return
      }
      settings.imageSpacing = Math.round(value)
    },
  )

  function setSettings(value: ComicViewerSettings) {
    Object.assign(settings, normalizeSettings(value))
  }

  function updateSettings(partial: Partial<ComicViewerSettings>) {
    const next: ComicViewerSettings = normalizeSettings({
      ...settings,
      ...partial,
    })
    Object.assign(settings, next)
  }

  function resetSettings() {
    setSettings({ ...DEFAULT_COMIC_VIEWER_SETTINGS })
  }

  watch(
    [
      () => settings.displayMode,
      () => settings.readingDirection,
      () => settings.zoomMode,
    ],
    ([displayMode, readingDirection, zoomMode]) => {
      const isHorizontalScroll =
        displayMode === 'scroll' && !isVerticalDirection(readingDirection)
      if (isHorizontalScroll && zoomMode !== 'fit-height') {
        settings.zoomMode = 'fit-height'
      }
    },
    { immediate: true },
  )

  return {
    resetSettings,
    setSettings,
    settings,
    updateSettings,
  }
})

import type { ComicViewerSettings } from '#/views/comic/types'

import { reactive, watch } from 'vue'

import { useLocalStorage } from '@vueuse/core'
import { defineStore } from 'pinia'

import {
  COMIC_VIEWER_STORAGE_KEY,
  DEFAULT_COMIC_VIEWER_SETTINGS,
} from '#/views/comic/types'

export const useComicStore = defineStore('comic', () => {
  const settings = reactive<ComicViewerSettings>({
    ...DEFAULT_COMIC_VIEWER_SETTINGS,
  })

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
    Object.assign(settings, value)
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
    Object.assign(settings, value)
  }

  function updateSettings(partial: Partial<ComicViewerSettings>) {
    Object.assign(settings, partial)
  }

  function resetSettings() {
    setSettings({ ...DEFAULT_COMIC_VIEWER_SETTINGS })
  }

  return {
    resetSettings,
    setSettings,
    settings,
    updateSettings,
  }
})

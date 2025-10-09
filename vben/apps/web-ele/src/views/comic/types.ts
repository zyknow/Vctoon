import type { ComicImage } from '@vben/api'

export type ReadingDirection = 'btt' | 'ltr' | 'rtl' | 'ttb'
export type DisplayMode = 'double' | 'scroll' | 'single'
export type ZoomMode = 'fit-height' | 'fit-screen' | 'fit-width' | 'original'
export type ComicQualityPreset =
  | '480p'
  | '720p'
  | '1080p'
  | 'custom'
  | 'original'

export interface ComicViewerSettings {
  alwaysFullscreen: boolean
  backgroundColor: string
  customQualityWidth: number
  displayMode: DisplayMode
  imageSpacing: number
  pageTransition: boolean
  qualityPreset: ComicQualityPreset
  readingDirection: ReadingDirection
  zoomMode: ZoomMode
}

export const COMIC_VIEWER_STORAGE_KEY = 'vctoon:comic-viewer-settings'

export const DEFAULT_COMIC_VIEWER_SETTINGS: ComicViewerSettings = {
  readingDirection: 'rtl',
  displayMode: 'single',
  zoomMode: 'fit-screen',
  qualityPreset: '1080p',
  customQualityWidth: 1920,
  imageSpacing: 24,
  pageTransition: true,
  backgroundColor: '#000000',
  alwaysFullscreen: false,
}

export const isReverseDirection = (direction: ReadingDirection) =>
  direction === 'rtl' || direction === 'btt'

export const isVerticalDirection = (direction: ReadingDirection) =>
  direction === 'ttb' || direction === 'btt'

export type ImageGroup = ComicImage[]

/** Video 模块类型（对齐 swagger 中的 Dtos） */

import type { Artist } from '../artist/typing'
import type { Tag } from '../tag/typing'

export type VideoGetListOutput = FullAuditedEntityDto & {
  bitrate: number
  codec?: string
  cover?: string
  description?: string
  duration: string
  framerate: number
  height: number
  libraryId: string
  path?: string
  ratio?: string
  readCount: number
  readingLastTime: Date
  readingProgress?: number
  title?: string
  width: number
}

export type Video = VideoGetListOutput & {
  artists?: Artist[]
  tags?: Tag[]
}

export type VideoCreateUpdate = {
  bitrate: number
  codec?: string
  cover?: string
  description?: string
  duration: string
  framerate: number
  height: number
  libraryId: string
  path?: string
  ratio?: string
  title?: string
  width: number
}

export type VideoGetListInput = BasePageRequest & {
  artists?: string[]
  description?: string
  hasReadingProgress?: boolean
  libraryId?: string
  tags?: string[]
  title?: string
}

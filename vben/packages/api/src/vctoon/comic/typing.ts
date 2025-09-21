/** Comic 模块类型（对齐 swagger 中的 Dtos） */

import type { Artist } from '../artist/typing'
import type { Tag } from '../tag/typing'

export type ComicGetListOutput = FullAuditedEntityDto & {
  cover?: string
  description?: string
  libraryId: string
  readCount: number
  readingLastTime: Date
  readingProgress?: boolean
  title?: string
}

export type Comic = ComicGetListOutput & {
  artists?: Artist[]
  comicImages?: ComicImage[]
  tags?: Tag[]
}

export type ComicCreateUpdate = {
  cover?: string
  description?: string
  libraryId: string
  title?: string
}

export type ComicGetListInput = BasePageRequest & {
  artists?: string[]
  description?: string
  hasReadingProgress?: boolean
  libraryId?: string
  tags?: string[]
  title?: string
}

export type ComicImage = {
  archiveInfoPathId?: null | string
  comicId: string
  id: string
  libraryId: string
  libraryPathId?: null | string
  name?: string
  path?: string
  size: number
}

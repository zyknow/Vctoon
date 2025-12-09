import type { Artist } from '../artist/typing'
import type { MediumType } from '../library/typing'
import type { Tag } from '../tag/typing'

export { MediumType }

/** Medium DTO (Vctoon.Mediums.Dtos.MediumDto) */
export type Medium = AuditedEntityDto<string> & {
  artists: Artist[]
  cover: string
  description: string
  libraryId: string
  readCount: number
  readingLastTime?: string
  readingProgress?: number
  tags: Tag[]
  title: string
  mediumType: MediumType
  videoDetail?: VideoDetail
}

export type MediumGetListOutput = AuditedEntityDto<string> & {
  cover: string
  description: string
  libraryId: string
  readCount: number
  readingLastTime?: Date
  readingProgress?: number
  mediumType: MediumType
  title: string
}

export type VideoDetail = {
  bitrate: number
  codec: string
  duration: string
  framerate: number
  height: number
  path: string
  ratio: string
  width: number
}

export type MediumGetListInput = BasePageRequest & {
  artists?: string[]
  createdInDays?: number
  filter?: string
  hasReadCount?: boolean
  libraryId?: string
  mediumType?: MediumType
  readingProgressType?: ReadingProgressType
  tags?: string[]
}

export enum ReadingProgressType {
  NotStarted,
  InProgress,
  Completed,
}

export type CreateUpdateMedium = {
  cover: string
  description: string
  libraryId: string
  libraryPathId?: string
  mediumType: MediumType
  title: string
  videoDetail?: VideoDetail
}

export type MediumMultiUpdate = {
  resourceIds: string[]
  mediumIds: string[]
}

export type ReadingProcessUpdate = {
  mediumId: string
  progress: number
  readingLastTime?: string
}

export type Subtitle = {
  fileName: string
  format: string
  label?: string
  language?: string
  url: string
}

export type ComicImage = EntityDto<string> & {
  archiveInfoPathId?: string
  libraryId: string
  libraryPathId?: string
  mediumId: string
  name: string
  path: string
  size: number
}

/** Medium 通用基础类型（对齐后端 MediumDtoBase） */

import type { Artist } from '../artist/typing'
import type { Tag } from '../tag/typing'

// 列表/详情通用基础输出（含审计字段）
export type MediumGetListOutputBase = FullAuditedEntityDto & {
  cover?: string
  description?: string
  libraryId: string
  readCount: number
  readingLastTime?: Date
  readingProgress?: number
  title?: string
}

// 关系型数据（标签/作者）
export type MediumRelations = {
  artists?: Artist[]
  tags?: Tag[]
}

// 新增/更新通用基础输入
export type MediumCreateUpdateBase = {
  cover?: string
  description?: string
  libraryId: string
  title?: string
}

// 列表查询通用基础输入
export type MediumGetListInputBase = BasePageRequest & {
  artists?: string[]
  createdInDays?: number
  description?: string
  hasReadCount?: boolean
  libraryId?: string
  readingProgressType?: ReadingProgressType
  tags?: string[]
  title?: string
}

export enum ReadingProgressType {
  NotStarted,
  InProgress,
  Completed,
}

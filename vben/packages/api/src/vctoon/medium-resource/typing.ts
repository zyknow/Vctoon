import type { MediumType } from '../library'

/**
 * 单条阅读进度更新（对齐后端 Vctoon.Mediums.Dtos.ReadingProcessUpdateDto）
 * 后端接口接收 List<ReadingProcessUpdateDto>，前端方法会以数组提交
 */
export type ReadingProcessUpdate = {
  /** 媒体 Id */
  mediumId: string
  mediumType: MediumType
  /** 进度 [0,1] */
  progress: number
  /** 最近阅读时间（可选 ISO 字符串） */
  readingLastTime?: null | string
}

/** 对齐后端 Vctoon.Mediums.Dtos.MediumMultiUpdateItemDto */
export interface MediumMultiUpdateItemDto {
  /** 媒体 Id */
  id: string
  mediumType: MediumType
}

/** 对齐后端 Vctoon.Mediums.Dtos.MediumMultiUpdateDto */
export interface MediumMultiUpdateDto {
  /** 关联实体 Id 列表（Artist 或 Tag Id，根据调用的方法语义） */
  ids: string[]
  /** 目标媒体集合（去重后端已处理，前端直接传） */
  items: MediumMultiUpdateItemDto[]
}

/** 批量阅读进度更新入参（前端辅助类型） */
export type ReadingProcessUpdateBatchInput = ReadingProcessUpdate[]

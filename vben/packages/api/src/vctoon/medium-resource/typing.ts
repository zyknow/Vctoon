/** 对齐后端 Vctoon.Mediums.Dtos.ReadingProcessUpdateDto */
export type ReadingProcessUpdate = {
  /** 最近阅读时间（可选 ISO 字符串） */
  readingLastTime?: null | string
  /** 媒体 Id */
  mediumId: string
  /** 进度 [0,1] */
  progress: number
}

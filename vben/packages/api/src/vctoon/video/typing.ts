/** Video 模块类型（复用 Medium 基础类型） */

import type {
  MediumCreateUpdateBase,
  MediumGetListInputBase,
  MediumGetListOutputBase,
  MediumRelations,
} from '../base/medium-base'

// 输出：基础 + 视频专有字段
export type VideoGetListOutput = MediumGetListOutputBase & {
  bitrate: number
  codec?: string
  duration: string
  framerate: number
  height: number
  path?: string
  ratio?: string
  width: number
}

// 详情：输出 + 关系
export type Video = VideoGetListOutput & MediumRelations

// 创建/更新：基础 + 视频专有输入
export type VideoCreateUpdate = MediumCreateUpdateBase & {
  bitrate: number
  codec?: string
  duration: string
  framerate: number
  height: number
  path?: string
  ratio?: string
  width: number
}

// 查询输入：复用基础
export type VideoGetListInput = MediumGetListInputBase

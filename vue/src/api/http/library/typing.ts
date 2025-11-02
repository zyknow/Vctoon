/** Library 模块前端类型定义（与后端 DTO 对齐） */

/** 列表项/详情 DTO 对齐 Vctoon.Libraries.Dtos.LibraryDto */
export type Library = {
  /** 媒体类型：后端枚举 MediumType(0|1) */
  mediumType: MediumType
  /** 名称 */
  name: string
  /** 路径集合 */
  paths?: string[]
} & FullAuditedEntityDto

/** 新建/更新 输入 对齐 Vctoon.Libraries.Dtos.LibraryCreateUpdateDto */
export type LibraryCreateUpdate = {
  /** 媒体类型：后端枚举 MediumType(0|1) */
  mediumType: MediumType
  /** 名称（必填） */
  name: string
  /** 路径集合（可空） */
  paths?: string[]
}

export enum MediumType {
  Comic,
  Video,
}

/** 分页查询 输入（若有扩展字段可于此补充） */
export type LibraryGetListInput = BasePageRequest & {
  // sorting?: string
}

/** 权限设置 DTO 对齐 Vctoon.Libraries.Dtos.LibraryPermissionCreateUpdateDto */
export type LibraryPermissionCreateUpdate = {
  canComment?: boolean
  canDownload?: boolean
  canShare?: boolean
  canStar?: boolean
  canView?: boolean
}

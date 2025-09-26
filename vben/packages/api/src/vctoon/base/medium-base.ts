/** Medium 通用基础类型（对齐后端 MediumDtoBase） */

import type { MediumDto } from '../..'
import type { Artist } from '../artist/typing'
import type { MediumType } from '../library'
import type { Tag } from '../tag/typing'

import { createBaseCurdUrl, requestClient } from '../..'

// 列表/详情通用基础输出（含审计字段）
export type MediumGetListOutputBase = FullAuditedEntityDto & {
  cover?: string
  description?: string
  libraryId: string
  mediumType: MediumType
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

export function createMediumBaseCurdApi<
  TId,
  TEntity,
  TGetListOutputEntity,
  TUpdateInputEntity,
  TCreateInputEntity,
  TPageRequest = BasePageRequest,
  TPageResult = PageResult<TEntity>,
>(baseUrl: string, mediumType: MediumType) {
  return {
    async update(input: TUpdateInputEntity, id: TId) {
      const item = await requestClient.put<TEntity>(`${baseUrl}/${id}`, input)
      ;(item as MediumDto).mediumType = mediumType
      return item
    },
    delete(id: TId) {
      return requestClient.delete(`${baseUrl}/${id}`)
    },
    async create(input: TCreateInputEntity) {
      const item = await requestClient.post<TEntity>(`${baseUrl}`, input)
      ;(item as MediumDto).mediumType = mediumType
      return item
    },

    async getById(id: string) {
      const item = await requestClient.get<TGetListOutputEntity>(
        `${baseUrl}/${id}`,
      )
      ;(item as MediumDto).mediumType = mediumType
      return item
    },
    async getPage(pageRequest: TPageRequest) {
      const result = (await requestClient.get<TPageResult>(baseUrl, {
        params: pageRequest,
      })) as unknown as PageResult<MediumGetListOutputBase>

      for (const element of result.items) {
        element.mediumType = mediumType
      }
      return result
    },

    /** 更新封面 */
    async updateCover(id: string, cover: File) {
      const formData = new FormData()
      formData.append('cover', cover)
      const entity = await requestClient.put<TEntity>(
        `${baseUrl}/${id}/cover`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        },
      )
      ;(entity as MediumDto).mediumType = mediumType

      return entity
    },

    /** 更新作者 */
    async updateArtists(id: string, artistIds: string[]) {
      const entity = await requestClient.put<TEntity>(
        `${baseUrl}/${id}/artists`,
        artistIds,
      )
      ;(entity as MediumDto).mediumType = mediumType
      return entity
    },

    /** 更新标签 */
    async updateTags(id: string, tagIds: string[]) {
      const entity = await requestClient.put<TEntity>(
        `${baseUrl}/${id}/tags`,
        tagIds,
      )
      ;(entity as MediumDto).mediumType = mediumType
      return entity
    },
  }
}

export function createMediumBaseCurdUrl(baseUrl: string) {
  return {
    ...createBaseCurdUrl(baseUrl),
    updateCover: `${baseUrl}/{id}/cover`,
    updateArtists: `${baseUrl}/{id}/artists`,
    updateTags: `${baseUrl}/{id}/tags`,
  }
}

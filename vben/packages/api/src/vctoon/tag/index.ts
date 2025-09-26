import type { Tag, TagCreateUpdate, TagGetListInput } from './typing'

import { requestClient } from '../..'
import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/curd-api-definition-base'

/** Tag API 根路径 */
const baseUrl = '/api/app/tag'

const url = {
  ...createBaseCurdUrl(baseUrl),
  /** 获取所有标签 */
  all: `${baseUrl}/all`,
  /** 批量删除 */
  deleteMany: `${baseUrl}/delete-many`,
}

export const tagApi = {
  url,
  ...createBaseCurdApi<
    string,
    Tag,
    Tag,
    TagCreateUpdate,
    TagCreateUpdate,
    TagGetListInput,
    PageResult<Tag>
  >(baseUrl),

  /** 获取所有标签 */
  async getAllTags(withResourceCount = false) {
    return requestClient.get<Tag[]>(url.all, {
      params: { withResourceCount },
    })
  },

  /** 批量删除标签 */
  async deleteMany(ids: string[]) {
    return requestClient.delete(url.deleteMany, {
      data: ids,
    })
  },
}

export * from './typing'

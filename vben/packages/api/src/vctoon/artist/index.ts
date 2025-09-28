import type { Artist, ArtistCreateUpdate, ArtistGetListInput } from './typing'

import { requestClient } from '../../request'
import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/curd-api-definition-base'

/** Artist API 根路径 */
const baseUrl = '/api/app/artist'

const url = {
  ...createBaseCurdUrl(baseUrl),
  /** 获取所有艺术家 */
  all: `${baseUrl}/all`,
  /** 批量删除 */
  deleteMany: `${baseUrl}/delete-many`,
}

export const artistApi = {
  url,
  ...createBaseCurdApi<
    string,
    Artist,
    Artist,
    ArtistCreateUpdate,
    ArtistCreateUpdate,
    ArtistGetListInput,
    PageResult<Artist>
  >(baseUrl),

  /** 获取所有艺术家 */
  async getAllArtist(withResourceCount = false) {
    return requestClient.get<Artist[]>(url.all, {
      params: { withResourceCount },
    })
  },

  /** 批量删除艺术家 */
  async deleteMany(ids: string[]) {
    return requestClient.delete(url.deleteMany, {
      data: ids,
    })
  },
}

export * from './typing'

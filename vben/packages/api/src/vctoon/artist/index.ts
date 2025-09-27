import type { Artist, ArtistCreateUpdate, ArtistGetListInput } from './typing'

import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/curd-api-definition-base'
import { requestClient } from '../../request'

/** Artist API 根路径 */
const baseUrl = '/api/app/artist'

export const artistApi = {
  url: {
    ...createBaseCurdUrl(baseUrl),
    getAllArtist: `${baseUrl}/all`,
  },
  ...createBaseCurdApi<
    string,
    Artist,
    Artist,
    ArtistCreateUpdate,
    ArtistCreateUpdate,
    ArtistGetListInput,
    PageResult<Artist>
  >(baseUrl),
  /** 获取所有艺术家列表 */
  getAllArtist(withResourceCount?: boolean) {
    return requestClient.get<Artist[]>(`${baseUrl}/all`, {
      params: { withResourceCount },
    })
  },
}

export * from './typing'

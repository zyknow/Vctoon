import type { Artist, ArtistCreateUpdate, ArtistGetListInput } from './typing'

import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/base-curd-api-definition'

/** Artist API 根路径 */
const baseUrl = '/api/app/artist'

export const artistApi = {
  url: {
    ...createBaseCurdUrl(baseUrl),
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
}

export * from './typing'

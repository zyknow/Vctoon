import type {
  Comic,
  ComicCreateUpdate,
  ComicGetListInput,
  ComicGetListOutput,
} from './typing'

import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/base-curd-api-definition'

/** Comic API 根路径 */
const baseUrl = '/api/app/comic'

export const comicApi = {
  url: {
    ...createBaseCurdUrl(baseUrl),
    cover: `${baseUrl}/cover?cover={cover}`,
  },
  ...createBaseCurdApi<
    string,
    Comic,
    ComicGetListOutput,
    ComicCreateUpdate,
    ComicCreateUpdate,
    ComicGetListInput,
    PageResult<ComicGetListOutput>
  >(baseUrl),
}

export * from './typing'

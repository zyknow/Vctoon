import type {
  Comic,
  ComicCreateUpdate,
  ComicGetListInput,
  ComicGetListOutput,
} from './typing'

import { MediumType, requestClient } from '../..'
import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/curd-api-definition-base'

/** Comic API 根路径 */
const baseUrl = '/api/app/comic'

export const comicApi = {
  url: {
    ...createBaseCurdUrl(baseUrl),
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

  async getById(id: string) {
    const item = await requestClient.get<ComicGetListOutput>(`${baseUrl}/${id}`)
    item.mediumType = MediumType.Comic
    return item
  },
  async getPage(pageRequest: ComicGetListInput) {
    const result = await requestClient.get<PageResult<ComicGetListOutput>>(
      baseUrl,
      {
        params: pageRequest,
      },
    )
    for (const element of result.items) {
      element.mediumType = MediumType.Comic
    }
    return result
  },
}

export * from './typing'

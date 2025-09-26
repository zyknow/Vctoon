import type {
  Comic,
  ComicCreateUpdate,
  ComicGetListInput,
  ComicGetListOutput,
} from './typing'

import { createMediumBaseCurdApi, createMediumBaseCurdUrl } from '../..'
import { MediumType } from '../library'

/** Comic API 根路径 */
const baseUrl = '/api/app/comic'

const url = {
  ...createMediumBaseCurdUrl(baseUrl),
}

export const comicApi = {
  url,
  ...createMediumBaseCurdApi<
    string,
    Comic,
    ComicGetListOutput,
    ComicCreateUpdate,
    ComicCreateUpdate,
    ComicGetListInput,
    PageResult<ComicGetListOutput>
  >(baseUrl, MediumType.Comic),
}

export * from './typing'

import type { Tag, TagCreateUpdate, TagGetListInput } from './typing'

import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/curd-api-definition-base'

/** Tag API 根路径 */
const baseUrl = '/api/app/tag'

export const tagApi = {
  url: {
    ...createBaseCurdUrl(baseUrl),
  },
  ...createBaseCurdApi<
    string,
    Tag,
    Tag,
    TagCreateUpdate,
    TagCreateUpdate,
    TagGetListInput,
    PageResult<Tag>
  >(baseUrl),
}

export * from './typing'

import type {
  Video,
  VideoCreateUpdate,
  VideoGetListInput,
  VideoGetListOutput,
} from './typing'

import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/base-curd-api-definition'

/** Video API 根路径 */
const baseUrl = '/api/app/video'

export const videoApi = {
  url: {
    ...createBaseCurdUrl(baseUrl),
    cover: `${baseUrl}/cover?cover={cover}`,
  },
  ...createBaseCurdApi<
    string,
    Video,
    VideoGetListOutput,
    VideoCreateUpdate,
    VideoCreateUpdate,
    VideoGetListInput,
    PageResult<VideoGetListOutput>
  >(baseUrl),
}

export * from './typing'

import type {
  Video,
  VideoCreateUpdate,
  VideoGetListInput,
  VideoGetListOutput,
} from './typing'

import { MediumType, requestClient } from '../..'
import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/curd-api-definition-base'

/** Video API 根路径 */
const baseUrl = '/api/app/video'

export const videoApi = {
  url: {
    ...createBaseCurdUrl(baseUrl),
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
  async getById(id: string) {
    const item = await requestClient.get<VideoGetListOutput>(`${baseUrl}/${id}`)
    item.mediumType = MediumType.Video
    return item
  },
  async getPage(pageRequest: VideoGetListInput) {
    const result = await requestClient.get<PageResult<VideoGetListOutput>>(
      baseUrl,
      {
        params: pageRequest,
      },
    )
    for (const element of result.items) {
      element.mediumType = MediumType.Video
    }
    return result
  },
}

export * from './typing'

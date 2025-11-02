import {
  createMediumBaseCurdApi,
  createMediumBaseCurdUrl,
} from '../base/medium-base'
import { MediumType } from '../library/typing'
import type {
  Video,
  VideoCreateUpdate,
  VideoGetListInput,
  VideoGetListOutput,
} from './typing'

/** Video API 根路径 */
const baseUrl = '/api/app/video'

const url = {
  ...createMediumBaseCurdUrl(baseUrl),
}

export const videoApi = {
  url,
  ...createMediumBaseCurdApi<
    string,
    Video,
    VideoGetListOutput,
    VideoCreateUpdate,
    VideoCreateUpdate,
    VideoGetListInput,
    PageResult<VideoGetListOutput>
  >(baseUrl, MediumType.Video),
}

export * from './typing'

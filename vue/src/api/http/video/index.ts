import { useEnvConfig } from '@/hooks/useEnvConfig'

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

const envConfig = useEnvConfig()

const url = {
  ...createMediumBaseCurdUrl(baseUrl),
  videoStreamUrl: `${baseUrl}/{id}/video-stream`,
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
  getVideoUrl(id: string) {
    return `${envConfig.apiURL}${this.url.videoStreamUrl.format({ id })}`
  },
}

export * from './typing'

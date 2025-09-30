import type {
  MediumMultiUpdateDto,
  ReadingProcessUpdateBatchInput,
} from './typing'

import { requestClient } from '../../request'

/** MediumResource API 根路径 */
const baseUrl = '/api/app/medium-resource'

export const mediumResourceApi = {
  url: {
    /** 获取封面流：GET /api/app/medium-resource/cover?cover={cover} */
    getCover: `${baseUrl}/cover?cover={cover}`,
    /** 批量更新阅读进度：PUT /api/app/medium-resource/reading-process */
    updateReadingProcess: `${baseUrl}/reading-process`,
    /** Artist 关系：Add (PUT) */
    addArtistList: `${baseUrl}/add-artist-list`,
    /** Artist 关系：Update (POST) */
    updateArtistList: `${baseUrl}/update-artist-list`,
    /** Artist 关系：Delete (PUT) */
    deleteArtistList: `${baseUrl}/delete-artist-list`,
    /** Tag 关系：Update (POST) */
    updateTagList: `${baseUrl}/update-tag-list`,
    /** Tag 关系：Add (PUT) */
    addTagList: `${baseUrl}/add-tag-list`,
    /** Tag 关系：Delete (PUT) */
    deleteTagList: `${baseUrl}/delete-tag-list`,
  },

  /**
   * 批量更新用户的阅读进度（必须为数组，后端要求 List<ReadingProcessUpdateDto>）
   */
  updateReadingProcess(input: ReadingProcessUpdateBatchInput) {
    return requestClient.put(this.url.updateReadingProcess, input)
  },

  /** 媒体关联：添加作者列表（PUT） */
  addArtistList(input: MediumMultiUpdateDto) {
    return requestClient.put(this.url.addArtistList, input)
  },
  /** 媒体关联：更新作者列表（POST） */
  updateArtistList(input: MediumMultiUpdateDto) {
    return requestClient.post(this.url.updateArtistList, input)
  },
  /** 媒体关联：删除作者列表（PUT） */
  deleteArtistList(input: MediumMultiUpdateDto) {
    return requestClient.put(this.url.deleteArtistList, input)
  },
  /** 媒体关联：更新标签列表（POST） */
  updateTagList(input: MediumMultiUpdateDto) {
    return requestClient.post(this.url.updateTagList, input)
  },
  /** 媒体关联：添加标签列表（PUT） */
  addTagList(input: MediumMultiUpdateDto) {
    return requestClient.put(this.url.addTagList, input)
  },
  /** 媒体关联：删除标签列表（PUT） */
  deleteTagList(input: MediumMultiUpdateDto) {
    return requestClient.put(this.url.deleteTagList, input)
  },
}

export * from './typing'

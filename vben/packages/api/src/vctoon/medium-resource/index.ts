import type { ReadingProcessUpdate } from './typing'

import { requestClient } from '../../request'

/** MediumResource API 根路径 */
const baseUrl = '/api/app/medium-resource'

export const mediumResourceApi = {
  url: {
    /** 获取封面流：GET /api/app/medium-resource/cover?cover={coverKey} */
    getCover: `${baseUrl}/cover?cover={cover}`,
    /** 更新阅读进度：PUT /api/app/medium-resource/reading-process */
    updateReadingProcess: `${baseUrl}/reading-process`,
  },

  /**
   * 更新用户的阅读进度
   * @param input 阅读进度（mediumId, progress, lastReadTime）
   */
  updateReadingProcess(input: ReadingProcessUpdate) {
    return requestClient.put(this.url.updateReadingProcess, input)
  },
}

export * from './typing'

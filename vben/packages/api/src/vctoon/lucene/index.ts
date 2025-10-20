import type { MultiSearchInput, SearchQueryInput, SearchResult } from './typing'

import { requestClient } from '../../request'

/** Lucene API 根路径（注意：此为模块路由，不是 /api/app） */
const baseUrl = '/api/lucene'

export const luceneApi = {
  url: {
    search: `${baseUrl}/search/{entity}`,
    rebuild: `${baseUrl}/rebuild/{entity}`,
    rebuildAndIndexAll: `${baseUrl}/rebuild-and-index/{entity}`,
    count: `${baseUrl}/count/{entity}`,
  },

  /** 搜索指定实体索引 */
  async search(entity: string, input: SearchQueryInput) {
    return requestClient.get<SearchResult>(`${baseUrl}/search/${entity}`, {
      params: input,
    })
  },

  /** 搜索指定实体索引 */
  async searchMany(input: MultiSearchInput) {
    return requestClient.get<SearchResult>(`${baseUrl}/search-many`, {
      params: input,
      paramsSerializer: 'repeat',
    })
  },

  /** 重建指定实体索引（清空后重建结构，不写入数据） */
  async rebuild(entity: string) {
    return requestClient.post<number>(`${baseUrl}/rebuild/${entity}`)
  },

  /** 重建并批量写入所有数据（默认批量大小 1000，可覆盖） */
  async rebuildAndIndexAll(entity: string, batchSize = 1000) {
    return requestClient.post<number>(
      `${baseUrl}/rebuild-and-index/${entity}`,
      undefined,
      { params: { batchSize } },
    )
  },

  /** 获取索引文档总数 */
  async getIndexDocumentCount(entity: string) {
    return requestClient.get<number>(`${baseUrl}/count/${entity}`)
  },
}

export * from './typing'

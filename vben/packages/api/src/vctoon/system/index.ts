import { requestClient } from '../../request'

/** System API 根路径 */
const baseUrl = '/api/app/system'

export const systemApi = {
  url: {
    systemPaths: `${baseUrl}/system-paths`,
  },

  /**
   * 查询系统路径（后端：GET /api/app/system/system-paths?path=...）
   * @param path 可选的路径前缀
   * @returns 字符串数组
   */
  getSystemPaths(path?: string) {
    return requestClient.get<string[]>(`${baseUrl}/system-paths`, {
      params: { path },
    })
  },
}

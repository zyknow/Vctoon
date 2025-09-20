import type {
  IdentitySecurityLog,
  IdentitySecurityLogPageRequest,
} from './typing'

import { requestClient } from '../../request'

/** 根路径 */
const baseUrl = '/api/app/identity-security-log'

const url = {
  /** 获取安全日志列表 */
  getPage: `${baseUrl}`,
  /** 获取当前用户安全日志列表 */
  getCurrentUserList: `${baseUrl}/current-user-list`,
}

/** 身份安全日志 API 封装 */
export const securityLogApi = {
  url,

  /**
   * 获取安全日志分页列表
   * @param req 分页查询请求参数
   * @returns 安全日志分页结果
   */
  getPage(req: IdentitySecurityLogPageRequest) {
    return requestClient.get<PageResult<IdentitySecurityLog>>(url.getPage, {
      params: req,
    })
  },

  /**
   * 获取当前用户的安全日志分页列表
   * @param req 分页查询请求参数
   * @returns 当前用户安全日志分页结果
   */
  getCurrentUserList(req: IdentitySecurityLogPageRequest) {
    return requestClient.get<PageResult<IdentitySecurityLog>>(
      url.getCurrentUserList,
      {
        params: req,
      },
    )
  },
}

export * from './typing'

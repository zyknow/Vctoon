/** 身份安全日志 DTO */
export type IdentitySecurityLog = {
  /** 操作行为 */
  action?: string
  /** 应用程序名称 */
  applicationName?: string
  /** 浏览器信息 */
  browserInfo?: string
  /** 客户端 ID */
  clientId?: string
  /** 客户端 IP 地址 */
  clientIpAddress?: string
  /** 关联 ID */
  correlationId?: string
  /** 创建时间 */
  creationTime: string
  /** 额外属性 */
  extraProperties?: Record<string, any>
  /** 主键 ID */
  id: string
  /** 身份标识 */
  identity?: string
  /** 租户 ID */
  tenantId?: string
  /** 租户名称 */
  tenantName?: string
  /** 用户 ID */
  userId?: string
  /** 用户名 */
  userName?: string
}

/** 安全日志分页查询请求 */
export type IdentitySecurityLogPageRequest = {
  /** 操作名称 */
  actionName?: string
  /** 应用程序名称 */
  applicationName?: string
  /** 客户端 ID */
  clientId?: string
  /** 客户端 IP 地址 */
  clientIpAddress?: string
  /** 关联 ID */
  correlationId?: string
  /** 结束时间 */
  endTime?: string
  /** 身份标识 */
  identity?: string
  /** 开始时间 */
  startTime?: string
  /** 用户名 */
  userName?: string
} & BasePageRequest

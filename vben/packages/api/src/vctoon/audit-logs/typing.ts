export type AuditLog = {
  actions: AuditAction[]
  applicationName: string
  browserInfo: string
  clientIpAddress: string
  clientName: string
  comments: string
  correlationId: string
  entityChanges: AuditEntityChange[]
  exceptions: string
  executionDuration: number
  executionTime: string
  extraProperties: any
  httpMethod: string
  httpStatusCode: number
  id: string
  impersonatorTenantId: string
  impersonatorTenantName: string
  impersonatorUserId: string
  impersonatorUserName: string
  tenantId: string
  tenantName: string
  url: string
  userId: string
  userName: string
}

export type AuditLogPageRequest = BasePageRequest & {
  applicationName?: string
  clientIpAddress?: string
  correlationId?: string
  endTime?: string
  hasException?: boolean
  httpMethod?: string
  httpStatusCode?: number
  maxExecutionDuration?: number
  minExecutionDuration?: number
  startTime?: string
  url?: string
  userName?: string
}

export type AuditEntityChangePageRequest = BasePageRequest & {
  auditLogId?: string
  endDate?: string
  entityChangeType?: AuditEntityChangeType
  entityId?: string
  entityTypeFullName?: string
  startDate?: string
}

export type AuditEntityChange = {
  auditLogId: string
  changeTime: string
  changeType: AuditEntityChangeType
  entityId: string
  entityTypeFullName: string
  extraProperties: any
  id: string
  propertyChanges: [
    {
      entityChangeId: string
      id: string
      newValue: string
      originalValue: string
      propertyName: string
      propertyTypeFullName: string
      tenantId: string
    },
  ]
  tenantId: string
}

export const AuditEntityChangeType = {
  created: 0,
  updated: 1,
  deleted: 2,
} as const

export type AuditEntityChangeType =
  (typeof AuditEntityChangeType)[keyof typeof AuditEntityChangeType]

export type AuditAction = {
  auditLogId: string
  executionDuration: number
  executionTime: string
  extraProperties: any
  id: string
  methodName: string
  parameters: string
  serviceName: string
  tenantId: string
}

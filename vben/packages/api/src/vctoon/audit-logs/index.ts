import type { AuditEntityChange, AuditLog, AuditLogPageRequest } from './typing'

import { requestClient } from '../../request'

const baseUrl = '/api/audit-logging/audit-logs'

const url = {
  getPage: `${baseUrl}`,
  getById: `${baseUrl}/{id}`,
  getEntityChangePage: `${baseUrl}/entity-changes`,
  getEntityChangeById: `${baseUrl}/entity-changes/{entityChangeId}`,
}

export const auditLogApi = {
  url,
  getPage(req: AuditLogPageRequest) {
    return requestClient.get<PageResult<AuditLog>>(url.getPage, {
      params: req,
    })
  },
  getById(id: string) {
    return requestClient.get<AuditLog>(url.getById.format({ id }))
  },
  getEntityChangePage(req: AuditLogPageRequest) {
    return requestClient.get<PageResult<AuditEntityChange>>(
      url.getEntityChangePage,
      {
        params: req,
      },
    )
  },
  getEntityChangeById(entityChangeId: string) {
    return requestClient.get<AuditEntityChange>(
      url.getEntityChangeById.format({ entityChangeId }),
    )
  },
}

export * from './typing'

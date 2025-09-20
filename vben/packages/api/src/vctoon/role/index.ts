import type {
  CreateIdentityRoleInput,
  IdentityRole,
  UpdateIdentityRoleInput,
} from './typing'

import { requestClient } from '../../request'
import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/base-curd-api-definition'

const baseUrl = '/api/identity/roles'

const url = {
  ...createBaseCurdUrl(baseUrl),
  getAll: `${baseUrl}/all`,
}

export const roleApi = {
  url,
  ...createBaseCurdApi<
    string,
    IdentityRole,
    IdentityRole,
    UpdateIdentityRoleInput,
    CreateIdentityRoleInput,
    FilterPageRequest,
    PageResult<IdentityRole>
  >(baseUrl),
  getAll() {
    return requestClient.get<ItemsResult<IdentityRole>>(url.getAll)
  },
}

export * from './typing'

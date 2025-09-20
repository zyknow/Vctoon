import type { IdentityRole } from '../role/typing'
import type {
  CreateIdentityUserInput,
  IdentityUser,
  UpdateIdentityUserInput,
} from './typing'

import { requestClient } from '../../request'
import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/base-curd-api-definition'

const baseUrl = '/api/identity/users'

const url = {
  ...createBaseCurdUrl(baseUrl),
  getUserRoles: `${baseUrl}/{userId}/roles`,
  updateUserRoles: `${baseUrl}/{userId}/roles`,
  getAssignableRoles: `${baseUrl}/assignable-roles`,
  getByUserName: `${baseUrl}/by-username/{userName}`,
  getByEmail: `${baseUrl}/by-email/{userEmail}`,
}

export const userApi = {
  url,
  ...createBaseCurdApi<
    string,
    IdentityUser,
    IdentityUser,
    UpdateIdentityUserInput,
    CreateIdentityUserInput,
    FilterPageRequest,
    PageResult<IdentityUser>
  >(baseUrl),
  getUserRoles(userId: string) {
    return requestClient.get<ItemsResult<IdentityRole>>(
      url.getUserRoles.format({ userId }),
    )
  },
  updateUserRoles(userId: string, roleNames: string[]) {
    return requestClient.put(url.updateUserRoles.format({ userId }), {
      roleNames,
    })
  },
  getAssignableRoles() {
    return requestClient.get<ItemsResult<IdentityRole>>(url.getAssignableRoles)
  },
  getByUserName(userName: string) {
    return requestClient.get<IdentityUser>(
      url.getByUserName.format({ userName }),
    )
  },
  getByEmail(userEmail: string) {
    return requestClient.get<IdentityUser>(url.getByEmail.format({ userEmail }))
  },
}

export * from './typing'

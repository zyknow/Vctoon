import type { IdentityUser } from '../user'
import type {
  Library,
  LibraryCreateUpdate,
  LibraryGetListInput,
  LibraryPermissionCreateUpdate,
} from './typing'

import { requestClient } from '../../request'
import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/curd-api-definition-base'

/** Library API 根路径 */
const baseUrl = '/api/app/library'

const url = {
  ...createBaseCurdUrl(baseUrl),
  /** 当前用户可见的库列表 */
  currentUserLibraryList: `${baseUrl}/current-user-library-list`,
  /** 扫描指定库 */
  scan: `${baseUrl}/scan/{libraryId}`,
  /** 配置库权限（query: userId, libraryId） */
  libraryPermissions: `${baseUrl}/library-permissions`,
  getAssignableUsers: `${baseUrl}/assignable-users`,
}

/** Library API（CRUD + 自定义接口） */
export const libraryApi = {
  url,
  ...createBaseCurdApi<
    string,
    Library,
    Library,
    LibraryCreateUpdate,
    LibraryCreateUpdate,
    LibraryGetListInput,
    PageResult<Library>
  >(baseUrl),

  /** 获取当前用户的库列表 */
  getCurrentUserLibraryList() {
    return requestClient.get<Library[]>(url.currentUserLibraryList)
  },

  /** 触发扫描库 */
  scan(libraryId: string) {
    return requestClient.post(url.scan.format({ libraryId }))
  },

  /** 设置库权限 */
  setLibraryPermissions(
    input: LibraryPermissionCreateUpdate,
    userId?: string,
    libraryId?: string,
  ) {
    return requestClient.put(url.libraryPermissions, input, {
      params: { userId, libraryId },
    })
  },

  /** 获取（某个用户或当前用户）在指定库的权限 */
  getLibraryPermissions(userId?: string, libraryId?: string) {
    return requestClient.get<LibraryPermissionCreateUpdate>(
      url.libraryPermissions,
      { params: { userId, libraryId } },
    )
  },
  /** 获取可分配用户列表 */
  getAssignableUsers(pageRequest: FilterPageRequest) {
    return requestClient.get<PageResult<IdentityUser>>(url.getAssignableUsers, {
      params: pageRequest,
    })
  },
}

export * from './typing'

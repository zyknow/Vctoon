import type { RouteRecordRaw } from 'vue-router'

import type { MenuRecordRaw } from '@vben-core/typings'

import { acceptHMRUpdate, defineStore } from 'pinia'

interface AccessState {
  /**
   * 权限码
   */
  accessCodes: string[]
  /**
   * 可访问的菜单列表
   */
  accessMenus: MenuRecordRaw[]
  /**
   * 可访问的路由列表
   */
  accessRoutes: RouteRecordRaw[]
  /**
   * 是否已经检查过权限
   */
  isAccessChecked: boolean
  /**
   * 是否锁屏状态
   */
  isLockScreen: boolean
  /**
   * 锁屏密码
   */
  lockScreenPassword?: string
  /**
   * 登录是否过期
   */
  loginExpired: boolean
}

/**
 * @zh_CN 访问权限相关
 */
export const useAccessStore = defineStore('core-access', {
  actions: {
    getMenuByPath(path: string) {
      function findMenu(
        menus: MenuRecordRaw[],
        path: string,
      ): MenuRecordRaw | undefined {
        for (const menu of menus) {
          if (menu.path === path) {
            return menu
          }
          if (menu.children) {
            const matched = findMenu(menu.children, path)
            if (matched) {
              return matched
            }
          }
        }
      }
      return findMenu(this.accessMenus, path)
    },
    lockScreen(password: string) {
      this.isLockScreen = true
      this.lockScreenPassword = password
    },
    setAccessCodes(codes: string[]) {
      this.accessCodes = codes
    },
    setAccessMenus(menus: MenuRecordRaw[]) {
      this.accessMenus = menus
    },
    setAccessRoutes(routes: RouteRecordRaw[]) {
      this.accessRoutes = routes
    },
    setIsAccessChecked(isAccessChecked: boolean) {
      this.isAccessChecked = isAccessChecked
    },
    setLoginExpired(loginExpired: boolean) {
      this.loginExpired = loginExpired
    },
    unlockScreen() {
      this.isLockScreen = false
      this.lockScreenPassword = undefined
    }
  },
  persist: {
    // 持久化
    pick: ['accessCodes', 'isLockScreen', 'lockScreenPassword'],
  },
  state: (): AccessState => ({
    accessCodes: [],
    accessMenus: [],
    accessRoutes: [],
    isAccessChecked: false,
    isLockScreen: false,
    lockScreenPassword: undefined,
    loginExpired: false
  }),
})

// 解决热更新问题
const hot = import.meta.hot
if (hot) {
  hot.accept(acceptHMRUpdate(useAccessStore, hot))
}

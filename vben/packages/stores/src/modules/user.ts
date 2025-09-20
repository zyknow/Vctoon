import type { Library } from '../../../api/src/vctoon/library/typing'

import { acceptHMRUpdate, defineStore } from 'pinia'

interface AccessState {
  libraries: Library[]
  /**
   * 用户信息
   */
  userInfo: CurrentUser | null
}

/**
 * @zh_CN 用户信息相关
 */
export const useUserStore = defineStore('core-user', {
  actions: {
    setUserInfo(userInfo: CurrentUser | null) {
      this.userInfo = userInfo
    },
    setLibraries(libraries: Library[]) {
      this.libraries = libraries
    },
  },
  state: (): AccessState => ({
    userInfo: null,
    libraries: [],
  }),
})

// 解决热更新问题
const hot = import.meta.hot
if (hot) {
  hot.accept(acceptHMRUpdate(useUserStore, hot))
}

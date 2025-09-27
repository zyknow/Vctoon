import { ref } from 'vue'
import { useRouter } from 'vue-router'

import { useOidcManager } from '@vben/api'
import { LOGIN_PATH } from '@vben/constants'
import { resetAllStores, useAccessStore, useUserStore } from '@vben/stores'

import { defineStore } from 'pinia'

import { useAbpStore } from './abp'

export const useAuthStore = defineStore('auth', () => {
  const accessStore = useAccessStore()
  const userStore = useUserStore()
  const router = useRouter()
  const oidc = useOidcManager()

  const loginLoading = ref(false)

  async function logout(redirect: boolean = true) {
    try {
      await oidc.manager.signoutRedirect()
    } catch {
      // 不做任何处理
    }
    resetAllStores()
    accessStore.setLoginExpired(false)

    // 回登录页带上当前路由地址
    await router.replace({
      path: LOGIN_PATH,
      query: redirect
        ? {
            redirect: encodeURIComponent(router.currentRoute.value.fullPath),
          }
        : {},
    })
  }

  async function fetchUserInfo() {
    const abpStore = useAbpStore()
    const oidcManager = useOidcManager()
    const user = await oidcManager.manager.getUser()
    if (!user?.access_token) {
      await oidcManager.manager.signinRedirect()
    }
    if (!abpStore.application?.currentUser?.id)
      await abpStore.initAbpApplicationConfiguration()

    const currentUser = abpStore.application.currentUser
    userStore.setUserInfo(currentUser)

    await userStore.reloadAllUserData()
    // 返回用户信息

    return abpStore.application.currentUser
  }

  function $reset() {
    loginLoading.value = false
  }

  return {
    $reset,
    fetchUserInfo,
    loginLoading,
    logout,
  }
})

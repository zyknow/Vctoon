import type {
  NavigationGuardNext,
  RouteLocationNormalized,
  Router,
} from 'vue-router'

import { useOidcManager } from '@/api/oidc'
import { useEnvConfig } from '@/hooks/useEnvConfig'
import { useAbpStore } from '@/stores/abp'
import { useUserStore } from '@/stores/user'

export function setupRouteGuards(router: Router) {
  const userStore = useUserStore()
  const oidc = useOidcManager()
  const abpStore = useAbpStore()

  const envConfig = useEnvConfig()
  router.beforeEach(async (to: RouteLocationNormalized) => {
    const res = await oidc.redirectHandler(to)
    if (res) {
      return res
    }
  })

  router.beforeEach(
    async (
      to: RouteLocationNormalized,
      _from: RouteLocationNormalized,
      next: NavigationGuardNext,
    ) => {
      const user = await oidc.manager.getUser()
      if (!user) {
        await oidc.signinRedirectWithState(to.fullPath)
        return
      }

      // 确保 ABP 配置已初始化
      if (!abpStore.application.currentUser) {
        await abpStore.loadAbpApplicationConfiguration()
      }

      // 基于用户权限生成路由
      if (!userStore.accessibleRoutes.length) {
        await userStore.generateRoutes()
        userStore.connectSignalRHubs()
        // 动态添加路由后，需要重新触发导航以匹配新路由
        return next({ ...to, replace: true })
      }

      return next()
    },
  )

  router.afterEach(() => {
    const baseTitle = envConfig.title
    document.title = baseTitle!
  })
}

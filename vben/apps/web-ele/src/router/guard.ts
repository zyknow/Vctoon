import type { Router } from 'vue-router'

import { dataChangedHub, useOidcManager } from '@vben/api'
import { LOGIN_PATH } from '@vben/constants'
import { preferences } from '@vben/preferences'
import { useAccessStore, useUserStore } from '@vben/stores'
import { startProgress, stopProgress } from '@vben/utils'

import { accessRoutes, coreRouteNames } from '#/router/routes'
import { useAuthStore } from '#/store'

import { generateAccess, refreshLibraryAccess } from './access'

function setupOidcRedirectGuard(router: Router) {
  const oidc = useOidcManager()
  router.beforeEach(async (to) => {
    if (to.path === '/auth/login') {
      const user = await oidc.manager.getUser()
      if (user) {
        // 用户已登录，执行其他逻辑
        return { path: '/home' }
      } else {
        await oidc.manager.signinRedirect()
      }
    }

    const res = await oidc.redirectHandler(to)
    if (res) {
      return res
    }
  })
}

/**
 * 通用守卫配置
 * @param router
 */
function setupCommonGuard(router: Router) {
  // 记录已经加载的页面
  const loadedPaths = new Set<string>()

  router.beforeEach((to) => {
    to.meta.loaded = loadedPaths.has(to.path)

    // 页面加载进度条
    if (!to.meta.loaded && preferences.transition.progress) {
      startProgress()
    }
    return true
  })

  router.afterEach((to) => {
    // 记录页面是否加载,如果已经加载，后续的页面切换动画等效果不在重复执行

    loadedPaths.add(to.path)

    // 关闭页面加载进度条
    if (preferences.transition.progress) {
      stopProgress()
    }
  })
}

/**
 * 权限访问守卫配置
 * @param router
 */
function setupAccessGuard(router: Router) {
  router.beforeEach(async (to, from) => {
    const accessStore = useAccessStore()
    const userStore = useUserStore()
    const authStore = useAuthStore()
    const oidc = useOidcManager()
    const accessToken = await oidc.getAccessToken()

    // 基本路由，这些路由不需要进入权限拦截
    if (coreRouteNames.includes(to.name as string)) {
      if (to.path === LOGIN_PATH && accessToken) {
        return decodeURIComponent(
          (to.query?.redirect as string) || preferences.app.defaultHomePath,
        )
      }
      return true
    }

    // accessToken 检查
    if (!accessToken) {
      // 明确声明忽略权限访问权限，则可以访问
      if (to.meta.ignoreAccess) {
        return true
      }

      const oidcUser = await oidc.manager.getUser()
      if (oidcUser?.access_token) {
        return to
      }

      // 没有访问权限，跳转登录页面
      if (to.fullPath !== LOGIN_PATH) {
        await oidc.manager.signinRedirect()
      }
      return to
    }

    // 是否已经生成过动态路由
    if (accessStore.isAccessChecked) {
      return true
    }

    // 生成路由表
    // 当前登录用户拥有的角色标识列表
    const userInfo = userStore.userInfo || (await authStore.fetchUserInfo())

    // 生成菜单和路由
    const { accessibleMenus, accessibleRoutes } = await generateAccess({
      roles: userInfo.roles,
      router,
      // 则会在菜单中显示，但是访问会被重定向到403
      routes: accessRoutes,
    })
    // 保存菜单信息和路由信息
    accessStore.setAccessMenus(accessibleMenus)
    accessStore.setAccessRoutes(accessibleRoutes)
    accessStore.setIsAccessChecked(true)
    const redirectPath = (from.query.redirect ??
      (to.path === preferences.app.defaultHomePath
        ? preferences.app.defaultHomePath
        : to.fullPath)) as string

    dataChangedHub.on('OnDeleted', (entityName, _) => {
      if (entityName === 'library') {
        refreshLibraryAccess()
      }
    })

    dataChangedHub.on('OnCreated', (entityName, _) => {
      if (entityName === 'library') {
        refreshLibraryAccess()
      }
    })

    dataChangedHub.on('OnUpdated', (entityName, _) => {
      if (entityName === 'library') {
        refreshLibraryAccess()
      }
    })

    return {
      ...router.resolve(decodeURIComponent(redirectPath)),
      replace: true,
    }
  })
}

/**
 * 项目守卫配置
 * @param router
 */
function createRouterGuard(router: Router) {
  setupOidcRedirectGuard(router)
  /** 通用 */
  setupCommonGuard(router)
  /** 权限访问 */
  setupAccessGuard(router)
}

export { createRouterGuard }

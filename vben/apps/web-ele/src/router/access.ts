import type {
  ComponentRecordType,
  GenerateMenuAndRoutesOptions,
  RouteRecordRaw,
} from '@vben/types'

import { generateAccessible } from '@vben/access'
import { libraryApi } from '@vben/api'
import { preferences } from '@vben/preferences'
import { useAccessStore, useUserStore } from '@vben/stores'

import { ElMessage } from 'element-plus'

import LibraryTitleComponent from '#/components/sider-menus/library-title-component.vue'
import { BasicLayout, IFrameView } from '#/layouts'
import { $t } from '#/locales'

import { router } from '.'
import { accessRoutes } from './routes'

const forbiddenComponent = () => import('#/views/_core/fallback/forbidden.vue')

async function generateAccess(options: GenerateMenuAndRoutesOptions) {
  const pageMap: ComponentRecordType = import.meta.glob('../views/**/*.vue')
  const layoutMap: ComponentRecordType = {
    BasicLayout,
    IFrameView,
  }

  const library = options.routes.find((x) => x.name === 'Library')

  if (library) {
    const userStore = useUserStore()

    const libraryRoutes = userStore.libraries
      .map(
        (x) =>
          ({
            meta: {
              keepAlive: false,
              order: 1001,
              title: x.name,
              menuTitleComponent: LibraryTitleComponent,
            },
            name: `library_${x.id}`,
            path: `library/${x.id}`,
            component: pageMap[`../views/library/library-medium.vue`],
          }) as RouteRecordRaw,
      )
      .filter((x) => !options.routes.some((r) => r.name === x.name)) // 防止重复添加

    options.routes.push(...libraryRoutes)
  }

  const access = await generateAccessible(preferences.app.accessMode, {
    ...options,
    fetchMenuListAsync: async () => {
      ElMessage({
        duration: 1500,
        message: `${$t('common.loadingMenu')}...`,
      })
      return []
    },
    // 可以指定没有权限跳转403页面
    forbiddenComponent,
    // 如果 route.meta.menuVisibleWithForbidden = true
    layoutMap,
    pageMap,
  })

  return access
}

async function refreshLibraryAccess() {
  const userStore = useUserStore()
  const accessStore = useAccessStore()
  // 生成路由表
  // 当前登录用户拥有的角色标识列表
  const libraries = await libraryApi.getCurrentUserLibraryList()
  userStore.setLibraries(libraries)
  // 生成菜单和路由
  const { accessibleMenus, accessibleRoutes } = await generateAccess({
    roles: userStore.userInfo?.roles,
    router,
    // 则会在菜单中显示，但是访问会被重定向到403
    routes: accessRoutes.filter(
      (x) => !x.name?.toString().startsWith('library_'),
    ),
  })
  // 保存菜单信息和路由信息
  accessStore.setAccessMenus(accessibleMenus)
  accessStore.setAccessRoutes(accessibleRoutes)
}

export { generateAccess, refreshLibraryAccess }

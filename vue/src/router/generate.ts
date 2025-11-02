import type { RouteRecordRaw } from 'vue-router'

import { MediumType } from '@/api/http/library'
import LibraryTrailingComponent from '@/components/sider-menus/LibraryTrailingComponent.vue'
import { $t } from '@/locales/i18n'
import { useAbpStore } from '@/stores/abp'
import { useUserStore } from '@/stores/user'

export const useGenerateRoutes = () => {
  const abpStore = useAbpStore()
  const userStore = useUserStore()

  const generateRoutes = (routes: RouteRecordRaw[]): RouteRecordRaw[] => {
    const filteredRoutes = filterAuthority(routes)
    const filteredRolesRoutes = filterRoles(filteredRoutes)

    filteredRolesRoutes.forEach((route) => {
      if (route.children?.length) {
        route.children = generateRoutes(route.children)
      }
    })

    return orderRoutes(filteredRolesRoutes)
  }

  const filterAuthority = (routes: RouteRecordRaw[]): RouteRecordRaw[] => {
    return routes.filter((r) => {
      // 忽略访问控制
      if (r.meta?.ignoreAccess) return true

      // 没有定义 authority，表示不需要权限检查
      if (!r.meta?.authority) return true

      // 有 authority，需要检查是否授予权限
      return (
        abpStore.application?.auth?.grantedPolicies[r.meta.authority] === true
      )
    })
  }

  const filterRoles = (routes: RouteRecordRaw[]): RouteRecordRaw[] => {
    return routes.filter((r) => {
      // 忽略访问控制
      if (r.meta?.ignoreAccess) return true

      // 没有定义 roles，表示不需要角色检查
      if (!r.meta?.roles?.length) return true

      // 有 roles，需要检查用户是否拥有对应角色
      return userStore.info?.roles.some((role) => r.meta!.roles!.includes(role))
    })
  }

  const orderRoutes = (routes: RouteRecordRaw[]): RouteRecordRaw[] => {
    return routes.sort((a, b) => (a.meta?.order || 0) - (b.meta?.order || 0))
  }
  return {
    generateRoutes,
  }
}

export const useGenerateMenus = () => {
  const generateMenus = (
    routes: RouteRecordRaw[],
    parentPath = '',
  ): MenuRecordRaw[] => {
    const menus: MenuRecordRaw[] = []

    for (const route of routes) {
      // 跳过在菜单中隐藏的路由
      if (route.meta?.hideInMenu) {
        continue
      }

      // 确保路由有 name 和 path
      if (!route.name || !route.path) {
        continue
      }

      // 计算完整路径
      const fullPath = route.path.startsWith('/')
        ? route.path
        : `${parentPath}/${route.path}`.replace(/\/+/g, '/')

      const menu: MenuRecordRaw = {
        localeKey: route.meta?.title || String(route.name),
        label: $t(route.meta?.title || String(route.name)),
        name: String(route.name),
        to: fullPath, // 使用完整路径
        icon: route.meta?.icon,
        order: route.meta?.order,
        selectable: route.meta?.menuSelectable ?? true,
        trailingComponent: route.meta?.trailingComponent,
        open: true,
        type: !(route.meta?.menuSelectable ?? true) ? 'label' : 'trigger',
      }

      // 递归处理子路由
      if (route.children?.length) {
        const childMenus = generateMenus(route.children, fullPath)
        if (childMenus.length > 0) {
          menu.children = childMenus
        }
      }

      menus.push(menu)
    }

    generateLibraryMenus(menus)
    // 按照 order 排序
    return menus.sort((a, b) => (a.order || 0) - (b.order || 0))
  }

  const generateLibraryMenus = (menus: MenuRecordRaw[]) => {
    const mediumTypeIconMap: Record<MediumType, string> = {
      [MediumType.Comic]: 'i-lucide-book-open',
      [MediumType.Video]: 'i-lucide-video',
    }

    const userStore = useUserStore()
    const libraryMenus: MenuRecordRaw[] = userStore.libraries.map(
      (library) => ({
        localeKey: library.name,
        label: library.name,
        name: `library_${library.id}`,
        to: `/library/${library.id}`,
        trailingComponent: markRaw(LibraryTrailingComponent),
        icon: mediumTypeIconMap[library.mediumType],
        order: 0,
        selectable: true,
        open: true,
        type: 'trigger',
      }),
    )

    const mainLibraryMenu = menus.find((menu) => menu.name === 'Library')
    if (mainLibraryMenu) {
      mainLibraryMenu.children = libraryMenus
    }
  }

  return {
    generateMenus,
  }
}

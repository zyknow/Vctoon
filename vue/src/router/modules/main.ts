import { markRaw } from 'vue'
import type { RouteRecordRaw } from 'vue-router'

import MainLibraryTrailingComponent from '@/components/sider-menus/MainLibraryTrailingComponent.vue'

// 不要使用 真实的 $t，会触发HMR 导致无法热更新
const $t = (key: string) => key

export const mainRoutes: RouteRecordRaw[] = [
  {
    path: '/home',
    name: 'Home',
    component: () => import('@/pages/home/index.vue'),
    meta: {
      title: $t('page.home.title'),
      requiresAuth: false,
      keepAlive: true,
      icon: 'i-lucide-house',
      isMobileBottomNav: true,
    },
  },
  {
    path: '/library',
    name: 'Library',
    component: () => import('@/pages/library/index.vue'),
    meta: {
      menuSelectable: false,
      title: $t('page.library.title'),
      requiresAuth: false,
      trailingComponent: markRaw(MainLibraryTrailingComponent),
      icon: 'mdi:page-layout-sidebar-right',
      isMobileBottomNav: true,
    },
    children: [],
  },
  {
    path: '/tag',
    name: 'Tag',
    component: () => import('@/pages/tag/index.vue'),
    meta: {
      title: $t('page.tag.title'),
      requiresAuth: false,
      keepAlive: true,
      icon: 'i-lucide-tag',
    },
  },
  {
    path: '/artist',
    name: 'Artist',
    component: () => import('@/pages/artist/index.vue'),
    meta: {
      title: $t('page.artist.title'),
      requiresAuth: false,
      keepAlive: true,
      icon: 'i-lucide-user',
    },
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('@/pages/profile/index.vue'),
    meta: {
      title: $t('preferences.userMenu.profile'),
      hideInMenu: true,
      requiresAuth: false,
      keepAlive: true,
      icon: 'i-lucide-user',
    },
  },
  {
    path: '/user',
    name: 'User',
    component: () => import('@/pages/user/index.vue'),
    meta: {
      title: $t('page.user.title'),
      requiresAuth: false,
      icon: 'i-lucide-users',
    },
  },
  // 安全日志
  {
    path: '/security-logs',
    name: 'SecurityLogs',
    component: () => import('@/pages/security-logs/index.vue'),
    meta: {
      title: $t('page.securityLogs.title'),
      requiresAuth: false,
      icon: 'i-lucide-shield-check',
    },
  },
  {
    path: '/setting',
    name: 'Setting',
    component: () => import('@/pages/setting/index.vue'),
    meta: {
      title: $t('page.setting.title'),
      requiresAuth: false,
      icon: 'i-lucide-settings',
      isMobileBottomNav: true,
    },
  },
  {
    meta: {
      icon: 'ci:search',
      keepAlive: true,
      hideInMenu: true,
      hideInTab: true,
      order: 5000,
      title: $t('page.search.title'),
    },
    name: 'Search',
    path: '/search',
    component: () => import('@/pages/search/index.vue'),
  },
  {
    meta: {
      keepAlive: true,
      keepAliveByParams: ['id'],
      hideInBreadcrumb: true,
      hideInMenu: true,
      hideInTab: true,
      title: '',
    },
    name: 'LibraryMedium',
    path: '/library/:id',
    component: () => import('@/pages/library/library-medium.vue'),
  },
  {
    component: () => import('@/pages/comic/index.vue'),
    meta: {
      hideInBreadcrumb: true,
      hideInMenu: true,
      hideInTab: true,
      title: $t('page.comic.reader.title'),
    },
    name: 'ComicReader',
    path: '/comic/:id/reader',
  },
  {
    component: () => import('@/pages/medium/medium-detail.vue'),
    meta: {
      hideInBreadcrumb: true,
      hideInMenu: true,
      title: $t('page.mediums.detail.title'),
    },
    name: 'MediumDetail',
    path: 'medium/:mediumId',
  },
]

export default mainRoutes

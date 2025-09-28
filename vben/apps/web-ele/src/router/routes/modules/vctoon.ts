import type { RouteRecordRaw } from 'vue-router'

import LibraryTitleComponent from '#/components/sider-menus/main-library-title-component.vue'
import { $t } from '#/locales'

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'ic:round-home',
      keepAlive: true,
      order: 10,
      title: $t('page.home.title'),
    },
    name: 'Home',
    path: '/home',
    component: () => import('#/views/home/index.vue'),
  },
  {
    meta: {
      icon: 'mdi:page-layout-sidebar-right',
      keepAlive: false,
      order: 100,
      title: $t('page.library.title'),
      menuTitleComponent: LibraryTitleComponent,
    },
    name: 'Library',
    path: '/library',
    component: () => import('#/views/library/library-main.vue'),
  },
  {
    meta: {
      icon: 'mdi:tag',
      keepAlive: true,
      order: 2001,
      title: $t('page.tag.title'),
    },
    name: 'Tag',
    path: '/tag',
    component: () => import('#/views/tag/index.vue'),
  },
  {
    meta: {
      icon: 'ci:user',
      keepAlive: true,
      order: 2002,
      title: $t('page.artist.title'),
    },
    name: 'Artist',
    path: '/artist',
    component: () => import('#/views/artist/index.vue'),
  },
  {
    meta: {
      icon: 'ci:user',
      keepAlive: false,
      order: 10_000,
      title: $t('page.user.title'),
    },
    name: 'User',
    path: '/user',
    component: () => import('#/views/user/index.vue'),
  },
  {
    meta: {
      icon: 'mdi:shield-lock',
      keepAlive: false,
      order: 10_001,
      title: $t('page.securityLogs.title'),
    },
    name: 'SecurityLogs',
    path: '/security-logs',
    component: () => import('#/views/security-logs/index.vue'),
  },
]

export default routes

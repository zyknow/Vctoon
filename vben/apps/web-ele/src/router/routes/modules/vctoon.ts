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
      menuSelectable: false,
    },
    name: 'Library',
    path: '/library',
    children: [],
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
      icon: 'mdi:cog',
      keepAlive: false,
      order: 10_002,
      title: $t('page.setting.title'),
    },
    name: 'Setting',
    path: '/setting',
    component: () => import('#/views/setting/index.vue'),
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
  {
    component: () => import('#/views/comic/index.vue'),
    meta: {
      hideInBreadcrumb: true,
      hideInMenu: true,
      hideInTab: true,
      noBasicLayout: true,
      title: $t('page.comic.reader.title'),
    },
    name: 'ComicReader',
    path: '/comic/:comicId/reader',
  },
  {
    component: () => import('#/views/medium/medium-detail.vue'),
    meta: {
      hideInBreadcrumb: true,
      hideInMenu: true,
      title: $t('page.mediums.detail.title'),
    },
    name: 'MediumDetail',
    path: 'medium/:type/:mediumId',
  },
  {
    meta: {
      icon: 'ci:search',
      keepAlive: false,
      order: 5000,
      title: $t('page.search.title'),
    },
    name: 'Search',
    path: '/search',
    component: () => import('#/views/search/index.vue'),
  },
]

export default routes

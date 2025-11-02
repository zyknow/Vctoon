import type { App } from 'vue'
import type { RouteRecordRaw } from 'vue-router'
import { createRouter, createWebHistory } from 'vue-router'

import MainLayout from '@/layouts/MainLayout.vue'

import { setupRouteGuards } from './guards'

const ROOT_ROUTE_NAME = 'Root'

export const staticRoutes: RouteRecordRaw[] = [
  {
    component: MainLayout,
    meta: {
      hideInBreadcrumb: true,
      title: 'Root',
    },
    name: ROOT_ROUTE_NAME,
    path: '/',
    redirect: '/home',
    children: [],
  },
]

// 404 路由需要在动态路由添加后最后注册，确保优先级最低
export const notFoundRoute: RouteRecordRaw = {
  path: '/:pathMatch(.*)*',
  name: 'not-found',
  component: () => import('@/pages/_core/not-found.vue'),
  meta: {
    title: 'Not Found',
  },
}

export const router = createRouter({
  history: createWebHistory(),
  routes: staticRoutes,
  scrollBehavior(_to, _from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    }

    return { top: 0, left: 0 }
  },
})

export function setupRouter(app: App) {
  app.use(router)
  setupRouteGuards(router)
}

export default router

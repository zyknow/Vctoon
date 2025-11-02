import { NavigationMenuItem } from '@nuxt/ui'
import type { Component } from 'vue'
import type { RouteRecordRaw } from 'vue-router'
interface RouteMeta {
  /**
   * abp authority permission key
   */
  authority?: string
  /**
   * abp authority permission roles
   */
  roles?: string[]
  /**
   * 当前路由在面包屑中不展现
   * @default false
   */
  hideInBreadcrumb?: boolean

  /**
   * 当前路由在菜单中不展现
   * @default false
   */
  hideInMenu?: boolean
  /**
   * 图标（菜单/tab）
   */
  icon?: Component | string
  /**
   * 忽略权限，直接可以访问
   * @default false
   */
  ignoreAccess?: boolean
  /**
   * 开启KeepAlive缓存
   */
  keepAlive?: boolean

  trailingComponent?: Component

  /**
   * 外链-跳转路径
   */
  link?: string
  /**
   * 菜单是否可选
   * @default true
   */
  menuSelectable?: boolean
  /**
   * 在新窗口打开
   */
  openInNewWindow?: boolean
  /**
   * 用于路由->菜单排序
   */
  order?: number
  /**
   * 菜单所携带的参数
   */
  query?: Recordable
  /**
   * 标题名称
   */
  title: string
}

export declare global {
  /**
   * 菜单原始对象
   */
  type MenuRecordRaw = NavigationMenuItem & {
    trailingComponent?: Component
  }
}

export type { RouteMeta, RouteRecordRaw }

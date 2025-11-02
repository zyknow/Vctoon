import { defineStore } from 'pinia'
import type { RouteRecordRaw } from 'vue-router'

import { type Artist, artistApi } from '@/api/http/artist'
import { type Library, libraryApi } from '@/api/http/library'
import { type Tag, tagApi } from '@/api/http/tag'
import { useOidcManager } from '@/api/oidc'
import { dataChangedHub } from '@/api/signalr//data-changed'
import { $t } from '@/locales/i18n'
import router, { notFoundRoute } from '@/router'
import { useGenerateMenus, useGenerateRoutes } from '@/router/generate'
import mainRoutes from '@/router/modules/main'

import { useAbpStore } from './abp'
export interface UserState {
  loadedKeys: string[]
  artists: Artist[]
  libraries: Library[]
  tags: Tag[]
  /** 可访问的路由列表（内部状态） */
  accessibleRoutes: RouteRecordRaw[]
  /** 可访问的菜单（内部状态） */
  accessMenus: MenuRecordRaw[]
}

export const useUserStore = defineStore('user', {
  state: (): UserState => ({
    accessibleRoutes: [],
    accessMenus: [],
    artists: [],
    tags: [],
    libraries: [],
    loadedKeys: [],
  }),
  getters: {
    info(): CurrentUser | null {
      const abpStore = useAbpStore()
      return abpStore.application.currentUser
    },
  },
  actions: {
    // 生成路由
    async generateRoutes() {
      await this.reloadLibraries()

      const { generateRoutes } = useGenerateRoutes()
      const { generateMenus } = useGenerateMenus()
      this.accessibleRoutes = generateRoutes(mainRoutes)

      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      this.accessMenus = generateMenus(this.accessibleRoutes)

      const root = router.getRoutes().find((item) => item.path === '/')
      // 获取已有的路由名称列表
      const names = root?.children?.map((item) => item.name) ?? []
      this.accessibleRoutes.forEach((route: RouteRecordRaw) => {
        // 根据router name判断，如果路由已经存在，则不再添加
        if (names?.includes(route.name)) {
          // 找到已存在的路由索引并更新，不更新会造成切换用户时，一级目录未更新，homePath 在二级目录导致的404问题
          const index = root?.children?.findIndex(
            (item) => item.name === route.name,
          )
          if (index !== undefined && index !== -1 && root?.children) {
            root.children[index] = route
          }
        } else {
          root?.children?.push(route)
        }
      })

      if (root) {
        if (root.name) {
          router.removeRoute(root.name)
        }
        router.addRoute(root)
      }

      // 确保 404 路由最后添加,优先级最低
      if (router.hasRoute('not-found')) {
        router.removeRoute('not-found')
      }
      router.addRoute(notFoundRoute)
    },
    async reloadLibraries() {
      if (this.loadedKeys.includes('libraries')) return
      this.libraries = await libraryApi.getCurrentUserLibraryList()
      this.loadedKeys.push('libraries')
      dataChangedHub.on('OnCreated', async (entityName, items) => {
        if (entityName === 'library') {
          this.libraries.push(...items)
          await this.generateRoutes()
        }
      })

      dataChangedHub.on('OnUpdated', (entityName, items) => {
        if (entityName === 'library') {
          for (const element of items) {
            const index = this.libraries.findIndex(
              (lib) => lib.id === element.id,
            )
            if (index !== -1) {
              this.libraries[index] = element
            }
          }
        }
      })
      dataChangedHub.on('OnDeleted', async (entityName, ids) => {
        if (entityName === 'library') {
          this.libraries = this.libraries.filter((lib) => !ids.includes(lib.id))
          await this.generateRoutes()
        }
      })
    },
    async reloadTags() {
      if (this.loadedKeys.includes('tags')) return
      this.tags = await tagApi.getAllTags()
      this.loadedKeys.push('tags')
      dataChangedHub.on('OnCreated', (entityName, items) => {
        if (entityName === 'tag') {
          this.tags.push(...items)
        }
      })

      dataChangedHub.on('OnUpdated', (entityName, items) => {
        if (entityName === 'tag') {
          for (const element of items) {
            const index = this.tags.findIndex((tag) => tag.id === element.id)
            if (index !== -1) {
              this.tags[index] = element
            }
          }
        }
      })
      dataChangedHub.on('OnDeleted', (entityName, ids) => {
        if (entityName === 'tag') {
          this.tags = this.tags.filter((tag) => !ids.includes(tag.id))
        }
      })
    },
    async reloadArtists() {
      if (this.loadedKeys.includes('artists')) return
      this.artists = await artistApi.getAllArtist()
      this.loadedKeys.push('artists')
      dataChangedHub.on('OnCreated', (entityName, items) => {
        if (entityName === 'artist') {
          this.artists.push(...items)
        }
      })

      dataChangedHub.on('OnUpdated', (entityName, items) => {
        if (entityName === 'artist') {
          for (const element of items) {
            const index = this.artists.findIndex(
              (artist) => artist.id === element.id,
            )
            if (index !== -1) {
              this.artists[index] = element
            }
          }
        }
      })
      dataChangedHub.on('OnDeleted', (entityName, ids) => {
        if (entityName === 'artist') {
          this.artists = this.artists.filter(
            (artist) => !ids.includes(artist.id),
          )
        }
      })
    },
    async connectSignalRHubs() {
      try {
        await dataChangedHub.start()
      } catch (error: any) {
        console.error('Error starting dataChangedHub:', error)
        if (error.message.includes("Status code '401'")) {
          // 401 错误，说明登录态过期，触发重新登录
          const oidcManager = useOidcManager()
          await oidcManager.manager.signinRedirect()
          return
        }
        throw error
      }
    },
    async refreshAccessMenuLocale() {
      const refresh = (menus: MenuRecordRaw[]) => {
        for (const menu of menus) {
          if (menu.localeKey) {
            menu.label = $t(menu.localeKey)
          }

          if (menu.children && menu.children.length > 0) {
            refresh(menu.children)
          }
        }
      }
      refresh(this.accessMenus)
    },
  },
})

import {
  artistApi,
  dataChangedHub,
  libraryApi,
  tagApi,
  type Artist,
  type Tag,
} from '@vben/api'
import type { Library } from '../../../api/src/vctoon/library/typing'

import { acceptHMRUpdate, defineStore } from 'pinia'

interface AccessState {
  libraries: Library[]
  /**
   * 用户信息
   */
  userInfo: CurrentUser | null

  tags: Tag[]
  artists: Artist[]
  loadedKeys: string[]
}

/**
 * @zh_CN 用户信息相关
 */
export const useUserStore = defineStore('core-user', {
  actions: {
    setUserInfo(userInfo: CurrentUser | null) {
      this.userInfo = userInfo
    },
    async reloadLibraries() {
      if (this.loadedKeys.includes('libraries')) return
      this.libraries = await libraryApi.getCurrentUserLibraryList()
      this.loadedKeys.push('libraries')
      dataChangedHub.on('OnCreated', (entityName, items) => {
        if (entityName === 'library') {
          this.libraries.push(...items)
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
      dataChangedHub.on('OnDeleted', (entityName, ids) => {
        if (entityName === 'library') {
          this.libraries = this.libraries.filter((lib) => !ids.includes(lib.id))
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
    async reloadAllUserData() {
      await dataChangedHub.start()

      await Promise.all([
        this.reloadLibraries(),
        this.reloadTags(),
        this.reloadArtists(),
      ])
    },
  },
  state: (): AccessState => ({
    userInfo: null,
    libraries: [],
    tags: [],
    artists: [],
    loadedKeys: [],
  }),
})

// 解决热更新问题
const hot = import.meta.hot
if (hot) {
  hot.accept(acceptHMRUpdate(useUserStore, hot))
}

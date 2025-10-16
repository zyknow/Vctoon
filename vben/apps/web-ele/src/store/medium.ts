import type { MediumViewTab } from '#/hooks/useMediumProvider'

import { defineStore } from 'pinia'

import { ItemDisplayMode } from './typing'

export const useMediumStore = defineStore('medium', {
  state: () => ({
    itemZoom: 1,
    itemDisplayMode: ItemDisplayMode.Grid,
    libraryTabs: {} as Record<string, MediumViewTab>,
  }),
  getters: {},
  actions: {
    setLibraryTab(libraryId: string, tab: MediumViewTab) {
      this.libraryTabs[libraryId] = tab
    },
  },
  persist: { pick: ['itemZoom', 'itemDisplayMode', 'libraryTabs'] },
})

import { defineStore } from 'pinia'

import { ItemDisplayMode } from './typing'

export const useMediumStore = defineStore('medium', {
  state: () => ({
    itemZoom: 1,
    itemDisplayMode: ItemDisplayMode.Grid,
  }),
  getters: {},
  actions: {},
  persist: { pick: ['itemZoom', 'itemDisplayMode'] },
})

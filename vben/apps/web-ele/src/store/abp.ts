import { abpApplicationConfigurationApi } from '@vben/api'

import { defineStore } from 'pinia'

export const useAbpStore = defineStore('Abp', {
  state: () => ({
    application: {} as ApplicationConfigurationDto,
  }),
  actions: {
    async initAbpApplicationConfiguration() {
      if (!this.application?.currentUser?.id)
        this.application = await abpApplicationConfigurationApi.get(false)
    },
  },
  persist: {
    // 持久化
    pick: ['application'],
  },
})

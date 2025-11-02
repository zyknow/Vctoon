import { defineStore } from 'pinia'

import { abpApplicationConfigurationApi } from '@/api/http/abp-application-configuration'

export const useAbpStore = defineStore('Abp', {
  state: () => ({
    application: {} as ApplicationConfigurationDto,
  }),
  actions: {
    async loadAbpApplicationConfiguration() {
      this.application = await abpApplicationConfigurationApi.get(false)
    },
  },
})

import type { FeatureGroup, UpdateFeatureInput } from './typing'

import { requestClient } from '../../request'

const baseUrl = '/api/feature-management/features'

const url = {
  default: baseUrl,
  get: baseUrl,
  update: baseUrl,
  delete: baseUrl,
}

export const featuresApi = {
  url,
  get(providerName: string, providerKey: string) {
    return requestClient.get<FeatureGroup>(url.get, {
      params: { providerName, providerKey },
    })
  },
  update(
    providerName: string,
    providerKey: string,
    updateFeatureInput: UpdateFeatureInput,
  ) {
    return requestClient.put(url.update, updateFeatureInput, {
      params: { providerName, providerKey },
    })
  },
  delete(providerName: string, providerKey: string) {
    return requestClient.delete(url.delete, {
      params: { providerName, providerKey },
    })
  },
}

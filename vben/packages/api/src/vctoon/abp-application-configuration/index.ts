import { requestClient } from '../../request'

const baseUrl = '/api/abp/application-configuration'

const url = {
  get: baseUrl,
}

export const abpApplicationConfigurationApi = {
  url,
  get(includeLocalizationResources?: boolean) {
    return requestClient.get<ApplicationConfigurationDto>(url.get, {
      params: { includeLocalizationResources },
    })
  },
}

import type { ChangePasswordInput, Profile, UpdateProfileInput } from './typing'

import { requestClient } from '../../request'

const baseUrl = '/api/account/my-profile'

const url = {
  get: baseUrl,
  update: baseUrl,
  changePassword: `${baseUrl}/change-password`,
}

export const profileApi = {
  url,
  get() {
    return requestClient.get<Profile>(url.get)
  },
  update(input: UpdateProfileInput) {
    return requestClient.put(url.update, input)
  },
  changePassword(input: ChangePasswordInput) {
    return requestClient.post(url.changePassword, input)
  },
}

export * from './typing'

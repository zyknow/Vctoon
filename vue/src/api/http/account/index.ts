import { requestClient } from '../../request'
import type { IdentityUser } from '../user/typing'
import type { RegisterAccountInput, SendPasswordResetCodeInput } from './typing'

const baseUrl = '/api/account'

const url = {
  register: `${baseUrl}/register`,
  sendPasswordResetCode: `${baseUrl}/send-password-reset-code`,
  verifyPasswordResetToken: `${baseUrl}/verify-password-reset-token`,
  resetPassword: `${baseUrl}/resetPassword`,
}

export const accountApi = {
  router: url,
  register(input: RegisterAccountInput) {
    return requestClient.post<IdentityUser>(url.register, input)
  },
  sendPasswordResetCode(input: SendPasswordResetCodeInput) {
    return requestClient.post(url.sendPasswordResetCode, input)
  },
  verifyPasswordResetToken(input: {
    resetToken: 'string'
    userId: '3fa85f64-5717-4562-b3fc-2c963f66afa6'
  }) {
    return requestClient.post(url.verifyPasswordResetToken, input)
  },
  resetPassword(input: {
    password: 'string'
    resetToken: 'string'
    userId: '3fa85f64-5717-4562-b3fc-2c963f66afa6'
  }) {
    return requestClient.post(url.resetPassword, input)
  },
}

export * from './typing'

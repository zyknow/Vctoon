export type RegisterAccountInput = {
  appName: string
  emailAddress: string
  password: string
  userName: string
}

export type SendPasswordResetCodeInput = RegisterAccountInput

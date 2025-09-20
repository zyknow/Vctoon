export type Profile = {
  email: string
  extraProperties: {
    additionalProp1: string
    additionalProp2: string
    additionalProp3: string
  }
  hasPassword: boolean
  isExternal: boolean
  name: string
  phoneNumber: string
  surname: string
  userName: string
}

export type UpdateProfileInput = {
  email: string
  name: string
  phoneNumber: string
  surname: string
  userName: string
}

export type ChangePasswordInput = {
  currentPassword: string
  newPassword: string
}

export type IdentityUser = FullAuditedEntityDto & {
  email?: null | string

  emailConfirmed?: boolean

  emailReminderEnabled: boolean

  // surname?: string | null

  isActive?: boolean

  lastPasswordChangeTime?: null | string

  name?: null | string

  phoneNumber?: null | string

  phoneNumberConfirmed?: boolean
  tenantId?: null | string
  userName?: null | string
}

export type CreateIdentityUserInput = UpdateIdentityUserInput

export type UpdateIdentityUserInput = {
  email: string
  emailReminderEnabled?: boolean
  isActive: boolean
  lockoutEnabled: boolean
  name: string
  password?: string
  phoneNumber?: string
  roleNames: string[]
  surname?: string
  userName: string
}

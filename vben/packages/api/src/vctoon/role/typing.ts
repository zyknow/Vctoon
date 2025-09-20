export interface IdentityRole extends FullAuditedEntityDto {
  extraProperties: any
  id: string
  isDefault: boolean
  isPublic: boolean
  isStatic: boolean
  name: string
}

export type CreateIdentityRoleInput = UpdateIdentityRoleInput

export type UpdateIdentityRoleInput = {
  isDefault: boolean
  isPublic: boolean
  name: string
}

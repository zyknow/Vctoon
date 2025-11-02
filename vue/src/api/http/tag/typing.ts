/** Tag 模块类型 */

export type Tag = {
  id: string
} & {
  name?: string
  resourceCount?: null | number
  slug: number
}

export type TagCreateUpdate = {
  name: string
}

export type TagGetListInput = BasePageRequest & {
  filter?: string
}

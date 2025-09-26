/** Artist 模块类型（对齐 Vctoon.Libraries.Dtos.ArtistDto） */

export type Artist = FullAuditedEntityDto & {
  description?: string
  name?: string
}

export type ArtistCreateUpdate = {
  description?: string
  name?: string
}

export type ArtistGetListInput = BasePageRequest & {
  filter?: string
}

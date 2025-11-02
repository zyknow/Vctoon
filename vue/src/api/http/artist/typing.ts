/** Artist 模块类型（对齐 Vctoon.Libraries.Dtos.ArtistDto） */

export type Artist = EntityDto<string> & {
  name?: string
  resourceCount?: null | number
  slug?: number
}

export type ArtistCreateUpdate = {
  name: string
}

export type ArtistGetListInput = BasePageRequest & {
  filter?: string
}

/** Comic 模块类型（复用 Medium 基础类型） */

import type {
  MediumCreateUpdateBase,
  MediumGetListInputBase,
  MediumGetListOutputBase,
  MediumRelations,
} from '../base/medium-base'

export type ComicGetListOutput = MediumGetListOutputBase

export type Comic = ComicGetListOutput & MediumRelations & {
  comicImages?: ComicImage[]
}

export type ComicCreateUpdate = MediumCreateUpdateBase

export type ComicGetListInput = MediumGetListInputBase

export type ComicImage = {
  archiveInfoPathId?: null | string
  comicId: string
  id: string
  libraryId: string
  libraryPathId?: null | string
  name?: string
  path?: string
  size: number
}

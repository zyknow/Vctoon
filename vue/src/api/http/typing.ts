import type { Comic, ComicGetListOutput } from './comic'
import type { Video, VideoGetListOutput } from './video'

export type MediumDto = Comic | Video
export type MediumGetListOutput = ComicGetListOutput | VideoGetListOutput

import { requestClient } from '@/api/request'

import {
  createMediumBaseCurdApi,
  createMediumBaseCurdUrl,
} from '../base/medium-base'
import { MediumType } from '../library'
import type {
  Comic,
  ComicCreateUpdate,
  ComicGetListInput,
  ComicGetListOutput,
  ComicImage,
} from './typing'

/** Comic API 根路径 */
const baseUrl = '/api/app/comic'

const url = {
  ...createMediumBaseCurdUrl(baseUrl),
  /** 获取单张漫画图片流 */
  getComicImage: `${baseUrl}/comic-image/{comicImageId}?maxWidth={maxWidth}`,
  /** 删除单张漫画图片 */
  deleteComicImage: `${baseUrl}/comic-image/{comicImageId}`,
  /** 根据漫画 Id 获取该漫画的所有图片（列表数据而非流） */
  getImagesByComicId: `${baseUrl}/by-comic-id/{comicId}`,
}

export const comicApi = {
  url,
  ...createMediumBaseCurdApi<
    string,
    Comic,
    ComicGetListOutput,
    ComicCreateUpdate,
    ComicCreateUpdate,
    ComicGetListInput,
    PageResult<ComicGetListOutput>
  >(baseUrl, MediumType.Comic),
  /**
   * 删除单张漫画图片
   * @param comicImageId 图片 Id
   * @param deleteFile 是否同时删除物理文件
   */
  deleteComicImage(comicImageId: string, deleteFile?: boolean) {
    const requestUrl = url.deleteComicImage.format({ comicImageId })
    return requestClient.delete(requestUrl, { params: { deleteFile } })
  },

  /**
   * 根据漫画 Id 获取所有图片元数据
   * @param comicId 漫画 Id
   * @returns ComicImage[]
   */
  getImagesByComicId(comicId: string) {
    const requestUrl = url.getImagesByComicId.format({ comicId })
    return requestClient.get<ComicImage[]>(requestUrl)
  },
}

export * from './typing'

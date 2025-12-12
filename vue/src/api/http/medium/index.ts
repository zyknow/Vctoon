import { useEnvConfig } from '@/hooks/useEnvConfig'

import { requestClient } from '../../request'
import {
  createBaseCurdApi,
  createBaseCurdUrl,
} from '../base/curd-api-definition-base'
import type {
  ComicImage,
  CreateUpdateMedium,
  Medium,
  MediumGetListInput,
  MediumGetListOutput,
  MediumMultiUpdate,
  MediumSeriesListUpdate,
  MediumSeriesSortUpdate,
  ReadingProcessUpdate,
  Subtitle,
} from './typing'

const baseUrl = '/api/app/medium'
const { apiURL } = useEnvConfig()

const url = {
  ...createBaseCurdUrl(baseUrl),
  addArtistList: `${baseUrl}/add-artist-list`,
  addSeriesMediumList: `${baseUrl}/series-medium-list`,
  addTagList: `${baseUrl}/add-tag-list`,
  deleteArtistList: `${baseUrl}/delete-artist-list`,
  deleteComicImage: `${baseUrl}/comic-image/{comicImageId}`,
  deleteSeriesMediumList: `${baseUrl}/delete-series-medium-list`,
  deleteTagList: `${baseUrl}/delete-tag-list`,
  getComicImage: `${baseUrl}/comic-image/{comicImageId}`,
  getComicImageList: `${baseUrl}/comic-image-list/{mediumId}`,
  getCover: `${baseUrl}/cover`,
  getSeriesList: `${baseUrl}/series-list/{mediumId}`,
  getSubtitle: `${baseUrl}/{id}/subtitle`,
  getSubtitles: `${baseUrl}/{id}/subtitles`,
  seriesSort: `${baseUrl}/series-sort`,
  updateArtistList: `${baseUrl}/update-artist-list`,
  updateArtists: `${baseUrl}/{id}/artists`,
  updateCover: `${baseUrl}/{id}/cover`,
  updateReadingProcess: `${baseUrl}/reading-process`,
  updateTagList: `${baseUrl}/update-tag-list`,
  updateTags: `${baseUrl}/{id}/tags`,
}

export const mediumApi = {
  url,
  ...createBaseCurdApi<
    string,
    Medium,
    MediumGetListOutput,
    CreateUpdateMedium,
    CreateUpdateMedium,
    MediumGetListInput,
    PageResult<MediumGetListOutput>
  >(baseUrl),

  async addArtistList(input: MediumMultiUpdate) {
    return requestClient.post(url.addArtistList, input)
  },

  async addSeriesMediumList(input: MediumSeriesListUpdate) {
    return requestClient.post(url.addSeriesMediumList, input)
  },

  async addTagList(input: MediumMultiUpdate) {
    return requestClient.post(url.addTagList, input)
  },

  async deleteArtistList(input: MediumMultiUpdate) {
    return requestClient.post(url.deleteArtistList, input)
  },

  async deleteComicImage(comicImageId: string, deleteFile: boolean) {
    return requestClient.delete(url.deleteComicImage.format({ comicImageId }), {
      params: { deleteFile },
    })
  },

  async deleteSeriesMediumList(input: MediumSeriesListUpdate) {
    return requestClient.post(url.deleteSeriesMediumList, input)
  },

  async deleteTagList(input: MediumMultiUpdate) {
    return requestClient.post(url.deleteTagList, input)
  },

  getComicImageUrl(comicImageId: string, maxWidth?: number) {
    const u = url.getComicImage.format({ comicImageId })
    return `${apiURL}${u}${maxWidth ? `?maxWidth=${maxWidth}` : ''}`
  },

  async getComicImageList(mediumId: string) {
    return requestClient.get<ComicImage[]>(
      url.getComicImageList.format({ mediumId }),
    )
  },

  getCoverUrl(cover: string) {
    return `${apiURL}${url.getCover}?cover=${cover}`
  },

  getVideoStreamUrl(id: string) {
    return `${apiURL}${baseUrl}/${id}/video-stream`
  },

  async getSubtitle(id: string, file: string) {
    return requestClient.get(url.getSubtitle.format({ id }), {
      params: { file },
      responseType: 'blob',
    })
  },

  async getSubtitles(id: string) {
    return requestClient.get<Subtitle[]>(url.getSubtitles.format({ id }))
  },

  async getSeriesList(mediumId: string) {
    return requestClient.get<MediumGetListOutput[]>(
      url.getSeriesList.format({ mediumId }),
    )
  },

  async seriesSort(input: MediumSeriesSortUpdate) {
    return requestClient.put(url.seriesSort, input)
  },

  async updateArtistList(input: MediumMultiUpdate) {
    return requestClient.post(url.updateArtistList, input)
  },

  async updateArtists(id: string, artistIds: string[]) {
    return requestClient.put<Medium>(
      url.updateArtists.format({ id }),
      artistIds,
    )
  },

  async updateCover(id: string, cover: File) {
    const formData = new FormData()
    formData.append('cover', cover)
    return requestClient.put<Medium>(url.updateCover.format({ id }), formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
  },

  async updateReadingProcess(items: ReadingProcessUpdate[]) {
    return requestClient.put(url.updateReadingProcess, items)
  },

  async updateTagList(input: MediumMultiUpdate) {
    return requestClient.post(url.updateTagList, input)
  },

  async updateTags(id: string, tagIds: string[]) {
    return requestClient.put<Medium>(url.updateTags.format({ id }), tagIds)
  },
}

export * from './typing'

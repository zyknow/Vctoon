import { requestClient } from '../../request'

export function createBaseCurdApi<
  TId,
  TEntity,
  TGetListOutputEntity,
  TUpdateInputEntity,
  TCreateInputEntity,
  TPageRequest = BasePageRequest,
  TPageResult = PageResult<TEntity>,
>(baseUrl: string) {
  return {
    getById(id: TId) {
      return requestClient.get<TGetListOutputEntity>(`${baseUrl}/${id}`)
    },
    getPage(pageRequest: TPageRequest) {
      return requestClient.get<TPageResult>(baseUrl, {
        params: pageRequest,
        paramsSerializer: 'repeat',
      })
    },
    update(input: TUpdateInputEntity, id: TId) {
      return requestClient.put<TEntity>(`${baseUrl}/${id}`, input)
    },
    delete(id: TId) {
      return requestClient.delete(`${baseUrl}/${id}`)
    },
    create(input: TCreateInputEntity) {
      return requestClient.post<TEntity>(`${baseUrl}`, input)
    },
  }
}

export function createBaseCurdUrl(baseUrl: string) {
  return {
    getById: `${baseUrl}/{id}`,
    getPage: `${baseUrl}`,
    update: `${baseUrl}/{id}`,
    delete: `${baseUrl}/{id}`,
    create: `${baseUrl}`,
  }
}

export declare global {
  interface CustomAxiosInstance
    extends Omit<
      AxiosInstance,
      'get' | 'post' | 'put' | 'patch' | 'delete' | 'request'
    > {
    request<T = any>(config: AxiosRequestConfig): Promise<T>
    get<T = any>(url: string, config?: AxiosRequestConfig): Promise<T>
    delete<T = any>(url: string, config?: AxiosRequestConfig): Promise<T>
    head<T = any>(url: string, config?: AxiosRequestConfig): Promise<T>
    options<T = any>(url: string, config?: AxiosRequestConfig): Promise<T>
    post<T = any>(
      url: string,
      data?: any,
      config?: AxiosRequestConfig,
    ): Promise<T>
    put<T = any>(
      url: string,
      data?: any,
      config?: AxiosRequestConfig,
    ): Promise<T>
    patch<T = any>(
      url: string,
      data?: any,
      config?: AxiosRequestConfig,
    ): Promise<T>
  }

  interface BasePageRequest {
    sorting?: string
    skipCount?: number
    maxResultCount?: number
  }

  interface FilterPageRequest extends BasePageRequest {
    filter?: string
  }

  interface ItemsResult<TEntity = any> {
    items: TEntity[]
  }

  interface PageResult<TEntity = any> extends ItemsResult<TEntity> {
    totalCount: number
  }
}

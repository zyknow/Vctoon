export declare global {
  // type AxiosResultNotifyOpt = {
  //   duration?: number
  //   placement?: MessagePlacement
  //   showClose?: boolean
  // }
  // type AxiosResultNotificationOpt = {
  //   duration?: number
  //   message: any
  //   position?: MessagePlacement
  //   showClose?: boolean
  //   title?: any
  // }
  // type AxiosResultNotify = {
  //   notification(context?: string, opt?: AxiosResultNotificationOpt): void
  //   notificationOnErr(context?: string, opt?: AxiosResultNotificationOpt): void
  //   notify(context?: string, opt?: AxiosResultNotifyOpt): void
  //   notifyOnErr(context?: string, opt?: AxiosResultNotifyOpt): void
  // }

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

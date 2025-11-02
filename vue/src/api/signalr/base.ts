import type {
  HubConnection,
  IHttpConnectionOptions,
  IRetryPolicy,
} from '@microsoft/signalr'
import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'

import { useOidcManager } from '../oidc'

export interface createSignalROptions {
  automaticReconnect?: boolean
  autoStart?: boolean
  nextRetryDelayInMilliseconds?: number
  serverUrl: string
  useAccessToken?: boolean
}

export type ReceiveMap = Record<string, (...args: any[]) => void>
export type SendMap = Record<string, (...args: any[]) => any>

export interface UseHubInstance<
  OnReceive extends ReceiveMap,
  OnSend extends SendMap,
> {
  /** 原生连接实例 */
  connection: HubConnection
  /** 获取 ConnectionId */
  getConnectionId: () => null | string | undefined
  /** 调用并获取返回值（等价于 SignalR invoke） */
  invoke: <K extends keyof OnSend & string>(
    method: K,
    ...args: Parameters<OnSend[K]>
  ) => Promise<Awaited<ReturnType<OnSend[K]>>>
  /** 状态便捷查询 */
  isConnected: () => boolean
  /** 取消事件监听（可选传 handler 精确移除） */
  off: <K extends keyof OnReceive & string>(
    event: K,
    handler?: OnReceive[K],
  ) => void
  /** 监听服务端事件（类型安全） */
  on: <K extends keyof OnReceive & string>(
    event: K,
    handler: OnReceive[K],
  ) => void
  /** 生命周期回调 */
  onClose: (cb: (error?: Error) => void) => void
  onReconnected: (cb: (connectionId?: string) => void) => void
  onReconnecting: (cb: (error?: Error) => void) => void
  /** 重启连接 */
  restart: () => Promise<void>
  /** 启动连接（幂等） */
  start: () => Promise<void>
  /** 停止连接（幂等） */
  stop: () => Promise<void>
}

/**
 * 一个轻量的 SignalR Hub 封装：
 * - 自动或手动启动
 * - 可选接入 OIDC AccessToken
 * - 简洁的 on/off、invoke 与生命周期回调
 * - 泛型 OnReceive/OnSend 让事件与调用有类型提示
 */
const createHub = <
  OnReceive extends ReceiveMap = ReceiveMap,
  OnSend extends SendMap = SendMap,
>(
  options: createSignalROptions,
): UseHubInstance<OnReceive, OnSend> => {
  const {
    serverUrl,
    automaticReconnect = true,
    autoStart = true,
    nextRetryDelayInMilliseconds,
    useAccessToken = true,
  } = options

  // 配置 AccessToken（如启用）
  const httpOptions: IHttpConnectionOptions = {}
  if (useAccessToken) {
    const { getAccessToken } = useOidcManager()
    httpOptions.accessTokenFactory = async () => {
      const token = await getAccessToken()
      return token ?? ''
    }
  }

  // 自定义重试策略（如启用自动重连）
  const retryPolicy: IRetryPolicy | number[] | undefined = automaticReconnect
    ? {
        nextRetryDelayInMilliseconds: (ctx) => {
          if (ctx.elapsedMilliseconds >= 60_000) return null // 最长重连 60s
          if (typeof nextRetryDelayInMilliseconds === 'number')
            return nextRetryDelayInMilliseconds
          // 简单线性退避：2s, 4s, 6s, ...（上限 10s）
          const base = (ctx.previousRetryCount + 1) * 2000
          return Math.min(base, 10_000)
        },
      }
    : undefined

  // 构建连接
  const builder = new HubConnectionBuilder()
    .withUrl(serverUrl, httpOptions)
    .configureLogging(
      import.meta.env?.DEV ? LogLevel.Information : LogLevel.Warning,
    )

  if (retryPolicy) builder.withAutomaticReconnect(retryPolicy)

  const connection = builder.build()

  let starting: null | Promise<void> = null
  let stopping: null | Promise<void> = null

  const start = async () => {
    if (connection.state === HubConnectionState.Connected) return
    if (starting) return starting
    starting = connection
      .start()
      .catch((error) => {
        // 将错误抛出给调用方，便于外部兜底
        throw error
      })
      .finally(() => {
        starting = null
      })
    return starting
  }

  const stop = async () => {
    if (
      connection.state === HubConnectionState.Disconnected ||
      connection.state === HubConnectionState.Disconnecting
    )
      return
    if (stopping) return stopping
    stopping = connection.stop().finally(() => {
      stopping = null
    })
    return stopping
  }

  const restart = async () => {
    await stop()
    await start()
  }

  // 类型安全的事件绑定/解绑
  const on = <K extends keyof OnReceive & string>(
    event: K,
    handler: OnReceive[K],
  ) => {
    connection.on(event, handler as any)
  }

  const off = <K extends keyof OnReceive & string>(
    event: K,
    handler?: OnReceive[K],
  ) => {
    connection.off(event, handler as any)
  }

  const invoke = async <K extends keyof OnSend & string>(
    method: K,
    ...args: Parameters<OnSend[K]>
  ): Promise<Awaited<ReturnType<OnSend[K]>>> => {
    return (await connection.invoke(method, ...(args as any))) as any
  }

  const onClose = (cb: (error?: Error) => void) => connection.onclose(cb)
  const onReconnected = (cb: (connectionId?: string) => void) =>
    connection.onreconnected(cb)
  const onReconnecting = (cb: (error?: Error) => void) =>
    connection.onreconnecting(cb)

  const isConnected = () => connection.state === HubConnectionState.Connected
  const getConnectionId = () => connection.connectionId

  // 自动启动（可关闭）
  if (autoStart) void start()

  return {
    connection,
    getConnectionId,
    invoke,
    isConnected,
    off,
    on,
    onClose,
    onReconnected,
    onReconnecting,
    restart,
    start,
    stop,
  }
}

export interface UseDefinedHub<
  OnReceive extends ReceiveMap,
  OnSend extends SendMap,
> {
  /** 便捷转发：等价于 use().getConnectionId() */
  getConnectionId: () => null | string | undefined
  /** 便捷转发：等价于 use().invoke(...) */
  invoke: <K extends keyof OnSend & string>(
    method: K,
    ...args: Parameters<OnSend[K]>
  ) => Promise<Awaited<ReturnType<OnSend[K]>>>
  /** 便捷转发：等价于 use().isConnected() */
  isConnected: () => boolean
  /** 便捷转发：等价于 use().off(...) */
  off: <K extends keyof OnReceive & string>(
    event: K,
    handler?: OnReceive[K],
  ) => void
  /** 便捷转发：等价于 use().on(...) */
  on: <K extends keyof OnReceive & string>(
    event: K,
    handler: OnReceive[K],
  ) => void
  /** 便捷转发：等价于 use().onclose(...) */
  onClose: (cb: (error?: Error) => void) => void
  /** 便捷转发：等价于 use().onreconnected(...) */
  onReconnected: (cb: (connectionId?: string) => void) => void
  /** 便捷转发：等价于 use().onreconnecting(...) */
  onReconnecting: (cb: (error?: Error) => void) => void
  /** 便捷转发：等价于 use().restart() */
  restart: () => Promise<void>
  /** 便捷转发：等价于 use().start() */
  start: () => Promise<void>
  /** 便捷转发：等价于 use().stop() */
  stop: () => Promise<void>
}

/**
 * 类似 defineStore 的 Hub 定义方法，返回一个可调用的 use 函数（单例）。
 */
export function defineHub<
  OnReceive extends ReceiveMap = ReceiveMap,
  OnSend extends SendMap = SendMap,
>(
  optionsOrFactory: (() => createSignalROptions) | createSignalROptions,
): UseDefinedHub<OnReceive, OnSend> {
  let instance: null | UseHubInstance<OnReceive, OnSend> = null
  const getOptions = () =>
    typeof optionsOrFactory === 'function'
      ? (optionsOrFactory as () => createSignalROptions)()
      : optionsOrFactory

  const getInstance = (): UseHubInstance<OnReceive, OnSend> => {
    if (instance) return instance
    const options = getOptions()
    instance = createHub<OnReceive, OnSend>(options)
    return instance
  }

  const api: UseDefinedHub<OnReceive, OnSend> = {
    getConnectionId: () => getInstance().getConnectionId(),
    invoke: (method, ...args) =>
      getInstance().invoke(method as any, ...(args as any)),
    isConnected: () => getInstance().isConnected(),
    off: (event, handler) => getInstance().off(event as any, handler as any),
    on: (event, handler) => getInstance().on(event as any, handler as any),
    onClose: (cb) => getInstance().onClose(cb),
    onReconnected: (cb) => getInstance().onReconnected(cb),
    onReconnecting: (cb) => getInstance().onReconnecting(cb),
    restart: () => getInstance().restart(),
    start: () => getInstance().start(),
    stop: () => getInstance().stop(),
  }

  return api
}

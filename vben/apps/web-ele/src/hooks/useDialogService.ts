import type { Component } from 'vue'

import { createApp, defineComponent, h, inject, provide, ref } from 'vue'

import { ElButton, ElDialog } from 'element-plus'

import { $t } from '#/locales'

/**
 * Dialog 基础可配置项（透传给 ElDialog）
 */
export interface DialogOptions {
  appendToBody?: boolean
  beforeClose?: (done: () => void) => void
  closeOnClickModal?: boolean
  customClass?: string
  destroyOnClose?: boolean
  fullscreen?: boolean
  modal?: boolean
  showClose?: boolean
  title?: string
  width?: number | string
}

/**
 * 旧版 API (openDialog / openConfirm) 已标记迁移期，可在后续版本移除。
 * 保留类型以避免外部引用立即报错。
 */
export interface LegacyDialogInstance {
  close: () => void
  destroy: () => void
}

interface InternalDialogRecord {
  id: number
  close: () => void
  destroy: () => void
}

interface DialogContext<R = any> {
  /** 结束并返回结果 */
  resolve: (value: R | undefined) => void
  /** 结束并标记异常 */
  reject: (reason?: any) => void
  /** 仅关闭（不触发 resolve/reject） */
  close: () => void
  /** 当前对话框 id */
  id: number
}

const DialogContextKey: unique symbol = Symbol('DialogContext')

export function useDialogContext<R = any>() {
  const ctx = inject<DialogContext<R>>(DialogContextKey as any)
  if (!ctx) {
    // 安全降级：在弹窗已经销毁后，某些异步回调/延迟触发仍调用该方法时，不再抛出硬错误，返回空实现避免中断。
    // 典型场景：beforeClose 异步确认 / resolve 后的延迟副作用再次触发。
    return {
      resolve: () => {},
      reject: () => {},
      close: () => {},
      id: -1,
    } as DialogContext<R>
  }
  return ctx
}

interface OpenParams<TProps> {
  dialog?: DialogOptions
  props?: TProps
  title?: string
  width?: number | string
}

type OpenReturn<R> = Promise<R | undefined> & { close: () => void }

type RenderFn<TProps> = (props: TProps) => any

// 全局注册表（仅当前模块内）
const dialogRegistry = new Map<number, InternalDialogRecord>()
let seed = 0

/**
 * 新一代对话服务
 */
export function useDialogService() {
  /**
   * 新 API：打开弹窗（Promise 化）
   */
  /**
   * 打开一个对话框。
   * 支持传入 组件 或 纯渲染函数（用于简易内容如 confirm）。
   */
  function open<TProps = any, R = any>(
    compOrRender: Component | RenderFn<TProps>,
    params: OpenParams<TProps> = {},
  ): OpenReturn<R> {
    const id = ++seed
    const {
      title = '', // 调用方可自行传入 i18n 文案
      width = '600px',
      props = {} as TProps,
      dialog: dialogOptions = {},
    } = params

    const visible = ref(true)
    let container: HTMLElement | null = document.createElement('div')
    document.body.append(container)

    let resolved = false

    let context!: DialogContext<R>

    const promise = new Promise<R | undefined>((resolve, reject) => {
      context = {
        resolve(value) {
          if (resolved) return
          resolved = true
          resolve(value)
          close()
        },
        reject(reason) {
          if (resolved) return
          resolved = true
          reject(reason)
          close()
        },
        close,
        id,
      }
    }) as OpenReturn<R>

    let internalClosing = false
    function runGuard(pass: () => void) {
      if (!dialogOptions?.beforeClose) return pass()
      dialogOptions.beforeClose(pass)
    }
    function proceedClose() {
      if (internalClosing) return
      internalClosing = true
      visible.value = false
    }
    function close() {
      if (!visible.value) return
      runGuard(() => {
        proceedClose()
      })
    }

    function destroy() {
      if (!container) return
      app.unmount()
      container.remove()
      container = null
      dialogRegistry.delete(id)
    }

    const Root = defineComponent({
      name: 'DynamicDialogRoot',
      setup() {
        // provide 一次上下文
        provide(DialogContextKey as any, context)
        const renderContent = () => {
          const isRenderFn =
            typeof compOrRender === 'function' &&
            !('setup' in (compOrRender as any))
          if (isRenderFn) {
            // 使用函数式组件包装，避免额外 defineComponent 触发多组件 lint 规则
            const Wrapper = (p: any) =>
              (compOrRender as RenderFn<TProps>)(p as TProps)
            return h(Wrapper as any, props as any)
          }
          return h(compOrRender as Component, props as any)
        }
        // 统一：ElDialog 的 beforeClose 仅做 guard，真正把 visible 置 false 的逻辑在 pass 后执行。
        const restDialogOptions = dialogOptions as any
        return () =>
          h(
            ElDialog,
            {
              modelValue: visible.value,
              title,
              width,
              showClose: dialogOptions.showClose ?? true,
              ...restDialogOptions,
              beforeClose: (done: () => void) => {
                if (internalClosing) return done()
                runGuard(() => {
                  proceedClose()
                  // 交给 Element Plus 后续触发关闭过渡 & onClosed
                  done()
                })
              },
              'onUpdate:modelValue': (val: boolean) => {
                if (!val) close()
              },
              onClosed: () => {
                if (!resolved) context.resolve(undefined)
                // 关闭动画结束后销毁
                destroy()
              },
            },
            { default: () => renderContent() },
          )
      },
    })

    const app = createApp(Root)
    app.mount(container)

    dialogRegistry.set(id, { id, close, destroy })

    promise.close = close
    return promise
  }

  /**
   * 新 confirm
   */
  function confirm(options: {
    cancelText?: string
    confirmText?: string
    danger?: boolean
    message: string
    title?: string
    width?: number | string
  }): Promise<boolean> {
    const {
      message,
      title = $t('common.confirm'),
      confirmText = $t('common.confirm'),
      cancelText = $t('common.cancel'),
      danger = false,
      width = 400,
    } = options
    return open<undefined, boolean>(
      () => {
        const { resolve } = useDialogContext<boolean>()
        return h('div', { class: 'dialog-confirm space-y-4' }, [
          h('p', { class: 'text-sm whitespace-pre-line' }, message),
          h('div', { class: 'flex justify-end gap-3' }, [
            h(
              ElButton,
              { disabled: false, onClick: () => resolve(false) },
              () => cancelText,
            ),
            h(
              ElButton,
              {
                type: danger ? 'danger' : 'primary',
                onClick: () => resolve(true),
              },
              () => confirmText,
            ),
          ]),
        ])
      },
      { title, width, dialog: { showClose: true } },
    ) as Promise<boolean>
  }

  /**
   * 关闭所有对话框
   */
  function closeAll() {
    dialogRegistry.forEach((r) => r.close())
  }

  // 旧 API 兼容（保留原函数名，内部桥接到 open）
  const openDialog = <T extends Component = Component>(
    component: T,
    componentProps: Record<string, any> = {},
    dialogOptions: DialogOptions = {},
  ): LegacyDialogInstance => {
    const p = open(component as any, {
      title: dialogOptions.title,
      width: dialogOptions.width,
      props: componentProps,
      dialog: dialogOptions,
    })
    return {
      close: () => p.close(),
      destroy: () => p.close(),
    }
  }

  const openConfirm = (message: string, title = $t('common.confirm')) =>
    confirm({ message, title })

  return { open, confirm, closeAll, openDialog, openConfirm }
}

export default useDialogService

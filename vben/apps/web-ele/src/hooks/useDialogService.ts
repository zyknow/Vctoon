import type { Component } from 'vue'

import { createApp, h, nextTick } from 'vue'

import { ElDialog } from 'element-plus'

export interface DialogOptions {
  title?: string
  width?: number | string
  closeOnClickModal?: boolean
  appendToBody?: boolean
  destroyOnClose?: boolean
  showClose?: boolean
  modal?: boolean
  customClass?: string
}

export interface DialogInstance {
  close: () => void
  destroy: () => void
}

/**
 * 通用 Dialog 服务 Hook
 * 基于 Element Plus Dialog API 实现动态弹窗挂载
 */
export function useDialogService() {
  /**
   * 打开弹窗并挂载 Vue 组件
   * @param component - Vue 组件
   * @param componentProps - 组件 props
   * @param dialogOptions - 弹窗配置选项
   * @returns DialogInstance - 弹窗实例
   */
  const openDialog = <T extends Component = Component>(
    component: T,
    componentProps: Record<string, any> = {},
    dialogOptions: DialogOptions = {},
  ): DialogInstance => {
    const {
      title = '',
      width = '600px',
      closeOnClickModal = false,
      appendToBody = true,
      destroyOnClose = true,
      showClose = true,
      modal = true,
      customClass = '',
    } = dialogOptions

    // 创建弹窗容器
    const container = document.createElement('div')
    document.body.append(container)

    // 弹窗可见性状态
    let visible = true

    // 创建弹窗内容 VNode
    const createDialogContent = () => {
      return h(
        ElDialog,
        {
          modelValue: visible,
          title,
          width,
          closeOnClickModal,
          appendToBody,
          destroyOnClose,
          showClose,
          modal,
          customClass,
          'onUpdate:modelValue': (value: boolean) => {
            visible = value
            if (!value) {
              // 弹窗关闭时延迟销毁
              nextTick(() => {
                instance.destroy()
              })
            }
          },
        },
        {
          default: () => h(component, componentProps),
        },
      )
    }

    // 创建 Vue 应用实例
    const app = createApp({
      render: createDialogContent,
    })
    app.mount(container)

    const instance: DialogInstance = {
      close() {
        visible = false
        // 触发弹窗关闭，会自动调用 destroy
      },
      destroy() {
        if (container && container.parentNode) {
          container.remove()
        }
        app.unmount()
      },
    }

    return instance
  }

  /**
   * 快速打开确认弹窗
   * @param message - 确认信息
   * @param title - 弹窗标题
   * @returns Promise<boolean> - 用户选择结果
   */
  const openConfirm = (
    message: string,
    title: string = '确认',
  ): Promise<boolean> => {
    return new Promise((resolve) => {
      const ConfirmComponent = {
        setup() {
          const handleConfirm = () => {
            resolve(true)
            instance.close()
          }

          const handleCancel = () => {
            resolve(false)
            instance.close()
          }

          return { handleConfirm, handleCancel, message }
        },
        template: `
          <div class="dialog-confirm">
            <p class="mb-4">{{ message }}</p>
            <div class="flex justify-end gap-3">
              <el-button @click="handleCancel">取消</el-button>
              <el-button type="primary" @click="handleConfirm">确认</el-button>
            </div>
          </div>
        `,
      }

      const instance = openDialog(
        ConfirmComponent,
        {},
        { title, width: '400px' },
      )
    })
  }

  return {
    openDialog,
    openConfirm,
  }
}

export default useDialogService

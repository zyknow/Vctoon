import type { Tag } from '@vben/api'

import { tagApi } from '@vben/api'
import { useIsMobile } from '@vben/hooks'

import { $t } from '#/locales'

import { useDialogService } from '../hooks/useDialogService'
import { createDirtyGuard, createEditDeduper } from '../utils/dialog-helpers'
import { buildEditCreateTitle } from '../utils/dialog-title'
import CreateOrUpdateTagDialog from '../views/tag/create-or-update-tag-dialog.vue'

/**
 * Tag 实体弹窗服务。
 * 使用通用对话服务打开创建或编辑表单，Promise 返回提交成功状态。
 */
export function useTagDialogService() {
  const dialog = useDialogService()
  const mobile = useIsMobile()

  // 去重：同一 Tag 编辑弹窗只保留一个实例（使用抽离 helper）
  const deduper = createEditDeduper<string, Tag | undefined>()

  interface BaseOptions {
    width?: string
    closeOnClickModal?: boolean
  }

  async function openCreate(options?: BaseOptions) {
    const title = buildEditCreateTitle(
      false,
      undefined,
      $t('page.tag.edit.title'),
      $t('page.tag.create.title'),
    )
    const { registerDirtyChecker, beforeClose, markClean } = createDirtyGuard()
    return dialog.open<
      {
        markClean: () => void
        registerDirtyChecker: (fn: () => boolean) => void
        tag?: Tag
      },
      Tag
    >(CreateOrUpdateTagDialog, {
      props: { registerDirtyChecker, markClean },
      title,
      width: options?.width || '500px',
      dialog: {
        closeOnClickModal: options?.closeOnClickModal ?? false,
        fullscreen: mobile.isMobile.value,
        beforeClose,
      },
    })
  }

  async function openEdit(param: string | Tag, options?: BaseOptions) {
    const id = typeof param === 'string' ? param : param.id
    // 使用 deduper 保证同 id 只打开一个实例
    if (id) {
      return deduper.run(id, async () => {
        return _openEditInternal(param, id, options)
      })
    }
    return _openEditInternal(param, id, options)
  }

  async function _openEditInternal(
    param: string | Tag,
    _id: string | undefined,
    options?: BaseOptions,
  ): Promise<Tag | undefined> {
    let entity: Tag | undefined = typeof param === 'string' ? undefined : param
    if (typeof param === 'string') {
      try {
        entity = await tagApi.getById(param)
      } catch {
        /* ignore */
      }
    }
    const title = buildEditCreateTitle(
      true,
      entity?.name,
      $t('page.tag.edit.title'),
      $t('page.tag.create.title'),
    )
    const { registerDirtyChecker, beforeClose, markClean } = createDirtyGuard()
    return dialog.open<
      {
        markClean: () => void
        registerDirtyChecker: (fn: () => boolean) => void
        tag?: Tag
      },
      Tag
    >(CreateOrUpdateTagDialog, {
      props: { tag: entity, registerDirtyChecker, markClean },
      title,
      width: options?.width || '500px',
      dialog: {
        closeOnClickModal: options?.closeOnClickModal ?? false,
        fullscreen: mobile.isMobile.value,
        beforeClose,
      },
    })
  }

  return { openCreate, openEdit }
}

export type UseTagDialogService = ReturnType<typeof useTagDialogService>

import type { Library } from '@vben/api'

import { libraryApi } from '@vben/api'
import { useIsMobile } from '@vben/hooks'

import { $t } from '#/locales'

import { createDirtyGuard, createEditDeduper } from '../utils/dialog-helpers'
import { buildEditCreateTitle } from '../utils/dialog-title'
import CreateLibraryDialog from '../views/library/create-or-update-library-dialog.vue'
import LibraryPermissionDialog from '../views/library/library-permission-dialog.vue'
import { useDialogService } from './useDialogService'

/**
 * Library 领域弹窗统一服务。
 * 设计要点：
 * 1. 三类弹窗（创建 / 编辑 / 权限）均为“无壳”内容组件，Dialog 外壳统一由 useDialogService 提供。
 * 2. Promise 化返回：根据用户操作 resolve(value | undefined)，取消/关闭返回 undefined。
 * 3. 移动端自动 fullscreen；PC 端固定宽度可通过 options.width 调整。
 * 4. closeOnClickModal 默认 false，避免误触关闭导致数据丢失。
 *
 * 返回值说明：
 *  - openCreate() => Promise<Library | undefined>
 *  - openEdit(libraryId) => Promise<Library | undefined>
 *  - openPermission(libraryId, name) => Promise<boolean | undefined> （true 表示已保存权限）
 */
export function useLibraryDialogService() {
  const dialog = useDialogService()
  const mobile = useIsMobile()
  const deduper = createEditDeduper<string, Library | undefined>()

  interface BaseOptions {
    width?: string
    closeOnClickModal?: boolean
  }

  const openCreate = (options?: BaseOptions) => {
    const { registerDirtyChecker, beforeClose, markClean } = createDirtyGuard()
    return dialog.open<
      {
        library?: Library
        markClean: () => void
        registerDirtyChecker: (fn: () => boolean) => void
      },
      Library
    >(CreateLibraryDialog, {
      props: { registerDirtyChecker, markClean },
      title: $t('page.library.create.title'),
      width: options?.width || '600px',
      dialog: {
        closeOnClickModal: options?.closeOnClickModal ?? false,
        fullscreen: mobile.isMobile.value,
        beforeClose,
      },
    })
  }

  // 复用同一内容组件，通过传入 libraryId 进入编辑模式
  /**
   * 编辑库弹窗
   * @param library 库实体或其 id
   * @param options 额外配置（可包含 libraryName 以避免额外请求）
   */
  const openEdit = async (
    library: Library | string,
    options?: BaseOptions & { libraryName?: string },
  ) => {
    const id = typeof library === 'string' ? library : library.id
    if (id) {
      return deduper.run(id, () => _openEditInternal(library, id, options))
    }
    return _openEditInternal(library, id, options)
  }

  async function _openEditInternal(
    library: Library | string,
    _id?: string,
    options?: BaseOptions & { libraryName?: string },
  ): Promise<Library | undefined> {
    let entity: Library | undefined =
      typeof library === 'string' ? undefined : library
    if (typeof library === 'string') {
      try {
        entity = await libraryApi.getById(library)
      } catch {
        /* ignore */
      }
    }
    const name = options?.libraryName || entity?.name
    const title = buildEditCreateTitle(
      true,
      name,
      $t('page.library.edit.title'),
      $t('page.library.create.title'),
    )
    const { registerDirtyChecker, beforeClose, markClean } = createDirtyGuard()
    return dialog.open<
      {
        library?: Library
        markClean: () => void
        registerDirtyChecker: (fn: () => boolean) => void
      },
      Library
    >(CreateLibraryDialog, {
      props: { library: entity, registerDirtyChecker, markClean },
      title,
      width: options?.width || '600px',
      dialog: {
        closeOnClickModal: options?.closeOnClickModal ?? false,
        fullscreen: mobile.isMobile.value,
        beforeClose,
      },
    })
  }

  const openPermission = (
    libraryId: string,
    libraryName: string,
    options?: BaseOptions,
  ) =>
    dialog.open<{ libraryId: string; libraryName: string }, boolean>(
      LibraryPermissionDialog,
      {
        props: { libraryId, libraryName },
        title: $t('page.library.permission.dialogTitle'),
        width: options?.width || '600px',
        dialog: {
          closeOnClickModal: options?.closeOnClickModal ?? false,
          fullscreen: mobile.isMobile.value,
        },
      },
    )

  return { openCreate, openEdit, openPermission }
}

export type UseLibraryDialogService = ReturnType<typeof useLibraryDialogService>

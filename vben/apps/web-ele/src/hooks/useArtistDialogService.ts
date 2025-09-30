import type { Artist } from '@vben/api'

import { artistApi } from '@vben/api'

import { $t } from '#/locales'

import { useDialogService } from '../hooks/useDialogService'
import { createDirtyGuard, createEditDeduper } from '../utils/dialog-helpers'
import { buildEditCreateTitle } from '../utils/dialog-title'
import CreateOrUpdateArtistDialog from '../views/artist/create-or-update-artist-dialog.vue'

/**
 * Artist 实体弹窗服务。
 * 封装创建/编辑逻辑，内部使用通用 useDialogService.open。
 * 返回 Promise<boolean>：true 代表提交成功，undefined/false 表示取消或未提交。
 */
export function useArtistDialogService() {
  const dialog = useDialogService()
  const deduper = createEditDeduper<string, Artist | undefined>()

  interface BaseOptions {
    width?: string
    closeOnClickModal?: boolean
    artist?: Artist
  }

  async function openCreate(options?: BaseOptions) {
    const title = buildEditCreateTitle(
      false,
      undefined,
      $t('page.artist.edit.title'),
      $t('page.artist.create.title'),
    )
    const { registerDirtyChecker, beforeClose, markClean } = createDirtyGuard()
    return dialog.open<
      {
        artist?: Artist
        markClean: () => void
        registerDirtyChecker: (fn: () => boolean) => void
      },
      Artist
    >(CreateOrUpdateArtistDialog, {
      props: { registerDirtyChecker, markClean },
      title,
      width: options?.width || '500px',
      dialog: {
        closeOnClickModal: options?.closeOnClickModal ?? false,
        beforeClose,
      },
    })
  }

  async function openEdit(param: Artist | string, options?: BaseOptions) {
    const id = typeof param === 'string' ? param : param.id
    if (id) {
      return deduper.run(id, () => _openEditInternal(param, id, options))
    }
    return _openEditInternal(param, id, options)
  }

  async function _openEditInternal(
    param: Artist | string,
    _id?: string,
    options?: BaseOptions,
  ): Promise<Artist | undefined> {
    let entity: Artist | undefined =
      typeof param === 'string' ? undefined : param

    if (typeof param === 'string') {
      try {
        entity = await artistApi.getById(param)
      } catch {
        /* ignore */
      }
    }
    const title = buildEditCreateTitle(
      true,
      entity?.name,
      $t('page.artist.edit.title'),
      $t('page.artist.create.title'),
    )
    const { registerDirtyChecker, beforeClose, markClean } = createDirtyGuard()
    return dialog.open<
      {
        artist?: Artist
        markClean: () => void
        registerDirtyChecker: (fn: () => boolean) => void
      },
      Artist
    >(CreateOrUpdateArtistDialog, {
      props: { artist: entity, registerDirtyChecker, markClean },
      title,
      width: options?.width || '500px',
      dialog: {
        closeOnClickModal: options?.closeOnClickModal ?? false,
        beforeClose,
      },
    })
  }

  return { openCreate, openEdit }
}

export type UseArtistDialogService = ReturnType<typeof useArtistDialogService>

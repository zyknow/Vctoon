import type { MediumDto } from '@vben/api'

import { comicApi, MediumType, videoApi } from '@vben/api'

import { $t } from '#/locales'

import MediumEditDialog from '../components/mediums/medium-edit-dialog.vue'
import { createDirtyGuard, createEditDeduper } from '../utils/dialog-helpers'
import { buildEditCreateTitle } from '../utils/dialog-title'
import { useDialogService } from './useDialogService'

export interface OpenMediumEditOptions {
  /** Medium 主键 */
  mediumId: string
  mediumType: MediumType
  /** 点击遮罩是否关闭 */
  closeOnClickModal?: boolean
}

export interface UseMediumDialogService {
  /** 打开编辑对话框，返回更新后的 medium（如果用户保存），否则 undefined */
  openEdit: (options: OpenMediumEditOptions) => Promise<MediumDto | undefined>
}

/**
 * 统一的 Medium 相关弹窗服务
 * - 封装编辑对话框打开逻辑
 * - 集中处理 i18n & 默认配置
 */
export function useMediumDialogService(): UseMediumDialogService {
  const { open } = useDialogService()
  // 仅编辑：无 create（见 MEDIUM_DIALOG_NOTE）
  const deduper = createEditDeduper<string, MediumDto | undefined>()

  async function openEdit({
    mediumId,
    mediumType,
    closeOnClickModal = false,
  }: OpenMediumEditOptions) {
    // 去重 key 加上类型，避免不同类型同 id 冲突
    const dedupeKey = `${mediumType}:${mediumId}`
    return deduper.run(dedupeKey, async () => {
      const medium = await fetchMedium(mediumId, mediumType)
      if (!medium) return undefined
      const title = buildEditCreateTitle(
        true,
        medium.title,
        $t('page.mediums.edit.title'),
        $t('page.mediums.edit.title'),
      )
      return _openInternal(medium, title, 600, closeOnClickModal)
    })
  }

  async function fetchMedium(
    id: string,
    mediumType: MediumType,
  ): Promise<MediumDto | undefined> {
    try {
      return mediumType === MediumType.Comic
        ? await comicApi.getById(id)
        : await videoApi.getById(id)
    } catch {
      return undefined
    }
  }

  function _openInternal(
    medium: MediumDto,
    title: string,
    width: number | string,
    closeOnClickModal: boolean,
  ): Promise<MediumDto | undefined> {
    const { registerDirtyChecker, beforeClose, markClean } = createDirtyGuard()
    return open<
      {
        markClean: () => void
        medium: MediumDto
        registerDirtyChecker: (fn: () => boolean) => void
      },
      MediumDto
    >(MediumEditDialog, {
      title,
      width,
      props: { medium, registerDirtyChecker, markClean },
      dialog: { closeOnClickModal, beforeClose },
    })
  }

  return { openEdit }
}

export default useMediumDialogService

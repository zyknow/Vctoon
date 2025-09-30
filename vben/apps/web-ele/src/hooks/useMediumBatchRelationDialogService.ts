import { $t } from '#/locales'

import MediumBatchRelationDialog from '../components/mediums/medium-batch-relation-dialog.vue'
import { useDialogService } from './useDialogService'

export type MediumRelationEntity = 'artist' | 'tag'
export type MediumRelationAction = 'add' | 'remove' | 'update'

export interface OpenMediumRelationDialogOptions {
  action: MediumRelationAction
  defaultSelectedIds?: string[]
  entity: MediumRelationEntity
  targetCount: number
}

export function useMediumBatchRelationDialogService() {
  const { open } = useDialogService()

  const buildTitle = (
    entity: MediumRelationEntity,
    action: MediumRelationAction,
  ) => {
    const key = `page.mediums.selection.dialog.title.${entity}.${action}`
    return $t(key)
  }

  const openRelationDialog = async (
    options: OpenMediumRelationDialogOptions,
  ) => {
    const { action, entity, defaultSelectedIds, targetCount } = options
    const title = buildTitle(entity, action)

    return open<
      {
        defaultSelectedIds?: string[]
        entity: MediumRelationEntity
        mode: MediumRelationAction
        targetCount: number
      },
      string[]
    >(MediumBatchRelationDialog, {
      title,
      width: 480,
      props: {
        defaultSelectedIds,
        entity,
        mode: action,
        targetCount,
      },
      dialog: {
        closeOnClickModal: false,
        showClose: true,
      },
    })
  }

  return {
    openRelationDialog,
  }
}

export type UseMediumBatchRelationDialogService = ReturnType<
  typeof useMediumBatchRelationDialogService
>

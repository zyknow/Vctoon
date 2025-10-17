<script setup lang="ts">
import type { MediumGetListOutput, MediumMultiUpdateDto } from '@vben/api'

import type {
  MediumRelationAction,
  MediumRelationEntity,
} from '#/hooks/useMediumBatchRelationDialogService'

import { computed, ref } from 'vue'

import { mediumResourceApi } from '@vben/api'
import { MdiChevronDown, MdiClose, MdiSelectAll } from '@vben/icons'

import { useMediumBatchRelationDialogService } from '#/hooks/useMediumBatchRelationDialogService'
import {
  useInjectedMediumItemProvider,
  useInjectedMediumProvider,
} from '#/hooks/useMediumProvider'
import { $t } from '#/locales'

defineProps<{ showSelectedAllBtn?: boolean }>()

const { selectedMediumIds, items } = useInjectedMediumItemProvider()
const mediumProvider = useInjectedMediumProvider()
const { openRelationDialog } = useMediumBatchRelationDialogService()

type RelationActionKey = `${MediumRelationEntity}:${MediumRelationAction}`

const pendingAction = ref<null | RelationActionKey>(null)

const selectedMediums = computed<MediumGetListOutput[]>(() => {
  const idSet = new Set(selectedMediumIds.value)
  return items.value.filter((item) => idSet.has(item.id))
})

// 计算选中数量
const selectedCount = computed(() => selectedMediumIds.value.length)

// 计算是否全选
const isAllSelected = computed(() => {
  return items.value.length > 0 && selectedCount.value === items.value.length
})

const isBusy = computed(() => pendingAction.value !== null)
const artistLoading = computed(() => {
  return pendingAction.value?.startsWith('artist:') ?? false
})
const tagLoading = computed(() => {
  return pendingAction.value?.startsWith('tag:') ?? false
})

const relationApiMap: Record<
  MediumRelationEntity,
  Record<
    MediumRelationAction,
    (input: MediumMultiUpdateDto) => Promise<unknown>
  >
> = {
  artist: {
    add: (input) => mediumResourceApi.addArtistList(input),
    remove: (input) => mediumResourceApi.deleteArtistList(input),
    update: (input) => mediumResourceApi.updateArtistList(input),
  },
  tag: {
    add: (input) => mediumResourceApi.addTagList(input),
    remove: (input) => mediumResourceApi.deleteTagList(input),
    update: (input) => mediumResourceApi.updateTagList(input),
  },
}

const createPayload = (ids: string[]): MediumMultiUpdateDto => {
  return {
    ids,
    items: selectedMediums.value.map((medium) => ({
      id: medium.id,
      mediumType: medium.mediumType,
    })),
  }
}

const executeRelationAction = async (
  entity: MediumRelationEntity,
  action: MediumRelationAction,
) => {
  if (selectedMediums.value.length === 0) return
  if (pendingAction.value) return

  const selectedIds = await openRelationDialog({
    action,
    entity,
    targetCount: selectedMediums.value.length,
  })

  if (selectedIds === undefined) return
  if (selectedIds.length === 0 && action !== 'update') {
    return
  }

  const payload = createPayload(selectedIds)
  if (payload.items.length === 0) return

  const actionKey = `${entity}:${action}` as RelationActionKey
  pendingAction.value = actionKey
  try {
    await relationApiMap[entity][action](payload)
    await mediumProvider.loadItems()
  } catch (error) {
    console.error('批量更新媒体关联失败', error)
  } finally {
    pendingAction.value = null
  }
}

const handleArtistCommand = async (
  command: MediumRelationAction | number | string,
) => {
  if (typeof command !== 'string') return
  await executeRelationAction('artist', command as MediumRelationAction)
}

const handleTagCommand = async (
  command: MediumRelationAction | number | string,
) => {
  if (typeof command !== 'string') return
  await executeRelationAction('tag', command as MediumRelationAction)
}

// 全选/取消全选
const toggleSelectAll = () => {
  selectedMediumIds.value = isAllSelected.value
    ? []
    : items.value.map((item) => item.id)
}

// 取消选择
const clearSelection = () => {
  selectedMediumIds.value = []
}
</script>

<template>
  <div class="flex w-full flex-row items-center justify-between">
    <slot name="filter-left">
      <div class="flex items-center gap-3">
        <!-- 选中数量显示 -->
        <span class="text-primary text-sm font-medium">
          {{
            $t('page.mediums.selection.selectedCount', { count: selectedCount })
          }}
        </span>

        <!-- 全选按钮 -->
        <el-button
          v-if="showSelectedAllBtn"
          type="primary"
          size="small"
          :icon="MdiSelectAll"
          @click="toggleSelectAll"
        >
          {{
            isAllSelected
              ? $t('page.mediums.selection.deselectAll')
              : $t('page.mediums.selection.selectAll')
          }}
        </el-button>
      </div>
    </slot>

    <slot name="filter-center">
      <div class="flex items-center gap-2">
        <el-dropdown
          trigger="click"
          :disabled="isBusy"
          @command="handleArtistCommand"
        >
          <el-button type="default" size="small" :loading="artistLoading">
            {{ $t('page.mediums.selection.artistActions.label') }}
            <MdiChevronDown class="ml-1 text-base" />
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="add">
                {{ $t('page.mediums.selection.artistActions.add') }}
              </el-dropdown-item>
              <el-dropdown-item command="update">
                {{ $t('page.mediums.selection.artistActions.update') }}
              </el-dropdown-item>
              <el-dropdown-item command="remove">
                {{ $t('page.mediums.selection.artistActions.remove') }}
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>

        <el-dropdown
          trigger="click"
          :disabled="isBusy"
          @command="handleTagCommand"
        >
          <el-button type="default" size="small" :loading="tagLoading">
            {{ $t('page.mediums.selection.tagActions.label') }}
            <MdiChevronDown class="ml-1 text-base" />
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item command="add">
                {{ $t('page.mediums.selection.tagActions.add') }}
              </el-dropdown-item>
              <el-dropdown-item command="update">
                {{ $t('page.mediums.selection.tagActions.update') }}
              </el-dropdown-item>
              <el-dropdown-item command="remove">
                {{ $t('page.mediums.selection.tagActions.remove') }}
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </slot>

    <slot name="filter-right">
      <div class="flex items-center">
        <!-- 退出选择模式按钮 -->
        <el-button
          size="small"
          type="default"
          :icon="MdiClose"
          @click="clearSelection"
        >
          {{ $t('page.mediums.selection.cancel') }}
        </el-button>
      </div>
    </slot>
  </div>
</template>

<style lang="scss" scoped></style>

<script setup lang="ts">
import { mediumResourceApi } from '@/api/http/medium-resource'
import type { MediumMultiUpdateDto } from '@/api/http/medium-resource/typing'
import type { MediumGetListOutput } from '@/api/http/typing'
import MediumBatchRelationModal from '@/components/overlays/MediumBatchRelationModal.vue'
import { useInjectedMediumItemProvider } from '@/hooks/useMediumProvider'

defineProps<{ showSelectedAllBtn?: boolean }>()

const { selectedMediumIds, items } = useInjectedMediumItemProvider()
const overlay = useOverlay()

// 创建 modal 实例
const relationModal = overlay.create(MediumBatchRelationModal)

type MediumRelationEntity = 'artist' | 'tag'
type MediumRelationAction = 'add' | 'remove' | 'update'
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

  const selectedIds = await relationModal.open({
    mode: action,
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
        <UButton
          v-if="showSelectedAllBtn"
          color="primary"
          size="sm"
          variant="ghost"
          icon="i-heroicons-check-circle"
          @click="toggleSelectAll"
        >
          {{
            isAllSelected
              ? $t('page.mediums.selection.deselectAll')
              : $t('page.mediums.selection.selectAll')
          }}
        </UButton>
      </div>
    </slot>

    <slot name="filter-center">
      <div class="flex items-center gap-2">
        <UDropdownMenu
          :items="[
            {
              label: $t('page.mediums.selection.artistActions.add'),
              icon: 'i-heroicons-plus',
              onSelect: () => handleArtistCommand('add'),
            },
            {
              label: $t('page.mediums.selection.artistActions.update'),
              icon: 'i-heroicons-arrow-path',
              onSelect: () => handleArtistCommand('update'),
            },
            {
              label: $t('page.mediums.selection.artistActions.remove'),
              icon: 'i-heroicons-trash',
              color: 'error',
              onSelect: () => handleArtistCommand('remove'),
            },
          ]"
          :disabled="isBusy"
        >
          <UButton size="sm" :loading="artistLoading" variant="ghost">
            {{ $t('page.mediums.selection.artistActions.label') }}
            <UIcon name="i-heroicons-chevron-down" class="ml-1" />
          </UButton>
        </UDropdownMenu>

        <UDropdownMenu
          :items="[
            {
              label: $t('page.mediums.selection.tagActions.add'),
              icon: 'i-heroicons-plus',
              onSelect: () => handleTagCommand('add'),
            },
            {
              label: $t('page.mediums.selection.tagActions.update'),
              icon: 'i-heroicons-arrow-path',
              onSelect: () => handleTagCommand('update'),
            },
            {
              label: $t('page.mediums.selection.tagActions.remove'),
              icon: 'i-heroicons-trash',
              color: 'error',
              onSelect: () => handleTagCommand('remove'),
            },
          ]"
          :disabled="isBusy"
        >
          <UButton size="sm" :loading="tagLoading" variant="ghost">
            {{ $t('page.mediums.selection.tagActions.label') }}
            <UIcon name="i-heroicons-chevron-down" class="ml-1" />
          </UButton>
        </UDropdownMenu>
      </div>
    </slot>

    <slot name="filter-right">
      <div class="flex items-center">
        <!-- 退出选择模式按钮 -->
        <UButton
          size="sm"
          color="neutral"
          variant="ghost"
          icon="i-heroicons-x-mark"
          @click="clearSelection"
        >
          {{ $t('page.mediums.selection.cancel') }}
        </UButton>
      </div>
    </slot>
  </div>
</template>

<style lang="scss" scoped></style>

<script setup lang="ts">
import { mediumApi } from '@/api/http/medium'
import type {
  MediumGetListOutput,
  MediumMultiUpdate,
  MediumSeriesListUpdate,
} from '@/api/http/medium/typing'
import MediumBatchRelationModal from '@/components/overlays/MediumBatchRelationModal.vue'
import { useInjectedMediumItemProvider } from '@/hooks/useMediumProvider'
import { $t } from '@/locales/i18n'

defineProps<{ showSelectedAllBtn?: boolean }>()

const { selectedMediumIds, items } = useInjectedMediumItemProvider()
const overlay = useOverlay()

// 创建 modal 实例
const relationModal = overlay.create(MediumBatchRelationModal)
const toast = useToast()

type MediumRelationEntity = 'artist' | 'tag' | 'series'
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
const seriesLoading = computed(() => {
  return pendingAction.value?.startsWith('series:') ?? false
})

const relationApiMap: Record<
  MediumRelationEntity,
  Record<MediumRelationAction, (input: MediumMultiUpdate) => Promise<unknown>>
> = {
  artist: {
    add: (input) => mediumApi.addArtistList(input),
    remove: (input) => mediumApi.deleteArtistList(input),
    update: (input) => mediumApi.updateArtistList(input),
  },
  tag: {
    add: (input) => mediumApi.addTagList(input),
    remove: (input) => mediumApi.deleteTagList(input),
    update: (input) => mediumApi.updateTagList(input),
  },
  series: {
    add: () => Promise.resolve(),
    remove: () => Promise.resolve(),
    update: () => Promise.resolve(),
  },
}

const createPayload = (ids: string[]): MediumMultiUpdate => {
  return {
    resourceIds: ids,
    mediumIds: selectedMediums.value.map((medium) => medium.id),
  }
}

const createSeriesPayload = (seriesId: string): MediumSeriesListUpdate => {
  return {
    seriesId,
    mediumIds: selectedMediums.value.map((medium) => medium.id),
  }
}

const executeRelationAction = async (
  entity: MediumRelationEntity,
  action: MediumRelationAction,
) => {
  if (selectedMediums.value.length === 0) return
  if (pendingAction.value) return

  const libraryId = selectedMediums.value[0]?.libraryId
  const mediumType = selectedMediums.value[0]?.mediumType

  const selectedIds = await relationModal.open({
    mode: action,
    entity,
    targetCount: selectedMediums.value.length,
    libraryId,
    mediumType,
  })

  if (selectedIds === undefined) return
  if (selectedIds.length === 0 && action !== 'update') {
    return
  }

  const actionKey = `${entity}:${action}` as RelationActionKey
  pendingAction.value = actionKey
  try {
    if (entity === 'series') {
      const seriesId = selectedIds[0]
      if (!seriesId) return
      const payload = createSeriesPayload(seriesId)
      if (payload.mediumIds.length === 0) return
      if (action === 'add') {
        await mediumApi.addSeriesMediumList(payload)
      } else if (action === 'remove') {
        await mediumApi.deleteSeriesMediumList(payload)
      }
      return
    }

    const payload = createPayload(selectedIds)
    if (payload.mediumIds.length === 0) return
    await relationApiMap[entity][action](payload)
  } catch {
    toast.add({
      title: $t('common.updateFailed'),
      color: 'error',
    })
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

const handleSeriesCommand = async (command: number | string) => {
  if (typeof command !== 'string') return
  if (command !== 'add' && command !== 'remove') return
  await executeRelationAction('series', command)
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

        <UDropdownMenu
          :items="[
            {
              label: $t('page.mediums.selection.seriesActions.add'),
              icon: 'i-heroicons-plus',
              onSelect: () => handleSeriesCommand('add'),
            },
            {
              label: $t('page.mediums.selection.seriesActions.remove'),
              icon: 'i-heroicons-trash',
              color: 'error',
              onSelect: () => handleSeriesCommand('remove'),
            },
          ]"
          :disabled="isBusy"
        >
          <UButton size="sm" :loading="seriesLoading" variant="ghost">
            {{ $t('page.mediums.selection.seriesActions.label') }}
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

<script setup lang="ts">
import type { Artist, Tag } from '@vben/api'

import { computed, onMounted, ref, watch } from 'vue'

import { artistApi, tagApi } from '@vben/api'
import { useUserStore } from '@vben/stores'

import { ElMessage } from 'element-plus'

import { useDialogContext } from '#/hooks/useDialogService'
import { $t } from '#/locales'

type RelationEntity = 'artist' | 'tag'
type RelationMode = 'add' | 'remove' | 'update'

defineOptions({
  name: 'MediumBatchRelationDialog',
  inheritAttrs: false,
})

const props = defineProps<{
  /** 默认选中的关联实体 Id 列表 */
  defaultSelectedIds?: string[]
  /** 关联实体类型 */
  entity: RelationEntity
  /** 操作模式 */
  mode: RelationMode
  /** 影响的媒体数量 */
  targetCount: number
}>()

const { resolve, close } = useDialogContext<string[] | undefined>()
const userStore = useUserStore()

const loadingOptions = ref(false)
const submitting = ref(false)
const selectedIds = ref<string[]>([...(props.defaultSelectedIds ?? [])])
const isHandlingSelection = ref(false)

const optionIdSet = computed(() => {
  return new Set((options.value ?? []).map((item) => item.id))
})

const isUpdateMode = computed(() => props.mode === 'update')
const disableConfirm = computed(() => {
  if (isUpdateMode.value) return false
  return selectedIds.value.length === 0
})

const options = computed<Artist[] | Tag[]>(() => {
  return props.entity === 'artist' ? userStore.artists : userStore.tags
})

const placeholder = computed(() => {
  return $t(
    props.entity === 'artist'
      ? 'page.mediums.selection.dialog.placeholder.artist'
      : 'page.mediums.selection.dialog.placeholder.tag',
  )
})

const description = computed(() => {
  const key = `page.mediums.selection.dialog.description.${props.entity}.${props.mode}`
  const translation = $t(key, { count: props.targetCount })
  return translation === key ? '' : translation
})

const confirmText = computed(() => {
  return $t(`page.mediums.selection.dialog.confirm.${props.mode}`)
})

const cancelText = computed(() => $t('page.mediums.selection.dialog.cancel'))

const sortedOptions = computed(() => {
  const list = options.value ?? []
  return [...list].sort((a, b) => {
    const nameA = (a.name ?? '').toLowerCase()
    const nameB = (b.name ?? '').toLowerCase()
    return nameA.localeCompare(nameB)
  })
})

const ensureExistingByName = (name: string) => {
  const normalized = name.trim().toLowerCase()
  return (options.value ?? []).find((item) => {
    return (item.name ?? '').trim().toLowerCase() === normalized
  })
}

const replacePlaceholder = (placeholder: string, id: string) => {
  selectedIds.value = selectedIds.value.map((value) => {
    return value === placeholder ? id : value
  })
}

const removePlaceholder = (placeholder: string) => {
  selectedIds.value = selectedIds.value.filter((value) => value !== placeholder)
}

const creatingNames = new Set<string>()

const ensureOption = async (placeholder: string) => {
  const trimmed = placeholder.trim()
  if (!trimmed) {
    removePlaceholder(placeholder)
    return
  }

  const existing = ensureExistingByName(trimmed)
  if (existing) {
    replacePlaceholder(placeholder, existing.id)
    return
  }

  const normalized = trimmed.toLowerCase()
  if (creatingNames.has(normalized)) return
  creatingNames.add(normalized)

  try {
    if (props.entity === 'artist') {
      const created = await artistApi.create({ name: trimmed })
      if (!userStore.artists.some((item) => item.id === created.id)) {
        userStore.artists.push(created)
      }
      replacePlaceholder(placeholder, created.id)
      ElMessage.success(
        $t('page.mediums.selection.dialog.createSuccess.artist', {
          name: created.name ?? trimmed,
        }) as string,
      )
    } else {
      const created = await tagApi.create({ name: trimmed })
      if (!userStore.tags.some((item) => item.id === created.id)) {
        userStore.tags.push(created)
      }
      replacePlaceholder(placeholder, created.id)
      ElMessage.success(
        $t('page.mediums.selection.dialog.createSuccess.tag', {
          name: created.name ?? trimmed,
        }) as string,
      )
    }
  } catch (error) {
    console.error('创建关联项失败', error)
    removePlaceholder(placeholder)
  } finally {
    creatingNames.delete(normalized)
  }
}

const handleCustomEntries = async (placeholders: string[]) => {
  if (placeholders.length === 0) return
  isHandlingSelection.value = true
  try {
    for (const placeholder of placeholders) {
      await ensureOption(placeholder)
    }
  } finally {
    isHandlingSelection.value = false
  }
}

watch(
  selectedIds,
  (newValue) => {
    if (isHandlingSelection.value) return
    const placeholders = newValue.filter((value) => {
      return !optionIdSet.value.has(value)
    })
    if (placeholders.length === 0) return
    void handleCustomEntries(placeholders)
  },
  { deep: false },
)

onMounted(async () => {
  loadingOptions.value = true
  try {
    await (props.entity === 'artist'
      ? userStore.reloadArtists()
      : userStore.reloadTags())
  } catch (error) {
    console.error('加载关联选项失败', error)
  } finally {
    loadingOptions.value = false
  }
})

const handleCancel = () => {
  close()
}

const handleConfirm = async () => {
  if (disableConfirm.value || submitting.value) return
  submitting.value = true
  try {
    // resolve 返回一个新数组，避免外部直接引用内部状态
    resolve([...selectedIds.value])
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="space-y-4">
    <div v-if="description" class="bg-muted/60 rounded-lg p-3 text-sm">
      {{ description }}
    </div>
    <div class="space-y-2">
      <el-select
        v-model="selectedIds"
        class="w-full"
        :placeholder="placeholder"
        multiple
        filterable
        allow-create
        default-first-option
        :loading="loadingOptions"
        :disabled="loadingOptions"
        collapse-tags
        collapse-tags-tooltip
        clearable
      >
        <el-option
          v-for="option in sortedOptions"
          :key="option.id"
          :label="option.name"
          :value="option.id"
        />
      </el-select>
      <el-text size="small" type="info">
        {{ $t(`page.mediums.selection.dialog.hint.${props.entity}`) }}
      </el-text>
    </div>
    <div class="flex justify-end gap-2">
      <el-button :disabled="submitting" @click="handleCancel">
        {{ cancelText }}
      </el-button>
      <el-button
        type="primary"
        :loading="submitting"
        :disabled="disableConfirm"
        @click="handleConfirm"
      >
        {{ confirmText }}
      </el-button>
    </div>
  </div>
</template>

<style scoped></style>

<script setup lang="ts">
import { artistApi } from '@/api/http/artist'
import type { Artist } from '@/api/http/artist/typing'
import { tagApi } from '@/api/http/tag'
import type { Tag } from '@/api/http/tag/typing'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores'

type RelationEntity = 'artist' | 'tag'
type RelationMode = 'add' | 'remove' | 'update'

defineOptions({
  name: 'MediumBatchRelationModal',
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
const emit = defineEmits<{
  close: [value: string[] | undefined]
}>()

const userStore = useUserStore()

userStore.reloadArtists()
userStore.reloadTags()

const toast = useToast()

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
  return $t(key)
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
      toast.add({
        title: $t('page.mediums.selection.dialog.createSuccess.artist', {
          name: created.name ?? trimmed,
        }),
        color: 'success',
      })
    } else {
      const created = await tagApi.create({ name: trimmed })
      if (!userStore.tags.some((item) => item.id === created.id)) {
        userStore.tags.push(created)
      }
      replacePlaceholder(placeholder, created.id)
      toast.add({
        title: $t('page.mediums.selection.dialog.createSuccess.tag', {
          name: created.name ?? trimmed,
        }),
        color: 'success',
      })
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
  emit('close', undefined)
}

const handleConfirm = async () => {
  if (disableConfirm.value || submitting.value) return
  submitting.value = true
  try {
    // 返回选中的ids
    emit('close', [...selectedIds.value])
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <UModal
    :title="
      $t(`page.mediums.selection.dialog.title.${props.entity}.${props.mode}`)
    "
    :dismissible="false"
  >
    <template #body>
      <div class="space-y-4">
        <div v-if="description" class="bg-muted/60 rounded-lg p-3 text-sm">
          {{ description }}
        </div>
        <div class="space-y-2">
          <USelectMenu
            v-model="selectedIds"
            class="w-full"
            :placeholder="placeholder"
            multiple
            searchable
            creatable
            :loading="loadingOptions"
            :disabled="loadingOptions"
            :options="
              sortedOptions.map((option) => ({
                label: option.name || '',
                value: option.id,
              }))
            "
            value-key="value"
          />
          <p class="text-muted-foreground text-xs">
            {{ $t(`page.mediums.selection.dialog.hint.${props.entity}`) }}
          </p>
        </div>
      </div>
    </template>
    <template #footer>
      <UButton variant="ghost" :disabled="submitting" @click="handleCancel">
        {{ cancelText }}
      </UButton>
      <UButton
        :loading="submitting"
        :disabled="disableConfirm"
        @click="handleConfirm"
      >
        {{ confirmText }}
      </UButton>
    </template>
  </UModal>
</template>

<style scoped></style>

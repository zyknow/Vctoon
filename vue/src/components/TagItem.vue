<script setup lang="ts">
import { Artist } from '@/api/http/artist'
import type { Tag } from '@/api/http/tag'

interface Props {
  entity: Tag | Artist
  isMultiSelectMode: boolean
  isSelected: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  edit: [tagId: string]
  delete: [tagId: string]
  toggleSelection: [tagId: string]
}>()

const handleClick = () => {
  if (props.isMultiSelectMode) {
    emit('toggleSelection', props.entity.id)
  }
}

const handleEdit = () => {
  emit('edit', props.entity.id)
}

const handleDelete = () => {
  emit('delete', props.entity.id)
}

const handleToggleSelection = () => {
  emit('toggleSelection', props.entity.id)
}
</script>

<template>
  <UBadge
    class="group transition-all duration-200"
    :class="[isMultiSelectMode ? 'hover:border-primary cursor-pointer' : '']"
    @click="handleClick"
  >
    <!-- 多选模式下的复选框 -->
    <UCheckbox
      v-if="isMultiSelectMode"
      color="success"
      :model-value="isSelected"
      @change="handleToggleSelection"
      @click.stop
    />

    <!-- 标签内容 -->
    <div class="p-1">
      <span>{{ entity.name }}</span>
      <span v-if="entity.resourceCount"> ({{ entity.resourceCount }}) </span>
    </div>

    <!-- 操作按钮 -->
    <div
      v-if="!isMultiSelectMode"
      class="flex items-center gap-1 opacity-0 transition-opacity group-hover:opacity-100"
    >
      <button
        type="button"
        class="text-neutral hover:bg-neutral hover:text-neutral flex h-4 w-4 items-center justify-center rounded"
        :title="$t('page.tag.actions.edit')"
        @click.stop="handleEdit"
      >
        <UIcon name="i-lucide-pencil" class="h-3 w-3" />
      </button>
      <button
        type="button"
        class="flex h-4 w-4 items-center justify-center rounded text-red-500 hover:bg-red-50 hover:text-red-600"
        :title="$t('page.tag.actions.delete')"
        @click.stop="handleDelete"
      >
        <UIcon name="i-lucide-trash-2" class="h-3 w-3" />
      </button>
    </div>
  </UBadge>
</template>

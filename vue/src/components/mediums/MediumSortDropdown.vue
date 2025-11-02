<script setup lang="ts">
import { $t } from '@/locales/i18n'

import type {
  MediumSortDropdownEmits,
  MediumSortDropdownProps,
  SortField,
  SortOption,
} from './types'

defineOptions({
  name: 'MediumSortDropdown',
  inheritAttrs: false,
})

// Props定义
const props = withDefaults(defineProps<MediumSortDropdownProps>(), {
  size: 'default',
  disabled: false,
})

// Emits定义
const emit = defineEmits<MediumSortDropdownEmits>()

// 默认排序字段列表
const defaultSortFieldList: SortField[] = [
  {
    label: $t('page.mediums.sort.creationTime'),
    value: 'creationTime',
    listSort: 100,
  },
  {
    label: $t('page.mediums.sort.lastModificationTime'),
    value: 'lastModificationTime',
    listSort: 101,
  },
]

// 解析排序字符串为 SortOption 对象
const parseSortString = (sortString?: string): SortOption => {
  if (!sortString) {
    return { field: 'creationTime', order: 'desc' }
  }

  const parts = sortString.trim().split(' ')
  if (parts.length !== 2) {
    return { field: 'creationTime', order: 'desc' }
  }

  const [field, order] = parts
  if (order !== 'asc' && order !== 'desc') {
    return { field: 'creationTime', order: 'desc' }
  }

  return { field: field!, order: order as 'asc' | 'desc' }
}

// 将 SortOption 对象转换为排序字符串
const formatSortString = (sortOption: SortOption): string => {
  return `${sortOption.field} ${sortOption.order}`
}

// 计算完整的排序字段列表
const sortFieldList = computed(() =>
  props.additionalSortFieldList
    ? [...defaultSortFieldList, ...props.additionalSortFieldList].sort(
        (a, b) => (a.listSort ?? 1000) - (b.listSort ?? 1000),
      )
    : defaultSortFieldList,
)

// 当前排序值（内部 SortOption 对象）
const currentSortOption = computed(() => {
  const value = parseSortString(props.modelValue)
  return value
})

// 当前排序值（对外接口）
const currentSort = computed<string>({
  get: () => props.modelValue || 'creationTime desc',
  set: (value: string) => {
    emit('update:modelValue', value)
    emit('change', value)
  },
})

// 处理排序选项点击
const handleSortClick = (field: string) => {
  const current = currentSortOption.value

  if (current.field !== field) {
    // 如果是不同字段，设置为升序
    currentSort.value = formatSortString({ field, order: 'asc' })
  } else if (current.order === 'asc') {
    // 如果当前是升序，切换为降序
    currentSort.value = formatSortString({ field, order: 'desc' })
  } else {
    // 如果当前是降序，切换为升序
    currentSort.value = formatSortString({ field, order: 'asc' })
  }
}

// 获取字段的排序状态图标
const getFieldIcon = (fieldValue: string) => {
  if (currentSortOption.value.field !== fieldValue) {
    return null // 不是当前排序字段时不显示图标
  }
  return currentSortOption.value.order === 'asc'
    ? 'i-lucide-arrow-up-narrow-wide'
    : 'i-lucide-arrow-down-narrow-wide'
}

// 下拉按钮显示内容
const dropdownButtonContent = computed(() => {
  const field = sortFieldList.value.find(
    (f) => f.value === currentSortOption.value.field,
  )

  return field?.label || currentSortOption.value.field
})

// 下拉按钮图标
const dropdownButtonIcon = computed(() => {
  return currentSortOption.value.order === 'asc'
    ? 'i-lucide-arrow-up-narrow-wide'
    : 'i-lucide-arrow-down-narrow-wide'
})

// Dropdown items
const dropdownItems = computed(() => {
  return sortFieldList.value.map((field) => ({
    label: field.label,
    icon: getFieldIcon(field.value) || undefined,
    onSelect: () => handleSortClick(field.value),
  }))
})
</script>

<template>
  <div class="medium-sort-dropdown">
    <!-- 排序下拉菜单 -->
    <UDropdownMenu :items="dropdownItems" :disabled="disabled">
      <div
        class="text-muted-foreground hover:bg-muted inline-flex cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-sm"
        :class="{ 'cursor-not-allowed opacity-50': disabled }"
      >
        <UIcon :name="dropdownButtonIcon" class="text-base" />
        <span>{{ dropdownButtonContent }}</span>
      </div>
    </UDropdownMenu>

    <!-- 传递额外内容的插槽 -->
    <slot></slot>
  </div>
</template>

<style scoped></style>

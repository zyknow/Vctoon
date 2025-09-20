<script setup lang="ts">
import type { ComponentSize } from 'element-plus'

import type {
  MediumSortDropdownEmits,
  MediumSortDropdownProps,
  SortField,
  SortOption,
} from './types'

import { computed } from 'vue'

import {
  MdiChevronDown,
  MdiSortAscending,
  MdiSortDescending,
} from '@vben/icons'

import { $t } from '#/locales'

// 组件选项
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
  { label: $t('page.mediums.sort.title'), value: 'title' },
  { label: $t('page.mediums.sort.creationTime'), value: 'creationTime' },
  {
    label: $t('page.mediums.sort.lastModificationTime'),
    value: 'lastModificationTime',
  },
  // { label: $t('page.mediums.sort.size'), value: 'size' },
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
const sortFieldList = computed<SortField[]>(() =>
  props.additionalSortFieldList
    ? [...defaultSortFieldList, ...props.additionalSortFieldList]
    : defaultSortFieldList,
)

// 当前排序值（内部 SortOption 对象）
const currentSortOption = computed<SortOption>(() => {
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

// Element Plus组件尺寸
const elementSize = computed<ComponentSize>(() => {
  const sizeMap: Record<string, ComponentSize> = {
    small: 'small',
    default: 'default',
    large: 'large',
  }
  return sizeMap[props.size] || 'default'
})

// 获取字段的排序状态图标
const getFieldIcon = (fieldValue: string) => {
  if (currentSortOption.value.field !== fieldValue) {
    return null // 不是当前排序字段时不显示图标
  }
  return currentSortOption.value.order === 'asc'
    ? MdiSortAscending
    : MdiSortDescending
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
    ? MdiSortAscending
    : MdiSortDescending
})
</script>

<template>
  <div class="medium-sort-dropdown flex items-center gap-2">
    <!-- 排序下拉菜单 -->
    <el-dropdown
      :size="elementSize"
      :disabled="disabled"
      trigger="click"
      @command="handleSortClick"
    >
      <div>
        <div class="flex flex-row items-center gap-1">
          <div>{{ dropdownButtonContent }}</div>
          <component :is="dropdownButtonIcon" />
          <MdiChevronDown />
        </div>
      </div>
      <template #dropdown>
        <el-dropdown-menu>
          <el-dropdown-item
            v-for="field in sortFieldList"
            :key="field.value"
            :command="field.value"
            class="flex items-center justify-between"
          >
            <span>{{ field.label }}</span>
            <component
              v-if="getFieldIcon(field.value)"
              :is="getFieldIcon(field.value)"
              class="text-primary ml-2 text-sm"
            />
          </el-dropdown-item>
        </el-dropdown-menu>
      </template>
    </el-dropdown>

    <!-- 传递额外内容的插槽 -->
    <slot></slot>
  </div>
</template>

<style scoped>
.medium-sort-dropdown {
  /* 确保组件样式与项目主题一致 */
}
</style>

<script setup lang="ts">
import { computed } from 'vue'

import { MdiClose, MdiSelectAll } from '@vben/icons'

import { useInjectedMediumProvider } from '#/hooks/useMediumProvider'
import { $t } from '#/locales'

const { selectedMediumIds, items } = useInjectedMediumProvider()

// 计算选中数量
const selectedCount = computed(() => selectedMediumIds.value.length)

// 计算是否全选
const isAllSelected = computed(() => {
  return items.value.length > 0 && selectedCount.value === items.value.length
})

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
      <div class="flex items-center gap-2"></div>
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

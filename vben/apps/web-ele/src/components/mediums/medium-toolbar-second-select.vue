<script setup lang="ts">
import { computed } from 'vue'

import { MdiClose, MdiSelectAll } from '@vben/icons'

import { useInjectedMediumProvider } from '#/hooks/useMediumProvider'

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
          已选择 {{ selectedCount }} 项
        </span>

        <!-- 全选按钮 -->
        <el-button
          type="primary"
          size="small"
          :icon="MdiSelectAll"
          @click="toggleSelectAll"
        >
          {{ isAllSelected ? '取消全选' : '全选' }}
        </el-button>
      </div>
    </slot>

    <slot name="filter-center">
      <div class="flex items-center gap-2">
        <!-- 这里可以放批量操作按钮 -->
        <el-button size="small" type="default"> 批量下载 </el-button>
        <el-button size="small" type="default"> 批量删除 </el-button>
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
          取消
        </el-button>
      </div>
    </slot>
  </div>
</template>

<style lang="scss" scoped></style>

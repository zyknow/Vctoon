<script setup lang="ts">
import { useDebounceFn } from '@vueuse/core'

import { useIsMobile } from '@/hooks/useIsMobile'
import { useMediumStore } from '@/stores/medium'

defineProps<{
  title?: string
}>()

const { isMobile } = useIsMobile()

const mediumStore = useMediumStore()

// 本地缩放值，防抖同步到 Store
const localItemZoom = ref<number>(mediumStore.itemZoom)

// 防抖更新函数：200ms
const applyItemZoom = useDebounceFn((val: number) => {
  try {
    mediumStore.itemZoom = val
  } catch (error) {
    // 关键逻辑错误处理：防止异常打断交互
    console.error('更新缩放失败:', error)
  }
}, 200)

// 当滑块本地值变化时，防抖写入 Store
watch(localItemZoom, (val) => {
  applyItemZoom(val)
})

// 当 Store 值外部变更时，回填到本地，确保同步
watch(
  () => mediumStore.itemZoom,
  (val) => {
    localItemZoom.value = val
  },
  { immediate: true },
)
</script>

<template>
  <div class="flex w-full flex-row items-center justify-between">
    <slot name="left">
      <div v-if="!isMobile" class="text-xl font-bold">{{ title }}</div>
    </slot>
    <slot name="center">
      <div></div>
    </slot>
    <div v-if="!isMobile" name="right">
      <div class="flex w-40 flex-row items-center gap-2">
        <USlider
          v-model="localItemZoom"
          :min="0.8"
          :max="1.6"
          :step="0.05"
          size="sm"
        />
        <MediumLayoutDropdownSelect v-model="mediumStore.itemDisplayMode" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Comic, Video } from '@vben/api'

import { computed, ref } from 'vue'

import { MediumType } from '@vben/api'
import { ChevronLeft, ChevronRight } from '@vben/icons'

import MediumGuidItem from './medium-guid-item.vue'

interface Props {
  title: string
  items: (Comic | Video)[]
  mediumType: MediumType
  loading?: boolean
}

const props = defineProps<Props>()

const scrollContainer = ref<HTMLElement>()

// 滚动控制
const canScrollLeft = ref(false)
const canScrollRight = ref(true)

// 检查滚动状态
const checkScrollState = () => {
  if (!scrollContainer.value) return

  const { scrollLeft, scrollWidth, clientWidth } = scrollContainer.value
  canScrollLeft.value = scrollLeft > 0
  canScrollRight.value = scrollLeft < scrollWidth - clientWidth - 1
}

// 向左滚动
const scrollLeft = () => {
  if (!scrollContainer.value) return
  const scrollAmount = scrollContainer.value.clientWidth * 0.8
  scrollContainer.value.scrollBy({
    left: -scrollAmount,
    behavior: 'smooth',
  })
}

// 向右滚动
const scrollRight = () => {
  if (!scrollContainer.value) return
  const scrollAmount = scrollContainer.value.clientWidth * 0.8
  scrollContainer.value.scrollBy({
    left: scrollAmount,
    behavior: 'smooth',
  })
}

// 处理滚动事件
const handleScroll = () => {
  checkScrollState()
}

// 计算是否有内容
const hasItems = computed(() => props.items && props.items.length > 0)
</script>

<template>
  <div class="medium-recommendation-section">
    <!-- 标题行 -->
    <div class="mb-4 flex items-center justify-between">
      <h2 class="text-foreground text-xl font-bold">{{ title }}</h2>

      <!-- 滚动控制按钮 -->
      <div v-if="hasItems" class="flex items-center gap-2">
        <button
          :disabled="!canScrollLeft"
          :class="{
            'text-muted-foreground cursor-not-allowed': !canScrollLeft,
            'text-foreground hover:text-primary cursor-pointer': canScrollLeft,
          }"
          class="hover:bg-muted rounded-full p-2 transition-colors"
          @click="scrollLeft"
        >
          <ChevronLeft class="h-5 w-5" />
        </button>
        <button
          :disabled="!canScrollRight"
          :class="{
            'text-muted-foreground cursor-not-allowed': !canScrollRight,
            'text-foreground hover:text-primary cursor-pointer': canScrollRight,
          }"
          class="hover:bg-muted rounded-full p-2 transition-colors"
          @click="scrollRight"
        >
          <ChevronRight class="h-5 w-5" />
        </button>
      </div>
    </div>

    <!-- 内容区域 -->
    <div class="relative">
      <!-- 加载状态 -->
      <div v-if="loading" class="flex gap-6 overflow-hidden">
        <div
          v-for="i in 6"
          :key="`skeleton-${i}`"
          class="bg-muted animate-pulse rounded-xl"
          style="flex-shrink: 0; width: 10rem; height: 14rem"
        ></div>
      </div>

      <!-- 空状态 -->
      <div
        v-else-if="!hasItems"
        class="text-muted-foreground flex h-32 items-center justify-center text-sm"
      >
        暂无内容
      </div>

      <!-- 横向滚动容器 -->
      <div
        v-else
        ref="scrollContainer"
        class="scrollbar-hide flex gap-6 overflow-x-auto pb-4"
        @scroll="handleScroll"
        @scrollend="checkScrollState"
      >
        <div v-for="item in items" :key="item.id" class="flex-shrink-0">
          <MediumGuidItem :model-value="item" :medium-type="mediumType" />
        </div>
      </div>

      <!-- 左侧渐变遮罩 -->
      <div
        v-if="canScrollLeft"
        class="from-background pointer-events-none absolute left-0 top-0 h-full w-8 bg-gradient-to-r to-transparent"
      ></div>

      <!-- 右侧渐变遮罩 -->
      <div
        v-if="canScrollRight"
        class="from-background pointer-events-none absolute right-0 top-0 h-full w-8 bg-gradient-to-l to-transparent"
      ></div>
    </div>
  </div>
</template>

<style scoped>
/* 添加骨架屏动画 */
@keyframes pulse {
  0%,
  100% {
    opacity: 1;
  }

  50% {
    opacity: 0.5;
  }
}

/* 隐藏滚动条 */
.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}

.scrollbar-hide::-webkit-scrollbar {
  display: none;
}

/* 确保滚动容器平滑滚动 */
.medium-recommendation-section {
  scroll-behavior: smooth;
}

.animate-pulse {
  animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
}
</style>

<script setup lang="ts">
import type { RecommendMediumProvider } from '@/hooks/useRecommendProvider'

interface Props {
  data: RecommendMediumProvider
}

const props = defineProps<Props>()

const scrollContainer = ref<HTMLElement>()
// 编辑逻辑由子项统一处理（medium.ts）

// 滚动控制
const canScrollLeft = ref(false)
const canScrollRight = ref(true)

// 保存并恢复滚动位置的工具函数
const preserveScrollPosition = async (callback: () => Promise<void>) => {
  if (!scrollContainer.value) return

  const currentScrollLeft = scrollContainer.value.scrollLeft
  await callback()

  // 在下一个渲染周期恢复滚动位置
  await nextTick()
  if (scrollContainer.value) {
    scrollContainer.value.scrollLeft = currentScrollLeft
  }
}

// 防抖标志，避免重复触发加载
let isLoadingMore = false

// 防抖加载函数
const debouncedLoadNext = async () => {
  if (isLoadingMore || props.data.loading.value) return

  isLoadingMore = true
  try {
    await preserveScrollPosition(() => props.data.loadNext())
  } finally {
    isLoadingMore = false
  }
}

// 检查滚动状态
const checkScrollState = () => {
  if (!scrollContainer.value) return

  const { scrollLeft, scrollWidth, clientWidth } = scrollContainer.value
  canScrollLeft.value = scrollLeft > 0
  // 如果还有更多数据，始终可以向右滚动
  canScrollRight.value =
    scrollLeft < scrollWidth - clientWidth - 1 || props.data.hasMore.value
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
const scrollRight = async () => {
  if (!scrollContainer.value) return

  const { scrollLeft, scrollWidth, clientWidth } = scrollContainer.value
  const scrollAmount = scrollContainer.value.clientWidth * 0.8

  // 如果已经滚动到底部但还有更多数据，触发加载更多
  if (
    scrollLeft >= scrollWidth - clientWidth - 1 &&
    props.data.hasMore.value &&
    !isLoadingMore
  ) {
    await debouncedLoadNext()
  } else {
    scrollContainer.value.scrollBy({
      left: scrollAmount,
      behavior: 'smooth',
    })
  }
}

// 处理滚动事件
const handleScroll = () => {
  checkScrollState()
  checkNeedLoadMore()
}

// 检查是否需要加载更多
const checkNeedLoadMore = async () => {
  if (
    !scrollContainer.value ||
    !props.data.hasMore.value ||
    props.data.loading.value
  ) {
    return
  }

  const { scrollLeft, scrollWidth, clientWidth } = scrollContainer.value
  // 当滚动到距离右边界100px以内时开始加载更多
  const threshold = 100
  if (scrollLeft + clientWidth >= scrollWidth - threshold) {
    await debouncedLoadNext()
  }
}

// 计算是否有内容
const hasItems = computed(
  () => props.data.items && props.data.items.value.length > 0,
)
</script>

<template>
  <div class="medium-recommendation-section">
    <!-- 标题行 -->
    <div class="mb-4 flex items-center justify-between">
      <h2 class="text-foreground text-xl font-bold">{{ props.data.title }}</h2>

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
          <UIcon name="i-lucide-chevron-left" class="h-5 w-5" />
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
          <UIcon name="i-lucide-chevron-right" class="h-5 w-5" />
        </button>
      </div>
    </div>

    <!-- 内容区域 -->
    <div class="relative">
      <!-- 初始加载状态 -->
      <div
        v-if="props.data.loading.value && !hasItems"
        class="flex gap-6 overflow-hidden"
      >
        <div
          v-for="i in 6"
          :key="`skeleton-${i}`"
          class="bg-muted animate-pulse rounded-xl"
          style="flex-shrink: 0; width: 10rem; height: 14rem"
        ></div>
      </div>

      <!-- 空状态 -->
      <div
        v-else-if="!hasItems && !props.data.loading.value"
        class="text-muted-foreground flex h-32 items-center justify-center text-sm"
      >
        暂无内容
      </div>

      <!-- 横向滚动容器 -->
      <div
        v-else
        ref="scrollContainer"
        class="scrollbar-hide overflow-x-auto pb-4"
        @scroll="handleScroll"
        @scrollend="checkScrollState"
      >
        <TransitionGroup
          name="fade-slide"
          tag="div"
          class="flex gap-6 pt-1"
          appear
        >
          <div
            v-for="item in props.data.items.value"
            :key="item.id"
            class="flex-shrink-0"
          >
            <MediumGridItem
              :model-value="item"
              :medium-type="item.mediumType"
            />
          </div>
        </TransitionGroup>
      </div>

      <!-- 左侧渐变遮罩 -->
      <div
        v-if="canScrollLeft"
        class="from-background pointer-events-none absolute top-0 left-0 h-full w-8 bg-gradient-to-r to-transparent"
      ></div>

      <!-- 右侧渐变遮罩 -->
      <div
        v-if="canScrollRight && !props.data.loading.value"
        class="from-background pointer-events-none absolute top-0 right-0 h-full w-8 bg-gradient-to-l to-transparent"
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

/* 过渡动画 */
.fade-slide-enter-active {
  transition: all 0.5s cubic-bezier(0.4, 0, 0.2, 1);
}

.fade-slide-enter-from {
  opacity: 0;
  transform: translateX(30px) scale(0.9);
}

.fade-slide-enter-to {
  opacity: 1;
  transform: translateX(0) scale(1);
}

/* 确保动画的平滑性 */
.fade-slide-move {
  transition: transform 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

/* 优化硬件加速 */
.fade-slide-enter-active,
.fade-slide-move {
  will-change: transform, opacity;
}

/* 改善视觉效果的额外样式 */
.medium-recommendation-section .flex-shrink-0 {
  transition: transform 0.2s ease;
}

.medium-recommendation-section .flex-shrink-0:hover {
  transform: translateY(-2px);
}
</style>

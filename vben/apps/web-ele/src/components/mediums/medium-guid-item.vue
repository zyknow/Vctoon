<script setup lang="ts">
import type { Comic, Video } from '@vben/api'

import { computed } from 'vue'

import { mediumResourceApi, MediumType } from '@vben/api'
import { useAppConfig } from '@vben/hooks'
import {
  CiEditPencilLine01,
  CiMoreVertical,
  MdiCheck,
  MdiCheckCircle,
  MdiPlayCircle,
} from '@vben/icons'
import { formatDate } from '@vben/utils'

import { useInjectedMediumItemProvider } from '#/hooks/useMediumProvider'
import { useMediumStore } from '#/store'

type Medium = Comic | Video

const props = defineProps<{
  mediumType?: MediumType
  modelValue?: Medium
}>()

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
const mediumStore = useMediumStore()
const { selectedMediumIds, items } = useInjectedMediumItemProvider()

// 通过 CSS 变量传递缩放比例
const zoomStyle = computed(() => ({
  '--zoom': String(mediumStore.itemZoom || 1),
}))

// 检查当前项目是否被选中
const isSelected = computed(() => {
  return selectedMediumIds.value.includes(props.modelValue?.id || '')
})

// 检查是否处于选择模式
const isInSelectionMode = computed(() => {
  return selectedMediumIds.value.length > 0
})

// 选中状态变化处理（用于圆圈点击）
const toggleSelectionFromIcon = (event: MouseEvent) => {
  const currentId = props.modelValue?.id
  if (!currentId) return

  // 始终阻止事件冒泡，避免触发卡片的点击事件
  event.stopPropagation()

  if (event.shiftKey) {
    // Shift + 点击：范围选择
    handleRangeSelection(currentId)
  } else {
    // 普通点击：切换单个项目选中状态
    handleSingleSelection(currentId)
  }
}

// 选中状态变化处理（用于卡片点击）
const toggleSelection = (event: MouseEvent) => {
  const currentId = props.modelValue?.id
  if (!currentId) return

  if (event.shiftKey) {
    // Shift + 点击：范围选择
    handleRangeSelection(currentId)
  } else {
    // 普通点击：切换单个项目选中状态
    handleSingleSelection(currentId)
  }
}

// 单个项目选中切换
const handleSingleSelection = (itemId: string) => {
  const currentIndex = selectedMediumIds.value.indexOf(itemId)
  if (currentIndex === -1) {
    // 选中
    selectedMediumIds.value.push(itemId)
  } else {
    // 取消选中
    selectedMediumIds.value.splice(currentIndex, 1)
  }
}

// 范围选择
const handleRangeSelection = (currentId: string) => {
  const allIds = items.value.map((item) => item.id)
  const currentIndex = allIds.indexOf(currentId)

  if (selectedMediumIds.value.length === 0) {
    // 如果没有已选中项，则直接选中当前项
    selectedMediumIds.value.push(currentId)
    return
  }

  // 找到最后一个选中项的索引
  const selectedIndices = selectedMediumIds.value
    .map((id) => allIds.indexOf(id))
    .filter((index) => index !== -1)
    .sort((a, b) => a - b)

  const lastSelectedIndex = selectedIndices[selectedIndices.length - 1]

  if (lastSelectedIndex !== undefined) {
    // 确定范围选择的起始和结束索引
    const startIndex = Math.min(lastSelectedIndex, currentIndex)
    const endIndex = Math.max(lastSelectedIndex, currentIndex)

    // 选中范围内的所有项目
    const rangeIds = allIds.slice(startIndex, endIndex + 1)

    // 合并选中的项目（去重）
    const newSelectedIds = [
      ...new Set([...rangeIds, ...selectedMediumIds.value]),
    ]
    selectedMediumIds.value = newSelectedIds
  }
}

// 标题、年份、相对时间（简化展示：优先使用 creationTime 与 lastModificationTime）
const title = computed(() => props.modelValue?.title ?? '')
const year = computed(() => {
  const time = props.modelValue?.creationTime
  if (!time) return ''
  try {
    return formatDate(time, 'YYYY')
  } catch {
    return ''
  }
})
const timeAgo = computed(() => {
  const time =
    props.modelValue?.lastModificationTime || props.modelValue?.creationTime
  if (!time) return ''
  // 暂未引入 relativeTime，先用日期占位，后续可替换为 fromNow()
  try {
    return formatDate(time, 'YYYY-MM-DD')
  } catch {
    return ''
  }
})

// 阅读进度相关
const readingProgress = computed(() => {
  const progress = props.modelValue?.readingProgress
  if (typeof progress !== 'number') return 0
  // 确保进度值在 0-1 范围内，然后转换为百分比
  const normalizedProgress = Math.max(0, Math.min(1, progress))
  return Math.round(normalizedProgress * 100)
})

const showReadingProgress = computed(() => {
  const progress = props.modelValue?.readingProgress
  return typeof progress === 'number' && progress > 0 && progress < 1
})

// 是否已完成阅读
const isCompleted = computed(() => {
  const progress = props.modelValue?.readingProgress
  return typeof progress === 'number' && progress >= 1
})

const cover = computed(() => {
  let mediumUrl
  switch (props.mediumType) {
    case MediumType.Comic: {
      mediumUrl = mediumResourceApi.url.getCover.format({
        cover: props.modelValue?.cover,
      })
      break
    }
    case MediumType.Video: {
      mediumUrl = mediumResourceApi.url.getCover.format({
        cover: props.modelValue?.cover,
      })
      break
    }
    default: {
      break
    }
  }

  return `${apiURL}${mediumUrl}`
})
</script>

<template>
  <div
    :style="zoomStyle"
    class="text-foreground medium-card group relative select-none rounded-xl transition-colors"
    :class="{
      'cursor-pointer': isInSelectionMode,
    }"
    @click="isInSelectionMode ? toggleSelection($event) : null"
  >
    <!-- Cover -->
    <div
      class="relative overflow-hidden rounded-lg border transition-colors"
      :class="{
        'group-hover:border-primary border-transparent': !isSelected,
        'border-primary border-2': isSelected,
      }"
    >
      <img
        loading="lazy"
        v-if="cover"
        :src="cover"
        alt="cover"
        class="cover w-full rounded-lg object-cover"
      />
      <div v-else class="bg-muted cover w-full rounded-lg"></div>

      <!-- Hover overlay -->
      <div
        v-if="!isInSelectionMode"
        class="pointer-events-none absolute inset-0 hidden items-center justify-center gap-3 rounded-lg bg-black/40 group-hover:flex"
      >
        <!-- 非选择模式：显示所有操作图标 -->
        <!-- left-bottom: edit icon -->
        <div class="pointer-events-auto absolute bottom-2 left-2 text-white/90">
          <CiEditPencilLine01
            class="hover:text-primary cursor-pointer text-2xl"
          />
        </div>
        <!-- right-bottom: more icon -->
        <div
          class="pointer-events-auto absolute bottom-2 right-2 text-white/90"
        >
          <CiMoreVertical class="hover:text-primary cursor-pointer text-2xl" />
        </div>
        <!-- center: play circle -->
        <div class="pointer-events-auto text-white">
          <MdiPlayCircle
            class="hover:text-primary cursor-pointer text-6xl drop-shadow"
          />
        </div>
      </div>

      <!-- 选中状态圆圈 - 始终显示在左上角 -->
      <div class="absolute left-2 top-2" @click="toggleSelectionFromIcon">
        <div
          v-if="isSelected"
          class="bg-primary flex h-5 w-5 cursor-pointer items-center justify-center rounded-full text-white"
        >
          <MdiCheckCircle class="text-xl" />
        </div>
        <div
          v-else
          style="border-width: 3px"
          class="h-5 w-5 cursor-pointer rounded-full border-white/80 bg-transparent transition-opacity"
          :class="{
            'opacity-100': isInSelectionMode,
            'opacity-0 group-hover:opacity-100': !isInSelectionMode,
          }"
        ></div>
      </div>

      <!-- 完成标记 - 显示在右上角 -->
      <div
        v-if="isCompleted"
        class="bg-primary/80 absolute right-0 top-0 flex h-7 w-7 items-center justify-center rounded-bl-md text-white shadow-sm"
      >
        <MdiCheck class="text-sm" />
      </div>

      <!-- 阅读进度条 - 显示在图片底部 -->
      <div
        v-if="showReadingProgress"
        class="absolute bottom-0 left-0 right-0 h-1 overflow-hidden rounded-b-lg"
      >
        <div class="absolute inset-0 bg-black/20"></div>
        <div
          class="relative z-10 h-full transition-all duration-300 ease-out"
          :style="{
            width: `${readingProgress}%`,
            backgroundColor: `hsl(var(--primary))`,
          }"
        ></div>
      </div>
    </div>

    <!-- Meta -->
    <div class="mt-3 space-y-1">
      <div class="line-clamp-2 text-sm font-medium leading-snug">
        {{ title }}
      </div>
      <div class="text-muted-foreground text-xs">{{ year }}</div>
      <div class="text-muted-foreground text-xs">{{ timeAgo }}</div>
    </div>
  </div>
</template>

<style scoped>
.medium-card {
  /* 基础宽度：10rem（原 w-40），支持 0.8x ~ 1.6x 缩放 */
  width: calc(10rem * clamp(0.8, var(--zoom, 1), 1.6));
}

.cover {
  /* 基础高度：14rem（原 h-56），与宽度同比例缩放 */
  height: calc(14rem * clamp(0.8, var(--zoom, 1), 1.6));
}

.line-clamp-2 {
  display: -webkit-box;
  overflow: hidden;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}
</style>

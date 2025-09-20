<script setup lang="ts">
import type { Comic, Video } from '@vben/api'

import { computed } from 'vue'

import { comicApi, MediumType, videoApi } from '@vben/api'
import { useAppConfig } from '@vben/hooks'
import {
  CiEditPencilLine01,
  CiMoreVertical,
  MdiCheckCircle,
  MdiPlayCircle,
} from '@vben/icons'
import { formatDate } from '@vben/utils'

import { useInjectedMediumProvider } from '#/hooks/useMediumProvider'
import { useMediumStore } from '#/store'

type Medium = Comic | Video

const props = defineProps<{
  mediumType?: MediumType
  modelValue?: Medium
}>()

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
const mediumStore = useMediumStore()
const { selectedMediumIds, items } = useInjectedMediumProvider()

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

// 选中状态变化处理
const toggleSelection = (event: MouseEvent) => {
  const currentId = props.modelValue?.id
  if (!currentId) return

  // 阻止事件冒泡，避免触发其他点击事件
  event.stopPropagation()

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

const cover = computed(() => {
  let mediumUrl
  switch (props.mediumType) {
    case MediumType.Comic: {
      mediumUrl = comicApi.url.cover.format({ cover: props.modelValue?.cover })
      break
    }
    case MediumType.Video: {
      mediumUrl = videoApi.url.cover.format({ cover: props.modelValue?.cover })
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
    class="text-foreground medium-card group relative cursor-pointer select-none rounded-xl transition-colors"
    @click="toggleSelection"
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
        v-if="cover"
        :src="cover"
        alt="cover"
        class="cover w-full rounded-lg object-cover"
      />
      <div v-else class="bg-muted cover w-full rounded-lg"></div>

      <!-- Hover overlay -->
      <div
        class="pointer-events-none absolute inset-0 hidden items-center justify-center gap-3 rounded-lg bg-black/40 group-hover:flex"
      >
        <!-- 非选择模式：显示所有操作图标 -->
        <template v-if="!isInSelectionMode">
          <!-- left-bottom: edit icon -->
          <div
            class="pointer-events-auto absolute bottom-2 left-2 text-white/90"
          >
            <CiEditPencilLine01 class="h-5 w-5" />
          </div>
          <!-- right-bottom: more icon -->
          <div
            class="pointer-events-auto absolute bottom-2 right-2 text-white/90"
          >
            <CiMoreVertical class="h-5 w-5" />
          </div>
          <!-- center: play circle -->
          <div class="pointer-events-auto text-white">
            <MdiPlayCircle class="h-12 w-12 drop-shadow" />
          </div>
        </template>
      </div>

      <!-- 选中状态圆圈 - 始终显示在左上角 -->
      <div class="absolute left-2 top-2">
        <div
          v-if="isSelected"
          class="bg-primary flex h-6 w-6 items-center justify-center rounded-full text-white"
        >
          <MdiCheckCircle class="h-4 w-4" />
        </div>
        <div
          v-else
          class="h-6 w-6 rounded-full border-2 border-white/80 bg-transparent opacity-0 transition-opacity group-hover:opacity-100"
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

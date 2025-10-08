<script setup lang="ts">
import type { MediumGetListOutput } from '@vben/api'

import { computed } from 'vue'
import { useRouter } from 'vue-router'

import { comicApi, mediumResourceApi, MediumType, videoApi } from '@vben/api'
import { useAppConfig } from '@vben/hooks'
import {
  CiEditPencilLine01,
  CiMoreVertical,
  MdiCheck,
  MdiPlayCircle,
} from '@vben/icons'
import { formatDate } from '@vben/utils'

import { useDialogService } from '#/hooks/useDialogService'
import {
  useInjectedMediumAllItemProvider,
  useInjectedMediumItemProvider,
} from '#/hooks/useMediumProvider'
import { $t } from '#/locales'
import { useMediumStore } from '#/store'

import MediumCoverCard from './medium-cover-card.vue'
import MediumInfoDialog from './medium-info-dialog.vue'
import MediumSelectionIndicator from './medium-selection-indicator.vue'

const props = defineProps<{
  modelValue: MediumGetListOutput
}>()

const emit = defineEmits<{
  edit: [medium: MediumGetListOutput]
  updated: [medium: MediumGetListOutput]
}>()

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
const mediumStore = useMediumStore()
const { selectedMediumIds, items } = useInjectedMediumItemProvider()
const { open, confirm } = useDialogService()
const router = useRouter()

const { updateItemField } = useInjectedMediumAllItemProvider()!

// 编辑相关状态

const cardStyleVars = computed(() => ({
  '--cover-base-height': '14rem',
  '--cover-base-width': '10rem',
  '--cover-zoom': String(mediumStore.itemZoom || 1),
}))

const mediumType = computed(() => props.modelValue.mediumType)

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

const navigateToDetail = () => {
  if (isInSelectionMode.value) return
  const id = props.modelValue?.id
  if (!id) return
  const typeSegment = mediumType.value === MediumType.Video ? 'video' : 'comic'
  void router.push({
    name: 'MediumDetail',
    params: {
      mediumId: id,
      type: typeSegment,
    },
  })
}

const handleCardClick = (event: MouseEvent) => {
  if (isInSelectionMode.value) {
    toggleSelection(event)
    return
  }
  navigateToDetail()
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

// 编辑功能相关
const handleEdit = (event: MouseEvent) => {
  event.stopPropagation()
  emit('edit', props.modelValue)
}

// 打开信息弹窗
const openInfo = async (event: MouseEvent) => {
  event.stopPropagation()
  if (!props.modelValue?.id) return
  await open(MediumInfoDialog, {
    props: {
      mediumId: props.modelValue.id,
      mediumType: props.modelValue.mediumType,
    },
    title: $t('page.mediums.actions.info'),
    width: 520,
  })
}

// 标记为已观看
const markAsWatched = async (event: MouseEvent) => {
  event.stopPropagation()
  const id = props.modelValue?.id
  if (!id) return
  try {
    const updateObj = {
      mediumId: id,
      progress: 1,
      readingLastTime: new Date().toISOString(),
      mediumType: props.modelValue.mediumType,
    }

    await mediumResourceApi.updateReadingProcess([updateObj])

    // 更新本地状态
    updateItemField({
      id,
      readingProgress: 1,
      readingLastTime: new Date(),
    })

    emit('updated', props.modelValue)
  } catch {
    // 失败提示交由全局拦截器处理
  }
}

// 标记为未观看
const markAsUnwatched = async (event: MouseEvent) => {
  event.stopPropagation()
  const id = props.modelValue?.id
  if (!id) return
  try {
    const updateObj = {
      mediumId: id,
      progress: 0,
      readingLastTime: null,
      mediumType: props.modelValue.mediumType,
    }

    await mediumResourceApi.updateReadingProcess([updateObj])

    // 更新本地状态
    updateItemField({
      id,
      readingProgress: 0,
      readingLastTime: undefined,
    })

    emit('updated', props.modelValue)
  } catch {
    // 失败提示交由全局拦截器处理
  }
}

// 删除 Medium
const deleteMedium = async (event: MouseEvent) => {
  event.stopPropagation()
  const id = props.modelValue?.id
  if (!id) return
  const ok = await confirm({
    title: $t('page.mediums.actions.deleteConfirmTitle'),
    message: $t('page.mediums.actions.deleteConfirmMessage', {
      title: props.modelValue?.title || id,
    }),
    danger: true,
  })
  if (!ok) return
  try {
    props.modelValue.mediumType === MediumType.Comic
      ? await comicApi.delete(id)
      : await videoApi.delete(id)
    // 本地移除
    const idx = items.value.findIndex((m) => m.id === id)
    if (idx !== -1) items.value.splice(idx, 1)
  } catch {
    // 失败提示交由全局拦截器处理
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

  switch (mediumType.value) {
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
    :style="cardStyleVars"
    class="text-foreground medium-card group relative cursor-pointer select-none transition-colors"
    @click="handleCardClick"
  >
    <!-- Cover -->
    <MediumCoverCard
      :src="cover"
      class="relative border transition-colors"
      :class="{
        'group-hover:border-primary border-transparent': !isSelected,
        'border-primary border-4': isSelected,
      }"
    >
      <!-- Hover overlay -->
      <div
        v-if="!isInSelectionMode"
        class="absolute inset-0 hidden items-center justify-center gap-3 rounded-lg group-hover:flex"
      >
        <div
          class="pointer-events-none absolute inset-0 rounded-lg bg-black/40"
        ></div>
        <!-- 非选择模式：显示所有操作图标 -->
        <!-- left-bottom: edit icon -->
        <div class="pointer-events-auto absolute bottom-2 left-2 text-white/90">
          <CiEditPencilLine01
            class="hover:text-primary cursor-pointer text-2xl"
            @click="handleEdit"
          />
        </div>
        <!-- right-bottom: more icon -->
        <div
          class="pointer-events-auto absolute bottom-2 right-2 text-white/90"
        >
          <el-dropdown trigger="click" @click.stop>
            <span class="inline-flex items-center" @click.stop @mousedown.stop>
              <CiMoreVertical
                class="hover:text-primary cursor-pointer text-2xl"
              />
            </span>
            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item @click.stop="openInfo">
                  {{ $t('page.mediums.actions.info') }}
                </el-dropdown-item>
                <el-dropdown-item
                  v-if="!isCompleted"
                  @click.stop="markAsWatched"
                  class="min-w-[150px]"
                >
                  {{ $t('page.mediums.actions.markAsWatched') }}
                </el-dropdown-item>
                <el-dropdown-item
                  v-if="isCompleted"
                  @click.stop="markAsUnwatched"
                  class="min-w-[150px]"
                >
                  {{ $t('page.mediums.actions.markAsUnwatched') }}
                </el-dropdown-item>
                <el-dropdown-item
                  @click.stop="deleteMedium"
                  class="min-w-[150px]"
                >
                  {{ $t('page.mediums.actions.delete') }}
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
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
        <MediumSelectionIndicator
          :selected="isSelected"
          size="sm"
          :visible="isInSelectionMode"
        />
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
    </MediumCoverCard>

    <!-- Meta -->
    <div class="mt-3 space-y-1">
      <div
        :title="title"
        class="hover:text-primary line-clamp-2 cursor-pointer text-sm font-medium leading-snug"
        @click.stop="navigateToDetail"
      >
        {{ title }}
      </div>
      <div class="text-muted-foreground text-xs">{{ year }}</div>
      <div class="text-muted-foreground text-xs">{{ timeAgo }}</div>
    </div>

    <!-- 编辑对话框已移除，由父组件统一管理 -->
  </div>
</template>

<style scoped>
.medium-card {
  /* 基础宽度：10rem（原 w-40），支持 0.8x ~ 1.6x 缩放 */
  width: calc(
    var(--cover-base-width, 10rem) * clamp(0.8, var(--cover-zoom, 1), 1.6)
  );
}

.line-clamp-2 {
  display: -webkit-box;
  overflow: hidden;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
}
</style>

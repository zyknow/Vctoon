<script setup lang="ts">
import type { MediumGetListOutput } from '@vben/api'

import { computed } from 'vue'

import {
  CiEditPencilLine01,
  CiMoreVertical,
  MdiCheck,
  MdiPlayCircle,
} from '@vben/icons'

import { useMediumItem } from '#/hooks/useMediumItem'
import { $t } from '#/locales'
import { useMediumStore } from '#/store'

import MediumCoverCard from './medium-cover-card.vue'
import MediumSelectionIndicator from './medium-selection-indicator.vue'

const props = defineProps<{
  modelValue: MediumGetListOutput
}>()

const emit = defineEmits<{
  edit: [medium: MediumGetListOutput]
  updated: [medium: MediumGetListOutput]
}>()

const mediumStore = useMediumStore()

const cardStyleVars = computed(() => ({
  '--cover-base-height': '14rem',
  '--cover-base-width': '10rem',
  '--cover-zoom': String(mediumStore.itemZoom || 1),
}))

const mediumRef = computed(() => props.modelValue)

const {
  mediumAnchorId,
  isSelected,
  isInSelectionMode,
  title,
  year,
  timeAgo,
  readingProgress,
  showReadingProgress,
  isCompleted,
  cover,
  toggleSelectionFromIcon,
  navigateToDetail,
  navigateToPrimaryAction,
  handleCardClick,
  handleEdit,
  openInfo,
  markAsWatched,
  markAsUnwatched,
  deleteMedium,
} = useMediumItem(mediumRef, {
  onEdit: (medium) => emit('edit', medium),
  onUpdated: (medium) => emit('updated', medium),
})
</script>

<template>
  <div
    :id="mediumAnchorId"
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
      <!-- 保留下拉菜单实例，避免退出选择模式时大规模重新挂载导致卡顿 -->
      <div
        v-show="!isInSelectionMode"
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
            @click.stop="navigateToPrimaryAction"
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

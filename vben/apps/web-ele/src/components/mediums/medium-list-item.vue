<script setup lang="ts">
import type { MediumGetListOutput } from '@vben/api'

import { computed } from 'vue'

import { MediumType } from '@vben/api'
import {
  CiEditPencilLine01,
  CiMoreVertical,
  MdiCheck,
  MdiComic,
  MdiPlayCircle,
  MdiVideo,
} from '@vben/icons'

import { useMediumItem } from '#/hooks/useMediumItem'
import { $t } from '#/locales'

import MediumCoverCard from './medium-cover-card.vue'
import MediumSelectionIndicator from './medium-selection-indicator.vue'

const props = defineProps<{
  modelValue: MediumGetListOutput
}>()

const emit = defineEmits<{
  edit: [medium: MediumGetListOutput]
  updated: [medium: MediumGetListOutput]
}>()

const mediumRef = computed(() => props.modelValue)

const {
  mediumType,
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
    class="text-foreground medium-list-item hover:bg-muted/40 group relative cursor-pointer select-none rounded p-3 transition-colors"
    @click="handleCardClick"
  >
    <div class="flex items-start gap-4">
      <!-- 左侧选择图标（始终显示，未选中时可 hover 显示） -->
      <div
        class="flex items-center self-center pr-2"
        @click.stop="toggleSelectionFromIcon"
      >
        <MediumSelectionIndicator
          :selected="isSelected"
          size="sm"
          :visible="true"
        />
      </div>

      <!-- Cover (仅保留播放按钮，hover 显示) -->
      <div class="relative shrink-0">
        <MediumCoverCard
          :src="cover"
          base-width="7rem"
          base-height="10.5rem"
          class="relative border"
          :class="{
            'group-hover:border-primary border-transparent': !isSelected,
            'border-primary border-4': isSelected,
          }"
        >
          <!-- center: primary action（仅在 hover 时显示且可点击） -->
          <div
            class="pointer-events-none absolute inset-0 flex items-center justify-center text-white opacity-0 transition-opacity duration-200 ease-out group-hover:pointer-events-auto group-hover:opacity-100"
          >
            <MdiPlayCircle
              class="hover:text-primary cursor-pointer text-5xl drop-shadow"
              @click.stop="navigateToPrimaryAction"
            />
          </div>

          <!-- completed mark -->
          <div
            v-if="isCompleted"
            class="bg-primary/80 absolute right-0 top-0 flex h-7 w-7 items-center justify-center rounded-bl-md text-white shadow-sm"
          >
            <MdiCheck class="text-sm" />
          </div>
        </MediumCoverCard>
      </div>

      <!-- Meta -->
      <div class="flex-1">
        <div
          :title="title"
          class="hover:text-primary cursor-pointer text-base font-medium leading-tight"
          @click.stop="navigateToDetail"
        >
          {{ title }}
        </div>
        <div class="text-muted-foreground mt-0.5 text-xs">
          <span v-if="mediumType === MediumType.Comic">
            <MdiComic class="inline-block text-xs" />
            {{ $t('page.mediums.info.comic') }}
          </span>
          <span v-else-if="mediumType === MediumType.Video">
            <MdiVideo class="inline-block text-xs" />
            {{ $t('page.mediums.info.video') }}
          </span>
        </div>
        <div class="text-muted-foreground mt-0.5 text-xs">{{ year }}</div>
        <div class="text-muted-foreground mt-0.5 text-xs">{{ timeAgo }}</div>

        <!-- reading progress bar -->
        <div
          v-if="showReadingProgress"
          class="relative mt-2 h-1 overflow-hidden rounded"
        >
          <div class="absolute inset-0 bg-black/10"></div>
          <div
            class="relative z-10 h-full transition-all duration-300 ease-out"
            :style="{
              width: `${readingProgress}%`,
              backgroundColor: `hsl(var(--primary))`,
            }"
          ></div>
        </div>
      </div>

      <!-- 右侧操作区（编辑、更多） -->
      <div
        v-show="!isInSelectionMode"
        class="text-muted-foreground ml-auto flex items-center gap-3 self-center pr-1"
      >
        <CiEditPencilLine01
          class="hover:text-primary cursor-pointer text-xl"
          @click.stop="handleEdit"
        />
        <el-dropdown trigger="click" @click.stop>
          <span class="inline-flex items-center" @click.stop @mousedown.stop>
            <CiMoreVertical class="hover:text-primary cursor-pointer text-xl" />
          </span>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click.stop="openInfo">
                {{ $t('page.mediums.actions.info') }}
              </el-dropdown-item>
              <el-dropdown-item @click.stop="markAsWatched">
                {{ $t('page.mediums.actions.markAsWatched') }}
              </el-dropdown-item>
              <el-dropdown-item @click.stop="markAsUnwatched">
                {{ $t('page.mediums.actions.markAsUnwatched') }}
              </el-dropdown-item>
              <el-dropdown-item divided @click.stop="deleteMedium">
                {{ $t('common.delete') }}
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* 保持文字两行时的裁剪行为一致 */
.line-clamp-2 {
  display: -webkit-box;
  overflow: hidden;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
}
</style>

<script setup lang="ts">
import { MediumType } from '@/api/http/library/typing'
import type { MediumGetListOutput } from '@/api/http/typing'
import { useMediumItem } from '@/hooks/useMediumItem'
import { $t } from '@/locales/i18n'

const props = defineProps<{
  modelValue: MediumGetListOutput
}>()

// 移除对外 emit，改为内部统一处理

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
  buildMediumDropdown,
} = useMediumItem(mediumRef)

const dropdownItems = computed(() => buildMediumDropdown())
</script>

<template>
  <div
    :id="mediumAnchorId"
    class="text-foreground medium-list-item hover:bg-muted/40 group relative cursor-pointer rounded p-3 transition-colors select-none"
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
            <UIcon
              name="i-lucide-play-circle"
              class="hover:text-primary cursor-pointer text-5xl drop-shadow"
              @click.stop="navigateToPrimaryAction"
            />
          </div>

          <!-- completed mark -->
          <div
            v-if="isCompleted"
            class="bg-primary/80 absolute top-0 right-0 flex h-7 w-7 items-center justify-center rounded-bl-md text-white shadow-sm"
          >
            <UIcon name="i-lucide-check" class="text-sm" />
          </div>
        </MediumCoverCard>
      </div>

      <!-- Meta -->
      <div class="flex-1">
        <div
          :title="title"
          class="hover:text-primary cursor-pointer text-base leading-tight font-medium"
          @click.stop="navigateToDetail()"
        >
          {{ title }}
        </div>
        <div class="text-muted-foreground mt-0.5 text-xs">
          <span v-if="mediumType === MediumType.Comic">
            <UIcon name="i-lucide-book-open" class="inline-block text-xs" />
            {{ $t('page.mediums.info.comic') }}
          </span>
          <span v-else-if="mediumType === MediumType.Video">
            <UIcon name="i-lucide-video" class="inline-block text-xs" />
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
        <UIcon
          name="i-lucide-pencil"
          class="hover:text-primary cursor-pointer text-xl"
          @click.stop="handleEdit"
        />
        <UDropdownMenu :items="dropdownItems" @click.stop>
          <span class="inline-flex items-center" @click.stop @mousedown.stop>
            <UIcon
              name="i-lucide-more-vertical"
              class="hover:text-primary cursor-pointer text-xl"
            />
          </span>
        </UDropdownMenu>
      </div>
    </div>
  </div>
</template>

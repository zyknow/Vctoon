<script setup lang="ts">
import type { MediumGetListOutput } from '@/api/http/typing'
import { useMediumItem } from '@/hooks/useMediumItem'
import { useMediumStore } from '@/stores'

const props = defineProps<{
  modelValue: MediumGetListOutput
}>()

// 移除对外 emit，改为内部统一处理

const mediumStore = useMediumStore()

const cardStyleVars = computed<Record<string, string>>(() => ({
  '--cover-base-height': '14rem',
  '--cover-base-width': '10rem',
  '--cover-zoom': String(mediumStore.itemZoom ?? 1),
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
  buildMediumDropdown,
  handleEdit,
} = useMediumItem(mediumRef)

const dropdownItems = computed(() => buildMediumDropdown())

// Dropdown 锚点与打开状态
const anchorRef = ref<HTMLElement | null>(null)
const dropdownOpen = ref(false)
</script>

<template>
  <div
    :style="cardStyleVars"
    class="text-foreground medium-card group relative cursor-pointer transition-colors select-none"
    @click="handleCardClick"
  >
    <!-- Cover -->
    <MediumCoverCard
      :id="mediumAnchorId"
      :src="cover"
      class="relative border transition-colors"
      :class="{
        'group-hover:border-primary border-transparent': !isSelected,
        'border-primary border-4': isSelected,
      }"
    >
      <div
        ref="anchorRef"
        class="pointer-events-auto absolute right-2 bottom-2 h-5 w-5"
      >
        <UDropdownMenu
          v-model:open="dropdownOpen"
          :items="dropdownItems"
          :content="{ side: 'bottom', align: 'start' }"
          :ui="{ content: 'min-w-[180px] p-1' }"
        >
          <div></div>

          <template #content>
            <div class="flex flex-col py-1">
              <template v-for="(item, idx) in dropdownItems" :key="idx">
                <USeparator v-if="item.type === 'separator'" class="my-1" />
                <UButton
                  v-else
                  :color="item.color ?? 'neutral'"
                  variant="ghost"
                  class="w-full justify-start"
                  @click="
                    (e) => {
                      if (item.onSelect) {
                        item.onSelect(e)
                      }
                      dropdownOpen = false
                    }
                  "
                >
                  <UIcon v-if="item.icon" :name="item.icon" class="mr-2" />
                  <span class="truncate">{{ item.label }}</span>
                </UButton>
              </template>
            </div>
          </template>
        </UDropdownMenu>
      </div>

      <div
        v-show="!isInSelectionMode"
        class="absolute inset-0 hidden items-center justify-center gap-3 rounded-lg group-hover:flex"
      >
        <div
          class="pointer-events-none absolute inset-0 rounded-lg bg-black/40"
        ></div>
        <!-- 非选择模式：显示所有操作图标 -->
        <!-- left-bottom: edit icon -->
        <div class="pointer-events-auto absolute bottom-3 left-2 text-white/90">
          <UIcon
            name="i-lucide-pencil"
            class="hover:text-primary cursor-pointer text-xl"
            @click="handleEdit"
          />
        </div>
        <!-- right-bottom: more icon -->
        <div
          class="pointer-events-auto absolute right-2 bottom-2 text-white/90"
        >
          <UIcon
            name="i-lucide-more-vertical"
            class="hover:text-primary cursor-pointer text-2xl"
            @click.stop="dropdownOpen = true"
          />
        </div>
        <!-- center: play circle -->
        <div class="pointer-events-auto text-white">
          <UIcon
            name="i-lucide-play-circle"
            class="hover:text-primary cursor-pointer text-6xl drop-shadow"
            @click.stop="navigateToPrimaryAction"
          />
        </div>
      </div>

      <!-- 选中状态圆圈 - 始终显示在左上角 -->
      <div class="absolute top-2 left-2" @click="toggleSelectionFromIcon">
        <MediumSelectionIndicator
          :selected="isSelected"
          size="sm"
          :visible="isInSelectionMode"
        />
      </div>

      <!-- 完成标记 - 显示在右上角 -->
      <div
        v-if="isCompleted"
        class="bg-primary/80 absolute top-0 right-0 flex h-7 w-7 items-center justify-center rounded-bl-md text-white shadow-sm"
      >
        <UIcon name="i-lucide-check" class="text-sm" />
      </div>

      <!-- 阅读进度条 - 显示在图片底部 -->
      <div
        v-if="showReadingProgress"
        class="absolute right-0 bottom-0 left-0 h-1 overflow-hidden rounded-b-lg"
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
        class="hover:text-primary line-clamp-2 cursor-pointer text-sm leading-snug font-medium"
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

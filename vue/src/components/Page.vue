<script setup lang="ts">
import { computed } from 'vue'
import type { StyleValue } from 'vue'

import { useIsMobile } from '@/hooks/useIsMobile'

interface PageProps {
  title?: string
  description?: string
  contentClass?: string
  /**
   * 根据content可见高度自适应
   */
  autoContentHeight?: boolean
  headerClass?: string
  footerClass?: string
  /**
   * Custom height offset value (in pixels) to adjust content area sizing
   * when used with autoContentHeight
   * @default 0
   */
  heightOffset?: number
}

defineOptions({
  name: 'Page',
})

const { autoContentHeight = false, heightOffset = 0 } = defineProps<PageProps>()

const { isMobile } = useIsMobile()

const rootStyle = computed<StyleValue>(() => {
  if (autoContentHeight && heightOffset > 0) {
    return {
      height: `calc(100% - ${heightOffset}px)`,
    }
  }
  return {}
})
</script>

<template>
  <div
    class="relative"
    :class="{ 'flex h-full flex-col overflow-hidden': autoContentHeight }"
    :style="rootStyle"
  >
    <div
      v-if="
        description ||
        $slots.description ||
        title ||
        $slots.title ||
        $slots.extra
      "
      class="bg-card border-border relative flex shrink-0 items-end border-b px-6 py-2"
      :class="headerClass"
    >
      <div class="flex-auto">
        <slot name="title">
          <div v-if="title" class="mb-2 flex text-lg font-semibold">
            {{ title }}
          </div>
        </slot>

        <slot name="description">
          <p v-if="description" class="text-muted-foreground">
            {{ description }}
          </p>
        </slot>
      </div>

      <div v-if="$slots.extra">
        <slot name="extra"></slot>
      </div>
    </div>

    <div
      class="h-full"
      :class="[
        isMobile ? 'p-4 py-1' : 'p-4',
        contentClass,
        autoContentHeight ? 'min-h-0 flex-1 overflow-y-auto' : '',
      ]"
    >
      <slot></slot>
    </div>

    <div
      v-if="$slots.footer"
      class="bg-card align-center flex px-6 py-4"
      :class="[
        footerClass,
        autoContentHeight ? 'shrink-0' : 'absolute right-0 bottom-0 left-0',
      ]"
    >
      <slot name="footer"></slot>
    </div>
  </div>
</template>

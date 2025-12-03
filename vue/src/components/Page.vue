<script setup lang="ts">
import { computed, nextTick, onMounted, ref, useTemplateRef } from 'vue'
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

const headerHeight = ref(0)
const footerHeight = ref(0)
const layoutFooterHeight = ref(0)
const layoutHeaderHeight = ref(0)
const shouldAutoHeight = ref(false)

const headerRef = useTemplateRef<HTMLDivElement>('headerRef')
const footerRef = useTemplateRef<HTMLDivElement>('footerRef')

const contentStyle = computed<StyleValue>(() => {
  if (autoContentHeight) {
    const totalOffset =
      heightOffset +
      headerHeight.value +
      footerHeight.value +
      layoutFooterHeight.value +
      layoutHeaderHeight.value
    return {
      height: `calc(100vh - ${totalOffset}px)`,
      overflowY: shouldAutoHeight.value ? 'auto' : 'unset',
    }
  }
  return {}
})

async function calcContentHeight() {
  if (!autoContentHeight) {
    return
  }
  await nextTick()
  headerHeight.value = headerRef.value?.offsetHeight || 0
  footerHeight.value = footerRef.value?.offsetHeight || 0

  const mobileFooter = document.querySelector('.layout-footer')
  if (mobileFooter) {
    layoutFooterHeight.value = (mobileFooter as HTMLElement).offsetHeight
  } else {
    layoutFooterHeight.value = 0
  }

  const globalHeader = document.querySelector('.layout-header')
  if (globalHeader) {
    layoutHeaderHeight.value = (globalHeader as HTMLElement).offsetHeight
  } else {
    layoutHeaderHeight.value = 0
  }

  setTimeout(() => {
    shouldAutoHeight.value = true
  }, 30)
}

onMounted(() => {
  calcContentHeight()
})
</script>

<template>
  <div class="relative">
    <div
      v-if="
        description ||
        $slots.description ||
        title ||
        $slots.title ||
        $slots.extra
      "
      ref="headerRef"
      class="bg-card border-border relative flex items-end border-b px-6 py-2"
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
      :class="`${isMobile ? 'p-4 py-1' : 'p-4'} ${contentClass}`"
      :style="contentStyle"
    >
      <slot></slot>
    </div>

    <div
      v-if="$slots.footer"
      ref="footerRef"
      class="bg-card align-center absolute right-0 bottom-0 left-0 flex px-6 py-4"
      :class="footerClass"
    >
      <slot name="footer"></slot>
    </div>
  </div>
</template>

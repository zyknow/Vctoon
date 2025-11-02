<script setup lang="ts">
import { computed } from 'vue'

type IndicatorSize = 'md' | 'sm'

defineOptions({
  name: 'MediumSelectionIndicator',
})

const props = withDefaults(
  defineProps<{
    /**
     * 控制未选中时是否通过父级的 group-hover 显示。
     */
    hoverReveal?: boolean
    selected: boolean
    size?: IndicatorSize
    /**
     * 控制未选中时是否始终可见。
     * 例如选择模式下需要保持可见。
     */
    visible?: boolean
  }>(),
  {
    size: 'sm',
    visible: false,
    hoverReveal: true,
  },
)

const containerSizeClass = computed(() =>
  props.size === 'md' ? 'h-7 w-7' : 'h-5 w-5',
)

const iconClass = computed(() => (props.size === 'md' ? 'text-2xl' : 'text-xl'))

const borderWidthClass = computed(() =>
  props.size === 'md' ? 'border-2' : 'border-[3px]',
)

const rootClasses = computed(() => {
  const classes = [
    'medium-selection-indicator',
    'flex',
    'items-center',
    'justify-center',
    'rounded-full',
    'select-none',
    'transition-all',
    'transition-opacity',
    'duration-200',
    'ease-out',
    'cursor-pointer',
    'pointer-events-auto',
    containerSizeClass.value,
  ]

  if (props.selected) {
    classes.push('bg-primary', 'text-primary-foreground')
  } else {
    classes.push(
      'bg-transparent',
      'border',
      'border-white/80',
      borderWidthClass.value,
    )
  }

  if (props.selected || props.visible) {
    classes.push('opacity-100')
  } else if (props.hoverReveal) {
    classes.push('opacity-0', 'group-hover:opacity-100')
  } else {
    classes.push('opacity-0')
  }

  return classes
})
</script>

<template>
  <div :class="rootClasses">
    <UIcon v-if="selected" name="i-lucide-check-circle" :class="iconClass" />
  </div>
</template>

<style scoped>
.medium-selection-indicator {
  min-width: 1.25rem;
}
</style>

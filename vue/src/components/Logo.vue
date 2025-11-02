<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { RouterLink } from 'vue-router'

import { useEnvConfig } from '@/hooks/useEnvConfig'

type LogoSize = 'sm' | 'md' | 'lg'

const sizeClasses: Record<LogoSize, string> = {
  sm: 'h-6 w-auto',
  md: 'h-8 w-auto',
  lg: 'h-12 w-auto',
}

const titleSizeClasses: Record<LogoSize, string> = {
  sm: 'text-sm',
  md: 'text-base',
  lg: 'text-lg',
}

const props = withDefaults(
  defineProps<{
    /**
     * Logo 尺寸，默认 'md'
     */
    size?: LogoSize
    /**
     * 是否显示为链接，默认 true
     */
    linkable?: boolean
    /**
     * 链接目标，默认 '/'
     */
    to?: string
    /**
     * 是否显示标题，默认 true
     */
    showTitle?: boolean
  }>(),
  {
    size: 'md',
    linkable: true,
    to: '/',
    showTitle: true,
  },
)

const { t } = useI18n()

const tag = computed(() => (props.linkable ? RouterLink : 'div'))
const tagProps = computed(() => (props.linkable ? { to: props.to } : {}))
const ariaLabel = computed(() =>
  props.linkable ? t('page.home.title') : undefined,
)
const showTitle = computed(() => props.showTitle)

const rootClass = computed(() => [
  'flex items-center',
  props.linkable ? 'transition-opacity hover:opacity-80' : '',
])

const logoTitleClass = computed(
  () => `font-semibold text-foreground ${titleSizeClasses[props.size]}`,
)

const envConfig = useEnvConfig()
</script>

<template>
  <component
    :is="tag"
    v-bind="tagProps"
    :class="rootClass"
    :aria-label="ariaLabel"
  >
    <UIcon name="i-lineicons-nuxt" :class="sizeClasses[props.size]" />
    <span v-if="showTitle" :class="logoTitleClass">
      {{ envConfig.title }}
    </span>
  </component>
</template>

<script setup lang="ts">
import { computed, useAttrs } from 'vue'

import { useMediumStore } from '@/stores/medium'

type CoverLoadingType = 'eager' | 'lazy'

defineOptions({
  name: 'MediumCoverCard',
  inheritAttrs: false,
})

const props = withDefaults(
  defineProps<{
    alt?: string
    baseHeight?: string
    baseWidth?: string
    fit?: 'contain' | 'cover'
    loading?: CoverLoadingType
    preserveRatio?: boolean
    src?: string
    zoom?: number
  }>(),
  {
    alt: 'cover',
    baseHeight: '14rem',
    baseWidth: '10rem',
    fit: 'cover',
    loading: 'lazy' as CoverLoadingType,
    preserveRatio: false,
    src: '',
    zoom: undefined,
  },
)

const attrs = useAttrs()
const mediumStore = useMediumStore()

const resolvedZoom = computed(() => {
  const value = props.zoom ?? mediumStore.itemZoom ?? 1
  return Number.isFinite(value) ? value : 1
})

const styleVars = computed(() => ({
  '--cover-base-height': props.baseHeight,
  '--cover-base-width': props.baseWidth,
  '--cover-zoom': `${resolvedZoom.value}`,
}))

const hasImage = computed(() => Boolean(props.src))
</script>

<template>
  <div
    :class="{
      'medium-cover-card--auto-height': props.preserveRatio,
    }"
    class="medium-cover-card"
    :style="styleVars"
    v-bind="attrs"
  >
    <img
      v-if="hasImage"
      :alt="props.alt"
      decoding="async"
      :loading="props.loading"
      :src="props.src"
      class="medium-cover-card__image"
      :class="{
        'medium-cover-card__image--contain': props.fit === 'contain',
      }"
    />
    <div v-else class="medium-cover-card__placeholder">
      <slot name="placeholder">
        <div class="medium-cover-card__default-placeholder"></div>
      </slot>
    </div>
    <slot></slot>
  </div>
</template>

<style scoped>
.medium-cover-card {
  position: relative;
  display: block;
  width: calc(var(--cover-base-width) * clamp(0.8, var(--cover-zoom, 1), 1.6));
  height: calc(
    var(--cover-base-height) * clamp(0.8, var(--cover-zoom, 1), 1.6)
  );
  overflow: hidden;
  background-color: hsl(var(--muted));
  border-radius: 0.75rem;
}

.medium-cover-card__image,
.medium-cover-card__placeholder {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.medium-cover-card--auto-height {
  height: auto;
}

.medium-cover-card--auto-height .medium-cover-card__image,
.medium-cover-card--auto-height .medium-cover-card__placeholder {
  height: auto;
}

.medium-cover-card__image--contain {
  object-fit: contain;
}

.medium-cover-card__placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  color: hsl(var(--muted-foreground));
}

.medium-cover-card__default-placeholder {
  line-height: 1;
}
</style>

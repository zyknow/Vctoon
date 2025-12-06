<script setup lang="ts">
import { computed, useAttrs } from 'vue'

type CoverLoadingType = 'eager' | 'lazy'

defineOptions({
  name: 'MediumCoverCard',
  inheritAttrs: false,
})

const props = withDefaults(
  defineProps<{
    alt?: string
    fit?: 'contain' | 'cover'
    loading?: CoverLoadingType
    preserveRatio?: boolean
    src?: string
    zoom?: number
  }>(),
  {
    alt: 'cover',
    fit: 'cover',
    loading: 'lazy' as CoverLoadingType,
    preserveRatio: false,
    src: '',
    zoom: undefined,
  },
)

const attrs = useAttrs()

const hasImage = computed(() => Boolean(props.src))
</script>

<template>
  <div
    :class="{
      'medium-cover-card--preserve-ratio': props.preserveRatio,
    }"
    class="medium-cover-card"
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
  --card-width: calc(
    var(--cover-base-width) * clamp(0.8, var(--cover-zoom, 1), 1.6)
  );
  --card-height: calc(
    var(--cover-base-height) * clamp(0.8, var(--cover-zoom, 1), 1.6)
  );

  position: relative;
  display: block;
  width: var(--card-width);
  height: var(--card-height);
  overflow: hidden;
  background-color: hsl(var(--muted));
  border-radius: var(--ui-radius);
}

.medium-cover-card__image,
.medium-cover-card__placeholder {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.medium-cover-card--preserve-ratio {
  width: auto;
  height: auto;
  max-width: var(--card-width);
  max-height: var(--card-height);
  display: flex;
  justify-content: center;
  align-items: center;
}

.medium-cover-card--preserve-ratio .medium-cover-card__image,
.medium-cover-card--preserve-ratio .medium-cover-card__placeholder {
  width: auto;
  height: auto;
  max-width: 100%;
  max-height: 100%;
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

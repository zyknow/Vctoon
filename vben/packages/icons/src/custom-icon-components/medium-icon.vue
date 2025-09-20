<script setup lang="ts">
import { computed } from 'vue'

import { MdiComic, MdiVideo } from '../iconify'

const props = defineProps<{
  mediumType: 0 | 1
}>()

const icon = computed(() => {
  return props.mediumType === 0 ? MdiComic : MdiVideo
})
</script>

<template>
  <component :is="icon" />
</template>

<script setup lang="ts">
import ComicBrowser from './components/ComicBrowser.vue'

defineOptions({ name: 'ComicReaderPage' })
</script>

<template>
  <div class="fixed inset-0 z-50">
    <ComicBrowser />
  </div>
</template>

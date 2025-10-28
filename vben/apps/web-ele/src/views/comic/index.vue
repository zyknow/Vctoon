<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

import ComicBrowser from './components/comic-browser.vue'

defineOptions({ name: 'ComicReaderPage' })

const dialogVisible = ref<boolean>(true)
const router = useRouter()

const handleClosed = () => {
  router.back()
}
</script>

<template>
  <el-dialog
    v-model="dialogVisible"
    :fullscreen="true"
    :show-close="false"
    :close-on-press-escape="false"
    :close-on-click-modal="false"
    :destroy-on-close="true"
    class="comic-dialog"
    header-class="comic-dialog-header"
    @closed="handleClosed"
  >
    <ComicBrowser />
  </el-dialog>
</template>

<style>
.comic-dialog {
  padding: 0 !important;

  .comic-dialog-header {
    display: none !important;
  }
}
</style>

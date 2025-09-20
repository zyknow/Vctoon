<script setup lang="ts">
import type { MenuRecordRaw } from '@vben/types'

import { computed, ref } from 'vue'

import { CiAddPlus } from '@vben/icons'

import { refreshLibraryAccess } from '#/router/access'
import CreateLibraryDialog from '#/views/library/create-library-dialog.vue'

const props = withDefaults(defineProps<{ menu: MenuRecordRaw }>(), {})
const menu = computed(() => props.menu)

// 创建库对话框显隐
const showCreateDialog = ref(false)
async function onCreated(_library: any) {
  refreshLibraryAccess()
}
</script>

<template>
  <div class="library-title">
    <div class="font-bold">{{ menu.name }}</div>
    <div class="flex flex-row items-center gap-2">
      <el-button text link @click.stop @click="showCreateDialog = true">
        <CiAddPlus class="library-action-icon" />
      </el-button>
    </div>
  </div>
  <CreateLibraryDialog v-model="showCreateDialog" @success="onCreated" />
</template>

<style lang="scss" scoped>
.library-title {
  @apply flex flex-row items-center justify-between;
}

.library-action-icon {
  @apply text-xl hover:scale-125;
}
</style>

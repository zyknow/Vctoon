<script setup lang="ts">
import type { MenuRecordRaw } from '@vben/types'

import { computed } from 'vue'

import { CiAddPlus } from '@vben/icons'

import { useLibraryDialogService } from '#/hooks/useLibraryDialogService'

const props = withDefaults(defineProps<{ menu: MenuRecordRaw }>(), {})
const menu = computed(() => props.menu)

const dialog = useLibraryDialogService()
async function onCreate() {
  const created = await dialog.openCreate()
  if (created) {
    // TODO: 刷新库列表或触发 store reload
  }
}
</script>

<template>
  <div class="library-title">
    <div class="font-bold">{{ menu.name }}</div>
    <div class="flex flex-row items-center gap-2">
      <el-button text link @click.stop @click="onCreate">
        <CiAddPlus class="library-action-icon" />
      </el-button>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.library-title {
  @apply flex flex-row items-center justify-between;
}

.library-action-icon {
  @apply text-xl hover:scale-125;
}
</style>

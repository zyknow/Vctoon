<script setup lang="ts">
import CreateOrUpdateLibraryModal from '@/components/overlays/CreateOrUpdateLibraryModal.vue'

const overlay = useOverlay()
const createLibraryModal = overlay.create(CreateOrUpdateLibraryModal)

async function onCreate() {
  const result = await createLibraryModal.open()

  if (result) {
    // 创建成功，可以在这里触发刷新等操作
    // 由于使用了全局状态管理，组件会自动更新
  }
}
</script>

<template>
  <UButton
    class="hover:text-primary"
    icon="i-lucide-plus"
    color="neutral"
    variant="ghost"
    square
    size="xs"
    @click.stop
    @click="onCreate"
  />
</template>

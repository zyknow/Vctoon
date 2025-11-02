<script setup lang="ts">
import { ref } from 'vue'

import type { Library } from '@/api/http/library'
import { MediumType } from '@/api/http/library'
import { libraryHub } from '@/api/signalr/library'
import { useUserStore } from '@/stores/user'

interface ScanInfo {
  library: Library
  message: string
  title: string
}

// 扫描状态
const isScanning = ref(false)
const scanInfos = ref<ScanInfo[]>([])
// 计算属性：手动控制 Popover（点击展开）
const popoverVisible = ref(false)

const userStore = useUserStore()

libraryHub.start()

libraryHub.on('OnScanned', (libraryId) => {
  // 移除已完成扫描的图书馆
  scanInfos.value = scanInfos.value.filter(
    (info) => info.library.id !== libraryId,
  )
  isScanning.value = scanInfos.value.length > 0
})

libraryHub.on('OnScanning', ({ libraryId, message, title }) => {
  const library = userStore.libraries.find((lib) => lib.id === libraryId)!
  const newScanInfo = { library, message, title }

  // 先移除可能存在的旧扫描信息，再添加新的
  scanInfos.value = scanInfos.value.filter(
    (info) => info.library.id !== libraryId,
  )
  scanInfos.value.push(newScanInfo)

  isScanning.value = scanInfos.value.length > 0
})

// 根据 mediumType 获取图标类名
const getMediumTypeIcon = (mediumType: MediumType) => {
  return mediumType === MediumType.Comic
    ? 'i-lucide-book-open'
    : 'i-lucide-video'
}
</script>

<template>
  <UPopover
    v-model:open="popoverVisible"
    :content="{ side: 'bottom', align: 'center', sideOffset: 8 }"
  >
    <UButton color="neutral" variant="ghost" square class="relative">
      <!-- 雷电图标 -->
      <UIcon
        name="i-lucide-zap"
        class="size-5"
        :class="isScanning ? 'text-warning' : ''"
      />
      <!-- 扫描时的旋转加载图标，重叠在上方 -->
      <UIcon
        v-if="isScanning"
        name="i-lucide-loader-circle"
        :class="isScanning ? 'text-warning' : ''"
        class="absolute inset-0 z-10 size-8 animate-spin"
      />
    </UButton>

    <template #content>
      <div class="min-h-20 min-w-80 p-2">
        <div
          v-for="scanInfo in scanInfos"
          :key="scanInfo.library.id"
          class="mb-3 last:mb-0"
        >
          <div class="flex min-w-48 flex-col space-y-1">
            <div class="flex items-center space-x-2">
              <div class="bg-primary h-2 w-2 animate-pulse rounded-full"></div>
              <div
                class="text-primary flex flex-row items-center gap-2 text-sm font-medium"
              >
                <UIcon
                  :name="getMediumTypeIcon(scanInfo.library.mediumType)"
                  class="text-base"
                />
                <div>
                  {{ scanInfo.library.name }}
                </div>
              </div>
            </div>
            <div class="text-muted-foreground ml-4 text-xs">
              {{ scanInfo.message }}
            </div>
          </div>
        </div>
      </div>
    </template>
  </UPopover>
</template>

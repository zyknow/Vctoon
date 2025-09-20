<script setup lang="ts">
import type { Library } from '@vben/api'

import { computed, ref } from 'vue'

import { libraryHub } from '@vben/api'
import { MdiLoading, MdiWave, MediumIcon } from '@vben/icons'
import { useUserStore } from '@vben/stores'

import { VbenIconButton } from '../../../../../packages/@core/ui-kit/shadcn-ui/src/components'

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

// 动态计算图标 - 始终显示原本的扫描图标
const currentIcon = computed(() => {
  return MdiWave
})

const userStore = useUserStore()

// 动态计算按钮类
const iconClass = computed(() => {
  return 'my-0 mr-1 rounded-md text-xl'
})

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
</script>

<template>
  <el-popover
    v-model:visible="popoverVisible"
    :show-after="0"
    :hide-after="0"
    placement="bottom-start"
    trigger="click"
    width="320"
  >
    <template #reference>
      <VbenIconButton class="relative">
        <!-- 原本的图标 -->
        <component :is="currentIcon" :class="iconClass" />
        <!-- 扫描时的旋转黄色加载图标，重叠在上方 -->
        <MdiLoading
          v-if="isScanning"
          class="text-warning-500 absolute inset-0 z-10 animate-spin text-3xl"
        />
      </VbenIconButton>
    </template>

    <div class="p-2">
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
              <MediumIcon :medium-type="scanInfo.library.mediumType" />
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
  </el-popover>
</template>

import { defineStore } from 'pinia'

import { useIsMobile } from '@/hooks/useIsMobile'

import { ItemDisplayMode } from './typing'

export const useMediumStore = defineStore(
  'medium',
  () => {
    // State
    const itemZoom = ref(1)
    const itemDisplayMode = ref(ItemDisplayMode.Grid)

    // Composables - 只初始化一次
    const { isMobile } = useIsMobile()

    // Getters - 自动追踪依赖
    const mediumZoom = computed(() => {
      return isMobile.value ? 1 : itemZoom.value
    })

    return {
      itemZoom,
      itemDisplayMode,
      mediumZoom,
    }
  },
  {
    persist: { pick: ['itemZoom', 'itemDisplayMode'] },
  },
)

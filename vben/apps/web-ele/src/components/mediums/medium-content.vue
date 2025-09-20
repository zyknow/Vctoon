<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

import { useInjectedMediumProvider } from '#/hooks/useMediumProvider'
import { useMediumStore } from '#/store'
import { ItemDisplayMode } from '#/store/typing'

import MediumGuidItem from './medium-guid-item.vue'
import MediumListItem from './medium-list-item.vue'

defineProps<{
  modelValue?: boolean
}>()

const { items, loadType } = useInjectedMediumProvider()
const mediumStore = useMediumStore()

const currentItemComp = computed(() =>
  mediumStore.itemDisplayMode === ItemDisplayMode.Grid
    ? MediumGuidItem
    : MediumListItem,
)

const containerClass = computed(() =>
  mediumStore.itemDisplayMode === ItemDisplayMode.Grid
    ? 'flex flex-wrap gap-6'
    : 'flex flex-col gap-2',
)

// 使用已注入的 provider
const injected = useInjectedMediumProvider()

// 仅在 medium-content 内部滚动触发加载更多
const scrollWrapRef = ref<HTMLElement | null>(null)
const onInnerScroll = () => {
  const el = scrollWrapRef.value
  if (!el) return
  const threshold = 120 // px 提前量
  const scrollBottom = el.scrollHeight - (el.scrollTop + el.clientHeight)
  if (scrollBottom <= threshold) {
    void providerLoadNext()
  }
}

const providerLoadNext = async () => {
  if ('loadNext' in injected && typeof injected.loadNext === 'function') {
    await injected.loadNext()
  }
}

onMounted(() => {
  const el = scrollWrapRef.value
  if (el) el.addEventListener('scroll', onInnerScroll, { passive: true })
})

onBeforeUnmount(() => {
  const el = scrollWrapRef.value
  if (el) el.removeEventListener('scroll', onInnerScroll)
})
</script>

<template>
  <div ref="scrollWrapRef" class="medium-content">
    <div :class="containerClass">
      <component
        :is="currentItemComp"
        :medium-type="loadType"
        v-for="item in items"
        :key="item.id"
        :model-value="item"
      />
    </div>
  </div>
</template>

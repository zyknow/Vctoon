<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

import { ElMessage } from 'element-plus'

import useMediumDialogService from '#/hooks/useMediumDialogService'
import {
  useInjectedMediumItemProvider,
  useInjectedMediumProvider,
} from '#/hooks/useMediumProvider'
import { useMediumStore } from '#/store'
import { ItemDisplayMode } from '#/store/typing'

import MediumGridItem from './medium-grid-item.vue'
import MediumListItem from './medium-list-item.vue'

defineProps<{
  modelValue?: boolean
}>()

const { loadType } = useInjectedMediumProvider()
const { items } = useInjectedMediumItemProvider()
const mediumStore = useMediumStore()
const { openEdit } = useMediumDialogService()

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
  await injected.loadNext()
}

// 编辑弹窗处理（Promise 化）
const handleEdit = async (medium: any) => {
  try {
    const updated = await openEdit({
      mediumId: medium.id,
      mediumType: medium.mediumType,
      closeOnClickModal: false,
    })
    if (updated) {
      const index = items.value.findIndex((item) => item.id === updated.id)
      if (index !== -1) {
        items.value[index] = updated
      }
    }
  } catch (error) {
    // reject 场景（目前未使用）
    console.error('Error creating dialog:', error)
    ElMessage.error('创建弹窗失败，请查看控制台')
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
    <!-- Grid 模式动画容器 -->
    <TransitionGroup
      v-if="mediumStore.itemDisplayMode === ItemDisplayMode.Grid"
      name="fade-slide"
      tag="div"
      :class="containerClass"
      appear
    >
      <MediumGridItem
        :medium-type="loadType"
        v-for="item in items"
        :key="`grid-${item.id}`"
        :model-value="item"
        @edit="handleEdit"
      />
    </TransitionGroup>

    <!-- List 模式动画容器 -->
    <TransitionGroup
      v-else
      name="fade-slide"
      tag="div"
      :class="containerClass"
      appear
    >
      <MediumListItem
        :medium-type="loadType"
        v-for="item in items"
        :key="`list-${item.id}`"
        :model-value="item"
        @edit="handleEdit"
      />
    </TransitionGroup>
  </div>
</template>

<style scoped>
/* 动画（参考 medium-recommendation-section.vue） */
.fade-slide-enter-active {
  transition: all 0.5s cubic-bezier(0.4, 0, 0.2, 1);
}

.fade-slide-enter-from {
  opacity: 0;
  transform: translateX(30px) scale(0.9);
}

.fade-slide-enter-to {
  opacity: 1;
  transform: translateX(0) scale(1);
}

.fade-slide-move {
  transition: transform 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}

.fade-slide-enter-active,
.fade-slide-move {
  will-change: transform, opacity;
}
</style>

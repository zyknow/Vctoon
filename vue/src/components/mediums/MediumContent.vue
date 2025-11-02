<script setup lang="ts">
import {
  useInjectedMediumItemProvider,
  useInjectedMediumProvider,
} from '@/hooks/useMediumProvider'
import { $t } from '@/locales/i18n'
import { useMediumStore } from '@/stores'
import { ItemDisplayMode } from '@/stores/typing'

defineProps<{
  modelValue?: boolean
}>()

const { loadType, loading, hasMore, loadItems, loadNext } =
  useInjectedMediumProvider()
const { items } = useInjectedMediumItemProvider()
const mediumStore = useMediumStore()

const containerClass = computed(() =>
  mediumStore.itemDisplayMode === ItemDisplayMode.Grid
    ? 'flex flex-wrap gap-6'
    : 'flex flex-col gap-2',
)

const endReachedDisabled = computed(
  () => loading.value || !hasMore.value || loadType === undefined,
)

const onEndReached = (direction: 'top' | 'bottom' | 'left' | 'right') => {
  if (direction !== 'bottom') return
  if (endReachedDisabled.value) return
  void loadNext().catch((error) => {
    console.error('滚动加载失败', error)
  })
}

// 编辑逻辑由子项统一处理（medium.ts）

const ensureInitialLoad = async () => {
  if (items.value.length === 0) {
    await loadItems()
  }
}

onMounted(() => {
  void ensureInitialLoad()
})

onActivated(() => {
  void ensureInitialLoad()
})
</script>

<template>
  <UScrollbar
    remember
    class="medium-content"
    aria-orientation="vertical"
    @end-reached="onEndReached"
  >
    <div class="medium-content__body">
      <!-- Grid 模式动画容器 -->
      <TransitionGroup
        v-if="mediumStore.itemDisplayMode === ItemDisplayMode.Grid"
        name="fade-slide"
        tag="div"
        :class="containerClass"
        appear
      >
        <MediumGridItem
          v-for="item in items"
          :key="`grid-${item.id}`"
          :medium-type="loadType"
          :model-value="item"
        />
      </TransitionGroup>
      <!-- List 模式渲染容器 -->
      <div v-else :class="containerClass">
        <MediumListItem
          v-for="item in items"
          :key="`list-${item.id}`"
          :model-value="item"
        />
      </div>
      <div
        v-if="!loading && items.length === 0"
        class="text-muted-foreground py-12 text-center text-sm"
      >
        {{ $t('common.noData') }}
      </div>
      <div
        v-else-if="loading && items.length === 0"
        class="text-muted-foreground py-12 text-center text-sm"
      >
        {{ $t('common.loading') }}
      </div>
      <!-- 底部加载状态 -->
      <div class="flex justify-center py-4">
        <div v-if="loading" class="text-muted-foreground text-sm">
          {{ $t('common.loading') }}
        </div>
        <div v-else-if="!hasMore" class="text-muted-foreground text-sm">
          {{ $t('page.mediums.list.noMore') }}
        </div>
      </div>
    </div>
  </UScrollbar>
</template>

<style scoped>
.medium-content {
}

.medium-content__body {
  display: flex;
  flex-direction: column;
}

.medium-content__pagination {
  margin-top: 1.5rem;
}

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

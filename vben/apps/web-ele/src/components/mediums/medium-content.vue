<script setup lang="ts">
import type { ElScrollbar } from 'element-plus'

import type { MediumGetListOutput } from '@vben/api'

import { computed, nextTick, onActivated, onMounted, ref, watch } from 'vue'

import { ElMessage } from 'element-plus'

import useMediumDialogService from '#/hooks/useMediumDialogService'
import {
  useInjectedMediumItemProvider,
  useInjectedMediumProvider,
} from '#/hooks/useMediumProvider'
import { $t } from '#/locales'
import { useMediumStore } from '#/store'
import { ItemDisplayMode } from '#/store/typing'

import MediumGridItem from './medium-grid-item.vue'
import MediumListItem from './medium-list-item.vue'

defineProps<{
  modelValue?: boolean
}>()

const { loadType, loading, hasMore, loadItems, loadNext } =
  useInjectedMediumProvider()
const { items } = useInjectedMediumItemProvider()
const mediumStore = useMediumStore()
const { openEdit } = useMediumDialogService()

const containerClass = computed(() =>
  mediumStore.itemDisplayMode === ItemDisplayMode.Grid
    ? 'flex flex-wrap gap-6'
    : 'flex flex-col gap-2',
)

const scrollbarRef = ref<InstanceType<typeof ElScrollbar>>()
const endReachedDisabled = computed(
  () => loading.value || !hasMore.value || loadType === undefined,
)

const handleEndReached = (): void => {
  if (endReachedDisabled.value) return
  void loadNext().catch((error) => {
    console.error('滚动加载失败', error)
  })
}

const handleEdit = async (medium: MediumGetListOutput) => {
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
    console.error('Error creating dialog:', error)
    ElMessage.error('创建弹窗失败，请查看控制台')
  }
}

const ensureInitialLoad = async () => {
  if (items.value.length === 0) {
    await loadItems()
  }
  await nextTick()
  scrollbarRef.value?.update?.()
}

onMounted(() => {
  void ensureInitialLoad()
})

onActivated(() => {
  void ensureInitialLoad()
})

// 数据加载完成或列表长度变化后再次尝试恢复滚动
watch(
  () => loading.value,
  (isLoading) => {
    if (!isLoading) {
      scrollbarRef.value?.update?.()
    }
  },
)

watch(
  () => items.value.length,
  () => {
    scrollbarRef.value?.update?.()
  },
)
</script>

<template>
  <el-scrollbar
    ref="scrollbarRef"
    class="medium-content"
    height="100%"
    :distance="200"
    @end-reached="handleEndReached"
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
          :medium-type="loadType"
          v-for="item in items"
          :key="`grid-${item.id}`"
          :model-value="item"
          @edit="handleEdit"
        />
      </TransitionGroup>
      <!-- List 模式渲染容器 -->
      <div v-else :class="containerClass">
        <MediumListItem
          v-for="item in items"
          :key="`list-${item.id}`"
          :model-value="item"
          @edit="handleEdit"
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
        <div class="text-muted-foreground text-sm" v-if="loading">
          {{ $t('common.loading') }}
        </div>
        <div class="text-muted-foreground text-sm" v-else-if="!hasMore">
          {{ $t('page.mediums.list.noMore') }}
        </div>
      </div>
    </div>
  </el-scrollbar>
</template>

<style scoped>
.medium-content {
  height: 100%;
  padding: 0;
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

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'

import { ElMessage } from 'element-plus'

import { useDialogService } from '#/hooks/useDialogService'
import {
  useInjectedMediumItemProvider,
  useInjectedMediumProvider,
} from '#/hooks/useMediumProvider'
import { $t } from '#/locales'
import { useMediumStore } from '#/store'
import { ItemDisplayMode } from '#/store/typing'

import MediumEditDialog from './medium-edit-dialog.vue'
import MediumGridItem from './medium-grid-item.vue'
import MediumListItem from './medium-list-item.vue'

defineProps<{
  modelValue?: boolean
}>()

const { loadType } = useInjectedMediumProvider()
const { items } = useInjectedMediumItemProvider()
const mediumStore = useMediumStore()
const { openDialog } = useDialogService()

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

// 编辑弹窗处理
const handleEdit = (medium: any) => {
  try {
    const dialogInstance = openDialog(
      MediumEditDialog,
      {
        mediumId: medium.id,
        mediumType: medium.mediumType,
        onClose: () => {
          dialogInstance.close()
        },
        onUpdated: (updatedMedium: any) => {
          // 更新列表中的 medium 数据
          const index = items.value.findIndex(
            (item) => item.id === updatedMedium.id,
          )
          if (index !== -1) {
            items.value[index] = updatedMedium
          }
          dialogInstance.close()
        },
      },
      {
        title: $t('page.mediums.edit.title'),
        width: '600px',
        closeOnClickModal: false,
      },
    )
  } catch (error) {
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
    <div :class="containerClass">
      <template v-if="mediumStore.itemDisplayMode === ItemDisplayMode.Grid">
        <MediumGridItem
          :medium-type="loadType"
          v-for="item in items"
          :key="`grid-${item.id}`"
          :model-value="item"
          @edit="handleEdit"
        />
      </template>
      <template v-else>
        <MediumListItem
          :medium-type="loadType"
          v-for="item in items"
          :key="`list-${item.id}`"
          :model-value="item"
          @edit="handleEdit"
        />
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { refDebounced } from '@vueuse/core'

import { tagApi } from '@/api/http/tag'
import ConfirmModal from '@/components/overlays/ConfirmModal.vue'
import CreateOrUpdateTagModal from '@/components/overlays/CreateOrUpdateTagModal.vue'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores/user'

defineOptions({
  name: 'Tag',
})

// 响应式数据
const loading = ref(false)

const toast = useToast()
const overlay = useOverlay()

const userStore = useUserStore()

// 使用 userStore 中的 tags 数据
const tags = computed(() => userStore.tags || [])

// 搜索和排序状态
const searchFilter = ref('')
// 自动防抖的搜索值，延迟 300ms
const debouncedFilter = refDebounced(searchFilter, 300)

const sortState = reactive({
  field: 'name' as 'name' | 'resourceCount',
  order: 'asc' as 'asc' | 'desc',
})

// Modal 实例
const confirmModal = overlay.create(ConfirmModal)
const tagModal = overlay.create(CreateOrUpdateTagModal)

// 多选状态
const selectedTags = ref<Set<string>>(new Set())
const isMultiSelectMode = ref(false)

// 计算属性 - 处理搜索和排序
const processedTags = computed(() => {
  const source = Array.isArray(tags.value) ? tags.value : []
  let result = source.filter((t) => t && typeof t === 'object')

  // 搜索过滤 - 使用防抖后的值
  if (debouncedFilter.value.trim()) {
    const filter = debouncedFilter.value.toLowerCase().trim()
    result = result.filter((tag) => tag.name?.toLowerCase().includes(filter))
  }

  // 排序
  result.sort((a, b) => {
    let aValue: number | string
    let bValue: number | string

    if (sortState.field === 'name') {
      aValue = a.name?.toLowerCase() || ''
      bValue = b.name?.toLowerCase() || ''
    } else {
      aValue = a.resourceCount || 0
      bValue = b.resourceCount || 0
    }

    if (aValue < bValue) {
      return sortState.order === 'asc' ? -1 : 1
    }
    if (aValue > bValue) {
      return sortState.order === 'asc' ? 1 : -1
    }
    return 0
  })

  return result
})

// 统计信息
const statistics = computed(() => {
  const source = Array.isArray(tags.value) ? tags.value : []
  return {
    total: source.length,
    filtered: processedTags.value.length,
    totalResources: source.reduce(
      (sum, tag) => sum + (tag?.resourceCount || 0),
      0,
    ),
  }
})

// 方法
const loadTags = async () => {
  try {
    loading.value = true
    // 从 userStore 刷新标签数据
    await userStore.reloadTags()
  } catch (error) {
    console.error('加载标签列表失败:', error)
    toast.add({
      title: $t('page.tag.messages.loadTagsError'),
      color: 'error',
    })
  } finally {
    loading.value = false
  }
}

const handleReset = () => {
  searchFilter.value = ''
}

const handleSort = (field: 'name' | 'resourceCount') => {
  if (sortState.field === field) {
    // 如果点击的是当前排序字段，切换排序方向
    sortState.order = sortState.order === 'asc' ? 'desc' : 'asc'
  } else {
    // 如果点击的是新字段，设置为该字段并默认升序
    sortState.field = field
    sortState.order = 'asc'
  }
}

const getSortIcon = (field: 'name' | 'resourceCount') => {
  if (sortState.field !== field) {
    return ''
  }
  return sortState.order === 'asc'
    ? 'i-lucide-arrow-up-wide-narrow'
    : 'i-lucide-arrow-down-wide-narrow'
}

const handleCreate = async () => {
  const created = await tagModal.open({})
  if (created) {
    await loadTags()
  }
}

const handleEdit = async (tagId: string) => {
  const tag = tags.value.find((t) => t.id === tagId)
  if (!tag) return

  const updated = await tagModal.open({ tag })
  if (updated) {
    await loadTags()
  }
}

// 多选相关方法
const toggleMultiSelect = () => {
  isMultiSelectMode.value = !isMultiSelectMode.value
  if (!isMultiSelectMode.value) {
    selectedTags.value.clear()
  }
}

const selectAllTags = () => {
  if (selectedTags.value.size === processedTags.value.length) {
    selectedTags.value.clear()
  } else {
    selectedTags.value = new Set(processedTags.value.map((tag) => tag.id))
  }
}

const toggleTagSelection = (tagId: string) => {
  if (selectedTags.value.has(tagId)) {
    selectedTags.value.delete(tagId)
  } else {
    selectedTags.value.add(tagId)
  }
}

const handleDelete = async (tagId: string) => {
  const confirmed = await confirmModal.open({
    title: $t('page.tag.delete.confirmTitle'),
    message: $t('page.tag.delete.confirmMessage'),
    confirmText: $t('page.tag.delete.confirm'),
    cancelText: $t('page.tag.delete.cancel'),
    danger: true,
  })

  if (!confirmed) return

  try {
    await tagApi.delete(tagId)
    toast.add({
      title: $t('page.tag.delete.messages.success'),
      color: 'success',
    })
    await loadTags()
  } catch (error) {
    console.error('删除标签失败:', error)
    toast.add({
      title: $t('page.tag.delete.messages.error'),
      color: 'error',
    })
  }
}

const handleBatchDelete = async () => {
  if (selectedTags.value.size === 0) {
    toast.add({
      title: $t('page.tag.batchDelete.noSelection'),
      color: 'warning',
    })
    return
  }

  const confirmed = await confirmModal.open({
    title: $t('page.tag.batchDelete.confirmTitle'),
    message: $t('page.tag.batchDelete.confirmMessage', {
      count: selectedTags.value.size,
    }),
    confirmText: $t('page.tag.batchDelete.confirm'),
    cancelText: $t('page.tag.batchDelete.cancel'),
    danger: true,
  })

  if (!confirmed) return

  try {
    await tagApi.deleteMany([...selectedTags.value])
    toast.add({
      title: $t('page.tag.batchDelete.messages.success', {
        count: selectedTags.value.size,
      }),
      color: 'success',
    })
    selectedTags.value.clear()
    isMultiSelectMode.value = false
    await loadTags()
  } catch (error) {
    console.error('批量删除标签失败:', error)
    toast.add({
      title: $t('page.tag.batchDelete.messages.error'),
      color: 'error',
    })
  }
}

// 初始化
loadTags()
</script>

<template>
  <Page content-class="flex flex-col gap-4">
    <!-- 搜索和工具栏 -->
    <UCard class="mb-0">
      <div class="mb-4 flex flex-col gap-4">
        <!-- 标题行和统计信息 -->
        <div
          class="flex flex-col items-start justify-between gap-4 md:flex-row md:items-center"
        >
          <h2 class="text-foreground text-xl font-semibold">
            {{ $t('page.tag.title') }}
          </h2>
          <div class="flex flex-wrap items-center gap-4 text-sm">
            <div class="flex items-center gap-1.5">
              <span class="text-muted-foreground">{{
                $t('page.tag.statistics.total')
              }}</span>
              <span class="text-primary font-semibold">{{
                statistics.total
              }}</span>
            </div>
            <div class="flex items-center gap-1.5">
              <span class="text-muted-foreground">{{
                $t('page.tag.statistics.filtered')
              }}</span>
              <span class="text-success font-semibold">{{
                statistics.filtered
              }}</span>
            </div>
            <div class="flex items-center gap-1.5">
              <span class="text-muted-foreground">{{
                $t('page.tag.statistics.totalResources')
              }}</span>
              <span class="text-warning font-semibold">{{
                statistics.totalResources
              }}</span>
            </div>
          </div>
        </div>

        <!-- 搜索和操作按钮 -->
        <div
          class="flex flex-col items-center justify-between gap-3 md:flex-row md:gap-0"
        >
          <div class="flex items-center justify-center md:justify-start">
            <UInput
              v-model="searchFilter"
              icon="i-lucide-search"
              :placeholder="$t('page.tag.search.filterPlaceholder')"
              class="w-72"
            />
            <UButton variant="ghost" class="ml-2" @click="handleReset">
              {{ $t('page.tag.search.reset') }}
            </UButton>
          </div>

          <div class="flex justify-center gap-2 md:justify-end">
            <UButton @click="handleCreate">
              <UIcon name="i-lucide-plus" class="mr-1" />
              {{ $t('page.tag.actions.create') }}
            </UButton>
            <UButton variant="ghost" @click="loadTags">
              <UIcon name="i-lucide-refresh-ccw" class="mr-1" />
              {{ $t('page.tag.actions.refresh') }}
            </UButton>
            <UButton
              variant="ghost"
              :color="isMultiSelectMode ? 'warning' : 'neutral'"
              @click="toggleMultiSelect"
            >
              {{
                isMultiSelectMode
                  ? $t('page.tag.actions.exitMultiSelect')
                  : $t('page.tag.actions.multiSelect')
              }}
            </UButton>
          </div>
        </div>

        <!-- 排序按钮和多选操作 -->
        <div class="flex flex-col gap-3">
          <div
            class="flex flex-wrap items-center justify-center gap-2 md:justify-start"
          >
            <span class="text-muted-foreground mr-2 text-sm">{{
              $t('page.tag.sort.label')
            }}</span>
            <UButton
              :color="sortState.field === 'name' ? 'primary' : 'neutral'"
              variant="ghost"
              size="xs"
              @click="handleSort('name')"
            >
              {{ $t('page.tag.sort.name') }}
              <UIcon
                v-if="getSortIcon('name')"
                :name="getSortIcon('name')"
                class="ml-1 text-sm"
              />
            </UButton>
            <UButton
              :color="
                sortState.field === 'resourceCount' ? 'primary' : 'neutral'
              "
              variant="ghost"
              size="xs"
              @click="handleSort('resourceCount')"
            >
              {{ $t('page.tag.sort.resourceCount') }}
              <UIcon
                v-if="getSortIcon('resourceCount')"
                :name="getSortIcon('resourceCount')"
                class="ml-1 text-sm"
              />
            </UButton>
          </div>

          <!-- 多选操作栏 -->
          <div
            v-if="isMultiSelectMode"
            class="flex flex-wrap items-center gap-2"
          >
            <UCheckbox
              :model-value="
                selectedTags.size > 0 &&
                selectedTags.size === processedTags.length
              "
              :indeterminate="
                selectedTags.size > 0 &&
                selectedTags.size < processedTags.length
              "
              @change="selectAllTags"
            >
              {{ $t('page.tag.actions.selectAll') }}
            </UCheckbox>
            <span class="text-muted-foreground text-sm">
              {{
                $t('page.tag.actions.selectedCount', {
                  count: selectedTags.size,
                  total: processedTags.length,
                })
              }}
            </span>
            <UButton
              v-if="selectedTags.size > 0"
              color="error"
              size="xs"
              @click="handleBatchDelete"
            >
              <UIcon name="i-lucide-trash-2" class="mr-1" />
              {{ $t('page.tag.actions.batchDelete') }}
            </UButton>
          </div>
        </div>
      </div>
    </UCard>

    <!-- 标签列表 -->
    <UCard class="mb-0 flex-1">
      <div class="relative min-h-96">
        <div
          v-if="loading"
          class="bg-background/50 absolute inset-0 z-10 grid place-items-center backdrop-blur-sm"
        >
          <UIcon
            name="i-lucide-loader-2"
            class="text-primary h-6 w-6 animate-spin"
          />
        </div>
        <div
          v-if="processedTags.length === 0"
          class="flex h-96 flex-col items-center justify-center text-center"
        >
          <UIcon
            name="i-lucide-tag"
            class="text-muted-foreground mb-4 text-6xl"
          />
          <div class="text-muted-foreground text-base">
            {{
              debouncedFilter
                ? $t('page.tag.empty.noResults')
                : $t('page.tag.empty.noTags')
            }}
          </div>
        </div>

        <div class="space-y-3">
          <!-- 标签列表 -->
          <div class="flex flex-wrap gap-2">
            <TagItem
              v-for="(tag, index) in processedTags"
              :key="tag.id || (tag as any).slug || index"
              :entity="tag"
              :is-multi-select-mode="isMultiSelectMode"
              :is-selected="selectedTags.has(tag.id)"
              @edit="handleEdit"
              @delete="handleDelete"
              @toggle-selection="toggleTagSelection"
            />
          </div>
        </div>
      </div>
    </UCard>
  </Page>

  <!-- dialog 由 service 动态挂载 -->
</template>

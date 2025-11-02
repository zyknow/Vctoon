<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { refDebounced } from '@vueuse/core'

import { artistApi } from '@/api/http/artist'
import ConfirmModal from '@/components/overlays/ConfirmModal.vue'
import CreateOrUpdateArtistModal from '@/components/overlays/CreateOrUpdateArtistModal.vue'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores/user'

defineOptions({
  name: 'Artist',
})

// 响应式数据
const loading = ref(false)

const toast = useToast()
const overlay = useOverlay()

const userStore = useUserStore()

// 使用 userStore 中的 artists 数据
const artists = computed(() => userStore.artists || [])

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
const artistModal = overlay.create(CreateOrUpdateArtistModal)

// 多选状态
const selectedArtists = ref<Set<string>>(new Set())
const isMultiSelectMode = ref(false)

// 计算属性 - 处理搜索和排序
const processedArtists = computed(() => {
  const source = Array.isArray(artists.value) ? artists.value : []
  let result = source.filter((a) => a && typeof a === 'object')

  // 搜索过滤 - 使用防抖后的值
  if (debouncedFilter.value.trim()) {
    const filter = debouncedFilter.value.toLowerCase().trim()
    result = result.filter((artist) =>
      artist.name?.toLowerCase().includes(filter),
    )
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
  const source = Array.isArray(artists.value) ? artists.value : []
  return {
    total: source.length,
    filtered: processedArtists.value.length,
    totalResources: source.reduce(
      (sum, artist) => sum + (artist?.resourceCount || 0),
      0,
    ),
  }
})

// 方法
const loadArtists = async () => {
  try {
    loading.value = true
    // 从 userStore 刷新艺术家数据
    await userStore.reloadArtists()
  } catch (error) {
    console.error('加载艺术家列表失败:', error)
    toast.add({
      title: $t('page.artist.messages.loadArtistsError'),
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
  const created = await artistModal.open({})
  if (created) {
    await loadArtists()
  }
}

const handleEdit = async (artistId: string) => {
  const artist = artists.value.find((a) => a.id === artistId)
  if (!artist) return

  const updated = await artistModal.open({ artist })
  if (updated) {
    await loadArtists()
  }
}

// 多选相关方法
const toggleMultiSelect = () => {
  isMultiSelectMode.value = !isMultiSelectMode.value
  if (!isMultiSelectMode.value) {
    selectedArtists.value.clear()
  }
}

const selectAllArtists = () => {
  if (selectedArtists.value.size === processedArtists.value.length) {
    selectedArtists.value.clear()
  } else {
    selectedArtists.value = new Set(
      processedArtists.value.map((artist) => artist.id),
    )
  }
}

const toggleArtistSelection = (artistId: string) => {
  if (selectedArtists.value.has(artistId)) {
    selectedArtists.value.delete(artistId)
  } else {
    selectedArtists.value.add(artistId)
  }
}

const handleDelete = async (artistId: string) => {
  const confirmed = await confirmModal.open({
    title: $t('page.artist.delete.confirmTitle'),
    message: $t('page.artist.delete.confirmMessage'),
    confirmText: $t('page.artist.delete.confirm'),
    cancelText: $t('page.artist.delete.cancel'),
    danger: true,
  })

  if (!confirmed) return

  try {
    await artistApi.delete(artistId)
    toast.add({
      title: $t('page.artist.delete.messages.success'),
      color: 'success',
    })
    await loadArtists()
  } catch (error) {
    console.error('删除艺术家失败:', error)
    toast.add({
      title: $t('page.artist.delete.messages.error'),
      color: 'error',
    })
  }
}

const handleBatchDelete = async () => {
  if (selectedArtists.value.size === 0) {
    toast.add({
      title: $t('page.artist.batchDelete.noSelection'),
      color: 'warning',
    })
    return
  }

  const confirmed = await confirmModal.open({
    title: $t('page.artist.batchDelete.confirmTitle'),
    message: $t('page.artist.batchDelete.confirmMessage', {
      count: selectedArtists.value.size,
    }),
    confirmText: $t('page.artist.batchDelete.confirm'),
    cancelText: $t('page.artist.batchDelete.cancel'),
    danger: true,
  })

  if (!confirmed) return

  try {
    await artistApi.deleteMany([...selectedArtists.value])
    toast.add({
      title: $t('page.artist.batchDelete.messages.success', {
        count: selectedArtists.value.size,
      }),
      color: 'success',
    })
    selectedArtists.value.clear()
    isMultiSelectMode.value = false
    await loadArtists()
  } catch (error) {
    console.error('批量删除艺术家失败:', error)
    toast.add({
      title: $t('page.artist.batchDelete.messages.error'),
      color: 'error',
    })
  }
}

// 初始化
loadArtists()
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
            {{ $t('page.artist.title') }}
          </h2>
          <div class="flex flex-wrap items-center gap-4 text-sm">
            <div class="flex items-center gap-1.5">
              <span class="text-muted-foreground">{{
                $t('page.artist.statistics.total')
              }}</span>
              <span class="text-primary font-semibold">{{
                statistics.total
              }}</span>
            </div>
            <div class="flex items-center gap-1.5">
              <span class="text-muted-foreground">{{
                $t('page.artist.statistics.filtered')
              }}</span>
              <span class="text-success font-semibold">{{
                statistics.filtered
              }}</span>
            </div>
            <div class="flex items-center gap-1.5">
              <span class="text-muted-foreground">{{
                $t('page.artist.statistics.totalResources')
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
              :placeholder="$t('page.artist.search.filterPlaceholder')"
              class="w-72"
            />
            <UButton variant="ghost" class="ml-2" @click="handleReset">
              {{ $t('page.artist.search.reset') }}
            </UButton>
          </div>

          <div class="flex justify-center gap-2 md:justify-end">
            <UButton @click="handleCreate">
              <UIcon name="i-lucide-plus" class="mr-1" />
              {{ $t('page.artist.actions.create') }}
            </UButton>
            <UButton variant="ghost" @click="loadArtists">
              <UIcon name="i-lucide-refresh-ccw" class="mr-1" />
              {{ $t('page.artist.actions.refresh') }}
            </UButton>
            <UButton
              variant="ghost"
              :color="isMultiSelectMode ? 'warning' : 'neutral'"
              @click="toggleMultiSelect"
            >
              {{
                isMultiSelectMode
                  ? $t('page.artist.actions.exitMultiSelect')
                  : $t('page.artist.actions.multiSelect')
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
              $t('page.artist.sort.label')
            }}</span>
            <UButton
              :color="sortState.field === 'name' ? 'primary' : 'neutral'"
              variant="ghost"
              size="xs"
              @click="handleSort('name')"
            >
              {{ $t('page.artist.sort.name') }}
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
              {{ $t('page.artist.sort.resourceCount') }}
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
                selectedArtists.size > 0 &&
                selectedArtists.size === processedArtists.length
              "
              :indeterminate="
                selectedArtists.size > 0 &&
                selectedArtists.size < processedArtists.length
              "
              @change="selectAllArtists"
            >
              {{ $t('page.artist.actions.selectAll') }}
            </UCheckbox>
            <span class="text-muted-foreground text-sm">
              {{
                $t('page.artist.actions.selectedCount', {
                  count: selectedArtists.size,
                  total: processedArtists.length,
                })
              }}
            </span>
            <UButton
              v-if="selectedArtists.size > 0"
              color="error"
              size="xs"
              @click="handleBatchDelete"
            >
              <UIcon name="i-lucide-trash-2" class="mr-1" />
              {{ $t('page.artist.actions.batchDelete') }}
            </UButton>
          </div>
        </div>
      </div>
    </UCard>

    <!-- 艺术家列表 -->
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
          v-if="processedArtists.length === 0"
          class="flex h-96 flex-col items-center justify-center text-center"
        >
          <UIcon
            name="i-lucide-user"
            class="text-muted-foreground mb-4 text-6xl"
          />
          <div class="text-muted-foreground text-base">
            {{
              debouncedFilter
                ? $t('page.artist.empty.noResults')
                : $t('page.artist.empty.noArtists')
            }}
          </div>
        </div>

        <div class="space-y-3">
          <!-- 艺术家列表 -->
          <div class="flex flex-wrap gap-2">
            <TagItem
              v-for="(artist, index) in processedArtists"
              :key="artist.id || (artist as any).slug || index"
              :entity="artist"
              :is-multi-select-mode="isMultiSelectMode"
              :is-selected="selectedArtists.has(artist.id)"
              @edit="handleEdit"
              @delete="handleDelete"
              @toggle-selection="toggleArtistSelection"
            />
          </div>
        </div>
      </div>
    </UCard>
  </Page>

  <!-- dialog 由 service 动态挂载 -->
</template>

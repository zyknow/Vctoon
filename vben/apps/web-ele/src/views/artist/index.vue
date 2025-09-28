<script setup lang="ts">
import type { Artist } from '@vben/api'

import { computed, reactive, ref } from 'vue'

import { artistApi } from '@vben/api'
import { Page } from '@vben/common-ui'
import {
  CiEditPencilLine01,
  CiSearch,
  CiUser,
  MdiDelete,
  MdiPlus,
  MdiRefresh,
  MdiSortAscending,
  MdiSortDescending,
} from '@vben/icons'
import { useUserStore } from '@vben/stores'

import { ElMessage, ElMessageBox } from 'element-plus'

import { useArtistDialogService } from '#/hooks/useArtistDialogService'
import { $t } from '#/locales'

// 响应式数据
const loading = ref(false)

const userStore = useUserStore()

// 使用 userStore 中的 artists 数据
const artists = computed(() => userStore.artists || [])

// 搜索和排序状态
const searchForm = reactive({
  filter: '',
})

const sortState = reactive({
  field: 'name' as 'name' | 'resourceCount',
  order: 'asc' as 'asc' | 'desc',
})

// dialog service
const artistDialog = useArtistDialogService()

// 多选状态
const selectedArtists = ref<Set<string>>(new Set())
const isMultiSelectMode = ref(false)

// 计算属性 - 处理搜索和排序
const processedArtists = computed(() => {
  let result = [...artists.value]

  // 搜索过滤
  if (searchForm.filter.trim()) {
    const filter = searchForm.filter.toLowerCase().trim()
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
const statistics = computed(() => ({
  total: artists.value.length,
  filtered: processedArtists.value.length,
  totalResources: artists.value.reduce(
    (sum, artist) => sum + (artist.resourceCount || 0),
    0,
  ),
}))

// 方法
const loadArtists = async () => {
  try {
    loading.value = true
    // 从 userStore 刷新艺术家数据
    await userStore.reloadArtists()
  } catch (error) {
    console.error('加载艺术家列表失败:', error)
    ElMessage.error($t('page.artist.messages.loadArtistsError'))
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  // 搜索逻辑由计算属性自动处理
}

const handleReset = () => {
  searchForm.filter = ''
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
    return null
  }
  return sortState.order === 'asc' ? MdiSortAscending : MdiSortDescending
}

const getArtistColor = (
  artist: Artist,
): 'danger' | 'info' | 'primary' | 'success' | 'warning' => {
  // 根据资源数量决定艺术家颜色
  const count = artist.resourceCount || 0
  if (count === 0) return 'info'
  if (count < 5) return 'info'
  if (count < 20) return 'success'
  if (count < 50) return 'warning'
  return 'danger'
}

const handleCreate = async () => {
  const created = await artistDialog.openCreate()
  if (created) loadArtists()
}

const handleEdit = async (artistId: string) => {
  const updated = await artistDialog.openEdit(artistId)
  if (updated) loadArtists()
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
  try {
    await ElMessageBox.confirm(
      $t('page.artist.delete.confirmMessage'),
      $t('page.artist.delete.confirmTitle'),
      {
        confirmButtonText: $t('page.artist.delete.confirm'),
        cancelButtonText: $t('page.artist.delete.cancel'),
        type: 'warning',
      },
    )

    await artistApi.delete(artistId)
    ElMessage.success($t('page.artist.delete.messages.success'))
    loadArtists()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('删除艺术家失败:', error)
      ElMessage.error($t('page.artist.delete.messages.error'))
    }
  }
}

const handleBatchDelete = async () => {
  if (selectedArtists.value.size === 0) {
    ElMessage.warning($t('page.artist.batchDelete.noSelection'))
    return
  }

  try {
    await ElMessageBox.confirm(
      $t('page.artist.batchDelete.confirmMessage', {
        count: selectedArtists.value.size,
      }),
      $t('page.artist.batchDelete.confirmTitle'),
      {
        confirmButtonText: $t('page.artist.batchDelete.confirm'),
        cancelButtonText: $t('page.artist.batchDelete.cancel'),
        type: 'warning',
      },
    )

    await artistApi.deleteMany([...selectedArtists.value])
    ElMessage.success(
      $t('page.artist.batchDelete.messages.success', {
        count: selectedArtists.value.size,
      }),
    )
    selectedArtists.value.clear()
    isMultiSelectMode.value = false
    loadArtists()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('批量删除艺术家失败:', error)
      ElMessage.error($t('page.artist.batchDelete.messages.error'))
    }
  }
}

// 初始化
loadArtists()
</script>

<template>
  <Page content-class="flex flex-col gap-4">
    <!-- 统计信息卡片 -->
    <el-card shadow="never" class="mb-0">
      <div class="flex flex-col justify-around gap-8 md:flex-row">
        <div class="text-center">
          <div class="text-muted-foreground mb-1 text-sm">
            {{ $t('page.artist.statistics.total') }}
          </div>
          <div class="text-primary text-2xl font-bold">
            {{ statistics.total }}
          </div>
        </div>
        <div class="text-center">
          <div class="text-muted-foreground mb-1 text-sm">
            {{ $t('page.artist.statistics.filtered') }}
          </div>
          <div class="text-success text-2xl font-bold">
            {{ statistics.filtered }}
          </div>
        </div>
        <div class="text-center">
          <div class="text-muted-foreground mb-1 text-sm">
            {{ $t('page.artist.statistics.totalResources') }}
          </div>
          <div class="text-warning text-2xl font-bold">
            {{ statistics.totalResources }}
          </div>
        </div>
      </div>
    </el-card>

    <!-- 搜索和工具栏 -->
    <el-card shadow="never" class="mb-0">
      <div
        class="mb-4 flex flex-col items-center justify-between gap-3 md:flex-row md:gap-0"
      >
        <div class="flex items-center justify-center md:justify-start">
          <CiSearch class="text-muted-foreground mr-2 text-xl" />
          <el-input
            v-model="searchForm.filter"
            :placeholder="$t('page.artist.search.filterPlaceholder')"
            clearable
            class="w-72"
            @input="handleSearch"
          />
          <el-button class="ml-2" @click="handleReset">
            {{ $t('page.artist.search.reset') }}
          </el-button>
        </div>

        <div class="flex justify-center gap-2 md:justify-end">
          <el-button type="primary" :icon="MdiPlus" @click="handleCreate">
            {{ $t('page.artist.actions.create') }}
          </el-button>
          <el-button :icon="MdiRefresh" @click="loadArtists">
            {{ $t('page.artist.actions.refresh') }}
          </el-button>
          <el-button
            :type="isMultiSelectMode ? 'warning' : 'default'"
            @click="toggleMultiSelect"
          >
            {{
              isMultiSelectMode
                ? $t('page.artist.actions.exitMultiSelect')
                : $t('page.artist.actions.multiSelect')
            }}
          </el-button>
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
          <el-button
            :type="sortState.field === 'name' ? 'primary' : 'default'"
            size="small"
            @click="handleSort('name')"
          >
            {{ $t('page.artist.sort.name') }}
            <component
              :is="getSortIcon('name')"
              v-if="getSortIcon('name')"
              class="ml-1 text-sm"
            />
          </el-button>
          <el-button
            :type="sortState.field === 'resourceCount' ? 'primary' : 'default'"
            size="small"
            @click="handleSort('resourceCount')"
          >
            {{ $t('page.artist.sort.resourceCount') }}
            <component
              :is="getSortIcon('resourceCount')"
              v-if="getSortIcon('resourceCount')"
              class="ml-1 text-sm"
            />
          </el-button>
        </div>

        <!-- 多选操作栏 -->
        <div v-if="isMultiSelectMode" class="flex flex-wrap items-center gap-2">
          <el-checkbox
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
          </el-checkbox>
          <span class="text-muted-foreground text-sm">
            {{
              $t('page.artist.actions.selectedCount', {
                count: selectedArtists.size,
                total: processedArtists.length,
              })
            }}
          </span>
          <el-button
            v-if="selectedArtists.size > 0"
            type="danger"
            size="small"
            :icon="MdiDelete"
            @click="handleBatchDelete"
          >
            {{ $t('page.artist.actions.batchDelete') }}
          </el-button>
        </div>
      </div>
    </el-card>

    <!-- 艺术家列表 -->
    <el-card shadow="never" class="mb-0 flex-1">
      <div v-loading="loading" class="min-h-96">
        <div
          v-if="processedArtists.length === 0"
          class="flex h-96 flex-col items-center justify-center text-center"
        >
          <CiUser class="text-muted-foreground mb-4 text-6xl" />
          <div class="text-muted-foreground text-base">
            {{
              searchForm.filter
                ? $t('page.artist.empty.noResults')
                : $t('page.artist.empty.noArtists')
            }}
          </div>
        </div>

        <div class="space-y-3">
          <!-- 艺术家列表 -->
          <div class="flex flex-wrap gap-2">
            <div
              v-for="artist in processedArtists"
              :key="artist.id"
              class="group flex items-center gap-2 rounded-lg border p-2 transition-all duration-200"
              :class="[
                isMultiSelectMode ? 'hover:border-primary cursor-pointer' : '',
                selectedArtists.has(artist.id)
                  ? 'border-primary bg-primary/5'
                  : 'border-border bg-card',
              ]"
              @click="
                isMultiSelectMode ? toggleArtistSelection(artist.id) : undefined
              "
            >
              <!-- 多选模式下的复选框 -->
              <el-checkbox
                v-if="isMultiSelectMode"
                :model-value="selectedArtists.has(artist.id)"
                @change="toggleArtistSelection(artist.id)"
                @click.stop
              />

              <!-- 艺术家内容 -->
              <el-tag
                :type="getArtistColor(artist)"
                size="default"
                class="!border-0 !bg-transparent !px-0"
              >
                <div class="flex items-center gap-1">
                  <CiUser class="text-xs" />
                  <span>{{ artist.name }}</span>
                  <span v-if="artist.resourceCount" class="text-xs opacity-75">
                    ({{ artist.resourceCount }})
                  </span>
                </div>
              </el-tag>

              <!-- 操作按钮 -->
              <div
                v-if="!isMultiSelectMode"
                class="flex items-center gap-1 opacity-0 transition-opacity group-hover:opacity-100"
              >
                <button
                  type="button"
                  @click.stop="handleEdit(artist.id)"
                  class="flex h-4 w-4 items-center justify-center rounded text-blue-500 hover:bg-blue-50 hover:text-blue-600"
                  :title="$t('page.artist.actions.edit')"
                >
                  <CiEditPencilLine01 class="h-3 w-3" />
                </button>
                <button
                  type="button"
                  @click.stop="handleDelete(artist.id)"
                  class="flex h-4 w-4 items-center justify-center rounded text-red-500 hover:bg-red-50 hover:text-red-600"
                  :title="$t('page.artist.actions.delete')"
                >
                  <MdiDelete class="h-3 w-3" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </el-card>
  </Page>

  <!-- dialog 由 service 动态挂载 -->
</template>

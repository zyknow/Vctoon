<script setup lang="ts">
import type { Tag } from '@vben/api'

import { computed, reactive, ref } from 'vue'

import { tagApi } from '@vben/api'
import { Page } from '@vben/common-ui'
import {
  CiEditPencilLine01,
  CiSearch,
  MdiDelete,
  MdiPlus,
  MdiRefresh,
  MdiSortAscending,
  MdiSortDescending,
  MdiTag,
} from '@vben/icons'

import { ElMessage, ElMessageBox } from 'element-plus'

import { $t } from '#/locales'

import CreateOrUpdateTagDialog from './create-or-update-tag-dialog.vue'

// 响应式数据
const loading = ref(false)

// 标签列表数据
const tags = ref<Tag[]>([])

// 搜索和排序状态
const searchForm = reactive({
  filter: '',
})

const sortState = reactive({
  field: 'name' as 'name' | 'resourceCount',
  order: 'asc' as 'asc' | 'desc',
})

// 对话框状态
const dialogVisible = ref(false)
const currentTagId = ref('')

// 多选状态
const selectedTags = ref<Set<string>>(new Set())
const isMultiSelectMode = ref(false)

// 计算属性 - 处理搜索和排序
const processedTags = computed(() => {
  let result = [...tags.value]

  // 搜索过滤
  if (searchForm.filter.trim()) {
    const filter = searchForm.filter.toLowerCase().trim()
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
const statistics = computed(() => ({
  total: tags.value.length,
  filtered: processedTags.value.length,
  totalResources: tags.value.reduce(
    (sum, tag) => sum + (tag.resourceCount || 0),
    0,
  ),
}))

// 方法
const loadTags = async () => {
  try {
    loading.value = true
    const result = await tagApi.getAllTags(true) // 获取资源计数
    tags.value = result || []
  } catch (error) {
    console.error('加载标签列表失败:', error)
    ElMessage.error($t('page.tag.messages.loadTagsError'))
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

const getTagColor = (
  tag: Tag,
): 'danger' | 'info' | 'primary' | 'success' | 'warning' => {
  // 根据资源数量决定标签颜色
  const count = tag.resourceCount || 0
  if (count === 0) return 'info'
  if (count < 5) return 'info'
  if (count < 20) return 'success'
  if (count < 50) return 'warning'
  return 'danger'
}

const handleCreate = () => {
  currentTagId.value = ''
  dialogVisible.value = true
}

const handleEdit = (tagId: string) => {
  currentTagId.value = tagId
  dialogVisible.value = true
}

const handleDialogSuccess = () => {
  loadTags()
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
  try {
    await ElMessageBox.confirm(
      $t('page.tag.delete.confirmMessage'),
      $t('page.tag.delete.confirmTitle'),
      {
        confirmButtonText: $t('page.tag.delete.confirm'),
        cancelButtonText: $t('page.tag.delete.cancel'),
        type: 'warning',
      },
    )

    await tagApi.delete(tagId)
    ElMessage.success($t('page.tag.delete.messages.success'))
    loadTags()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('删除标签失败:', error)
      ElMessage.error($t('page.tag.delete.messages.error'))
    }
  }
}

const handleBatchDelete = async () => {
  if (selectedTags.value.size === 0) {
    ElMessage.warning($t('page.tag.batchDelete.noSelection'))
    return
  }

  try {
    await ElMessageBox.confirm(
      $t('page.tag.batchDelete.confirmMessage', {
        count: selectedTags.value.size,
      }),
      $t('page.tag.batchDelete.confirmTitle'),
      {
        confirmButtonText: $t('page.tag.batchDelete.confirm'),
        cancelButtonText: $t('page.tag.batchDelete.cancel'),
        type: 'warning',
      },
    )

    await tagApi.deleteMany([...selectedTags.value])
    ElMessage.success(
      $t('page.tag.batchDelete.messages.success', {
        count: selectedTags.value.size,
      }),
    )
    selectedTags.value.clear()
    isMultiSelectMode.value = false
    loadTags()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('批量删除标签失败:', error)
      ElMessage.error($t('page.tag.batchDelete.messages.error'))
    }
  }
}

// 初始化
loadTags()
</script>

<template>
  <Page content-class="flex flex-col gap-4">
    <!-- 统计信息卡片 -->
    <el-card shadow="never" class="mb-0">
      <div class="flex flex-col justify-around gap-8 md:flex-row">
        <div class="text-center">
          <div class="text-muted-foreground mb-1 text-sm">
            {{ $t('page.tag.statistics.total') }}
          </div>
          <div class="text-primary text-2xl font-bold">
            {{ statistics.total }}
          </div>
        </div>
        <div class="text-center">
          <div class="text-muted-foreground mb-1 text-sm">
            {{ $t('page.tag.statistics.filtered') }}
          </div>
          <div class="text-success text-2xl font-bold">
            {{ statistics.filtered }}
          </div>
        </div>
        <div class="text-center">
          <div class="text-muted-foreground mb-1 text-sm">
            {{ $t('page.tag.statistics.totalResources') }}
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
            :placeholder="$t('page.tag.search.filterPlaceholder')"
            clearable
            class="w-72"
            @input="handleSearch"
          />
          <el-button class="ml-2" @click="handleReset">
            {{ $t('page.tag.search.reset') }}
          </el-button>
        </div>

        <div class="flex justify-center gap-2 md:justify-end">
          <el-button type="primary" :icon="MdiPlus" @click="handleCreate">
            {{ $t('page.tag.actions.create') }}
          </el-button>
          <el-button :icon="MdiRefresh" @click="loadTags">
            {{ $t('page.tag.actions.refresh') }}
          </el-button>
          <el-button
            :type="isMultiSelectMode ? 'warning' : 'default'"
            @click="toggleMultiSelect"
          >
            {{
              isMultiSelectMode
                ? $t('page.tag.actions.exitMultiSelect')
                : $t('page.tag.actions.multiSelect')
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
            $t('page.tag.sort.label')
          }}</span>
          <el-button
            :type="sortState.field === 'name' ? 'primary' : 'default'"
            size="small"
            @click="handleSort('name')"
          >
            {{ $t('page.tag.sort.name') }}
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
            {{ $t('page.tag.sort.resourceCount') }}
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
              selectedTags.size > 0 &&
              selectedTags.size === processedTags.length
            "
            :indeterminate="
              selectedTags.size > 0 && selectedTags.size < processedTags.length
            "
            @change="selectAllTags"
          >
            {{ $t('page.tag.actions.selectAll') }}
          </el-checkbox>
          <span class="text-muted-foreground text-sm">
            {{
              $t('page.tag.actions.selectedCount', {
                count: selectedTags.size,
                total: processedTags.length,
              })
            }}
          </span>
          <el-button
            v-if="selectedTags.size > 0"
            type="danger"
            size="small"
            :icon="MdiDelete"
            @click="handleBatchDelete"
          >
            {{ $t('page.tag.actions.batchDelete') }}
          </el-button>
        </div>
      </div>
    </el-card>

    <!-- 标签列表 -->
    <el-card shadow="never" class="mb-0 flex-1">
      <div v-loading="loading" class="min-h-96">
        <div
          v-if="processedTags.length === 0"
          class="flex h-96 flex-col items-center justify-center text-center"
        >
          <MdiTag class="text-muted-foreground mb-4 text-6xl" />
          <div class="text-muted-foreground text-base">
            {{
              searchForm.filter
                ? $t('page.tag.empty.noResults')
                : $t('page.tag.empty.noTags')
            }}
          </div>
        </div>

        <div class="space-y-3">
          <!-- 标签列表 -->
          <div class="flex flex-wrap gap-2">
            <div
              v-for="tag in processedTags"
              :key="tag.id"
              class="group flex items-center gap-2 rounded-lg border p-2 transition-all duration-200"
              :class="[
                isMultiSelectMode ? 'hover:border-primary cursor-pointer' : '',
                selectedTags.has(tag.id)
                  ? 'border-primary bg-primary/5'
                  : 'border-border bg-card',
              ]"
              @click="
                isMultiSelectMode ? toggleTagSelection(tag.id) : undefined
              "
            >
              <!-- 多选模式下的复选框 -->
              <el-checkbox
                v-if="isMultiSelectMode"
                :model-value="selectedTags.has(tag.id)"
                @change="toggleTagSelection(tag.id)"
                @click.stop
              />

              <!-- 标签内容 -->
              <el-tag
                :type="getTagColor(tag)"
                size="default"
                class="!border-0 !bg-transparent !px-0"
              >
                <div class="flex items-center gap-1">
                  <MdiTag class="text-xs" />
                  <span>{{ tag.name }}</span>
                  <span v-if="tag.resourceCount" class="text-xs opacity-75">
                    ({{ tag.resourceCount }})
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
                  @click.stop="handleEdit(tag.id)"
                  class="flex h-4 w-4 items-center justify-center rounded text-blue-500 hover:bg-blue-50 hover:text-blue-600"
                  :title="$t('page.tag.actions.edit')"
                >
                  <CiEditPencilLine01 class="h-3 w-3" />
                </button>
                <button
                  type="button"
                  @click.stop="handleDelete(tag.id)"
                  class="flex h-4 w-4 items-center justify-center rounded text-red-500 hover:bg-red-50 hover:text-red-600"
                  :title="$t('page.tag.actions.delete')"
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

  <!-- 创建/编辑对话框 -->
  <CreateOrUpdateTagDialog
    v-model:visible="dialogVisible"
    :tag-id="currentTagId"
    @success="handleDialogSuccess"
  />
</template>

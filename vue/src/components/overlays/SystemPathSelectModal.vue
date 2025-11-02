<script setup lang="ts">
import { ref, shallowRef } from 'vue'
import { TreeItem } from '@nuxt/ui'

import { systemApi } from '@/api/http/system'
import { $t } from '@/locales/i18n'

interface Props {
  selectedPaths?: string[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: string[] | null]
}>()

const loading = ref(false)
const selectedItems = ref<string[]>(props.selectedPaths || [])
const items = shallowRef<TreeItem[]>([])
const loadingPaths = ref<Set<string>>(new Set())
const toast = useToast()

// 获取路径的显示名称
function getPathLabel(path: string): string {
  const parts = path.split(/[/\\]/).filter(Boolean)
  return parts[parts.length - 1] || path
}

// 创建一个占位符节点，用于标记节点可展开但未加载
function createPlaceholderNode(): TreeItem {
  return {
    label: 'Loading...',
    value: '__loading__',
    icon: 'i-lucide-loader-circle',
  }
}

// 加载根路径
async function loadRootPaths() {
  try {
    loading.value = true
    const paths = await systemApi.getSystemPaths()

    items.value = paths.map((path) => ({
      label: path,
      value: path,
      icon: 'i-lucide-hard-drive',
      collapsedIcon: 'i-lucide-hard-drive',
      expandedIcon: 'i-lucide-hard-drive',
      // 添加一个占位符子节点，这样会显示展开箭头
      children: [createPlaceholderNode()],
    }))
  } catch {
    toast.add({
      title: $t('common.error'),
      description: $t('page.library.create.hints.loadPathFailed'),
      color: 'error',
    })
  } finally {
    loading.value = false
  }
}

// 递归查找并更新树节点
function findAndUpdateNode(
  nodes: TreeItem[],
  targetValue: string,
  updateFn: (node: TreeItem) => void,
): boolean {
  for (const node of nodes) {
    if (node.value === targetValue) {
      updateFn(node)
      return true
    }
    if (node.children && node.children.length > 0) {
      if (findAndUpdateNode(node.children, targetValue, updateFn)) {
        return true
      }
    }
  }
  return false
}

// 处理树节点展开 - 懒加载子路径
async function handleToggle(event: any, item: TreeItem) {
  // 从参数或事件中获取节点信息
  const targetItem = item || event.detail?.item || event
  const path = targetItem.value as string

  // 忽略占位符节点的点击
  if (path === '__loading__') {
    return
  }

  // 如果正在加载，不重复加载
  if (loadingPaths.value.has(path)) {
    return
  }

  // 检查是否是占位符节点（未加载状态）
  const isPlaceholder =
    targetItem.children &&
    targetItem.children.length === 1 &&
    targetItem.children[0]?.value === '__loading__'

  // 如果不是占位符状态（已经加载过真实数据），不重复加载
  if (!isPlaceholder) {
    return
  }

  try {
    loadingPaths.value.add(path)

    const childPaths = await systemApi.getSystemPaths(path)

    // 更新节点的子节点
    const newItems = [...items.value]
    findAndUpdateNode(newItems, path, (node) => {
      if (childPaths && childPaths.length > 0) {
        node.children = childPaths.map((childPath) => ({
          label: getPathLabel(childPath),
          value: childPath,
          icon: 'i-lucide-folder',
          collapsedIcon: 'i-lucide-folder',
          expandedIcon: 'i-lucide-folder-open',
          // 也给子节点添加占位符，表示它们也可能有子节点
          children: [createPlaceholderNode()],
        }))
      } else {
        // 如果没有子节点，移除 children 属性
        delete node.children
      }
    })

    items.value = newItems
  } catch (error) {
    console.error('Failed to load child paths:', error)
    toast.add({
      title: $t('common.error'),
      description: $t('page.library.create.hints.loadChildPathFailed', {
        path,
      }),
      color: 'error',
    })
    // 加载失败时，移除子节点数组（让节点不可展开）
    const newItems = [...items.value]
    findAndUpdateNode(newItems, path, (node) => {
      delete node.children
    })
    items.value = newItems
  } finally {
    loadingPaths.value.delete(path)
  }
}

// 处理选择
function handleSelect(item: TreeItem) {
  const path = item.value as string
  const index = selectedItems.value.indexOf(path)

  if (index > -1) {
    selectedItems.value.splice(index, 1)
  } else {
    selectedItems.value.push(path)
  }
}

// 移除选中的路径
function handleRemovePath(path: string) {
  const index = selectedItems.value.indexOf(path)
  if (index > -1) {
    selectedItems.value.splice(index, 1)
  }
}

// 确认选择
function handleConfirm() {
  emit('close', selectedItems.value)
}

// 取消
function handleCancel() {
  emit('close', null)
}

// 初始化加载
loadRootPaths()
</script>

<template>
  <UModal :title="$t('page.library.create.fields.paths')" :dismissible="false">
    <template #body>
      <div class="space-y-4">
        <!-- 已选择的路径列表 -->
        <div v-if="selectedItems.length > 0" class="space-y-2">
          <div class="text-muted text-sm">
            {{
              $t('page.library.create.hints.selectedCount', {
                count: selectedItems.length,
              })
            }}
          </div>
          <div
            class="bg-elevated/50 max-h-32 space-y-1 overflow-y-auto rounded-md border p-2"
          >
            <div
              v-for="path in selectedItems"
              :key="path"
              class="hover:bg-elevated flex items-center justify-between gap-2 rounded px-2 py-1 text-sm"
            >
              <span class="flex-1 truncate" :title="path">{{ path }}</span>
              <UButton
                icon="i-lucide-x"
                color="neutral"
                variant="ghost"
                size="xs"
                square
                @click="handleRemovePath(path)"
              />
            </div>
          </div>
        </div>

        <!-- 路径树浏览器 -->
        <div>
          <div class="text-muted mb-2 text-sm">
            {{ $t('page.library.create.hints.pathBrowser') }}
          </div>
          <div v-if="loading" class="flex items-center justify-center py-8">
            <UIcon name="i-lucide-loader-circle" class="size-8 animate-spin" />
          </div>

          <div
            v-else
            class="max-h-[400px] min-h-[300px] overflow-y-auto rounded-md border p-2"
          >
            <UTree
              :items="items"
              :get-key="(item) => item.value as string"
              @toggle="handleToggle"
            >
              <template #item-leading="{ item }">
                <!-- 不显示占位符节点的复选框 -->
                <UCheckbox
                  v-if="item.value !== '__loading__'"
                  :model-value="selectedItems.includes(item.value as string)"
                  tabindex="-1"
                  @click.stop
                  @change="() => handleSelect(item)"
                />
              </template>
              <template #item-label="{ item }">
                <!-- 占位符节点显示加载提示 -->
                <span
                  v-if="item.value === '__loading__'"
                  class="text-muted text-sm italic"
                >
                  {{ $t('common.loading') }}...
                </span>
                <span v-else>{{ item.label }}</span>
              </template>
              <template #item-trailing="{ item }">
                <UIcon
                  v-if="loadingPaths.has(item.value as string)"
                  name="i-lucide-loader-circle"
                  class="text-muted size-4 animate-spin"
                />
              </template>
            </UTree>
          </div>
        </div>
      </div>
    </template>

    <template #footer>
      <div class="flex justify-end gap-3">
        <UButton color="neutral" variant="ghost" @click="handleCancel">
          {{ $t('page.library.create.actions.cancel') }}
        </UButton>
        <UButton color="primary" @click="handleConfirm">
          {{ $t('page.library.create.actions.confirm') }}
        </UButton>
      </div>
    </template>
  </UModal>
</template>

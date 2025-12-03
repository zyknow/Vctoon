<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import type { TableColumn } from '@nuxt/ui'
import type { SortingState } from '@tanstack/vue-table'

import type {
  IdentitySecurityLog,
  IdentitySecurityLogPageRequest,
} from '@/api/http/security-logs'
import { securityLogApi } from '@/api/http/security-logs'
import Page from '@/components/Page.vue'
import { $t } from '@/locales/i18n'
import { createSortableHeader } from '@/utils/tables/table'

// 响应式数据
const loading = ref(false)
const toast = useToast()

// 安全日志列表数据
const securityLogs = ref<IdentitySecurityLog[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(10)

// 排序状态
const sorting = ref<SortingState>([
  { id: 'creationTime', desc: true }, // 默认按创建时间降序
])

// 搜索表单
const searchForm = reactive<IdentitySecurityLogPageRequest>({
  skipCount: 0,
  maxResultCount: 10,
  startTime: '',
  endTime: '',
  applicationName: '',
  identity: '',
  actionName: '',
  userName: '',
  clientId: '',
  clientIpAddress: '',
  correlationId: '',
})

// 查看模式切换
const viewMode = ref<'all' | 'current'>('all')

const viewModeOptions = computed(() => [
  { label: $t('page.securityLogs.viewMode.all'), value: 'all' as const },
  {
    label: $t('page.securityLogs.viewMode.current'),
    value: 'current' as const,
  },
])

// 使用通用工具中的 createSortableHeader

// 表格列定义
const columns: TableColumn<IdentitySecurityLog>[] = [
  {
    accessorKey: 'applicationName',
    header: createSortableHeader('page.securityLogs.table.applicationName'),
    cell: ({ row }) => row.getValue('applicationName') || '-',
  },
  {
    accessorKey: 'identity',
    header: createSortableHeader('page.securityLogs.table.identity'),
    cell: ({ row }) => row.getValue('identity') || '-',
  },
  {
    accessorKey: 'action',
    header: createSortableHeader('page.securityLogs.table.action'),
    cell: ({ row }) => row.getValue('action') || '-',
  },
  {
    accessorKey: 'userName',
    header: createSortableHeader('page.securityLogs.table.userName'),
    cell: ({ row }) => row.getValue('userName') || '-',
  },
  {
    accessorKey: 'clientId',
    header: createSortableHeader('page.securityLogs.table.clientId'),
    cell: ({ row }) => row.getValue('clientId') || '-',
  },
  {
    accessorKey: 'clientIpAddress',
    header: createSortableHeader('page.securityLogs.table.clientIpAddress'),
    cell: ({ row }) => row.getValue('clientIpAddress') || '-',
  },
  {
    accessorKey: 'browserInfo',
    header: () => $t('page.securityLogs.table.browserInfo'),
    cell: ({ row }) => {
      const text = row.getValue('browserInfo') as string | undefined
      if (!text) return '-'
      if (text.length <= 50) return text
      return `${text.slice(0, 50)}...`
    },
  },
  {
    accessorKey: 'creationTime',
    header: createSortableHeader('page.securityLogs.table.creationTime'),
    cell: ({ row }) => {
      const date = row.getValue('creationTime') as string | undefined
      if (!date) return '-'
      return new Date(date).toLocaleString('zh-CN')
    },
  },
]

// 监听排序变化，触发数据加载
watch(sorting, () => {
  page.value = 1
  loadSecurityLogs()
})

const buildQueryParams = (): IdentitySecurityLogPageRequest => {
  // 构建排序字符串
  let sortingString: string | undefined
  if (sorting.value.length > 0) {
    const sort = sorting.value[0]
    const field = sort.id
    if (field) {
      sortingString = `${field} ${sort.desc ? 'desc' : 'asc'}`
    }
  }

  return {
    skipCount: (page.value - 1) * pageSize.value,
    maxResultCount: pageSize.value,
    startTime: searchForm.startTime || undefined,
    endTime: searchForm.endTime || undefined,
    applicationName: searchForm.applicationName || undefined,
    identity: searchForm.identity || undefined,
    actionName: searchForm.actionName || undefined,
    userName: searchForm.userName || undefined,
    clientId: searchForm.clientId || undefined,
    clientIpAddress: searchForm.clientIpAddress || undefined,
    correlationId: searchForm.correlationId || undefined,
    sorting: sortingString,
  }
}

// 方法
const loadSecurityLogs = async () => {
  try {
    loading.value = true
    const params = buildQueryParams()

    const result = await (viewMode.value === 'current'
      ? securityLogApi.getCurrentUserList(params)
      : securityLogApi.getPage(params))

    securityLogs.value = result.items || []
    total.value = result.totalCount || 0
  } catch (error) {
    console.error('加载安全日志失败:', error)
    toast.add({
      title: $t('page.securityLogs.messages.loadError'),
      color: 'error',
    })
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  page.value = 1
  loadSecurityLogs()
}
const handleReset = () => {
  Object.assign(searchForm, {
    startTime: '',
    endTime: '',
    applicationName: '',
    identity: '',
    actionName: '',
    userName: '',
    clientId: '',
    clientIpAddress: '',
    correlationId: '',
  })
  handleSearch()
}

const handleViewModeChange = (value: 'all' | 'current') => {
  viewMode.value = value
  page.value = 1
  loadSecurityLogs()
}

// 初始化
loadSecurityLogs()
</script>

<template>
  <Page content-class="flex flex-col gap-4">
    <!-- 搜索表单 -->
    <UCard>
      <div class="flex flex-col gap-4">
        <div
          class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between"
        >
          <h2 class="text-foreground text-xl font-semibold">
            {{ $t('page.securityLogs.title') }}
          </h2>
          <div>
            <UButton
              v-for="option in viewModeOptions"
              :key="option.value"
              :variant="viewMode === option.value ? 'solid' : 'ghost'"
              @click="handleViewModeChange(option.value)"
            >
              {{ option.label }}
            </UButton>
          </div>
        </div>

        <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-2 xl:grid-cols-2">
          <!-- 开始时间 -->
          <div class="flex flex-col gap-1.5">
            <label class="text-foreground text-sm font-medium">
              {{ $t('page.securityLogs.search.startTime') }}
            </label>
            <UInput
              v-model="searchForm.startTime"
              type="datetime-local"
              :placeholder="$t('page.securityLogs.search.startTime')"
            />
          </div>

          <!-- 结束时间 -->
          <div class="flex flex-col gap-1.5">
            <label class="text-foreground text-sm font-medium">
              {{ $t('page.securityLogs.search.endTime') }}
            </label>
            <UInput
              v-model="searchForm.endTime"
              type="datetime-local"
              :placeholder="$t('page.securityLogs.search.endTime')"
            />
          </div>

          <!-- 应用名称 -->
          <div class="flex flex-col gap-1.5">
            <label class="text-foreground text-sm font-medium">
              {{ $t('page.securityLogs.search.applicationName') }}
            </label>
            <UInput
              v-model="searchForm.applicationName"
              :placeholder="
                $t('page.securityLogs.search.applicationNamePlaceholder')
              "
            />
          </div>

          <!-- 用户名 -->
          <div class="flex flex-col gap-1.5">
            <label class="text-foreground text-sm font-medium">
              {{ $t('page.securityLogs.search.userName') }}
            </label>
            <UInput
              v-model="searchForm.userName"
              :placeholder="$t('page.securityLogs.search.userNamePlaceholder')"
            />
          </div>

          <!-- 操作 -->
          <div class="flex flex-col gap-1.5">
            <label class="text-foreground text-sm font-medium">
              {{ $t('page.securityLogs.search.action') }}
            </label>
            <UInput
              v-model="searchForm.actionName"
              :placeholder="$t('page.securityLogs.search.actionPlaceholder')"
            />
          </div>

          <!-- 客户端IP -->
          <div class="flex flex-col gap-1.5">
            <label class="text-foreground text-sm font-medium">
              {{ $t('page.securityLogs.search.clientIp') }}
            </label>
            <UInput
              v-model="searchForm.clientIpAddress"
              :placeholder="$t('page.securityLogs.search.clientIpPlaceholder')"
            />
          </div>

          <!-- 搜索按钮 -->
          <div class="md:col-span-2 lg:col-span-3 xl:col-span-4">
            <div class="flex flex-wrap justify-end gap-3">
              <UButton color="primary" @click="handleSearch">
                <template #leading>
                  <UIcon name="i-heroicons-magnifying-glass" />
                </template>
                {{ $t('page.securityLogs.search.search') }}
              </UButton>
              <UButton variant="outline" @click="handleReset">
                <template #leading>
                  <UIcon name="i-heroicons-arrow-path" />
                </template>
                {{ $t('page.securityLogs.search.reset') }}
              </UButton>
            </div>
          </div>
        </div>
      </div>
    </UCard>

    <!-- 数据表格 -->
    <UCard>
      <UTable
        v-model:sorting="sorting"
        :data="securityLogs"
        :columns="columns"
        :loading="loading"
        :empty="$t('page.securityLogs.messages.noData')"
      />

      <!-- 分页 -->
      <div
        v-if="total > 0"
        class="border-border/60 flex justify-end border-t px-6 py-4"
      >
        <UPagination
          v-model="page"
          :page-count="pageSize"
          :total="total"
          @update:model-value="loadSecurityLogs"
        />
      </div>
    </UCard>
  </Page>
</template>

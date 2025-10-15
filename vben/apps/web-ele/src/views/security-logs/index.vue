<script setup lang="ts">
import type { FormInstance } from 'element-plus'

import type {
  IdentitySecurityLog,
  IdentitySecurityLogPageRequest,
} from '@vben/api'

import { computed, reactive, ref } from 'vue'

import { securityLogApi } from '@vben/api'
import { Page } from '@vben/common-ui'
import { useIsMobile } from '@vben/hooks'
import { CiSearch, MdiRefresh } from '@vben/icons'

import { ElMessage } from 'element-plus'

import { $t } from '#/locales'

interface SecurityLogColumn {
  prop: 'action' | keyof IdentitySecurityLog
  label: string
  minWidth?: number
  width?: number
  showOverflowTooltip?: boolean

  sortable?: 'custom'
}

type SortDirection = 'ascending' | 'descending'

interface SortChangeContext {
  prop?: string
  order?: null | SortDirection
}

// 响应式数据
const loading = ref(false)
const { isMobile } = useIsMobile()

// 安全日志列表数据
const securityLogs = ref<IdentitySecurityLog[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)
const sorting = ref<string | undefined>()

// 搜索表单
const searchFormRef = ref<FormInstance>()
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

// 表格列定义
const columns: SecurityLogColumn[] = [
  {
    prop: 'applicationName',
    label: $t('page.securityLogs.table.applicationName'),
    minWidth: 180,
    sortable: 'custom',
  },
  {
    prop: 'identity',
    label: $t('page.securityLogs.table.identity'),
    minWidth: 220,
    sortable: 'custom',
  },
  {
    prop: 'action',
    label: $t('page.securityLogs.table.action'),
    minWidth: 160,
    sortable: 'custom',
  },
  {
    prop: 'userName',
    label: $t('page.securityLogs.table.userName'),
    minWidth: 160,
    sortable: 'custom',
  },
  {
    prop: 'clientId',
    label: $t('page.securityLogs.table.clientId'),
    minWidth: 220,
    sortable: 'custom',
  },
  {
    prop: 'clientIpAddress',
    label: $t('page.securityLogs.table.clientIpAddress'),
    minWidth: 180,
    sortable: 'custom',
  },
  {
    prop: 'browserInfo',
    label: $t('page.securityLogs.table.browserInfo'),
    minWidth: 280,
    showOverflowTooltip: true,
  },
  {
    prop: 'creationTime',
    label: $t('page.securityLogs.table.creationTime'),
    minWidth: 200,
    sortable: 'custom',
  },
]

const tableMinWidth = computed(() =>
  columns.reduce(
    (totalWidth, column) => totalWidth + (column.minWidth ?? column.width ?? 0),
    0,
  ),
)

const serverSortFieldMap = {
  action: 'Action',
  applicationName: 'ApplicationName',
  clientId: 'ClientId',
  clientIpAddress: 'ClientIpAddress',
  creationTime: 'CreationTime',
  identity: 'Identity',
  userName: 'UserName',
} as const

const defaultSort: {
  order: SortDirection
  prop: keyof typeof serverSortFieldMap
} = {
  prop: 'creationTime',
  order: 'descending',
}

const toSortingString = (
  prop: string,
  order: SortDirection,
): string | undefined => {
  const field = serverSortFieldMap[prop as keyof typeof serverSortFieldMap]
  if (!field) return undefined
  const direction = order === 'ascending' ? 'asc' : 'desc'
  return `${field} ${direction}`
}

sorting.value = toSortingString(defaultSort.prop, defaultSort.order)

const showReset = computed(() =>
  Boolean(
    searchForm.startTime ||
      searchForm.endTime ||
      searchForm.applicationName ||
      searchForm.identity ||
      searchForm.actionName ||
      searchForm.userName ||
      searchForm.clientId ||
      searchForm.clientIpAddress ||
      searchForm.correlationId,
  ),
)

// 查看模式切换
const viewMode = ref<'all' | 'current'>('all')

const buildQueryParams = (): IdentitySecurityLogPageRequest => ({
  skipCount: (currentPage.value - 1) * pageSize.value,
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
  sorting: sorting.value,
})

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
    ElMessage.error($t('page.securityLogs.messages.loadError'))
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  currentPage.value = 1
  loadSecurityLogs()
}

const handleReset = () => {
  searchFormRef.value?.resetFields()
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

const handleViewModeChange = () => {
  currentPage.value = 1
  loadSecurityLogs()
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  currentPage.value = 1
  loadSecurityLogs()
}

const handleCurrentChange = (page: number) => {
  currentPage.value = page
  loadSecurityLogs()
}

const handleSortChange = ({ prop, order }: SortChangeContext) => {
  if (!prop || !order) {
    sorting.value = undefined
    currentPage.value = 1
    loadSecurityLogs()
    return
  }

  sorting.value = toSortingString(prop, order)
  currentPage.value = 1
  loadSecurityLogs()
}

const formatDate = (date: string | undefined) => {
  if (!date) return '-'
  return new Date(date).toLocaleString('zh-CN')
}

const formatText = (text: string | undefined, maxLength = 50) => {
  if (!text) return '-'
  if (text.length <= maxLength) return text
  return `${text.slice(0, Math.max(0, maxLength))}...`
}

// 初始化
loadSecurityLogs()
</script>

<template>
  <Page content-class="flex flex-col gap-4">
    <div class="border-border bg-card rounded-xl border px-6 py-4 shadow-sm">
      <div
        class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between"
      >
        <h2 class="text-foreground text-xl font-semibold">
          {{ $t('page.securityLogs.title') }}
        </h2>
        <el-radio-group
          v-model="viewMode"
          class="flex flex-wrap gap-2"
          @change="handleViewModeChange"
        >
          <el-radio-button value="all">
            {{ $t('page.securityLogs.viewMode.all') }}
          </el-radio-button>
          <el-radio-button value="current">
            {{ $t('page.securityLogs.viewMode.current') }}
          </el-radio-button>
        </el-radio-group>
      </div>
    </div>

    <div class="border-border bg-card rounded-xl border px-6 py-4 shadow-sm">
      <div class="flex flex-col gap-4">
        <div class="flex flex-wrap items-center justify-between gap-3">
          <div class="text-muted-foreground flex items-center gap-2 text-sm">
            <CiSearch class="text-lg" />
            {{ $t('page.securityLogs.title') }}
          </div>
          <el-button v-if="showReset" text type="primary" @click="handleReset">
            <template #icon>
              <MdiRefresh />
            </template>
            {{ $t('page.securityLogs.search.reset') }}
          </el-button>
        </div>

        <el-form
          ref="searchFormRef"
          :model="searchForm"
          :inline="!isMobile"
          :label-position="isMobile ? 'top' : 'left'"
          class="grid gap-4 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
        >
          <el-form-item :label="$t('page.securityLogs.search.timeRange')">
            <div class="flex w-full items-center gap-2">
              <el-date-picker
                v-model="searchForm.startTime"
                class="w-full"
                type="datetime"
                :placeholder="$t('page.securityLogs.search.startTime')"
                format="YYYY-MM-DD HH:mm:ss"
                value-format="YYYY-MM-DDTHH:mm:ss.sssZ"
              />
              <span class="text-muted-foreground">-</span>
              <el-date-picker
                v-model="searchForm.endTime"
                class="w-full"
                type="datetime"
                :placeholder="$t('page.securityLogs.search.endTime')"
                format="YYYY-MM-DD HH:mm:ss"
                value-format="YYYY-MM-DDTHH:mm:ss.sssZ"
              />
            </div>
          </el-form-item>

          <el-form-item :label="$t('page.securityLogs.search.applicationName')">
            <el-input
              v-model="searchForm.applicationName"
              :placeholder="
                $t('page.securityLogs.search.applicationNamePlaceholder')
              "
              clearable
            />
          </el-form-item>

          <el-form-item :label="$t('page.securityLogs.search.userName')">
            <el-input
              v-model="searchForm.userName"
              :placeholder="$t('page.securityLogs.search.userNamePlaceholder')"
              clearable
            />
          </el-form-item>

          <el-form-item :label="$t('page.securityLogs.search.action')">
            <el-input
              v-model="searchForm.actionName"
              :placeholder="$t('page.securityLogs.search.actionPlaceholder')"
              clearable
            />
          </el-form-item>

          <el-form-item :label="$t('page.securityLogs.search.clientIp')">
            <el-input
              v-model="searchForm.clientIpAddress"
              :placeholder="$t('page.securityLogs.search.clientIpPlaceholder')"
              clearable
            />
          </el-form-item>

          <div class="md:col-span-2 lg:col-span-3 xl:col-span-4">
            <div class="flex flex-wrap justify-end gap-3">
              <el-button type="primary" @click="handleSearch">
                <template #icon>
                  <CiSearch />
                </template>
                {{ $t('page.securityLogs.search.search') }}
              </el-button>
              <el-button @click="handleReset">
                <template #icon>
                  <MdiRefresh />
                </template>
                {{ $t('page.securityLogs.search.reset') }}
              </el-button>
            </div>
          </div>
        </el-form>
      </div>
    </div>

    <div class="border-border bg-card rounded-xl border shadow-sm">
      <div class="overflow-x-auto">
        <el-table
          v-loading="loading"
          :data="securityLogs"
          border
          stripe
          :size="isMobile ? 'small' : 'default'"
          :row-key="(row) => row.id"
          class="w-full"
          :style="{ minWidth: `${tableMinWidth}px` }"
          :default-sort="defaultSort"
          @sort-change="handleSortChange"
        >
          <el-table-column
            v-for="column in columns"
            :key="column.prop"
            :prop="column.prop"
            :label="column.label"
            :min-width="column.minWidth"
            :show-overflow-tooltip="column.showOverflowTooltip"
            :sortable="column.sortable ?? false"
          >
            <template v-if="column.prop === 'creationTime'" #default="{ row }">
              {{ formatDate(row.creationTime) }}
            </template>
            <template
              v-else-if="column.prop === 'browserInfo'"
              #default="{ row }"
            >
              <el-tooltip
                v-if="row.browserInfo && row.browserInfo.length > 50"
                :content="row.browserInfo"
                placement="top"
              >
                <span>{{ formatText(row.browserInfo, 50) }}</span>
              </el-tooltip>
              <span v-else>{{ row.browserInfo || '-' }}</span>
            </template>
            <template v-else #default="{ row }">
              {{ row[column.prop as keyof IdentitySecurityLog] || '-' }}
            </template>
          </el-table-column>
        </el-table>
      </div>
      <div class="border-border/60 flex justify-end border-t px-6 py-4">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="total"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="handlePageSizeChange"
          @current-change="handleCurrentChange"
        />
      </div>
    </div>
  </Page>
</template>

<style scoped>
@media (max-width: 768px) {
  .el-radio-group {
    width: 100%;
  }

  .el-radio-button {
    flex: 1;
  }
}
</style>

<script setup lang="ts">
import type { FormInstance } from 'element-plus'

import type {
  IdentitySecurityLog,
  IdentitySecurityLogPageRequest,
} from '@vben/api'

import { reactive, ref, watch } from 'vue'

import { securityLogApi } from '@vben/api'
import { Page } from '@vben/common-ui'

import { ElMessage } from 'element-plus'

import { $t } from '#/locales'

// 响应式数据
const loading = ref(false)

// 安全日志列表数据
const securityLogs = ref<IdentitySecurityLog[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)

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
const columns = [
  {
    prop: 'applicationName',
    label: $t('page.securityLogs.table.applicationName'),
    width: 120,
  },
  {
    prop: 'identity',
    label: $t('page.securityLogs.table.identity'),
    width: 150,
  },
  { prop: 'action', label: $t('page.securityLogs.table.action'), width: 120 },
  {
    prop: 'userName',
    label: $t('page.securityLogs.table.userName'),
    width: 120,
  },
  {
    prop: 'clientId',
    label: $t('page.securityLogs.table.clientId'),
    width: 150,
  },
  {
    prop: 'clientIpAddress',
    label: $t('page.securityLogs.table.clientIpAddress'),
    width: 130,
  },
  {
    prop: 'browserInfo',
    label: $t('page.securityLogs.table.browserInfo'),
    width: 200,
    showOverflowTooltip: true,
  },
  {
    prop: 'creationTime',
    label: $t('page.securityLogs.table.creationTime'),
    width: 160,
  },
]

// 查看模式切换
const viewMode = ref<'all' | 'current'>('all')

// 方法
const loadSecurityLogs = async () => {
  try {
    loading.value = true
    const params: IdentitySecurityLogPageRequest = {
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
    }

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
  if (searchFormRef.value) {
    searchFormRef.value.resetFields()
  }
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

const formatDate = (date: string | undefined) => {
  if (!date) return '-'
  return new Date(date).toLocaleString('zh-CN')
}

const formatText = (text: string | undefined, maxLength = 50) => {
  if (!text) return '-'
  if (text.length <= maxLength) return text
  return `${text.slice(0, Math.max(0, maxLength))}...`
}

// 监听分页变化
watch([currentPage, pageSize], () => {
  loadSecurityLogs()
})

// 初始化
loadSecurityLogs()
</script>

<template>
  <Page class="security-logs">
    <!-- 页面标题和模式切换 -->
    <el-card class="header-card" shadow="never">
      <div class="header-content">
        <h2>{{ $t('page.securityLogs.title') }}</h2>
        <el-radio-group v-model="viewMode" @change="handleViewModeChange">
          <el-radio-button value="all">
            {{ $t('page.securityLogs.viewMode.all') }}
          </el-radio-button>
          <el-radio-button value="current">
            {{ $t('page.securityLogs.viewMode.current') }}
          </el-radio-button>
        </el-radio-group>
      </div>
    </el-card>

    <!-- 搜索区域 -->
    <el-card class="search-card" shadow="never">
      <el-form ref="searchFormRef" :model="searchForm" inline>
        <el-form-item
          :label="$t('page.securityLogs.search.timeRange')"
          prop="startTime"
        >
          <el-date-picker
            v-model="searchForm.startTime"
            type="datetime"
            :placeholder="$t('page.securityLogs.search.startTime')"
            format="YYYY-MM-DD HH:mm:ss"
            value-format="YYYY-MM-DDTHH:mm:ss.sssZ"
            style="width: 180px"
          />
          <span class="mx-2">-</span>
          <el-date-picker
            v-model="searchForm.endTime"
            type="datetime"
            :placeholder="$t('page.securityLogs.search.endTime')"
            format="YYYY-MM-DD HH:mm:ss"
            value-format="YYYY-MM-DDTHH:mm:ss.sssZ"
            style="width: 180px"
          />
        </el-form-item>

        <el-form-item
          :label="$t('page.securityLogs.search.applicationName')"
          prop="applicationName"
        >
          <el-input
            v-model="searchForm.applicationName"
            :placeholder="
              $t('page.securityLogs.search.applicationNamePlaceholder')
            "
            clearable
            style="width: 200px"
          />
        </el-form-item>

        <el-form-item
          :label="$t('page.securityLogs.search.userName')"
          prop="userName"
        >
          <el-input
            v-model="searchForm.userName"
            :placeholder="$t('page.securityLogs.search.userNamePlaceholder')"
            clearable
            style="width: 200px"
          />
        </el-form-item>

        <el-form-item
          :label="$t('page.securityLogs.search.action')"
          prop="actionName"
        >
          <el-input
            v-model="searchForm.actionName"
            :placeholder="$t('page.securityLogs.search.actionPlaceholder')"
            clearable
            style="width: 200px"
          />
        </el-form-item>

        <el-form-item
          :label="$t('page.securityLogs.search.clientIp')"
          prop="clientIpAddress"
        >
          <el-input
            v-model="searchForm.clientIpAddress"
            :placeholder="$t('page.securityLogs.search.clientIpPlaceholder')"
            clearable
            style="width: 200px"
          />
        </el-form-item>

        <el-form-item>
          <el-button type="primary" @click="handleSearch">
            <span class="icon-[ci:search] mr-1 text-lg"></span>
            {{ $t('page.securityLogs.search.search') }}
          </el-button>
          <el-button @click="handleReset">
            <span class="icon-[ci:refresh] mr-1 text-lg"></span>
            {{ $t('page.securityLogs.search.reset') }}
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 安全日志列表 -->
    <el-card class="table-card" shadow="never">
      <el-table
        v-loading="loading"
        :data="securityLogs"
        stripe
        style="width: 100%"
        :scroll="{ x: 1200 }"
      >
        <el-table-column
          v-for="column in columns"
          :key="column.prop"
          :prop="column.prop"
          :label="column.label"
          :width="column.width"
          :show-overflow-tooltip="column.showOverflowTooltip"
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
            {{ row[column.prop] || '-' }}
          </template>
        </el-table-column>
      </el-table>

      <!-- 分页 -->
      <div class="pagination-container">
        <el-pagination
          :current-page="currentPage"
          :page-size="pageSize"
          :page-sizes="[10, 20, 50, 100]"
          :total="total"
          layout="total, sizes, prev, pager, next, jumper"
          @size-change="
            (size: number) => {
              pageSize = size
              loadSecurityLogs()
            }
          "
          @current-change="
            (page: number) => {
              currentPage = page
              loadSecurityLogs()
            }
          "
        />
      </div>
    </el-card>
  </Page>
</template>

<style scoped>
.header-card,
.search-card,
.table-card {
  margin-bottom: 20px;
}

.header-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-content h2 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
}

.search-card .el-form {
  display: flex;
  flex-wrap: wrap;
  gap: 16px;
}

.search-card .el-form-item {
  margin-right: 0;
  margin-bottom: 16px;
}

.pagination-container {
  display: flex;
  justify-content: center;
  margin-top: 20px;
}

.mx-2 {
  margin: 0 8px;
}

@media (max-width: 768px) {
  .security-logs {
    padding: 10px;
  }

  .header-content {
    flex-direction: column;
    gap: 16px;
    align-items: flex-start;
  }

  .search-card .el-form {
    flex-direction: column;
  }

  .search-card .el-form-item {
    margin-right: 0;
    margin-bottom: 10px;
  }
}
</style>

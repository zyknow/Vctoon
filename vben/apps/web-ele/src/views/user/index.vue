<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import type {
  CreateIdentityUserInput,
  IdentityRole,
  IdentityUser,
  UpdateIdentityUserInput,
} from '@vben/api'

import { reactive, ref } from 'vue'

import { userApi } from '@vben/api'
import { Page } from '@vben/common-ui'
import { useIsMobile } from '@vben/hooks'
import { CiAddPlus } from '@vben/icons'

import { ElMessage, ElMessageBox } from 'element-plus'

import { $t } from '#/locales'

type SortDirection = 'ascending' | 'descending'

interface UserTableColumn {
  prop: string
  label: string
  width?: number
  minWidth?: number
  fixed?: 'right'
  sortable?: 'custom'
  sortField?: string
  align?: 'center' | 'left' | 'right'
}

interface SortChangeContext {
  prop?: string
  order?: null | SortDirection
}

const serverSortFieldMap = {
  userName: 'UserName',
  name: 'Name',
  email: 'Email',
  phoneNumber: 'PhoneNumber',
  isActive: 'IsActive',
  emailConfirmed: 'EmailConfirmed',
  phoneNumberConfirmed: 'PhoneNumberConfirmed',
  creationTime: 'CreationTime',
} as const

interface TableSortState {
  prop: keyof typeof serverSortFieldMap
  order: SortDirection
}

const defaultSort: TableSortState = {
  prop: 'creationTime',
  order: 'descending',
}

const toSortingString = (
  prop: string,
  order: SortDirection,
): string | undefined => {
  const target = serverSortFieldMap[prop as keyof typeof serverSortFieldMap]
  if (!target) return undefined
  const direction = order === 'ascending' ? 'asc' : 'desc'
  return `${target} ${direction}`
}

// 响应式数据
const loading = ref(false)
const { isMobile } = useIsMobile()
// 用户列表数据
const users = ref<IdentityUser[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)
const sorting = ref<string | undefined>(
  toSortingString(defaultSort.prop, defaultSort.order),
)

// 搜索表单
const searchForm = reactive({
  filter: '',
})

// 表格列定义
const columns: UserTableColumn[] = [
  {
    prop: 'userName',
    label: $t('page.user.table.userName'),
    minWidth: 160,
    sortable: 'custom',
    sortField: serverSortFieldMap.userName,
  },
  {
    prop: 'name',
    label: $t('page.user.table.name'),
    minWidth: 160,
    sortable: 'custom',
    sortField: serverSortFieldMap.name,
  },
  {
    prop: 'email',
    label: $t('page.user.table.email'),
    minWidth: 220,
    sortable: 'custom',
    sortField: serverSortFieldMap.email,
  },
  {
    prop: 'phoneNumber',
    label: $t('page.user.table.phoneNumber'),
    minWidth: 160,
    sortable: 'custom',
    sortField: serverSortFieldMap.phoneNumber,
  },
  {
    prop: 'isActive',
    label: $t('page.user.table.status'),
    width: 120,
    sortable: 'custom',
    sortField: serverSortFieldMap.isActive,
  },
  {
    prop: 'emailConfirmed',
    label: $t('page.user.table.emailConfirmed'),
    width: 140,
    sortable: 'custom',
    sortField: serverSortFieldMap.emailConfirmed,
  },
  {
    prop: 'phoneNumberConfirmed',
    label: $t('page.user.table.phoneNumberConfirmed'),
    width: 160,
    sortable: 'custom',
    sortField: serverSortFieldMap.phoneNumberConfirmed,
  },
  {
    prop: 'creationTime',
    label: $t('page.user.table.creationTime'),
    minWidth: 200,
    sortable: 'custom',
    sortField: serverSortFieldMap.creationTime,
  },
  {
    prop: 'actions',
    label: $t('page.user.table.actions'),
    width: 220,
    fixed: 'right',
    align: 'center',
  },
]

// 对话框状态
const createDialogVisible = ref(false)
const editDialogVisible = ref(false)
const roleDialogVisible = ref(false)
const currentUser = ref<IdentityUser | null>(null)

// 创建用户表单
const createFormRef = ref<FormInstance>()
const createForm = reactive<CreateIdentityUserInput>({
  userName: '',
  name: '', // 可选字段，默认为空
  email: '',
  phoneNumber: '',
  password: '',
  isActive: true,
  lockoutEnabled: true,
  emailReminderEnabled: true, // 根据类型定义，这个字段在User中是必填的
  roleNames: [],
})

// 编辑用户表单
const editFormRef = ref<FormInstance>()
const editForm = reactive<UpdateIdentityUserInput>({
  userName: '',
  name: '', // 可选字段，默认为空
  email: '',
  phoneNumber: '',
  password: '', // 编辑时密码是可选的
  isActive: true,
  lockoutEnabled: true,
  emailReminderEnabled: true, // 根据类型定义，这个字段在User中是必填的
  roleNames: [],
})

// 角色管理
const availableRoles = ref<IdentityRole[]>([])
const userRoles = ref<IdentityRole[]>([])
const selectedRoles = ref<string[]>([])

// 表单验证规则
const createRules: FormRules<CreateIdentityUserInput> = {
  userName: [
    {
      required: true,
      message: $t('page.user.create.validation.userNameRequired'),
      trigger: 'blur',
    },
    {
      min: 3,
      max: 50,
      message: $t('page.user.create.validation.userNameLength'),
      trigger: 'blur',
    },
  ],
  email: [
    {
      required: true,
      message: $t('page.user.create.validation.emailRequired'),
      trigger: 'blur',
    },
    {
      type: 'email',
      message: $t('page.user.create.validation.emailFormat'),
      trigger: 'blur',
    },
  ],
  phoneNumber: [
    {
      pattern: /^1[3-9]\d{9}$/,
      message: $t('page.user.create.validation.phoneFormat'),
      trigger: 'blur',
    },
  ],
  password: [
    {
      required: true,
      message: $t('page.user.create.validation.passwordRequired'),
      trigger: 'blur',
    },
    {
      min: 6,
      message: $t('page.user.create.validation.passwordLength'),
      trigger: 'blur',
    },
  ],
  // name 字段在类型定义中是可选的，移除必填验证
}

const editRules: FormRules<UpdateIdentityUserInput> = {
  userName: [
    {
      required: true,
      message: $t('page.user.create.validation.userNameRequired'),
      trigger: 'blur',
    },
    {
      min: 3,
      max: 50,
      message: $t('page.user.create.validation.userNameLength'),
      trigger: 'blur',
    },
  ],
  email: [
    {
      required: true,
      message: $t('page.user.create.validation.emailRequired'),
      trigger: 'blur',
    },
    {
      type: 'email',
      message: $t('page.user.create.validation.emailFormat'),
      trigger: 'blur',
    },
  ],
  phoneNumber: [
    {
      pattern: /^1[3-9]\d{9}$/,
      message: $t('page.user.create.validation.phoneFormat'),
      trigger: 'blur',
    },
  ],
  name: [
    {
      required: true,
      message: $t('page.user.create.validation.nameRequired'),
      trigger: 'blur',
    },
  ],
  // password 字段在编辑时是可选的，如果为空则不更新密码
}

// 方法
const loadUsers = async () => {
  try {
    loading.value = true
    const params: FilterPageRequest = {
      skipCount: (currentPage.value - 1) * pageSize.value,
      maxResultCount: pageSize.value,
      filter: searchForm.filter || undefined,
    }
    if (sorting.value) {
      params.sorting = sorting.value
    }

    const result = await userApi.getPage(params)
    users.value = result.items || []
    total.value = result.totalCount || 0
  } catch (error) {
    console.error('加载用户列表失败:', error)
    ElMessage.error($t('page.user.messages.loadUsersError'))
  } finally {
    loading.value = false
  }
}

const handleSortChange = ({ prop, order }: SortChangeContext) => {
  if (!prop || !order) {
    sorting.value = undefined
    if (currentPage.value !== 1) {
      currentPage.value = 1
    }
    loadUsers()
    return
  }

  sorting.value = toSortingString(prop, order)
  if (currentPage.value !== 1) {
    currentPage.value = 1
  }
  loadUsers()
}

const handlePageSizeChange = (size: number) => {
  pageSize.value = size
  if (currentPage.value !== 1) {
    currentPage.value = 1
  }
  loadUsers()
}

const handleCurrentChange = (page: number) => {
  currentPage.value = page
  loadUsers()
}

const loadAvailableRoles = async () => {
  try {
    const result = await userApi.getAssignableRoles()
    availableRoles.value = result.items || []
  } catch (error) {
    console.error('加载角色列表失败:', error)
    ElMessage.error($t('page.user.messages.loadRolesError'))
  }
}

const loadUserRoles = async (userId: string) => {
  try {
    const result = await userApi.getUserRoles(userId)
    userRoles.value = result.items || []
    selectedRoles.value = userRoles.value.map((role: IdentityRole) => role.name)
  } catch (error) {
    console.error('加载用户角色失败:', error)
    ElMessage.error($t('page.user.messages.loadUserRolesError'))
  }
}

const handleSearch = () => {
  currentPage.value = 1
  loadUsers()
}

const handleReset = () => {
  Object.assign(searchForm, {
    filter: '',
  })
  handleSearch()
}

const handleCreate = () => {
  Object.assign(createForm, {
    userName: '',
    name: '', // 可选字段
    email: '',
    phoneNumber: '',
    password: '',
    isActive: true,
    lockoutEnabled: true,
    emailReminderEnabled: true,
    roleNames: [],
  })
  createDialogVisible.value = true
}

const handleEdit = (user: IdentityUser) => {
  currentUser.value = user
  Object.assign(editForm, {
    userName: user.userName || '',
    name: user.name || '', // 可选字段
    email: user.email || '',
    phoneNumber: user.phoneNumber || '',
    password: '', // 编辑时密码是可选的，如果为空则不更新
    isActive: user.isActive ?? true,
    lockoutEnabled: true,
    emailReminderEnabled: user.emailReminderEnabled ?? true,
    roleNames: [],
  })
  editDialogVisible.value = true
}

const handleRoleManage = async (user: IdentityUser) => {
  currentUser.value = user
  await loadUserRoles(user.id!)
  roleDialogVisible.value = true
}

const handleDelete = async (user: IdentityUser) => {
  try {
    await ElMessageBox.confirm(
      $t('page.user.actions.deleteConfirm', { userName: user.userName }),
      $t('page.user.actions.deleteTitle'),
      {
        confirmButtonText: $t('common.confirm'),
        cancelButtonText: $t('common.cancel'),
        type: 'warning',
      },
    )

    await userApi.delete(user.id!)
    ElMessage.success($t('page.user.actions.deleteSuccess'))
    loadUsers()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('删除用户失败:', error)
      ElMessage.error($t('page.user.actions.deleteError'))
    }
  }
}

const handleCreateSubmit = async () => {
  if (!createFormRef.value) return

  const valid = await createFormRef.value.validate()
  if (!valid) return

  try {
    await userApi.create(createForm)
    ElMessage.success($t('page.user.create.messages.success'))
    createDialogVisible.value = false
    loadUsers()
  } catch (error) {
    console.error('创建用户失败:', error)
    ElMessage.error($t('page.user.create.messages.error'))
  }
}

const handleEditSubmit = async () => {
  if (!editFormRef.value || !currentUser.value) return

  const valid = await editFormRef.value.validate()
  if (!valid) return

  try {
    const updateData = { ...editForm }
    // 如果密码为空，则不更新密码
    if (!updateData.password) {
      delete updateData.password
    }

    await userApi.update(updateData, currentUser.value.id!)
    ElMessage.success($t('page.user.edit.messages.success'))
    editDialogVisible.value = false
    loadUsers()
  } catch (error) {
    console.error('更新用户失败:', error)
    ElMessage.error($t('page.user.edit.messages.error'))
  }
}

const handleRoleSubmit = async () => {
  if (!currentUser.value) return

  try {
    await userApi.updateUserRoles(currentUser.value.id!, selectedRoles.value)
    ElMessage.success($t('page.user.role.messages.success'))
    roleDialogVisible.value = false
    loadUsers()
  } catch (error) {
    console.error('更新用户角色失败:', error)
    ElMessage.error($t('page.user.role.messages.error'))
  }
}

const formatDate = (date: string | undefined) => {
  if (!date) return '-'
  return new Date(date).toLocaleString('zh-CN')
}

const formatStatus = (isActive: boolean | undefined) => {
  return isActive
    ? $t('page.user.status.enabled')
    : $t('page.user.status.disabled')
}

const formatConfirmed = (confirmed: boolean | undefined) => {
  return confirmed ? $t('page.user.status.yes') : $t('page.user.status.no')
}

// 初始化
loadUsers()
loadAvailableRoles()
</script>

<template>
  <Page content-class="flex flex-col gap-4">
    <div class="border-border bg-card rounded-xl border px-6 py-4 shadow-sm">
      <div
        class="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between"
      >
        <el-form
          :model="searchForm"
          :inline="!isMobile"
          :label-position="isMobile ? 'top' : 'left'"
          class="flex flex-wrap items-end gap-4"
        >
          <el-form-item :label="$t('page.user.search.filter')" class="!mb-0">
            <el-input
              v-model="searchForm.filter"
              :placeholder="$t('page.user.search.filterPlaceholder')"
              clearable
              class="w-full sm:w-72"
              @keyup.enter="handleSearch"
            />
          </el-form-item>
          <el-form-item class="!mb-0">
            <div class="flex flex-wrap gap-2">
              <el-button type="primary" @click="handleSearch">
                {{ $t('page.user.search.search') }}
              </el-button>
              <el-button @click="handleReset">
                {{ $t('page.user.search.reset') }}
              </el-button>
            </div>
          </el-form-item>
        </el-form>
        <div class="flex justify-end">
          <el-button type="primary" @click="handleCreate">
            <template #icon>
              <CiAddPlus class="text-lg" />
            </template>
            {{ $t('page.user.actions.create') }}
          </el-button>
        </div>
      </div>
    </div>
    <div class="border-border bg-card rounded-xl border shadow-sm">
      <div class="overflow-x-auto">
        <el-table
          v-loading="loading"
          :data="users"
          border
          stripe
          class="min-w-[960px]"
          :size="isMobile ? 'small' : 'default'"
          :row-key="(row) => row.id"
          :default-sort="defaultSort"
          @sort-change="handleSortChange"
        >
          <el-table-column
            v-for="column in columns"
            :key="column.prop"
            :prop="column.prop"
            :label="column.label"
            :width="column.width"
            :min-width="column.minWidth"
            :fixed="column.fixed"
            :sortable="column.sortable ?? false"
            :align="column.align || 'left'"
            :show-overflow-tooltip="column.prop !== 'actions'"
          >
            <template v-if="column.prop === 'isActive'" #default="{ row }">
              <el-tag :type="row.isActive ? 'success' : 'danger'">
                {{ formatStatus(row.isActive) }}
              </el-tag>
            </template>
            <template
              v-else-if="column.prop === 'emailConfirmed'"
              #default="{ row }"
            >
              <el-tag :type="row.emailConfirmed ? 'success' : 'info'">
                {{ formatConfirmed(row.emailConfirmed) }}
              </el-tag>
            </template>
            <template
              v-else-if="column.prop === 'phoneNumberConfirmed'"
              #default="{ row }"
            >
              <el-tag :type="row.phoneNumberConfirmed ? 'success' : 'info'">
                {{ formatConfirmed(row.phoneNumberConfirmed) }}
              </el-tag>
            </template>
            <template
              v-else-if="column.prop === 'creationTime'"
              #default="{ row }"
            >
              {{ formatDate(row.creationTime) }}
            </template>
            <template v-else-if="column.prop === 'actions'" #default="{ row }">
              <div class="flex flex-wrap justify-center gap-2">
                <el-button type="primary" size="small" @click="handleEdit(row)">
                  {{ $t('page.user.table.edit') }}
                </el-button>
                <el-button
                  type="warning"
                  size="small"
                  @click="handleRoleManage(row)"
                >
                  {{ $t('page.user.table.role') }}
                </el-button>
                <el-button
                  type="danger"
                  size="small"
                  @click="handleDelete(row)"
                >
                  {{ $t('page.user.table.delete') }}
                </el-button>
              </div>
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

    <el-dialog
      v-model="createDialogVisible"
      :title="$t('page.user.create.title')"
      :close-on-click-modal="false"
      :fullscreen="isMobile"
      width="600px"
    >
      <el-form
        ref="createFormRef"
        :model="createForm"
        :rules="createRules"
        label-width="100px"
      >
        <el-form-item
          :label="$t('page.user.create.fields.userName')"
          prop="userName"
        >
          <el-input
            v-model="createForm.userName"
            :placeholder="$t('page.user.create.placeholders.userName')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.create.fields.name')" prop="name">
          <el-input
            v-model="createForm.name"
            :placeholder="$t('page.user.create.placeholders.name')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.create.fields.email')" prop="email">
          <el-input
            v-model="createForm.email"
            :placeholder="$t('page.user.create.placeholders.email')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.create.fields.phoneNumber')">
          <el-input
            v-model="createForm.phoneNumber"
            :placeholder="$t('page.user.create.placeholders.phoneNumber')"
          />
        </el-form-item>
        <el-form-item
          :label="$t('page.user.create.fields.password')"
          prop="password"
        >
          <el-input
            v-model="createForm.password"
            type="password"
            :placeholder="$t('page.user.create.placeholders.password')"
            show-password
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.create.fields.status')">
          <el-switch
            v-model="createForm.isActive"
            :active-text="$t('page.user.status.enabled')"
            :inactive-text="$t('page.user.status.disabled')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.create.fields.lockoutEnabled')">
          <el-switch
            v-model="createForm.lockoutEnabled"
            :active-text="$t('page.user.status.enabled')"
            :inactive-text="$t('page.user.status.disabled')"
          />
        </el-form-item>
        <el-form-item
          :label="$t('page.user.create.fields.emailReminderEnabled')"
        >
          <el-switch
            v-model="createForm.emailReminderEnabled"
            :active-text="$t('page.user.status.enabled')"
            :inactive-text="$t('page.user.status.disabled')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.create.fields.roles')">
          <el-select
            v-model="createForm.roleNames"
            multiple
            :multiple-limit="1"
            :placeholder="$t('page.user.create.placeholders.roles')"
            style="width: 100%"
          >
            <el-option
              v-for="role in availableRoles"
              :key="role.id"
              :label="role.name"
              :value="role.name"
            />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="createDialogVisible = false">
          {{ $t('page.user.create.actions.cancel') }}
        </el-button>
        <el-button type="primary" @click="handleCreateSubmit">
          {{ $t('page.user.create.actions.confirm') }}
        </el-button>
      </template>
    </el-dialog>

    <el-dialog
      v-model="editDialogVisible"
      :title="$t('page.user.edit.title')"
      :close-on-click-modal="false"
      :fullscreen="isMobile"
      width="600px"
    >
      <el-form
        ref="editFormRef"
        :model="editForm"
        :rules="editRules"
        label-width="100px"
      >
        <el-form-item
          :label="$t('page.user.edit.fields.userName')"
          prop="userName"
        >
          <el-input
            v-model="editForm.userName"
            :placeholder="$t('page.user.edit.placeholders.userName')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.edit.fields.name')" prop="name">
          <el-input
            v-model="editForm.name"
            :placeholder="$t('page.user.edit.placeholders.name')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.edit.fields.email')" prop="email">
          <el-input
            v-model="editForm.email"
            :placeholder="$t('page.user.edit.placeholders.email')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.edit.fields.phoneNumber')">
          <el-input
            v-model="editForm.phoneNumber"
            :placeholder="$t('page.user.edit.placeholders.phoneNumber')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.edit.fields.password')">
          <el-input
            v-model="editForm.password"
            type="password"
            :placeholder="$t('page.user.edit.placeholders.password')"
            show-password
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.edit.fields.status')">
          <el-switch
            v-model="editForm.isActive"
            :active-text="$t('page.user.status.enabled')"
            :inactive-text="$t('page.user.status.disabled')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.edit.fields.lockoutEnabled')">
          <el-switch
            v-model="editForm.lockoutEnabled"
            :active-text="$t('page.user.status.enabled')"
            :inactive-text="$t('page.user.status.disabled')"
          />
        </el-form-item>
        <el-form-item :label="$t('page.user.edit.fields.emailReminderEnabled')">
          <el-switch
            v-model="editForm.emailReminderEnabled"
            :active-text="$t('page.user.status.enabled')"
            :inactive-text="$t('page.user.status.disabled')"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="editDialogVisible = false">
          {{ $t('page.user.edit.actions.cancel') }}
        </el-button>
        <el-button type="primary" @click="handleEditSubmit">
          {{ $t('page.user.edit.actions.confirm') }}
        </el-button>
      </template>
    </el-dialog>

    <el-dialog
      v-model="roleDialogVisible"
      :title="$t('page.user.role.title')"
      :close-on-click-modal="false"
      :fullscreen="isMobile"
      width="500px"
    >
      <div class="space-y-4">
        <p class="text-foreground text-base font-medium">
          {{ $t('page.user.role.userInfo') }}
          <span class="text-primary ml-1">{{ currentUser?.userName }}</span>
        </p>
        <el-divider class="!my-0" />
        <el-form label-width="120px">
          <el-form-item :label="$t('page.user.role.assignRoles')" class="!mb-0">
            <el-select
              v-model="selectedRoles"
              multiple
              :multiple-limit="1"
              :placeholder="$t('page.user.role.placeholders.roles')"
              style="width: 100%"
            >
              <el-option
                v-for="role in availableRoles"
                :key="role.id"
                :label="role.name"
                :value="role.name"
              />
            </el-select>
          </el-form-item>
        </el-form>
      </div>
      <template #footer>
        <el-button @click="roleDialogVisible = false">
          {{ $t('page.user.role.actions.cancel') }}
        </el-button>
        <el-button type="primary" @click="handleRoleSubmit">
          {{ $t('page.user.role.actions.confirm') }}
        </el-button>
      </template>
    </el-dialog>
  </Page>
</template>

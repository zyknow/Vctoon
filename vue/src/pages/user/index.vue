<script setup lang="ts">
import { type Component, h, reactive, ref, resolveComponent } from 'vue'
import type { TableColumn } from '@nuxt/ui'

import type { IdentityRole } from '@/api/http/role'
import type { IdentityUser } from '@/api/http/user'
import { userApi } from '@/api/http/user'
import ConfirmModal from '@/components/overlays/ConfirmModal.vue'
import CreateUserModal from '@/components/overlays/CreateUserModal.vue'
import EditUserModal from '@/components/overlays/EditUserModal.vue'
import UserRoleModal from '@/components/overlays/UserRoleModal.vue'
import { $t, formatDateTime } from '@/locales/i18n'
import { createSortableHeader } from '@/utils/tables/table'

const UIcon = resolveComponent('UIcon')
const UButton = resolveComponent('UButton') as Component

// 响应式数据
const loading = ref(false)
const toast = useToast()
const overlay = useOverlay()

// 取消 useI18n 的直接使用，统一使用项目封装的 $t，确保语言切换时响应式

// 用户列表数据
const users = ref<IdentityUser[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)
const sorting = ref([
  {
    id: 'CreationTime',
    desc: true,
  },
])

// 搜索表单
const searchForm = reactive({
  filter: '',
})

// 角色管理
const availableRoles = ref<IdentityRole[]>([])

// Modal 实例
const confirmModal = overlay.create(ConfirmModal)
const createUserModal = overlay.create(CreateUserModal)
const editUserModal = overlay.create(EditUserModal)
const userRoleModal = overlay.create(UserRoleModal)

// 创建可排序列头的辅助函数（与安全日志页面保持一致，使用 i18n key）
// 使用通用工具中的 createSortableHeader

// 表格列定义
const columns: TableColumn<IdentityUser>[] = [
  {
    accessorKey: 'userName',
    header: createSortableHeader('page.user.table.userName'),
    enableSorting: true,
  },
  {
    accessorKey: 'name',
    header: createSortableHeader('page.user.table.name'),
    enableSorting: true,
  },
  {
    accessorKey: 'email',
    header: createSortableHeader('page.user.table.email'),
    enableSorting: true,
  },
  {
    accessorKey: 'phoneNumber',
    header: createSortableHeader('page.user.table.phoneNumber'),
    enableSorting: true,
  },
  {
    accessorKey: 'isActive',
    header: createSortableHeader('page.user.table.status'),
    enableSorting: true,
    cell: ({ row }) => {
      return h(
        'div',
        { class: 'flex items-center justify-center' },
        h(UIcon as any, {
          name: row.original.isActive
            ? 'i-lucide-check-circle'
            : 'i-lucide-x-circle',
          class: row.original.isActive
            ? 'text-success h-5 w-5'
            : 'text-error h-5 w-5',
        }),
      )
    },
  },
  {
    accessorKey: 'emailConfirmed',
    header: createSortableHeader('page.user.table.emailConfirmed'),
    enableSorting: true,
    cell: ({ row }) => {
      return h(
        'div',
        { class: 'flex items-center justify-center' },
        h(UIcon as any, {
          name: row.original.emailConfirmed
            ? 'i-lucide-circle-check'
            : 'i-lucide-circle-x',
          class: row.original.emailConfirmed
            ? 'text-success h-5 w-5'
            : 'text-muted-foreground h-5 w-5',
        }),
      )
    },
  },
  {
    accessorKey: 'phoneNumberConfirmed',
    header: createSortableHeader('page.user.table.phoneNumberConfirmed'),
    enableSorting: true,
    cell: ({ row }) => {
      return h(
        'div',
        { class: 'flex items-center justify-center' },
        h(UIcon as any, {
          name: row.original.phoneNumberConfirmed
            ? 'i-lucide-circle-check'
            : 'i-lucide-circle-x',
          class: row.original.phoneNumberConfirmed
            ? 'text-success h-5 w-5'
            : 'text-muted-foreground h-5 w-5',
        }),
      )
    },
  },
  {
    accessorKey: 'creationTime',
    // 服务端字段为 PascalCase，这里指定列 id 以保持与后端排序字段一致
    id: 'CreationTime',
    header: createSortableHeader('page.user.table.creationTime'),
    enableSorting: true,
    cell: ({ row }) => {
      return h(
        'span',
        { class: 'text-muted-foreground' },
        formatDateTime(row.original.creationTime, 'long'),
      )
    },
  },
  {
    id: 'actions',
    header: $t('page.user.table.actions'),
    cell: ({ row }) => {
      return h('div', { class: 'flex flex-wrap gap-2' }, [
        h(
          UButton as any,
          {
            size: 'xs',
            onClick: () => handleEdit(row.original),
          },
          () => $t('page.user.table.edit'),
        ),
        h(
          UButton as any,
          {
            size: 'xs',
            color: 'warning',
            onClick: () => handleRoleManage(row.original),
          },
          () => $t('page.user.table.role'),
        ),
        h(
          UButton as any,
          {
            size: 'xs',
            color: 'error',
            onClick: () => handleDelete(row.original),
          },
          () => $t('page.user.table.delete'),
        ),
      ])
    },
  },
]

// 方法
const loadUsers = async () => {
  try {
    loading.value = true
    const params: FilterPageRequest = {
      skipCount: (currentPage.value - 1) * pageSize.value,
      maxResultCount: pageSize.value,
      filter: searchForm.filter || undefined,
    }

    // 处理排序
    if (sorting.value.length > 0) {
      const sort = sorting.value[0]
      const serverField = sort.id
      if (serverField) {
        const direction = sort.desc ? 'desc' : 'asc'
        params.sorting = `${serverField} ${direction}`
      }
    }

    const result = await userApi.getPage(params)
    users.value = result.items || []
    total.value = result.totalCount || 0
  } catch (error) {
    console.error('加载用户列表失败:', error)
    toast.add({
      title: $t('page.user.messages.loadUsersError'),
      color: 'error',
    })
  } finally {
    loading.value = false
  }
}

const loadAvailableRoles = async () => {
  try {
    const result = await userApi.getAssignableRoles()
    availableRoles.value = result.items || []
  } catch (error) {
    console.error('加载角色列表失败:', error)
    toast.add({
      title: $t('page.user.messages.loadRolesError'),
      color: 'error',
    })
  }
}

const handleSearch = () => {
  currentPage.value = 1
  loadUsers()
}

const handleReset = () => {
  searchForm.filter = ''
  handleSearch()
}

const handleCreate = async () => {
  const created = await createUserModal.open({
    availableRoles: availableRoles.value,
  })
  if (created) {
    await loadUsers()
  }
}

const handleEdit = async (user: IdentityUser) => {
  const updated = await editUserModal.open({ user })
  if (updated) {
    await loadUsers()
  }
}

const handleRoleManage = async (user: IdentityUser) => {
  const success = await userRoleModal.open({
    user,
    availableRoles: availableRoles.value,
  })
  if (success) {
    await loadUsers()
  }
}

const handleDelete = async (user: IdentityUser) => {
  const confirmed = await confirmModal.open({
    title: $t('page.user.actions.deleteTitle'),
    message: $t('page.user.actions.deleteConfirm', { userName: user.userName }),
    confirmText: $t('common.confirm'),
    cancelText: $t('common.cancel'),
    danger: true,
  })

  if (!confirmed) return

  try {
    await userApi.delete(user.id!)
    toast.add({
      title: $t('page.user.actions.deleteSuccess'),
      color: 'success',
    })
    await loadUsers()
  } catch (error) {
    console.error('删除用户失败:', error)
    toast.add({
      title: $t('page.user.actions.deleteError'),
      color: 'error',
    })
  }
}

// 分页处理
const handlePageChange = (page: number) => {
  currentPage.value = page
  loadUsers()
}

// 排序处理
const handleSort = () => {
  currentPage.value = 1
  loadUsers()
}

// 初始化
loadUsers()
loadAvailableRoles()
</script>

<template>
  <Page content-class="flex flex-col gap-4">
    <!-- 搜索和工具栏 -->
    <UCard class="mb-0">
      <div class="flex flex-col gap-4">
        <div
          class="flex flex-col gap-4 md:flex-row md:items-center md:justify-between"
        >
          <h2 class="text-foreground text-xl font-semibold">
            {{ $t('page.user.title') }}
          </h2>
          <UButton @click="handleCreate">
            <UIcon name="i-lucide-plus" class="mr-1" />
            {{ $t('page.user.actions.create') }}
          </UButton>
        </div>
        <div
          class="flex flex-col items-start justify-between gap-4 md:flex-row md:items-end"
        >
          <div
            class="flex w-full flex-col gap-4 md:w-auto md:flex-row md:items-end"
          >
            <div class="flex flex-col gap-2">
              <UInput
                v-model="searchForm.filter"
                icon="i-lucide-search"
                :placeholder="$t('page.user.search.filterPlaceholder')"
                class="w-full md:w-72"
                @keyup.enter="handleSearch"
              >
                <template v-if="searchForm.filter?.length" #trailing>
                  <UButton
                    color="neutral"
                    variant="link"
                    size="sm"
                    icon="i-lucide-circle-x"
                    aria-label="Clear input"
                    @click="searchForm.filter = ''"
                  />
                </template>
              </UInput>
            </div>
            <div class="flex gap-2">
              <UButton @click="handleSearch">
                {{ $t('page.user.search.search') }}
              </UButton>
              <UButton variant="ghost" @click="handleReset">
                {{ $t('page.user.search.reset') }}
              </UButton>
            </div>
          </div>
        </div>
      </div>
    </UCard>

    <!-- 用户列表 -->
    <UCard class="mb-0">
      <div class="relative">
        <div class="overflow-x-auto">
          <UTable
            v-model:sorting="sorting"
            :loading="loading"
            :data="users"
            :columns="columns"
            class="min-w-[960px]"
            @update:sorting="handleSort"
          />
        </div>

        <div
          v-if="total > 0"
          class="border-border/60 flex items-center justify-between border-t px-4 py-3"
        >
          <div class="text-muted-foreground text-sm">
            {{ $t('common.pagination.total', { total }) }}
          </div>
          <UPagination
            v-model="currentPage"
            :total="total"
            :page-size="pageSize"
            show-edges
            @update:model-value="handlePageChange"
          />
        </div>
      </div>
    </UCard>
  </Page>
</template>

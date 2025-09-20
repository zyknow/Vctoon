<script setup lang="ts">
import type { IdentityUser, LibraryPermissionCreateUpdate } from '@vben/api'

import { computed, ref, watch } from 'vue'

import { libraryApi, MediumType } from '@vben/api'
import {
  CiChat,
  CiDownload,
  CiEye,
  CiShare,
  CiStar,
  CiUser,
  MdiComic,
  MdiVideo,
} from '@vben/icons'
import { useUserStore } from '@vben/stores'

import { ElMessage } from 'element-plus'

import { $t } from '#/locales'

interface Props {
  modelValue?: boolean
  libraryId?: string
  libraryName?: string
}

interface Emits {
  (e: 'update:modelValue', value: boolean): void
  (e: 'success'): void
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: false,
  libraryId: '',
  libraryName: '',
})

const emit = defineEmits<Emits>()
const userStore = useUserStore()
const library = computed(() =>
  userStore.libraries.find((lib) => lib.id === props.libraryId),
)

const icon = computed(() => {
  switch (library.value?.mediumType) {
    case MediumType.Comic: {
      return MdiComic
    }
    case MediumType.Video: {
      return MdiVideo
    }
    default: {
      return null
    }
  }
})

// 对话框显隐控制
const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})

// 用户搜索相关
const searchKeyword = ref('')
const searchLoading = ref(false)
const userOptions = ref<IdentityUser[]>([])
const selectedUser = ref<IdentityUser | null>(null)
// 加载选中用户在当前库的权限
const permissionsLoading = ref(false)

// 权限配置
const permissions = ref<LibraryPermissionCreateUpdate>({
  canView: false,
  canDownload: false,
  canComment: false,
  canShare: false,
  canStar: false,
})

// 提交状态
const submitting = ref(false)

// 搜索用户
const searchUsers = async (query: string) => {
  searchLoading.value = true
  try {
    const response = await libraryApi.getAssignableUsers({
      filter: query || '',
      maxResultCount: 20,
      skipCount: 0,
    })
    userOptions.value = response.items || []
  } catch (error) {
    console.error('搜索用户失败:', error)
    ElMessage.error($t('page.library.permission.searchUserFailed'))
  } finally {
    searchLoading.value = false
  }
}

// 初始化加载前20个用户
const loadInitialUsers = async () => {
  await searchUsers('')
}

// 用户选择处理
const handleUserSelect = (user: IdentityUser) => {
  selectedUser.value = user
  // 选择用户后拉取该用户在当前库的权限
  if (user && props.libraryId) {
    fetchUserLibraryPermissions(user.id, props.libraryId)
  }
}

// 重置表单
const resetForm = () => {
  searchKeyword.value = ''
  selectedUser.value = null
  permissions.value = {
    canView: false,
    canDownload: false,
    canComment: false,
    canShare: false,
    canStar: false,
  }
  // 重新加载初始用户列表
  loadInitialUsers()
}

// 提交权限设置
const handleSubmit = async () => {
  if (!selectedUser.value) {
    ElMessage.warning($t('page.library.permission.pleaseSelectUser'))
    return
  }

  if (!props.libraryId) {
    ElMessage.error($t('page.library.permission.libraryIdRequired'))
    return
  }

  submitting.value = true
  try {
    await libraryApi.setLibraryPermissions(
      permissions.value,
      selectedUser.value.id,
      props.libraryId,
    )
    ElMessage.success($t('page.library.permission.setSuccess'))
    emit('success')
    dialogVisible.value = false
  } catch (error) {
    console.error('设置权限失败:', error)
    ElMessage.error($t('page.library.permission.setFailed'))
  } finally {
    submitting.value = false
  }
}

// 监听对话框显隐，重置表单
watch(
  () => props.modelValue,
  (newVal) => {
    if (newVal) {
      resetForm()
    }
  },
)

// 搜索防抖
let searchTimer: NodeJS.Timeout | null = null
const handleSearchInput = (query: string) => {
  if (searchTimer) {
    clearTimeout(searchTimer)
  }

  // 如果没有查询内容，立即加载初始用户列表
  if (!query || query.trim() === '') {
    searchUsers('')
    return
  }

  // 有查询内容时进行防抖搜索
  searchTimer = setTimeout(() => {
    searchUsers(query)
  }, 300)
}

// 拉取指定用户在指定库的权限
const fetchUserLibraryPermissions = async (
  userId: string,
  libraryId: string,
) => {
  if (!userId || !libraryId) return
  permissionsLoading.value = true
  try {
    const res = await libraryApi.getLibraryPermissions(userId, libraryId)
    // 合并权限，未返回的字段按 false 处理
    permissions.value = {
      canView: !!res?.canView,
      canDownload: !!res?.canDownload,
      canComment: !!res?.canComment,
      canShare: !!res?.canShare,
      canStar: !!res?.canStar,
    }
  } catch (error) {
    console.error('获取库权限失败:', error)
    ElMessage.error($t('page.library.permission.getFailed'))
  } finally {
    permissionsLoading.value = false
  }
}

// 当库ID变化或选中用户变化时，自动刷新权限
watch(
  () => [props.libraryId, selectedUser.value?.id] as const,
  ([lid, uid]) => {
    if (lid && uid) fetchUserLibraryPermissions(uid, lid)
  },
)
</script>

<template>
  <el-dialog
    v-model="dialogVisible"
    :title="$t('page.library.permission.dialogTitle')"
    width="600px"
    :close-on-click-modal="false"
    :close-on-press-escape="false"
  >
    <div class="space-y-6">
      <!-- 库信息 -->
      <div class="rounded-lg border p-4">
        <div class="flex items-center space-x-3">
          <component :is="icon" class="text-3xl" />
          <div>
            <div class="font-semibold">{{ libraryName }}</div>
            <p class="text-sm opacity-70">
              {{ $t('page.library.permission.libraryDescription') }}
            </p>
          </div>
        </div>
      </div>

      <!-- 用户搜索选择 -->
      <div>
        <label class="mb-3 block text-sm font-medium">
          {{ $t('page.library.permission.selectUser') }}
        </label>
        <el-select
          v-model="selectedUser"
          filterable
          remote
          reserve-keyword
          :placeholder="$t('page.library.permission.searchUserPlaceholder')"
          :remote-method="handleSearchInput"
          :loading="searchLoading"
          value-key="id"
          class="w-full"
          @change="handleUserSelect"
        >
          <el-option
            v-for="user in userOptions"
            :key="user.id"
            :label="`${user.name || user.userName} (${user.email})`"
            :value="user"
          >
            <div class="flex items-center justify-between">
              <div>
                <div class="font-medium">
                  {{ user.name || user.userName }}
                </div>
                <div class="text-sm opacity-70">{{ user.email }}</div>
              </div>
              <el-tag v-if="!user.isActive" type="danger" size="small">
                {{ $t('page.library.permission.inactive') }}
              </el-tag>
            </div>
          </el-option>
        </el-select>
      </div>

      <!-- 权限配置 -->
      <div v-if="selectedUser">
        <label class="mb-4 block text-sm font-medium">
          {{ $t('page.library.permission.permissionSettings') }}
        </label>
        <div class="space-y-3">
          <div
            class="hover:bg-primary/5 flex items-center justify-between rounded-lg border p-4 transition-colors"
          >
            <div class="flex items-center space-x-4">
              <div
                class="flex h-10 w-10 items-center justify-center rounded-full bg-blue-500/10"
              >
                <CiEye class="text-lg text-blue-500" />
              </div>
              <div>
                <div class="font-medium">
                  {{ $t('page.library.permission.canView') }}
                </div>
                <div class="text-sm opacity-70">
                  {{ $t('page.library.permission.canViewDescription') }}
                </div>
              </div>
            </div>
            <el-switch v-model="permissions.canView" />
          </div>

          <div
            class="hover:bg-primary/5 flex items-center justify-between rounded-lg border p-4 transition-colors"
          >
            <div class="flex items-center space-x-4">
              <div
                class="flex h-10 w-10 items-center justify-center rounded-full bg-green-500/10"
              >
                <CiDownload class="text-lg text-green-500" />
              </div>
              <div>
                <div class="font-medium">
                  {{ $t('page.library.permission.canDownload') }}
                </div>
                <div class="text-sm opacity-70">
                  {{ $t('page.library.permission.canDownloadDescription') }}
                </div>
              </div>
            </div>
            <el-switch v-model="permissions.canDownload" />
          </div>

          <div
            class="hover:bg-primary/5 flex items-center justify-between rounded-lg border p-4 transition-colors"
          >
            <div class="flex items-center space-x-4">
              <div
                class="flex h-10 w-10 items-center justify-center rounded-full bg-purple-500/10"
              >
                <CiChat class="text-lg text-purple-500" />
              </div>
              <div>
                <div class="font-medium">
                  {{ $t('page.library.permission.canComment') }}
                </div>
                <div class="text-sm opacity-70">
                  {{ $t('page.library.permission.canCommentDescription') }}
                </div>
              </div>
            </div>
            <el-switch v-model="permissions.canComment" />
          </div>

          <div
            class="hover:bg-primary/5 flex items-center justify-between rounded-lg border p-4 transition-colors"
          >
            <div class="flex items-center space-x-4">
              <div
                class="flex h-10 w-10 items-center justify-center rounded-full bg-orange-500/10"
              >
                <CiShare class="text-lg text-orange-500" />
              </div>
              <div>
                <div class="font-medium">
                  {{ $t('page.library.permission.canShare') }}
                </div>
                <div class="text-sm opacity-70">
                  {{ $t('page.library.permission.canShareDescription') }}
                </div>
              </div>
            </div>
            <el-switch v-model="permissions.canShare" />
          </div>

          <div
            class="hover:bg-primary/5 flex items-center justify-between rounded-lg border p-4 transition-colors"
          >
            <div class="flex items-center space-x-4">
              <div
                class="flex h-10 w-10 items-center justify-center rounded-full bg-yellow-500/10"
              >
                <CiStar class="text-lg text-yellow-500" />
              </div>
              <div>
                <div class="font-medium">
                  {{ $t('page.library.permission.canStar') }}
                </div>
                <div class="text-sm opacity-70">
                  {{ $t('page.library.permission.canStarDescription') }}
                </div>
              </div>
            </div>
            <el-switch v-model="permissions.canStar" />
          </div>
        </div>
      </div>

      <!-- 选中用户信息 -->
      <div
        v-if="selectedUser"
        class="border-primary/20 bg-primary/5 rounded-lg border p-4"
      >
        <div class="flex items-center space-x-3">
          <CiUser class="text-primary text-2xl" />
          <div>
            <div class="font-semibold">
              {{ selectedUser.name || selectedUser.userName }}
            </div>
            <div class="text-sm opacity-70">{{ selectedUser.email }}</div>
          </div>
        </div>
      </div>
    </div>

    <template #footer>
      <div class="flex justify-end space-x-2">
        <el-button @click="dialogVisible = false">
          {{ $t('common.cancel') }}
        </el-button>
        <el-button
          type="primary"
          :loading="submitting"
          :disabled="!selectedUser"
          @click="handleSubmit"
        >
          {{ $t('page.library.permission.confirm') }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<style lang="scss" scoped>
:deep(.el-select-dropdown__item) {
  height: auto;
  padding: 8px 20px;
}
</style>

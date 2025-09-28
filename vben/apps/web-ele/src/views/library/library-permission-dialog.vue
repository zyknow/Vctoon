<script setup lang="ts">
import type { IdentityUser, LibraryPermissionCreateUpdate } from '@vben/api'

import { computed, onMounted, ref } from 'vue'

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

import { useDialogContext } from '#/hooks/useDialogService'
import { $t } from '#/locales'

interface Props {
  libraryId: string
  libraryName: string
}
// 必须接收返回值才能在脚本中访问 props
const props = defineProps<Props>()
const { close, resolve } = useDialogContext<boolean>()

const userStore = useUserStore()
const library = computed(() =>
  userStore.libraries.find((lib) => lib.id === props.libraryId),
)

// 不再使用 declare props，已通过 const props = defineProps<Props>() 获取

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

// 搜索 & 选择用户
const searchLoading = ref(false)
const userOptions = ref<IdentityUser[]>([])
const selectedUser = ref<IdentityUser | null>(null)
const permissionsLoading = ref(false)
const permissions = ref<LibraryPermissionCreateUpdate>({
  canView: false,
  canDownload: false,
  canComment: false,
  canShare: false,
  canStar: false,
})
const submitting = ref(false)

async function searchUsers(query: string) {
  searchLoading.value = true
  try {
    const res = await libraryApi.getAssignableUsers({
      filter: query || '',
      maxResultCount: 20,
      skipCount: 0,
    })
    userOptions.value = res.items || []
  } catch (error) {
    console.error(error)
    // 全局拦截器处理搜索失败提示
  } finally {
    searchLoading.value = false
  }
}

function handleUserSelect(user: IdentityUser) {
  selectedUser.value = user
  if (user && props.libraryId) fetchUserPermissions(user.id, props.libraryId)
}

function resetForm() {
  selectedUser.value = null
  permissions.value = {
    canView: false,
    canDownload: false,
    canComment: false,
    canShare: false,
    canStar: false,
  }
  searchUsers('')
}

async function fetchUserPermissions(userId: string, libraryId: string) {
  if (!userId || !libraryId) return
  permissionsLoading.value = true
  try {
    const res = await libraryApi.getLibraryPermissions(userId, libraryId)
    permissions.value = {
      canView: !!res?.canView,
      canDownload: !!res?.canDownload,
      canComment: !!res?.canComment,
      canShare: !!res?.canShare,
      canStar: !!res?.canStar,
    }
  } catch (error) {
    console.error(error)
    // 全局拦截器处理获取权限失败提示
  } finally {
    permissionsLoading.value = false
  }
}

async function submit() {
  if (!selectedUser.value) {
    // 未选择用户，保持静默或可考虑局部 UI 提示（不使用 ElMessage）
    return
  }
  submitting.value = true
  try {
    await libraryApi.setLibraryPermissions(
      permissions.value,
      selectedUser.value.id,
      props.libraryId,
    )
    // 成功提示交由全局拦截器
    resolve(true)
  } catch (error) {
    console.error(error)
    // 全局拦截器处理设置失败提示
  } finally {
    submitting.value = false
  }
}

// 初始加载
onMounted(() => {
  resetForm()
})
</script>

<template>
  <div class="space-y-6">
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

    <div>
      <label class="mb-3 block text-sm font-medium">
        {{ $t('page.library.permission.selectUser') }}
      </label>
      <el-select
        v-model="selectedUser"
        filterable
        remote
        reserve-keyword
        value-key="id"
        class="w-full"
        :placeholder="$t('page.library.permission.searchUserPlaceholder')"
        :remote-method="searchUsers"
        :loading="searchLoading"
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

    <div class="flex justify-end gap-3">
      <el-button @click="close()">{{ $t('common.cancel') }}</el-button>
      <el-button
        type="primary"
        :loading="submitting"
        :disabled="!selectedUser"
        @click="submit"
      >
        {{ $t('page.library.permission.confirm') }}
      </el-button>
    </div>
  </div>
</template>

<style scoped lang="scss">
:deep(.el-select-dropdown__item) {
  height: auto;
  padding: 8px 20px;
}
</style>

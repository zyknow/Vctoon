<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'

import {
  libraryApi,
  type LibraryPermissionCreateUpdate,
} from '@/api/http/library'
import { MediumType } from '@/api/http/library'
import type { IdentityUser } from '@/api/http/user'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores/user'

interface Props {
  libraryId: string
  libraryName: string
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: boolean]
}>()

const { isMobile } = useIsMobile()
const toast = useToast()

const userStore = useUserStore()
const library = computed(() =>
  userStore.libraries.find((lib) => lib.id === props.libraryId),
)

const icon = computed(() => {
  switch (library.value?.mediumType) {
    case MediumType.Comic: {
      return 'i-lucide-book-image'
    }
    case MediumType.Video: {
      return 'i-lucide-video'
    }
    default: {
      return 'i-lucide-library'
    }
  }
})

// 用户搜索与选择
const searchTerm = ref('')
const searchLoading = ref(false)
const userOptions = ref<IdentityUser[]>([])
const selectedUser = ref<IdentityUser | undefined>(undefined)

// 权限设置
const permissionsLoading = ref(false)
const permissions = ref<LibraryPermissionCreateUpdate>({
  canView: false,
  canDownload: false,
  canComment: false,
  canShare: false,
  canStar: false,
})
const submitting = ref(false)

// 监听搜索词变化
watch(searchTerm, async (newVal) => {
  await searchUsers(newVal)
})

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
    console.error('搜索用户失败:', error)
    toast.add({
      title: $t('page.library.permission.messages.searchFailed'),
      color: 'error',
    })
  } finally {
    searchLoading.value = false
  }
}

function handleUserSelect(user: IdentityUser | undefined) {
  selectedUser.value = user
  if (user && props.libraryId) {
    fetchUserPermissions(user.id, props.libraryId)
  } else {
    // 重置权限
    permissions.value = {
      canView: false,
      canDownload: false,
      canComment: false,
      canShare: false,
      canStar: false,
    }
  }
}

function resetForm() {
  selectedUser.value = undefined
  permissions.value = {
    canView: false,
    canDownload: false,
    canComment: false,
    canShare: false,
    canStar: false,
  }
  searchTerm.value = ''
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
    console.error('获取用户权限失败:', error)
    toast.add({
      title: $t('page.library.permission.messages.fetchFailed'),
      color: 'error',
    })
  } finally {
    permissionsLoading.value = false
  }
}

async function submit() {
  if (!selectedUser.value) {
    return
  }
  submitting.value = true
  try {
    await libraryApi.setLibraryPermissions(
      permissions.value,
      selectedUser.value.id,
      props.libraryId,
    )
    emit('close', true)
  } catch (error) {
    console.error('设置用户权限失败:', error)
    toast.add({
      title: $t('page.library.permission.messages.saveFailed'),
      color: 'error',
    })
  } finally {
    submitting.value = false
  }
}

function handleCancel() {
  emit('close', false)
}

// 初始加载
onMounted(() => {
  resetForm()
})

// 将用户对象转换为 SelectMenu 所需的格式
const userItems = computed(() => {
  return userOptions.value.map((user) => ({
    label: `${user.name || user.userName} (${user.email})`,
    value: user.id,
    user,
    disabled: !user.isActive,
  }))
})
</script>

<template>
  <UModal
    :title="$t('page.library.permission.title')"
    :fullscreen="isMobile"
    :dismissible="false"
  >
    <template #body>
      <div class="space-y-6">
        <!-- 库信息 -->
        <div class="rounded-lg border p-4">
          <div class="flex items-center gap-3">
            <UIcon :name="icon" class="text-primary text-3xl" />
            <div>
              <div class="font-semibold">{{ libraryName }}</div>
              <p class="text-muted-foreground text-sm">
                {{ $t('page.library.permission.libraryDescription') }}
              </p>
            </div>
          </div>
        </div>

        <!-- 用户选择 -->
        <div>
          <label class="mb-3 block text-sm font-medium">
            {{ $t('page.library.permission.selectUser') }}
          </label>
          <USelectMenu
            v-model="selectedUser"
            v-model:search-term="searchTerm"
            :items="userItems"
            :loading="searchLoading"
            ignore-filter
            value-key="user"
            label-key="label"
            :placeholder="$t('page.library.permission.searchUserPlaceholder')"
            :search-input="{
              placeholder: $t('page.library.permission.searchUserPlaceholder'),
            }"
            class="w-full"
            @update:model-value="handleUserSelect"
          >
            <template #item="{ item }">
              <div class="flex w-full items-center justify-between">
                <div class="flex-1">
                  <div class="font-medium">
                    {{ item.user.name || item.user.userName }}
                  </div>
                  <div class="text-muted-foreground text-sm">
                    {{ item.user.email }}
                  </div>
                </div>
                <UBadge
                  v-if="!item.user.isActive"
                  color="error"
                  size="sm"
                  variant="soft"
                >
                  {{ $t('page.library.permission.inactive') }}
                </UBadge>
              </div>
            </template>
          </USelectMenu>
        </div>

        <!-- 权限设置 -->
        <div v-if="selectedUser">
          <label class="mb-4 block text-sm font-medium">
            {{ $t('page.library.permission.permissionSettings') }}
          </label>
          <div class="space-y-3">
            <!-- 查看权限 -->
            <div
              class="hover:bg-primary/5 flex items-center justify-between rounded-lg border p-4 transition-colors"
            >
              <div class="flex items-center gap-4">
                <div
                  class="bg-primary/10 text-primary flex size-10 items-center justify-center rounded-full"
                >
                  <UIcon name="i-lucide-eye" class="text-lg" />
                </div>
                <div>
                  <div class="font-medium">
                    {{ $t('page.library.permission.canView') }}
                  </div>
                  <div class="text-muted-foreground text-sm">
                    {{ $t('page.library.permission.canViewDescription') }}
                  </div>
                </div>
              </div>
              <USwitch
                v-model="permissions.canView"
                :disabled="permissionsLoading"
              />
            </div>

            <!-- 下载权限 -->
            <div
              class="hover:bg-primary/5 flex items-center justify-between rounded-lg border p-4 transition-colors"
            >
              <div class="flex items-center gap-4">
                <div
                  class="bg-success/10 text-success flex size-10 items-center justify-center rounded-full"
                >
                  <UIcon name="i-lucide-download" class="text-lg" />
                </div>
                <div>
                  <div class="font-medium">
                    {{ $t('page.library.permission.canDownload') }}
                  </div>
                  <div class="text-muted-foreground text-sm">
                    {{ $t('page.library.permission.canDownloadDescription') }}
                  </div>
                </div>
              </div>
              <USwitch
                v-model="permissions.canDownload"
                :disabled="permissionsLoading"
              />
            </div>

            <!-- 评论权限 -->
            <div
              class="hover:bg-primary/5 flex items-center justify-between rounded-lg border p-4 transition-colors"
            >
              <div class="flex items-center gap-4">
                <div
                  class="bg-info/10 text-info flex size-10 items-center justify-center rounded-full"
                >
                  <UIcon name="i-lucide-message-square" class="text-lg" />
                </div>
                <div>
                  <div class="font-medium">
                    {{ $t('page.library.permission.canComment') }}
                  </div>
                  <div class="text-muted-foreground text-sm">
                    {{ $t('page.library.permission.canCommentDescription') }}
                  </div>
                </div>
              </div>
              <USwitch
                v-model="permissions.canComment"
                :disabled="permissionsLoading"
              />
            </div>

            <!-- 分享权限 -->
            <div
              class="hover:bg-primary/5 flex items-center justify-between rounded-lg border p-4 transition-colors"
            >
              <div class="flex items-center gap-4">
                <div
                  class="bg-warning/10 text-warning flex size-10 items-center justify-center rounded-full"
                >
                  <UIcon name="i-lucide-share-2" class="text-lg" />
                </div>
                <div>
                  <div class="font-medium">
                    {{ $t('page.library.permission.canShare') }}
                  </div>
                  <div class="text-muted-foreground text-sm">
                    {{ $t('page.library.permission.canShareDescription') }}
                  </div>
                </div>
              </div>
              <USwitch
                v-model="permissions.canShare"
                :disabled="permissionsLoading"
              />
            </div>

            <!-- 收藏权限 -->
            <div
              class="hover:bg-primary/5 flex items-center justify-between rounded-lg border p-4 transition-colors"
            >
              <div class="flex items-center gap-4">
                <div
                  class="bg-warning/10 text-warning flex size-10 items-center justify-center rounded-full"
                >
                  <UIcon name="i-lucide-star" class="text-lg" />
                </div>
                <div>
                  <div class="font-medium">
                    {{ $t('page.library.permission.canStar') }}
                  </div>
                  <div class="text-muted-foreground text-sm">
                    {{ $t('page.library.permission.canStarDescription') }}
                  </div>
                </div>
              </div>
              <USwitch
                v-model="permissions.canStar"
                :disabled="permissionsLoading"
              />
            </div>
          </div>
        </div>

        <!-- 已选用户信息 -->
        <div
          v-if="selectedUser"
          class="border-primary/20 bg-primary/5 rounded-lg border p-4"
        >
          <div class="flex items-center gap-3">
            <UIcon name="i-lucide-user" class="text-primary text-2xl" />
            <div>
              <div class="font-semibold">
                {{ selectedUser.name || selectedUser.userName }}
              </div>
              <div class="text-muted-foreground text-sm">
                {{ selectedUser.email }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>

    <template #footer>
      <div class="flex justify-end gap-3">
        <UButton color="neutral" variant="ghost" @click="handleCancel">
          {{ $t('common.cancel') }}
        </UButton>
        <UButton
          color="primary"
          :loading="submitting"
          :disabled="!selectedUser"
          @click="submit"
        >
          {{ $t('page.library.permission.confirm') }}
        </UButton>
      </div>
    </template>
  </UModal>
</template>

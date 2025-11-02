<script setup lang="ts">
import { ref, watch } from 'vue'

import type { IdentityRole } from '@/api/http/role'
import type { IdentityUser } from '@/api/http/user'
import { userApi } from '@/api/http/user'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'

interface Props {
  user: IdentityUser
  availableRoles: IdentityRole[]
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: boolean]
}>()

// 移动端判断
const { isMobile } = useIsMobile()

const loading = ref(false)
const selectedRoles = ref<string[]>([])
const toast = useToast()

// 加载用户角色
async function loadUserRoles() {
  try {
    const result = await userApi.getUserRoles(props.user.id!)
    const userRoles = result.items || []
    selectedRoles.value = userRoles.map((role: IdentityRole) => role.name)
  } catch (error) {
    console.error('加载用户角色失败:', error)
    toast.add({
      title: $t('page.user.messages.loadUserRolesError'),
      color: 'error',
    })
  }
}

// 监听 user 变化，重新加载角色
watch(
  () => props.user,
  () => {
    if (props.user?.id) {
      loadUserRoles()
    }
  },
  { immediate: true },
)

async function handleSubmit() {
  if (loading.value) return

  try {
    loading.value = true
    await userApi.updateUserRoles(props.user.id!, selectedRoles.value)
    // 静默成功，无需提示
    emit('close', true)
  } catch {
    // 失败必须提示
    toast.add({
      title: $t('page.user.role.messages.error'),
      color: 'error',
    })
  } finally {
    loading.value = false
  }
}

function handleCancel() {
  emit('close', false)
}
</script>

<template>
  <UModal
    :title="$t('page.user.role.title')"
    :fullscreen="isMobile"
    :dismissible="false"
  >
    <template #body>
      <div class="space-y-4">
        <p class="text-foreground text-base font-medium">
          {{ $t('page.user.role.userInfo') }}
          <span class="text-primary ml-1">{{ user.userName }}</span>
        </p>

        <USeparator />

        <div class="space-y-2">
          <label class="text-foreground block text-sm font-medium">
            {{ $t('page.user.role.assignRoles') }}
          </label>
          <USelectMenu
            v-model="selectedRoles"
            class="w-full"
            :items="availableRoles"
            label-key="name"
            value-key="name"
            :placeholder="$t('page.user.role.placeholders.roles')"
            :disabled="loading"
            multiple
          />
        </div>

        <div class="flex justify-end gap-3 pt-2">
          <UButton
            color="neutral"
            variant="ghost"
            :disabled="loading"
            @click="handleCancel"
          >
            {{ $t('page.user.role.actions.cancel') }}
          </UButton>
          <UButton color="primary" :loading="loading" @click="handleSubmit">
            {{ $t('page.user.role.actions.confirm') }}
          </UButton>
        </div>
      </div>
    </template>
  </UModal>
</template>

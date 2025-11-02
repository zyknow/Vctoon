<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { z } from 'zod'

import type { IdentityUser, UpdateIdentityUserInput } from '@/api/http/user'
import { userApi } from '@/api/http/user'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'

type FormSubmitEvent<T> = {
  data: T
}

interface Props {
  user: IdentityUser
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: IdentityUser | null]
}>()

// 移动端判断
const { isMobile } = useIsMobile()

const loading = ref(false)
const toast = useToast()

const state = reactive<UpdateIdentityUserInput>({
  userName: '',
  name: '',
  email: '',
  phoneNumber: '',
  password: '',
  isActive: true,
  lockoutEnabled: true,
  emailReminderEnabled: true,
  roleNames: [],
})

// Zod 验证规则
const schema = z.object({
  userName: z
    .string()
    .min(3, $t('page.user.create.validation.userNameLength'))
    .max(50, $t('page.user.create.validation.userNameLength')),
  name: z.string().min(1, $t('page.user.create.validation.nameRequired')),
  email: z
    .string()
    .min(1, $t('page.user.create.validation.emailRequired'))
    .email($t('page.user.create.validation.emailFormat')),
  phoneNumber: z
    .string()
    .optional()
    .refine(
      (val) => !val || /^1[3-9]\d{9}$/.test(val),
      $t('page.user.create.validation.phoneFormat'),
    ),
  password: z.string().optional(),
  isActive: z.boolean(),
  lockoutEnabled: z.boolean(),
  emailReminderEnabled: z.boolean().optional(),
  roleNames: z.array(z.string()),
})

type Schema = z.output<typeof schema>

// 初始化表单数据
watch(
  () => props.user,
  (user) => {
    if (user) {
      state.userName = user.userName || ''
      state.name = user.name || ''
      state.email = user.email || ''
      state.phoneNumber = user.phoneNumber || ''
      state.password = ''
      state.isActive = user.isActive ?? true
      state.lockoutEnabled = true
      state.emailReminderEnabled = user.emailReminderEnabled ?? true
      state.roleNames = []
    }
  },
  { immediate: true },
)

async function onSubmit(event: FormSubmitEvent<Schema>) {
  if (loading.value) return

  try {
    loading.value = true

    const updateData = { ...event.data }
    // 如果密码为空，则不更新密码
    if (!updateData.password) {
      delete updateData.password
    }

    const updated = await userApi.update(updateData, props.user.id!)
    // 静默成功，无需提示
    emit('close', updated as IdentityUser)
  } catch {
    // 失败必须提示
    toast.add({
      title: $t('page.user.edit.messages.error'),
      color: 'error',
    })
  } finally {
    loading.value = false
  }
}

function handleCancel() {
  emit('close', null)
}
</script>

<template>
  <UModal
    :title="$t('page.user.edit.title')"
    :fullscreen="isMobile"
    :dismissible="false"
  >
    <template #body>
      <UForm
        :schema="schema"
        :state="state"
        class="space-y-4"
        @submit="onSubmit"
      >
        <UFormField
          :label="$t('page.user.edit.fields.userName')"
          name="userName"
        >
          <UInput
            v-model="state.userName"
            class="w-full"
            :placeholder="$t('page.user.edit.placeholders.userName')"
            :disabled="loading"
          />
        </UFormField>

        <UFormField :label="$t('page.user.edit.fields.name')" name="name">
          <UInput
            v-model="state.name"
            class="w-full"
            :placeholder="$t('page.user.edit.placeholders.name')"
            :disabled="loading"
          />
        </UFormField>

        <UFormField :label="$t('page.user.edit.fields.email')" name="email">
          <UInput
            v-model="state.email"
            class="w-full"
            type="email"
            :placeholder="$t('page.user.edit.placeholders.email')"
            :disabled="loading"
          />
        </UFormField>

        <UFormField
          :label="$t('page.user.edit.fields.phoneNumber')"
          name="phoneNumber"
        >
          <UInput
            v-model="state.phoneNumber"
            class="w-full"
            :placeholder="$t('page.user.edit.placeholders.phoneNumber')"
            :disabled="loading"
          />
        </UFormField>

        <UFormField
          :label="$t('page.user.edit.fields.password')"
          name="password"
        >
          <UInput
            v-model="state.password"
            class="w-full"
            type="password"
            :placeholder="$t('page.user.edit.placeholders.password')"
            :disabled="loading"
          />
        </UFormField>

        <UFormField :label="$t('page.user.edit.fields.status')" name="isActive">
          <USwitch v-model="state.isActive" :disabled="loading" />
          <span class="text-muted-foreground ml-2 text-sm">
            {{
              state.isActive
                ? $t('page.user.status.enabled')
                : $t('page.user.status.disabled')
            }}
          </span>
        </UFormField>

        <UFormField
          :label="$t('page.user.edit.fields.lockoutEnabled')"
          name="lockoutEnabled"
        >
          <USwitch v-model="state.lockoutEnabled" :disabled="loading" />
          <span class="text-muted-foreground ml-2 text-sm">
            {{
              state.lockoutEnabled
                ? $t('page.user.status.enabled')
                : $t('page.user.status.disabled')
            }}
          </span>
        </UFormField>

        <UFormField
          :label="$t('page.user.edit.fields.emailReminderEnabled')"
          name="emailReminderEnabled"
        >
          <USwitch v-model="state.emailReminderEnabled" :disabled="loading" />
          <span class="text-muted-foreground ml-2 text-sm">
            {{
              state.emailReminderEnabled
                ? $t('page.user.status.enabled')
                : $t('page.user.status.disabled')
            }}
          </span>
        </UFormField>

        <div class="flex justify-end gap-3 pt-2">
          <UButton
            color="neutral"
            variant="ghost"
            :disabled="loading"
            @click="handleCancel"
          >
            {{ $t('page.user.edit.actions.cancel') }}
          </UButton>
          <UButton color="primary" type="submit" :loading="loading">
            {{ $t('page.user.edit.actions.confirm') }}
          </UButton>
        </div>
      </UForm>
    </template>
  </UModal>
</template>

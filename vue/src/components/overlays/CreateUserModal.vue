<script setup lang="ts">
import { reactive, ref } from 'vue'
import { z } from 'zod'

import type { IdentityRole } from '@/api/http/role'
import type { CreateIdentityUserInput, IdentityUser } from '@/api/http/user'
import { userApi } from '@/api/http/user'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'

type FormSubmitEvent<T> = {
  data: T
}

interface Props {
  availableRoles: IdentityRole[]
}

defineProps<Props>()

const emit = defineEmits<{
  close: [value: IdentityUser | null]
}>()

// 移动端判断
const { isMobile } = useIsMobile()

const loading = ref(false)
const toast = useToast()

const state = reactive<CreateIdentityUserInput>({
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
  name: z.string().optional(),
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
  password: z.string().min(6, $t('page.user.create.validation.passwordLength')),
  isActive: z.boolean(),
  lockoutEnabled: z.boolean(),
  emailReminderEnabled: z.boolean().optional(),
  roleNames: z.array(z.string()),
})

type Schema = z.output<typeof schema>

async function onSubmit(event: FormSubmitEvent<Schema>) {
  if (loading.value) return

  try {
    loading.value = true
    // 确保 name 字段有值
    const createData: CreateIdentityUserInput = {
      ...event.data,
      name: event.data.name || '',
    }
    const created = await userApi.create(createData)
    // 静默成功，无需提示
    emit('close', created as IdentityUser)
  } catch {
    // 失败必须提示
    toast.add({
      title: $t('page.user.create.messages.error'),
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
    :title="$t('page.user.create.title')"
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
          :label="$t('page.user.create.fields.userName')"
          name="userName"
        >
          <UInput
            v-model="state.userName"
            class="w-full"
            :placeholder="$t('page.user.create.placeholders.userName')"
            :disabled="loading"
          />
        </UFormField>

        <UFormField :label="$t('page.user.create.fields.name')" name="name">
          <UInput
            v-model="state.name"
            class="w-full"
            :placeholder="$t('page.user.create.placeholders.name')"
            :disabled="loading"
          />
        </UFormField>

        <UFormField :label="$t('page.user.create.fields.email')" name="email">
          <UInput
            v-model="state.email"
            class="w-full"
            type="email"
            :placeholder="$t('page.user.create.placeholders.email')"
            :disabled="loading"
          />
        </UFormField>

        <UFormField
          :label="$t('page.user.create.fields.phoneNumber')"
          name="phoneNumber"
        >
          <UInput
            v-model="state.phoneNumber"
            class="w-full"
            :placeholder="$t('page.user.create.placeholders.phoneNumber')"
            :disabled="loading"
          />
        </UFormField>

        <UFormField
          :label="$t('page.user.create.fields.password')"
          name="password"
        >
          <UInput
            v-model="state.password"
            class="w-full"
            type="password"
            :placeholder="$t('page.user.create.placeholders.password')"
            :disabled="loading"
          />
        </UFormField>

        <UFormField
          :label="$t('page.user.create.fields.status')"
          name="isActive"
        >
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
          :label="$t('page.user.create.fields.lockoutEnabled')"
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
          :label="$t('page.user.create.fields.emailReminderEnabled')"
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

        <UFormField
          :label="$t('page.user.create.fields.roles')"
          name="roleNames"
        >
          <USelectMenu
            v-model="state.roleNames"
            class="w-full"
            :items="availableRoles"
            label-key="name"
            value-key="name"
            :placeholder="$t('page.user.create.placeholders.roles')"
            :disabled="loading"
            multiple
          />
        </UFormField>

        <div class="flex justify-end gap-3 pt-2">
          <UButton
            color="neutral"
            variant="ghost"
            :disabled="loading"
            @click="handleCancel"
          >
            {{ $t('page.user.create.actions.cancel') }}
          </UButton>
          <UButton color="primary" type="submit" :loading="loading">
            {{ $t('page.user.create.actions.confirm') }}
          </UButton>
        </div>
      </UForm>
    </template>
  </UModal>
</template>

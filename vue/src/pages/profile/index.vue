<script setup lang="ts">
import { z } from 'zod'

import type {
  ChangePasswordInput,
  Profile,
  UpdateProfileInput,
} from '@/api/http/profile'
import { profileApi } from '@/api/http/profile'
import { $t } from '@/locales/i18n'

defineOptions({ name: 'ProfilePage' })

const toast = useToast()

const loading = ref(false)
const saving = ref(false)
const profile = ref<Profile | null>(null)

const updateState = reactive<UpdateProfileInput>({
  userName: '',
  name: '',
  surname: '',
  email: '',
  phoneNumber: '',
})

const updateSchema = z.object({
  userName: z
    .string()
    .min(3, $t('page.user.create.validation.userNameLength'))
    .max(50, $t('page.user.create.validation.userNameLength')),
  name: z.string().optional(),
  surname: z.string().optional(),
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
})

type UpdateSchema = z.output<typeof updateSchema>

type FormSubmitEvent<T> = { data: T }

async function load() {
  loading.value = true
  try {
    const data = await profileApi.get()
    profile.value = data as Profile
    updateState.userName = profile.value.userName || ''
    updateState.name = profile.value.name || ''
    updateState.surname = profile.value.surname || ''
    updateState.email = profile.value.email || ''
    updateState.phoneNumber = profile.value.phoneNumber || ''
  } catch {
    toast.add({ title: $t('common.operationFailed'), color: 'error' })
  } finally {
    loading.value = false
  }
}

async function onUpdateSubmit(event: FormSubmitEvent<UpdateSchema>) {
  if (saving.value) return
  saving.value = true
  try {
    const input: UpdateProfileInput = {
      userName: event.data.userName || '',
      name: event.data.name || '',
      surname: event.data.surname || '',
      email: event.data.email || '',
      phoneNumber: event.data.phoneNumber || '',
    }
    await profileApi.update(input)
    await load()
  } catch {
    toast.add({ title: $t('common.updateFailed'), color: 'error' })
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  void load()
})

type ChangePasswordFormState = {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

const changeFormState = reactive<ChangePasswordFormState>({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const changeSchema = z
  .object({
    currentPassword: z.string().min(1),
    newPassword: z
      .string()
      .min(6, $t('page.user.create.validation.passwordLength')),
    confirmPassword: z
      .string()
      .min(6, $t('page.user.create.validation.passwordLength')),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    path: ['confirmPassword'],
    message: $t('page.user.create.validation.passwordLength'),
  })

type ChangeSchema = z.output<typeof changeSchema>

const changing = ref(false)

async function onChangePasswordSubmit(event: FormSubmitEvent<ChangeSchema>) {
  if (changing.value) return
  changing.value = true
  try {
    const input: ChangePasswordInput = {
      currentPassword: event.data.currentPassword,
      newPassword: event.data.newPassword,
    }
    await profileApi.changePassword(input)
    toast.add({ title: $t('common.updateSuccess'), color: 'success' })
    changeFormState.currentPassword = ''
    changeFormState.newPassword = ''
    changeFormState.confirmPassword = ''
  } catch {
    toast.add({ title: $t('common.updateFailed'), color: 'error' })
  } finally {
    changing.value = false
  }
}
</script>

<template>
  <div class="p-4">
    <UCard class="mb-4">
      <div class="flex items-center gap-2">
        <h2 class="text-foreground text-xl font-semibold">
          {{ $t('preferences.userMenu.profile') }}
        </h2>
        <div class="flex-1"></div>
        <UButton
          :loading="loading"
          variant="ghost"
          icon="i-lucide-refresh-cw"
          size="sm"
          @click="load"
        >
          <span class="hidden sm:inline">{{ $t('common.refresh') }}</span>
        </UButton>
      </div>
    </UCard>

    <UTabs
      :items="[
        { label: $t('preferences.userMenu.profile'), icon: 'i-lucide-user', slot: 'profile' },
        { label: $t('page.profile.actions.changePassword'), icon: 'i-lucide-lock', slot: 'password' },
      ]"
    >
      <template #profile>
          <UCard>
            <template #header>
              <div class="flex items-center justify-between">
                <span class="text-foreground font-medium">
                  {{ $t('preferences.userMenu.profile') }}
                </span>
                <UButton
                  :loading="saving"
                  icon="i-lucide-save"
                  size="sm"
                  @click="undefined"
                >
                  <span class="hidden sm:inline">{{ $t('common.save') }}</span>
                </UButton>
              </div>
            </template>

            <div v-if="loading" class="flex items-center justify-center py-8">
              <UIcon
                name="i-lucide-loader-2"
                class="text-primary h-6 w-6 animate-spin"
              />
            </div>

            <UForm
              v-else
              :schema="updateSchema"
              :state="updateState"
              class="space-y-4"
              @submit="onUpdateSubmit"
            >
              <UFormField
                :label="$t('page.user.create.fields.userName')"
                name="userName"
              >
                <UInput
                  v-model="updateState.userName"
                  class="w-full"
                  :placeholder="$t('ui.placeholder.input')"
                  :disabled="saving"
                />
              </UFormField>

              <UFormField
                :label="$t('page.user.create.fields.name')"
                name="name"
              >
                <UInput
                  v-model="updateState.name"
                  class="w-full"
                  :placeholder="$t('ui.placeholder.input')"
                  :disabled="saving"
                />
              </UFormField>

              <UFormField
                :label="$t('page.profile.fields.surname')"
                name="surname"
              >
                <UInput
                  v-model="updateState.surname"
                  class="w-full"
                  :placeholder="$t('ui.placeholder.input')"
                  :disabled="saving"
                />
              </UFormField>

              <UFormField
                :label="$t('page.user.create.fields.email')"
                name="email"
              >
                <UInput
                  v-model="updateState.email"
                  class="w-full"
                  type="email"
                  :placeholder="$t('ui.placeholder.input')"
                  :disabled="saving"
                />
              </UFormField>

              <UFormField
                :label="$t('page.user.create.fields.phoneNumber')"
                name="phoneNumber"
              >
                <UInput
                  v-model="updateState.phoneNumber"
                  class="w-full"
                  :placeholder="$t('ui.placeholder.input')"
                  :disabled="saving"
                />
              </UFormField>

              <div class="flex justify-end gap-3 pt-2">
                <UButton color="primary" type="submit" :loading="saving">
                  {{ $t('common.save') }}
                </UButton>
              </div>
            </UForm>
          </UCard>
      </template>

      <template #password>
          <UCard>
            <template #header>
              <div class="text-foreground font-medium">
                {{ $t('page.profile.actions.changePassword') }}
              </div>
            </template>

            <div v-if="profile?.isExternal" class="mb-2">
              <UAlert
                color="neutral"
                icon="i-lucide-info"
                :title="$t('page.profile.hints.externalPasswordManaged')"
              />
            </div>

            <UForm
              :schema="changeSchema"
              :state="changeFormState"
              class="space-y-4"
              @submit="onChangePasswordSubmit"
            >
              <UFormField
                :label="$t('page.profile.fields.currentPassword')"
                name="currentPassword"
              >
                <UInput
                  v-model="changeFormState.currentPassword"
                  class="w-full"
                  type="password"
                  :placeholder="$t('ui.placeholder.input')"
                  :disabled="changing || profile?.isExternal"
                />
              </UFormField>

              <UFormField
                :label="$t('page.profile.fields.newPassword')"
                name="newPassword"
              >
                <UInput
                  v-model="changeFormState.newPassword"
                  class="w-full"
                  type="password"
                  :placeholder="$t('ui.placeholder.input')"
                  :disabled="changing || profile?.isExternal"
                />
              </UFormField>

              <UFormField
                :label="$t('page.profile.fields.confirmPassword')"
                name="confirmPassword"
              >
                <UInput
                  v-model="changeFormState.confirmPassword"
                  class="w-full"
                  type="password"
                  :placeholder="$t('ui.placeholder.input')"
                  :disabled="changing || profile?.isExternal"
                />
              </UFormField>

              <div class="flex justify-end gap-3 pt-2">
                <UButton
                  color="primary"
                  type="submit"
                  :disabled="profile?.isExternal"
                  :loading="changing"
                >
                  {{ $t('common.confirm') }}
                </UButton>
              </div>
            </UForm>
          </UCard>
      </template>
    </UTabs>
  </div>
</template>

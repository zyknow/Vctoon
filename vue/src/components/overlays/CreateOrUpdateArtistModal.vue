<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { z } from 'zod'

import type { Artist, ArtistCreateUpdate } from '@/api/http/artist'
import { artistApi } from '@/api/http/artist'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'

type FormSubmitEvent<T> = {
  data: T
}

interface Props {
  artist?: Artist
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: Artist | null]
}>()

// 移动端判断
const { isMobile } = useIsMobile()

const loading = ref(false)
const state = reactive<ArtistCreateUpdate>({ name: '' })
const isEdit = computed(() => !!props.artist?.id)
const toast = useToast()

// 表单标题
const title = computed(() =>
  isEdit.value ? $t('page.artist.edit.title') : $t('page.artist.create.title'),
)

// Zod 验证规则
const schema = z.object({
  name: z
    .string()
    .min(1, $t('page.artist.create.validation.nameRequired'))
    .max(50, $t('page.artist.create.validation.nameLength')),
})

type Schema = z.output<typeof schema>

// 初始化表单数据
watch(
  () => props.artist,
  (artist) => {
    if (artist) {
      state.name = artist.name || ''
    } else {
      state.name = ''
    }
  },
  { immediate: true },
)

async function onSubmit(event: FormSubmitEvent<Schema>) {
  if (loading.value) return // 防重复提交

  try {
    loading.value = true

    if (isEdit.value && props.artist?.id) {
      const updated = await artistApi.update(event.data, props.artist.id)
      // 静默成功，无需提示
      emit('close', updated as Artist)
    } else {
      const created = await artistApi.create(event.data)
      // 静默成功，无需提示
      emit('close', created as Artist)
    }
  } catch {
    // 失败必须提示
    toast.add({
      title: isEdit.value
        ? $t('page.artist.edit.messages.error')
        : $t('page.artist.create.messages.error'),
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
  <UModal :title="title" :fullscreen="isMobile" :dismissible="false">
    <template #body>
      <UForm
        :schema="schema"
        :state="state"
        class="space-y-4"
        @submit="onSubmit"
      >
        <UFormField :label="$t('page.artist.create.fields.name')" name="name">
          <UInput
            v-model="state.name"
            class="w-full"
            :placeholder="$t('page.artist.create.placeholders.name')"
            maxlength="50"
            :disabled="loading"
          />
        </UFormField>

        <div class="flex justify-end gap-3">
          <UButton
            color="neutral"
            variant="ghost"
            :disabled="loading"
            @click="handleCancel"
          >
            {{ $t('page.artist.create.actions.cancel') }}
          </UButton>
          <UButton color="primary" type="submit" :loading="loading">
            {{
              isEdit
                ? $t('page.artist.edit.actions.confirm')
                : $t('page.artist.create.actions.confirm')
            }}
          </UButton>
        </div>
      </UForm>
    </template>
  </UModal>
</template>

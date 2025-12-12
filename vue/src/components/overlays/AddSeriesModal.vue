<script setup lang="ts">
import { reactive, ref } from 'vue'
import { z } from 'zod'

import type { MediumType } from '@/api/http/library'
import { mediumApi } from '@/api/http/medium'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'

interface Props {
  libraryId: string
  mediumType: MediumType
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: boolean]
}>()

const { isMobile } = useIsMobile()

const loading = ref(false)

const schema = z.object({
  title: z
    .string()
    .min(1, $t('page.library.addSeries.validation.titleRequired')),
  description: z.string().optional(),
})

type Schema = z.output<typeof schema>

const state = reactive<Schema>({
  title: '',
  description: '',
})

async function onSubmit() {
  loading.value = true
  try {
    await mediumApi.create({
      title: state.title,
      description: state.description || '',
      libraryId: props.libraryId,
      mediumType: props.mediumType,
      isSeries: true,
      cover: '', // 系列可能初始没有封面，或者后端有默认处理
    })
    emit('close', true)
  } catch (error) {
    console.error('Failed to create series:', error)
    // 错误处理通常由全局拦截器或 toast 处理
  } finally {
    loading.value = false
  }
}

function onCancel() {
  emit('close', false)
}
</script>

<template>
  <UModal
    :title="$t('page.library.addSeries.title')"
    :fullscreen="isMobile"
    :dismissible="false"
  >
    <template #body>
      <UForm
        :schema="schema"
        :state="state"
        class="flex flex-col gap-4"
        @submit="onSubmit"
      >
        <UFormField
          :label="$t('page.library.addSeries.fields.title')"
          name="title"
          required
        >
          <UInput
            v-model="state.title"
            class="w-full"
            :placeholder="$t('page.library.addSeries.placeholders.title')"
            autofocus
          />
        </UFormField>

        <UFormField
          :label="$t('page.library.addSeries.fields.description')"
          name="description"
        >
          <UTextarea
            v-model="state.description"
            class="w-full"
            :placeholder="$t('page.library.addSeries.placeholders.description')"
            :rows="3"
          />
        </UFormField>

        <div class="flex justify-end gap-3">
          <UButton variant="ghost" :disabled="loading" @click="onCancel">
            {{ $t('common.cancel') }}
          </UButton>
          <UButton type="submit" :loading="loading">
            {{ $t('common.confirm') }}
          </UButton>
        </div>
      </UForm>
    </template>
  </UModal>
</template>

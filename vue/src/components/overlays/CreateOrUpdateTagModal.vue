<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { z } from 'zod'

import type { Tag, TagCreateUpdate } from '@/api/http/tag'
import { tagApi } from '@/api/http/tag'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'

type FormSubmitEvent<T> = {
  data: T
}

interface Props {
  tag?: Tag
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: Tag | null]
}>()

// 移动端判断
const { isMobile } = useIsMobile()

const loading = ref(false)
const state = reactive<TagCreateUpdate>({ name: '' })
const isEdit = computed(() => !!props.tag?.id)
const toast = useToast()

// 表单标题
const title = computed(() =>
  isEdit.value ? $t('page.tag.edit.title') : $t('page.tag.create.title'),
)

// Zod 验证规则
const schema = z.object({
  name: z
    .string()
    .min(1, $t('page.tag.create.validation.nameRequired'))
    .max(50, $t('page.tag.create.validation.nameLength')),
})

type Schema = z.output<typeof schema>

// 初始化表单数据
watch(
  () => props.tag,
  (tag) => {
    if (tag) {
      state.name = tag.name || ''
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

    if (isEdit.value && props.tag?.id) {
      const updated = await tagApi.update(event.data, props.tag.id)
      // 静默成功，无需提示
      emit('close', updated as Tag)
    } else {
      const created = await tagApi.create(event.data)
      // 静默成功，无需提示
      emit('close', created as Tag)
    }
  } catch {
    // 失败必须提示
    toast.add({
      title: isEdit.value
        ? $t('page.tag.edit.messages.error')
        : $t('page.tag.create.messages.error'),
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
        <UFormField :label="$t('page.tag.create.fields.name')" name="name">
          <UInput
            v-model="state.name"
            class="w-full"
            :placeholder="$t('page.tag.create.placeholders.name')"
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
            {{ $t('page.tag.create.actions.cancel') }}
          </UButton>
          <UButton color="primary" type="submit" :loading="loading">
            {{
              isEdit
                ? $t('page.tag.edit.actions.confirm')
                : $t('page.tag.create.actions.confirm')
            }}
          </UButton>
        </div>
      </UForm>
    </template>
  </UModal>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { z } from 'zod'

import type {
  Library,
  LibraryCreateUpdate,
  MediumType,
} from '@/api/http/library'
import { libraryApi } from '@/api/http/library'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'

import PathBrowserModal from './SystemPathSelectModal.vue'

type FormSubmitEvent<T> = {
  data: T
}

interface Props {
  library?: Library
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: Library | null]
}>()

// 移动端判断
const { isMobile } = useIsMobile()

const loading = ref(false)
const isEdit = computed(() => !!props.library?.id)

const toast = useToast()
const overlay = useOverlay()

// 表单标题
const title = computed(() =>
  isEdit.value
    ? $t('page.library.create.editTitle')
    : $t('page.library.create.title'),
)

// 媒体类型选项
const mediumTypeOptions = [
  { label: $t('page.library.create.medium.image'), value: 0 },
  { label: $t('page.library.create.medium.video'), value: 1 },
]

// Zod 验证规则
const schema = z.object({
  name: z
    .string()
    .min(1, $t('page.library.create.validation.nameRequired'))
    .max(100, $t('page.library.create.validation.nameLength')),
  mediumType: z.number().refine((val) => val === 0 || val === 1, {
    message: $t('page.library.create.validation.mediumTypeRequired'),
  }),
  paths: z
    .array(z.string())
    .min(1, $t('page.library.create.validation.pathsRequired')),
})

type Schema = z.output<typeof schema>

const state = reactive<LibraryCreateUpdate>({
  name: '',
  mediumType: 0 as MediumType,
  paths: [],
})

// 初始化表单数据
watch(
  () => props.library,
  (library) => {
    if (library) {
      state.name = library.name || ''
      state.mediumType =
        typeof library.mediumType === 'number' ? library.mediumType : 0
      state.paths =
        library.paths && Array.isArray(library.paths) ? [...library.paths] : []
    } else {
      state.name = ''
      state.mediumType = 0
      state.paths = []
    }
  },
  { immediate: true },
)

// 打开路径浏览器
const pathBrowserModal = overlay.create(PathBrowserModal)

async function handleBrowsePaths() {
  const result = await pathBrowserModal.open({
    selectedPaths: state.paths || [],
  })

  if (result && Array.isArray(result)) {
    state.paths = result
  }
}

// 移除单个路径
function handleRemovePath(path: string) {
  if (!state.paths) return
  const index = state.paths.indexOf(path)
  if (index > -1) {
    state.paths.splice(index, 1)
  }
}

async function onSubmit(event: FormSubmitEvent<Schema>) {
  if (loading.value) return

  try {
    loading.value = true

    const payload: LibraryCreateUpdate = {
      name: event.data.name,
      mediumType: event.data.mediumType,
      paths: event.data.paths,
    }

    if (isEdit.value && props.library?.id) {
      const updated = await libraryApi.update(payload, props.library.id)
      emit('close', updated as Library)
    } else {
      const created = await libraryApi.create(payload)
      emit('close', created as Library)
    }
  } catch {
    toast.add({
      title: $t('page.library.create.messages.error'),
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
        <UFormField :label="$t('page.library.create.fields.name')" name="name">
          <UInput
            v-model="state.name"
            class="w-full"
            :placeholder="$t('page.library.create.placeholders.name')"
            maxlength="100"
            :disabled="loading"
          />
        </UFormField>

        <UFormField
          :label="$t('page.library.create.fields.mediumType')"
          name="mediumType"
        >
          <USelectMenu
            v-model="state.mediumType"
            class="w-full"
            :items="mediumTypeOptions"
            :placeholder="$t('page.library.create.placeholders.mediumType')"
            value-key="value"
            :disabled="loading"
          />
        </UFormField>

        <UFormField
          :label="$t('page.library.create.fields.paths')"
          name="paths"
        >
          <div class="space-y-3">
            <!-- 已选择的路径列表 -->
            <div v-if="state.paths && state.paths.length > 0">
              <div class="text-muted mb-2 text-sm">
                {{
                  $t('page.library.create.hints.selectedCount', {
                    count: state.paths.length,
                  })
                }}
              </div>
              <div
                class="bg-elevated/50 max-h-32 space-y-1 overflow-y-auto rounded-md border p-2"
              >
                <div
                  v-for="path in state.paths"
                  :key="path"
                  class="hover:bg-elevated flex items-center justify-between gap-2 rounded px-2 py-1 text-sm"
                >
                  <span class="flex-1 truncate" :title="path">{{ path }}</span>
                  <UButton
                    icon="i-lucide-x"
                    color="neutral"
                    variant="ghost"
                    size="xs"
                    square
                    :disabled="loading"
                    @click="handleRemovePath(path)"
                  />
                </div>
              </div>
            </div>

            <!-- 浏览按钮 -->
            <UButton
              icon="i-lucide-folder-open"
              color="neutral"
              variant="soft"
              block
              :disabled="loading"
              @click="handleBrowsePaths"
            >
              {{ $t('page.library.create.actions.browse') }}
            </UButton>
          </div>
        </UFormField>

        <div class="flex justify-end gap-3">
          <UButton
            color="neutral"
            variant="ghost"
            :disabled="loading"
            @click="handleCancel"
          >
            {{ $t('page.library.create.actions.cancel') }}
          </UButton>
          <UButton color="primary" type="submit" :loading="loading">
            {{ $t('page.library.create.actions.confirm') }}
          </UButton>
        </div>
      </UForm>
    </template>
  </UModal>
</template>

<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import type { TagCreateUpdate } from '@vben/api'

import { computed, reactive, ref, watch } from 'vue'

import { tagApi } from '@vben/api'
import { useIsMobile } from '@vben/hooks'
import { MdiTag } from '@vben/icons'

import { ElMessage } from 'element-plus'

import { $t } from '#/locales'

interface Props {
  visible: boolean
  tagId?: string
}

interface Emits {
  (e: 'update:visible', value: boolean): void
  (e: 'success'): void
}

const props = withDefaults(defineProps<Props>(), {
  tagId: '',
})

const emit = defineEmits<Emits>()

const mobile = useIsMobile()

// 表单引用和数据
const formRef = ref<FormInstance>()
const loading = ref(false)
const isEdit = ref(false)

const form = reactive<TagCreateUpdate>({
  name: '',
})

// 表单验证规则
const rules: FormRules<TagCreateUpdate> = {
  name: [
    {
      required: true,
      message: $t('page.tag.create.validation.nameRequired'),
      trigger: 'blur',
    },
    {
      min: 1,
      max: 50,
      message: $t('page.tag.create.validation.nameLength'),
      trigger: 'blur',
    },
  ],
}

// 计算属性
const dialogTitle = computed(() => {
  if (isEdit.value && form.name) {
    return `${$t('page.tag.edit.title')} - ${form.name}`
  }
  return isEdit.value ? $t('page.tag.edit.title') : $t('page.tag.create.title')
})

// 方法
const loadTagData = async (id: string) => {
  try {
    loading.value = true
    const tag = await tagApi.getById(id)
    form.name = tag.name || ''
  } catch (error) {
    console.error('加载标签数据失败:', error)
    ElMessage.error($t('page.tag.messages.loadTagError'))
  } finally {
    loading.value = false
  }
}

const resetForm = () => {
  form.name = ''
  formRef.value?.resetFields()
}

const handleClose = () => {
  emit('update:visible', false)
  resetForm()
}

const handleSubmit = async () => {
  if (!formRef.value) return

  const valid = await formRef.value.validate()
  if (!valid) return

  try {
    loading.value = true

    if (isEdit.value && props.tagId) {
      // 更新标签
      await tagApi.update(form, props.tagId)
      ElMessage.success($t('page.tag.edit.messages.success'))
    } else {
      // 创建标签
      await tagApi.create(form)
      ElMessage.success($t('page.tag.create.messages.success'))
    }

    emit('success')
    handleClose()
  } catch (error) {
    console.error('保存标签失败:', error)
    const errorMessage = isEdit.value
      ? $t('page.tag.edit.messages.error')
      : $t('page.tag.create.messages.error')
    ElMessage.error(errorMessage)
  } finally {
    loading.value = false
  }
}

// 监听属性变化
watch(
  () => props.visible,
  (newVisible) => {
    if (newVisible) {
      isEdit.value = !!props.tagId
      if (props.tagId) {
        loadTagData(props.tagId)
      } else {
        resetForm()
      }
    }
  },
)
</script>

<template>
  <el-dialog
    :model-value="visible"
    :title="dialogTitle"
    :close-on-click-modal="false"
    :fullscreen="mobile.isMobile.value"
    width="500px"
    @update:model-value="handleClose"
  >
    <!-- 对话框头部图标 -->
    <template #header>
      <div class="flex items-center">
        <MdiTag class="text-primary mr-2 text-xl" />
        <span class="text-foreground text-lg font-medium">
          {{ dialogTitle }}
        </span>
      </div>
    </template>

    <!-- 表单内容 -->
    <div v-loading="loading" class="min-h-32 py-4">
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="80px"
        label-position="left"
      >
        <el-form-item :label="$t('page.tag.create.fields.name')" prop="name">
          <el-input
            v-model="form.name"
            :placeholder="$t('page.tag.create.placeholders.name')"
            maxlength="50"
            show-word-limit
            clearable
          />
        </el-form-item>
      </el-form>
    </div>

    <!-- 对话框底部 -->
    <template #footer>
      <div class="flex justify-end gap-3">
        <el-button @click="handleClose">
          {{ $t('page.tag.create.actions.cancel') }}
        </el-button>
        <el-button type="primary" :loading="loading" @click="handleSubmit">
          {{
            isEdit
              ? $t('page.tag.edit.actions.confirm')
              : $t('page.tag.create.actions.confirm')
          }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

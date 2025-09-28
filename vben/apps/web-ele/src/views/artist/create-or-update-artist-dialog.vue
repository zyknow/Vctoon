<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import type { Artist, ArtistCreateUpdate } from '@vben/api'

import { computed, reactive, ref, watch } from 'vue'

import { artistApi } from '@vben/api'

import { $t } from '#/locales'

import { useDialogContext } from '../../hooks/useDialogService'

interface InjectedProps {
  artist?: Artist
  registerDirtyChecker: (fn: () => boolean) => void
  markClean?: () => void
}
const props = defineProps<InjectedProps>()

const { close, resolve } = useDialogContext<Artist>()

const formRef = ref<FormInstance>()
const loading = ref(false)
const form = reactive<ArtistCreateUpdate>({ name: '' })
let initialSnapshot = ''
const isDirty = ref(false)
const isEdit = computed(() => !!props.artist?.id)

const rules: FormRules<ArtistCreateUpdate> = {
  name: [
    {
      required: true,
      message: $t('page.artist.create.validation.nameRequired'),
      trigger: 'blur',
    },
    {
      min: 1,
      max: 50,
      message: $t('page.artist.create.validation.nameLength'),
      trigger: 'blur',
    },
  ],
}

async function ensureData() {
  if (props.artist) {
    form.name = props.artist.name || ''
    initialSnapshot = JSON.stringify(form)
    isDirty.value = false
    return
  }
  resetForm()
  initialSnapshot = JSON.stringify(form)
  isDirty.value = false
}
watch(
  () => ({ ...form }),
  () => {
    const current = JSON.stringify(form)
    isDirty.value = current !== initialSnapshot
  },
  { deep: true },
)

// 注册脏检查
props.registerDirtyChecker(() => isDirty.value)

const resetForm = () => {
  form.name = ''
  formRef.value?.resetFields()
}

const submit = async () => {
  if (loading.value) return
  if (!formRef.value) return
  const valid = await formRef.value.validate()
  if (!valid) return
  try {
    loading.value = true
    if (isEdit.value && props.artist?.id) {
      const updated = await artistApi.update(form, props.artist.id)
      // 成功提示交由全局拦截器处理
      // 提交成功后禁用脏检查，避免关闭确认
      props.markClean?.()
      isDirty.value = false
      initialSnapshot = JSON.stringify(form)
      resolve(updated as Artist)
    } else {
      const created = await artistApi.create(form)
      // 成功提示交由全局拦截器处理
      props.markClean?.()
      isDirty.value = false
      initialSnapshot = JSON.stringify(form)
      resolve(created as Artist)
    }
  } catch (error) {
    console.error('保存艺术家失败:', error)
    // 失败提示交由全局拦截器处理
  } finally {
    loading.value = false
  }
}

watch(() => props.artist, ensureData, { immediate: true })
</script>

<template>
  <div class="space-y-4">
    <div>
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="80px"
        label-position="left"
      >
        <el-form-item :label="$t('page.artist.create.fields.name')" prop="name">
          <el-input
            v-model="form.name"
            :placeholder="$t('page.artist.create.placeholders.name')"
            maxlength="50"
            show-word-limit
            clearable
          />
        </el-form-item>
      </el-form>
    </div>

    <div class="flex justify-end gap-3 pt-2">
      <el-button :disabled="loading" @click="close()">
        {{ $t('page.artist.create.actions.cancel') }}
      </el-button>
      <el-button type="primary" :loading="loading" @click="submit">
        {{
          isEdit
            ? $t('page.artist.edit.actions.confirm')
            : $t('page.artist.create.actions.confirm')
        }}
      </el-button>
    </div>
  </div>
</template>

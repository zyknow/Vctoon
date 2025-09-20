<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'
import type {
  Library,
  LibraryCreateUpdate,
} from 'node_modules/@vben/api/src/vctoon/library/typing'

import { reactive, ref, watch } from 'vue'

import { libraryApi, systemApi } from '@vben/api'
import { useIsMobile } from '@vben/hooks'

import { ElMessage } from 'element-plus'

import { $t } from '#/locales'

// v-model
const props = defineProps<{ modelValue: boolean }>()
const emit = defineEmits<{
  (e: 'update:modelValue', v: boolean): void
  (e: 'success', data: Library): void
}>()

const visible = ref(false)
watch(
  () => props.modelValue,
  (v) => (visible.value = v),
  { immediate: true },
)
watch(visible, (v) => emit('update:modelValue', v))

const mobile = useIsMobile()

// 表单
const formRef = ref<FormInstance>()
const form = reactive<LibraryCreateUpdate>({
  name: '',
  mediumType: 0,
  paths: [],
})

const rules = reactive<FormRules<LibraryCreateUpdate>>({
  name: [
    {
      required: true,
      message: $t('page.library.create.placeholders.name'),
      trigger: 'blur',
    },
  ],
  mediumType: [
    {
      required: true,
      message: $t('page.library.create.placeholders.mediumType'),
      trigger: 'change',
    },
  ],
  paths: [
    {
      validator: (_r, val, cb) => {
        if (!val || (Array.isArray(val) && val.length === 0)) {
          return cb(new Error($t('page.library.create.actions.requirePath')))
        }
        cb()
      },
      trigger: 'change',
    },
  ],
})

// 媒体类型（根据后端枚举 0/1）
const mediumTypeOptions = [
  { key: 'image' as const, value: 0 },
  { key: 'video' as const, value: 1 },
]

// 路径树懒加载
type PathNode = {
  children?: PathNode[]
  isLeaf?: boolean
  label: string
  value: string
}
const treeProps = {
  children: 'children',
  isLeaf: 'isLeaf',
  label: 'label',
  value: 'value',
}
function makeNode(fullPath: string): PathNode {
  const parts = fullPath.split(/\\|\//).filter(Boolean)
  const base = parts.at(-1) ?? fullPath
  return { label: base, value: fullPath }
}
async function loadPathNodes(node: any, resolve: (data: PathNode[]) => void) {
  const parent = node?.level === 0 ? '' : (node.data?.value as string)
  const list = await systemApi.getSystemPaths(parent)
  const nodes = (Array.isArray(list) ? list : []).map((p) => makeNode(p))
  resolve(nodes)
}

// 提交
const submitting = ref(false)
async function onSubmit() {
  if (!formRef.value) return
  const valid = await formRef.value.validate()
  if (!valid) return
  try {
    submitting.value = true
    const created = await libraryApi.create({
      name: form.name,
      mediumType: form.mediumType,
      paths: form.paths && form.paths.length > 0 ? form.paths : undefined,
    })
    ElMessage.success($t('page.library.create.actions.success'))
    emit('success', created as unknown as Library)
    visible.value = false

    resetForm()
  } finally {
    submitting.value = false
  }
}

function onCancel() {
  visible.value = false
  resetForm()
}

function resetForm() {
  form.name = ''
  form.mediumType = 0
  form.paths = []
  // 重置树选择，无需预置数据（lazy 模式）
}

watch(
  () => visible.value,
  (v) => {
    if (v) {
      // 打开时无需预加载，树为懒加载
    }
  },
)
</script>

<template>
  <el-dialog
    v-model="visible"
    :fullscreen="mobile.isMobile.value"
    :title="$t('page.library.create.title')"
    :close-on-click-modal="false"
  >
    <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
      <el-form-item :label="$t('page.library.create.fields.name')" prop="name">
        <el-input
          v-model="form.name"
          :placeholder="$t('page.library.create.placeholders.name')"
        />
      </el-form-item>

      <el-form-item
        :label="$t('page.library.create.fields.mediumType')"
        prop="mediumType"
      >
        <el-select
          v-model="form.mediumType"
          :placeholder="$t('page.library.create.placeholders.mediumType')"
        >
          <el-option
            v-for="opt in mediumTypeOptions"
            :key="opt.value"
            :label="$t(`page.library.create.medium.${opt.key}`)"
            :value="opt.value"
          />
        </el-select>
      </el-form-item>

      <el-form-item
        :label="$t('page.library.create.fields.paths')"
        prop="paths"
      >
        <el-tree-select
          v-model="form.paths"
          multiple
          filterable
          check-strictly
          :lazy="true"
          :load="loadPathNodes"
          :props="treeProps"
          node-key="value"
          :placeholder="$t('page.library.create.placeholders.paths')"
          collapse-tags
          collapse-tags-tooltip
          clearable
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <el-button @click="onCancel">
        {{ $t('page.library.create.actions.cancel') }}
      </el-button>
      <el-button type="primary" :loading="submitting" @click="onSubmit">
        {{ $t('page.library.create.actions.confirm') }}
      </el-button>
    </template>
  </el-dialog>
</template>

<style scoped>
.el-select,
.el-tree-select {
  width: 100%;
}
</style>

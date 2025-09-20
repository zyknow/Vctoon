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

type Props = {
  library?: Library | null | undefined
  modelValue: boolean
}
const props = withDefaults(defineProps<Props>(), {
  library: null,
})
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
watch(visible, async (v) => {
  emit('update:modelValue', v)
  // 打开时获取最新 library 数据
  if (v && props.library?.id) {
    try {
      const latest = await libraryApi.getById(props.library.id)
      // 更新表单
      form.name = latest.name ?? ''
      form.mediumType =
        typeof latest.mediumType === 'number' ? latest.mediumType : 0
      form.paths =
        latest.paths && Array.isArray(latest.paths) ? [...latest.paths] : []
    } catch {
      // 获取失败时可选提示
      ElMessage.error($t('page.library.edit.actions.fetchFailed'))
    }
  }
})
const mobile = useIsMobile()
// 表单
const formRef = ref<FormInstance>()
const form = reactive<LibraryCreateUpdate>({
  name: '',
  mediumType: 0,
  paths: [],
})

watch(
  () => props.library,
  (lib) => {
    if (!lib) {
      // 重置表单为初始状态
      form.name = ''
      form.mediumType = 0
      form.paths = []
      return
    }
    form.name = lib.name ?? ''
    form.mediumType = typeof lib.mediumType === 'number' ? lib.mediumType : 0
    // 确保 paths 正确绑定
    form.paths = lib.paths && Array.isArray(lib.paths) ? [...lib.paths] : []
  },
  { immediate: true, deep: true },
)

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

  // 如果当前是根节点加载，且表单中有已选择的paths，确保这些路径的父级目录被包含
  if (node?.level === 0 && form.paths && form.paths.length > 0) {
    const selectedParents = new Set<string>()
    for (const path of form.paths) {
      const parentPath = path.slice(0, Math.max(0, path.lastIndexOf('/')))
      if (parentPath && !selectedParents.has(parentPath)) {
        selectedParents.add(parentPath)
      }
    }

    // 为每个已选择路径的父目录创建节点（如果不存在的话）
    for (const parentPath of selectedParents) {
      if (!nodes.some((n) => n.value === parentPath)) {
        nodes.push(makeNode(parentPath))
      }
    }
  }

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
    const id = (props.library as any)?.id as string | undefined
    if (!id) {
      ElMessage.error($t('page.library.edit.actions.missingId'))
      return
    }
    const updated = await libraryApi.update(
      {
        name: form.name,
        mediumType: form.mediumType,
        paths: form.paths && form.paths.length > 0 ? form.paths : undefined,
      },
      id,
    )
    ElMessage.success($t('page.library.edit.actions.success'))
    emit('success', updated as unknown as Library)
    visible.value = false
  } finally {
    submitting.value = false
  }
}

function onCancel() {
  visible.value = false
}
</script>

<template>
  <el-dialog
    v-model="visible"
    :fullscreen="mobile.isMobile.value"
    :title="$t('page.library.edit.title')"
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

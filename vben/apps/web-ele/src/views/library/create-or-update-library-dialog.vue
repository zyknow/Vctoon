<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import type { Library, LibraryCreateUpdate } from '@vben/api'

import { reactive, ref, watch } from 'vue'

import { libraryApi, systemApi } from '@vben/api'

import { useDialogContext } from '#/hooks/useDialogService'
import { $t } from '#/locales'

// 统一：传 libraryId => 编辑；不传 => 创建
interface InjectedProps {
  library?: Library
  registerDirtyChecker?: (fn: () => boolean) => void
  markClean?: () => void
}
defineOptions({ name: 'CreateOrUpdateLibraryDialog' })
// 使用 defineProps 以在运行时真正接收父级传入的 props（原先使用 declare 仅是类型声明，运行时 props 永远为空，导致编辑时无法预填）
const props = defineProps<InjectedProps>()
const { resolve, close } = useDialogContext<Library | undefined>()

const formRef = ref<FormInstance>()
const form = reactive<LibraryCreateUpdate>({
  name: '',
  mediumType: 0,
  paths: [],
})
let initialSnapshot = ''
const isDirty = ref(false)

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
        if (!val || (Array.isArray(val) && val.length === 0))
          return cb(new Error($t('page.library.create.actions.requirePath')))
        cb()
      },
      trigger: 'change',
    },
  ],
})

const mediumTypeOptions = [
  { key: 'image' as const, value: 0 },
  { key: 'video' as const, value: 1 },
]

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
async function loadPathNodes(
  node: any,
  resolveNodes: (data: PathNode[]) => void,
) {
  const parent = node?.level === 0 ? '' : (node.data?.value as string)
  const list = await systemApi.getSystemPaths(parent)
  const nodes = (Array.isArray(list) ? list : []).map((p) => makeNode(p))
  resolveNodes(nodes)
}

const submitting = ref(false)
async function submit() {
  if (submitting.value) return
  if (!formRef.value) return
  const valid = await formRef.value.validate()
  if (!valid) return
  try {
    submitting.value = true
    if (props.library?.id) {
      const updated = await libraryApi.update(
        {
          name: form.name,
          mediumType: form.mediumType,
          paths: form.paths && form.paths.length > 0 ? form.paths : undefined,
        },
        props.library.id,
      )
      // 全局拦截器已处理成功提示，这里仅负责关闭与返回数据
      props.markClean?.()
      initialSnapshot = JSON.stringify(form)
      isDirty.value = false
      resolve(updated as Library)
    } else {
      const created = await libraryApi.create({
        name: form.name,
        mediumType: form.mediumType,
        paths: form.paths && form.paths.length > 0 ? form.paths : undefined,
      })
      // 全局拦截器已处理成功提示，这里仅负责关闭与返回数据
      props.markClean?.()
      initialSnapshot = JSON.stringify(form)
      isDirty.value = false
      resolve(created as Library)
      resetForm()
    }
  } finally {
    submitting.value = false
  }
}

function resetForm() {
  form.name = ''
  form.mediumType = 0
  form.paths = []
}

function fillForm(lib: Library) {
  form.name = lib.name ?? ''
  form.mediumType = typeof lib.mediumType === 'number' ? lib.mediumType : 0
  form.paths = lib.paths && Array.isArray(lib.paths) ? [...lib.paths] : []
  initialSnapshot = JSON.stringify(form)
  isDirty.value = false
}

async function ensureData() {
  // 1. 若外部已传入完整 library，则直接填充
  if (props.library) {
    fillForm(props.library)
    // 已有实体直接填充
  }
  // 2. 仅当有 libraryId 且尚未传 library 时再请求
  // 不再通过 id 请求，服务层已预取；若未来需要兼容可在此添加 fallback
}

// 监听 libraryId 与 library（支持复用组件或服务层动态更新）
watch(() => props.library, ensureData, { immediate: true })
watch(
  () => ({ ...form }),
  () => {
    const current = JSON.stringify(form)
    isDirty.value = current !== initialSnapshot
  },
  { deep: true },
)

if (props.registerDirtyChecker) {
  props.registerDirtyChecker(() => isDirty.value)
}
</script>

<template>
  <div class="space-y-4">
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
    <div class="flex justify-end gap-3">
      <el-button :disabled="submitting" @click="close()">
        {{ $t('page.library.create.actions.cancel') }}
      </el-button>
      <el-button type="primary" :loading="submitting" @click="submit">
        {{ $t('page.library.create.actions.confirm') }}
      </el-button>
    </div>
  </div>
</template>

<style scoped>
.el-select,
.el-tree-select {
  width: 100%;
}
</style>

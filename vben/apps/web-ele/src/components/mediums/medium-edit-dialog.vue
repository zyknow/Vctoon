<script setup lang="ts">
// 1. 导入类型
import type { Artist, MediumDto, Tag } from '@vben/api'

// 2. 导入外部依赖
import { computed, onMounted, ref, watch } from 'vue'

// 3. 导入内部依赖
import { comicApi, MediumType, videoApi } from '@vben/api'
import { useUserStore } from '@vben/stores'

import { useDialogContext } from '#/hooks/useDialogService'
import { $t } from '#/locales'

// 4. 定义组件选项
defineOptions({
  name: 'MediumEditDialog',
  inheritAttrs: false,
})

// 5. 定义Props和Emits
const props = defineProps<Props>()

const emit = defineEmits<Emits>()

type Medium = MediumDto

interface Props {
  medium: Medium // 直接传完整实体
  registerDirtyChecker?: (fn: () => boolean) => void
  markClean?: () => void
}

interface Emits {
  (e: 'updated', medium: Medium): void
}
// 新增：获取对话上下文（用于 resolve/close）
const dialog = useDialogContext<Medium | undefined>()

// 6. 响应式数据
const mediumState = ref<Medium | null>(null)
let initialSnapshot = ''
const isDirty = ref(false)
const loading = ref(false)
const initialLoading = ref(true)

// 使用 userStore 获取 artists 和 tags 数据
const userStore = useUserStore()
const allArtists = computed(() => userStore.artists || [])
const allTags = computed(() => userStore.tags || [])

// 8. 方法
const loadMedium = async () => {
  // 直接使用传入实体（保持异步结构以便后续扩展）
  mediumState.value = props.medium
  initialSnapshot = JSON.stringify(mediumState.value)
  isDirty.value = false
}

const loadOptions = async () => {
  try {
    // 使用 userStore 确保数据已加载（支持缓存机制）
    await Promise.all([userStore.reloadArtists(), userStore.reloadTags()])
  } catch (error) {
    console.error('加载选项失败:', error)
    // 全局拦截器处理错误提示
  }
}

// 9. 初始化
onMounted(async () => {
  initialLoading.value = true
  try {
    await Promise.all([loadMedium(), loadOptions()])
  } catch (error) {
    console.error('初始化失败:', error)
  } finally {
    initialLoading.value = false
  }
})

// 监听外部传入的 medium 变化（支持后续动态注入或刷新）
watch(
  () => props.medium,
  (val) => {
    if (val) {
      mediumState.value = val as Medium
      initialSnapshot = JSON.stringify(mediumState.value)
      isDirty.value = false
    }
  },
)
watch(
  () => mediumState.value,
  () => {
    if (!mediumState.value) return
    const current = JSON.stringify(mediumState.value)
    isDirty.value = current !== initialSnapshot
  },
  { deep: true },
)

if (props.registerDirtyChecker) {
  props.registerDirtyChecker(() => isDirty.value)
}

const handleSave = async () => {
  if (loading.value) return
  if (!mediumState.value) return
  loading.value = true
  try {
    const api =
      mediumState.value.mediumType === MediumType.Comic ? comicApi : videoApi
    await api.update(mediumState.value as any, mediumState.value.id)

    // 更新作者
    const artistIds = mediumState.value.artists?.map((a: Artist) => a.id) || []
    await api.updateArtists(mediumState.value.id, artistIds)

    // 更新标签
    const tagIds = mediumState.value.tags?.map((t: Tag) => t.id) || []
    await api.updateTags(mediumState.value.id, tagIds)

    // 成功提示交给全局拦截器，这里仅刷新数据
    // 重新获取更新后的数据
    const updatedMedium = await api.getById(mediumState.value.id)
    props.markClean?.()
    initialSnapshot = JSON.stringify(updatedMedium)
    isDirty.value = false
    emit('updated', updatedMedium as Medium)
    dialog.resolve(updatedMedium as Medium)
  } catch (error) {
    console.error('更新失败:', error)
    // 全局拦截器处理错误提示
  } finally {
    loading.value = false
  }
}

const handleCoverUpload = async (file: File) => {
  if (!mediumState.value) return
  try {
    const updateCoverApi =
      mediumState.value.mediumType === MediumType.Comic ? comicApi : videoApi
    const updatedMedium = await updateCoverApi.updateCover(
      mediumState.value.id,
      file,
    )
    // 成功提示交给全局拦截器
    emit('updated', updatedMedium as Medium)
  } catch (error) {
    console.error('封面更新失败:', error)
    // 全局拦截器处理错误提示
  }
}

const beforeCoverUpload = (file: File) => {
  const isImage = file.type.startsWith('image/')
  const isLt10M = file.size / 1024 / 1024 < 10

  if (!isImage) {
    // 校验失败：图片类型限制（局部不走全局拦截器）
    return false
  }
  if (!isLt10M) {
    // 校验失败：大小限制（局部不走全局拦截器）
    return false
  }
  return true
}
</script>

<template>
  <div class="medium-edit-content">
    <div v-if="initialLoading" class="py-4 text-center">
      <el-text>{{ $t('common.loading') }}</el-text>
    </div>
    <el-form
      v-else-if="mediumState"
      :model="mediumState"
      label-width="80px"
      label-position="left"
    >
      <el-form-item :label="$t('page.mediums.edit.titleLabel')">
        <el-input
          v-model="mediumState.title"
          :placeholder="$t('page.mediums.edit.titlePlaceholder')"
          maxlength="200"
          show-word-limit
        />
      </el-form-item>

      <el-form-item :label="$t('page.mediums.edit.description')">
        <el-input
          v-model="mediumState.description"
          type="textarea"
          :placeholder="$t('page.mediums.edit.descriptionPlaceholder')"
          :rows="3"
          maxlength="1000"
          show-word-limit
        />
      </el-form-item>

      <el-form-item :label="$t('page.mediums.edit.artists')">
        <el-select
          v-model="mediumState.artists"
          multiple
          filterable
          :placeholder="$t('page.mediums.edit.artistsPlaceholder')"
          class="w-full"
          value-key="id"
        >
          <el-option
            v-for="artist in allArtists"
            :key="artist.id"
            :label="artist.name"
            :value="artist"
          />
        </el-select>
      </el-form-item>

      <el-form-item :label="$t('page.mediums.edit.tags')">
        <el-select
          v-model="mediumState.tags"
          multiple
          filterable
          :placeholder="$t('page.mediums.edit.tagsPlaceholder')"
          class="w-full"
          value-key="id"
        >
          <el-option
            v-for="tag in allTags"
            :key="tag.id"
            :label="tag.name"
            :value="tag"
          />
        </el-select>
      </el-form-item>

      <el-form-item :label="$t('page.mediums.edit.cover')">
        <el-upload
          :before-upload="beforeCoverUpload"
          :http-request="({ file }: any) => handleCoverUpload(file as File)"
          :show-file-list="false"
          accept="image/*"
        >
          <el-button type="primary">
            {{ $t('page.mediums.edit.uploadCover') }}
          </el-button>
          <template #tip>
            <div class="text-muted-foreground mt-2 text-xs">
              {{ $t('page.mediums.edit.coverTip') }}
            </div>
          </template>
        </el-upload>
      </el-form-item>
    </el-form>

    <div class="mt-6 flex justify-end gap-3">
      <el-button :disabled="loading" @click="dialog.close()">
        {{ $t('common.cancel') }}
      </el-button>
      <el-button type="primary" :loading="loading" @click="handleSave">
        {{ $t('common.save') }}
      </el-button>
    </div>
  </div>
</template>

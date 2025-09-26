<script setup lang="ts">
// 1. 导入类型
import type { Artist, Comic, Tag, Video } from '@vben/api'

// 2. 导入外部依赖
import { computed, ref, watch } from 'vue'

// 3. 导入内部依赖
import { artistApi, comicApi, MediumType, tagApi, videoApi } from '@vben/api'

import { ElMessage } from 'element-plus'

import { $t } from '#/locales'

// 4. 定义组件选项
defineOptions({
  name: 'MediumEditDialog',
  inheritAttrs: false,
})

// 5. 定义Props和Emits
const props = defineProps<Props>()

const emit = defineEmits<Emits>()

type Medium = Comic | Video

interface Props {
  modelValue: boolean
  medium?: Medium
  mediumType: MediumType
}

interface Emits {
  (e: 'update:modelValue', value: boolean): void
  (e: 'updated', medium: Medium): void
}

// 6. 响应式数据
const formData = ref({
  title: '',
  description: '',
  artistIds: [] as string[],
  tagIds: [] as string[],
})

const allArtists = ref<Artist[]>([])
const allTags = ref<Tag[]>([])
const loading = ref(false)

// 7. 计算属性
const dialogVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})

// 8. 方法
const loadOptions = async () => {
  try {
    const [artistsResult, tagsResult] = await Promise.all([
      artistApi.getPage({ maxResultCount: 1000 }),
      tagApi.getAllTags(),
    ])
    allArtists.value = artistsResult.items
    allTags.value = tagsResult
  } catch (error) {
    console.error('加载选项失败:', error)
    ElMessage.error($t('common.loadFailed'))
  }
}

const initFormData = () => {
  if (!props.medium) return

  formData.value = {
    title: props.medium.title || '',
    description: props.medium.description || '',
    artistIds: props.medium.artists?.map((a: Artist) => a.id) || [],
    tagIds: props.medium.tags?.map((t: Tag) => t.id) || [],
  }
}

// 9. 监听器
watch(
  () => props.modelValue,
  (visible) => {
    if (visible) {
      loadOptions()
      initFormData()
    }
  },
)

const handleSave = async () => {
  if (!props.medium) return

  loading.value = true
  try {
    const currentArtistIds = (
      props.medium.artists?.map((a: Artist) => a.id) || []
    ).sort()
    const currentTagIds = (
      props.medium.tags?.map((t: Tag) => t.id) || []
    ).sort()

    // 更新作者
    if (
      JSON.stringify(formData.value.artistIds.sort()) !==
      JSON.stringify(currentArtistIds)
    ) {
      const updateArtistsApi =
        props.mediumType === MediumType.Comic ? comicApi : videoApi
      await updateArtistsApi.updateArtists(
        props.medium.id,
        formData.value.artistIds,
      )
    }

    // 更新标签
    if (
      JSON.stringify(formData.value.tagIds.sort()) !==
      JSON.stringify(currentTagIds)
    ) {
      const updateTagsApi =
        props.mediumType === MediumType.Comic ? comicApi : videoApi
      await updateTagsApi.updateTags(props.medium.id, formData.value.tagIds)
    }

    ElMessage.success($t('common.updateSuccess'))
    // 重新获取更新后的数据
    const api = props.mediumType === MediumType.Comic ? comicApi : videoApi
    const updatedMedium = await api.getById(props.medium.id)
    emit('updated', updatedMedium as Medium)
    dialogVisible.value = false
  } catch (error) {
    console.error('更新失败:', error)
    ElMessage.error($t('common.updateFailed'))
  } finally {
    loading.value = false
  }
}

const handleCoverUpload = async (file: File) => {
  if (!props.medium) return

  try {
    const updateCoverApi =
      props.mediumType === MediumType.Comic ? comicApi : videoApi
    const updatedMedium = await updateCoverApi.updateCover(
      props.medium.id,
      file,
    )
    ElMessage.success($t('common.updateSuccess'))
    emit('updated', updatedMedium as Medium)
  } catch (error) {
    console.error('封面更新失败:', error)
    ElMessage.error($t('common.updateFailed'))
  }
}

const beforeCoverUpload = (file: File) => {
  const isImage = file.type.startsWith('image/')
  const isLt10M = file.size / 1024 / 1024 < 10

  if (!isImage) {
    ElMessage.error($t('common.validation.imageOnly'))
    return false
  }
  if (!isLt10M) {
    ElMessage.error($t('common.validation.fileSizeLimit'))
    return false
  }
  return true
}
</script>

<template>
  <el-dialog
    v-model="dialogVisible"
    :title="$t('page.mediums.edit.title')"
    width="600px"
    append-to-body
    :close-on-click-modal="false"
  >
    <el-form
      v-if="medium"
      :model="formData"
      label-width="80px"
      label-position="left"
    >
      <el-form-item :label="$t('page.mediums.edit.titleLabel')">
        <el-input
          v-model="formData.title"
          :placeholder="$t('page.mediums.edit.titlePlaceholder')"
          maxlength="200"
          show-word-limit
        />
      </el-form-item>

      <el-form-item :label="$t('page.mediums.edit.description')">
        <el-input
          v-model="formData.description"
          type="textarea"
          :placeholder="$t('page.mediums.edit.descriptionPlaceholder')"
          :rows="3"
          maxlength="1000"
          show-word-limit
        />
      </el-form-item>

      <el-form-item :label="$t('page.mediums.edit.artists')">
        <el-select
          v-model="formData.artistIds"
          multiple
          filterable
          :placeholder="$t('page.mediums.edit.artistsPlaceholder')"
          class="w-full"
        >
          <el-option
            v-for="artist in allArtists"
            :key="artist.id"
            :label="artist.name"
            :value="artist.id"
          />
        </el-select>
      </el-form-item>

      <el-form-item :label="$t('page.mediums.edit.tags')">
        <el-select
          v-model="formData.tagIds"
          multiple
          filterable
          :placeholder="$t('page.mediums.edit.tagsPlaceholder')"
          class="w-full"
        >
          <el-option
            v-for="tag in allTags"
            :key="tag.id"
            :label="tag.name"
            :value="tag.id"
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

    <template #footer>
      <div class="flex justify-end gap-3">
        <el-button @click="dialogVisible = false">
          {{ $t('common.cancel') }}
        </el-button>
        <el-button type="primary" :loading="loading" @click="handleSave">
          {{ $t('common.save') }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

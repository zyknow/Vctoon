<script setup lang="ts">
// 1. 导入类型
import type { Artist, MediumDto, Tag } from '@vben/api'

// 2. 导入外部依赖
import { onMounted, ref } from 'vue'

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

type Medium = MediumDto

interface Props {
  mediumId: string
  mediumType: MediumType
  onClose?: () => void
}

interface Emits {
  (e: 'updated', medium: Medium): void
  (e: 'close'): void
}

// 6. 响应式数据
const medium = ref<Medium | null>(null)
const allArtists = ref<Artist[]>([])
const allTags = ref<Tag[]>([])
const loading = ref(false)
const initialLoading = ref(true)

// 8. 方法
const loadMedium = async () => {
  try {
    const api = props.mediumType === MediumType.Comic ? comicApi : videoApi
    medium.value = await api.getById(props.mediumId)
  } catch (error) {
    console.error('加载媒体数据失败:', error)
    ElMessage.error($t('common.loadFailed'))
  }
}

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

const handleSave = async () => {
  if (!medium.value) return

  loading.value = true
  try {
    const api = props.mediumType === MediumType.Comic ? comicApi : videoApi

    // 直接使用 medium 对象更新
    await api.update(medium.value as any, props.mediumId)

    // 更新作者
    const artistIds = medium.value.artists?.map((a: Artist) => a.id) || []
    await api.updateArtists(props.mediumId, artistIds)

    // 更新标签
    const tagIds = medium.value.tags?.map((t: Tag) => t.id) || []
    await api.updateTags(props.mediumId, tagIds)

    ElMessage.success($t('common.updateSuccess'))
    // 重新获取更新后的数据
    const updatedMedium = await api.getById(props.mediumId)
    emit('updated', updatedMedium as Medium)
    emit('close')
  } catch (error) {
    console.error('更新失败:', error)
    ElMessage.error($t('common.updateFailed'))
  } finally {
    loading.value = false
  }
}

const handleCoverUpload = async (file: File) => {
  if (!medium.value) return

  try {
    const updateCoverApi =
      props.mediumType === MediumType.Comic ? comicApi : videoApi
    const updatedMedium = await updateCoverApi.updateCover(props.mediumId, file)
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
  <div class="medium-edit-content">
    <div v-if="initialLoading" class="py-4 text-center">
      <el-text>{{ $t('common.loading') }}</el-text>
    </div>
    <el-form
      v-else-if="medium"
      :model="medium"
      label-width="80px"
      label-position="left"
    >
      <el-form-item :label="$t('page.mediums.edit.titleLabel')">
        <el-input
          v-model="medium.title"
          :placeholder="$t('page.mediums.edit.titlePlaceholder')"
          maxlength="200"
          show-word-limit
        />
      </el-form-item>

      <el-form-item :label="$t('page.mediums.edit.description')">
        <el-input
          v-model="medium.description"
          type="textarea"
          :placeholder="$t('page.mediums.edit.descriptionPlaceholder')"
          :rows="3"
          maxlength="1000"
          show-word-limit
        />
      </el-form-item>

      <el-form-item :label="$t('page.mediums.edit.artists')">
        <el-select
          v-model="medium.artists"
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
          v-model="medium.tags"
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
      <el-button @click="emit('close')">
        {{ $t('common.cancel') }}
      </el-button>
      <el-button type="primary" :loading="loading" @click="handleSave">
        {{ $t('common.save') }}
      </el-button>
    </div>
  </div>
</template>

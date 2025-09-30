<script setup lang="ts">
// 1. 导入类型
import type { Artist, ComicImage, MediumDto, Tag } from '@vben/api'

// 2. 导入外部依赖
import { computed, onMounted, ref, watch } from 'vue'

// 3. 导入内部依赖
import {
  artistApi,
  comicApi,
  mediumResourceApi,
  MediumType,
  tagApi,
  videoApi,
} from '@vben/api'
import { useAppConfig, useIsMobile } from '@vben/hooks'
import { useUserStore } from '@vben/stores'

import { ElMessage } from 'element-plus'

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
// comic 图片相关
const comicImages = ref<ComicImage[]>([])
const comicImagesLoading = ref(false)
// 当前封面对应的漫画图片（如果是从图片设置的，我们记录其 id；否则可能为空）
const selectedCoverComicImageId = ref<null | string>(null)
// 正在设置封面的图片 id（局部 loading）
const selectingCoverImageId = ref<null | string>(null)
const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
// 注意：如果直接保留为 const mobile = useIsMobile()，在模板中使用 mobile.isMobile 会得到一个 Ref 对象（始终 truthy），
// 导致布局条件判断恒为“移动端”分支。这里直接解构出 isMobile，模板会自动解包 Ref，条件判断才正确。
const { isMobile } = useIsMobile()

// 当前封面 URL（独立显示，避免与图片列表 id 不匹配时丢失）
const currentCoverUrl = computed(() => {
  if (!mediumState.value?.cover) return ''
  const coverPath = mediumResourceApi.url.getCover.format({
    cover: mediumState.value.cover,
  })
  return `${apiURL}${coverPath}`
})

function isSelectedComicImage(id: string) {
  return selectedCoverComicImageId.value === id
}

// 使用 userStore 获取 artists 和 tags 数据
const userStore = useUserStore()
const allArtists = computed(() => userStore.artists || [])
const allTags = computed(() => userStore.tags || [])

const normalizeName = (name: string) => name.trim().toLowerCase()

const artistNameMap = computed(() => {
  const map = new Map<string, Artist>()
  for (const item of allArtists.value) {
    if (!item.name) continue
    map.set(normalizeName(item.name), item)
  }
  return map
})

const tagNameMap = computed(() => {
  const map = new Map<string, Tag>()
  for (const item of allTags.value) {
    if (!item.name) continue
    map.set(normalizeName(item.name), item)
  }
  return map
})

const creatingArtistNames = new Set<string>()
const creatingTagNames = new Set<string>()

const ensureArtistEntry = async (
  entry: Artist | string,
): Promise<Artist | null> => {
  if (typeof entry !== 'string') return entry ?? null

  const trimmed = entry.trim()
  if (!trimmed) return null

  const normalized = normalizeName(trimmed)
  const existing = artistNameMap.value.get(normalized)
  if (existing) return existing

  if (creatingArtistNames.has(normalized)) {
    // 若正在创建同名作者，等待微任务后再次尝试获取
    await Promise.resolve()
    return artistNameMap.value.get(normalized) ?? null
  }

  creatingArtistNames.add(normalized)
  try {
    const created = await artistApi.create({ name: trimmed })
    if (!userStore.artists.some((item) => item.id === created.id)) {
      userStore.artists.push(created)
    }
    ElMessage.success(
      $t('page.mediums.selection.dialog.createSuccess.artist', {
        name: created.name ?? trimmed,
      }) as string,
    )
    return created
  } catch (error) {
    console.error('创建作者失败:', error)
    return null
  } finally {
    creatingArtistNames.delete(normalized)
  }
}

const ensureTagEntry = async (entry: string | Tag): Promise<null | Tag> => {
  if (typeof entry !== 'string') return entry ?? null

  const trimmed = entry.trim()
  if (!trimmed) return null

  const normalized = normalizeName(trimmed)
  const existing = tagNameMap.value.get(normalized)
  if (existing) return existing

  if (creatingTagNames.has(normalized)) {
    await Promise.resolve()
    return tagNameMap.value.get(normalized) ?? null
  }

  creatingTagNames.add(normalized)
  try {
    const created = await tagApi.create({ name: trimmed })
    if (!userStore.tags.some((item) => item.id === created.id)) {
      userStore.tags.push(created)
    }
    ElMessage.success(
      $t('page.mediums.selection.dialog.createSuccess.tag', {
        name: created.name ?? trimmed,
      }) as string,
    )
    return created
  } catch (error) {
    console.error('创建标签失败:', error)
    return null
  } finally {
    creatingTagNames.delete(normalized)
  }
}

const handleArtistsChange = async (value: Array<Artist | string>) => {
  if (!mediumState.value) return
  const resolved: Artist[] = []
  for (const item of value ?? []) {
    const artist = await ensureArtistEntry(item)
    if (artist && !resolved.some((entry) => entry.id === artist.id)) {
      resolved.push(artist)
    }
  }
  mediumState.value.artists = resolved
}

const handleTagsChange = async (value: Array<string | Tag>) => {
  if (!mediumState.value) return
  const resolved: Tag[] = []
  for (const item of value ?? []) {
    const tag = await ensureTagEntry(item)
    if (tag && !resolved.some((entry) => entry.id === tag.id)) {
      resolved.push(tag)
    }
  }
  mediumState.value.tags = resolved
}

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

const loadComicImages = async () => {
  if (!mediumState.value) return
  if (mediumState.value.mediumType !== MediumType.Comic) return
  comicImagesLoading.value = true
  try {
    comicImages.value = await comicApi.getImagesByComicId(mediumState.value.id)
    // 初次加载后尝试匹配封面：若 cover 恰好等于某 image.id 则标记
    if (mediumState.value.cover) {
      const match = comicImages.value.find(
        (i) => i.id === mediumState.value?.cover,
      )
      if (match) selectedCoverComicImageId.value = match.id
    }
  } catch (error) {
    console.error('加载漫画图片失败:', error)
  } finally {
    comicImagesLoading.value = false
  }
}

// 9. 初始化
onMounted(async () => {
  initialLoading.value = true
  try {
    await Promise.all([loadMedium(), loadOptions()])
    await loadComicImages()
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
      loadComicImages()
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

// 从漫画图片选为封面：拉取原图 blob 再上传
const setCoverFromComicImage = async (image: ComicImage) => {
  if (!mediumState.value) return
  if (selectingCoverImageId.value) return
  selectingCoverImageId.value = image.id
  try {
    // 尽量取较大宽度保证清晰度，如未限制可给 1200
    const rawUrl = comicApi.url.getComicImage.format({
      comicImageId: image.id,
      maxWidth: 1200,
    })
    const fullUrl = `${apiURL}${rawUrl}`
    // 使用原生 fetch，手动补授权头
    // 复用 OIDC 逻辑获取 token
    const { useOidcManager } = await import('@vben/api')
    const { getAccessToken } = useOidcManager()
    const token = await getAccessToken()
    const blob = await fetch(fullUrl, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    }).then((r) => r.blob())

    const ext = image.name?.split('.').pop() || 'jpg'
    const file = new File([blob], image.name || `${image.id}.${ext}`, {
      type: blob.type || 'image/jpeg',
    })
    const updateCoverApi =
      mediumState.value.mediumType === MediumType.Comic ? comicApi : videoApi
    const updatedMedium = await updateCoverApi.updateCover(
      mediumState.value.id,
      file,
    )
    emit('updated', updatedMedium as Medium)
    // 更新本地 cover (重新获取最新数据保持一致) 并记录选中图片 id
    const fresh = await updateCoverApi.getById(mediumState.value.id)
    mediumState.value.cover = (fresh as any).cover
    selectedCoverComicImageId.value = image.id
  } catch (error) {
    console.error('通过漫画图片设置封面失败:', error)
  } finally {
    selectingCoverImageId.value = null
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
    <div
      v-else-if="mediumState"
      :class="isMobile ? 'flex flex-col gap-6' : 'flex flex-row gap-8'"
    >
      <!-- 封面预览区 -->
      <div
        class="flex flex-col items-start gap-4"
        :class="isMobile ? '' : 'w-44'"
      >
        <div class="relative w-44" v-if="currentCoverUrl">
          <img
            :src="currentCoverUrl"
            alt="cover"
            class="h-64 w-44 rounded border object-cover shadow-sm"
          />
        </div>
        <el-upload
          :before-upload="beforeCoverUpload"
          :http-request="({ file }: any) => handleCoverUpload(file as File)"
          :show-file-list="false"
          accept="image/*"
        >
          <el-button type="primary" size="small">
            {{ $t('page.mediums.edit.uploadCover') }}
          </el-button>
        </el-upload>
        <el-text size="small" type="info" class="max-w-44 leading-snug">
          {{ $t('page.mediums.edit.coverTip') }}
        </el-text>
      </div>

      <!-- 表单 + 图片选择区 -->
      <div class="flex-1 space-y-4">
        <el-form :model="mediumState" label-width="80px" label-position="left">
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
            <div class="flex w-full flex-col gap-1">
              <el-select
                v-model="mediumState.artists"
                multiple
                filterable
                allow-create
                default-first-option
                :placeholder="$t('page.mediums.edit.artistsPlaceholder')"
                class="w-full"
                value-key="id"
                @change="handleArtistsChange"
              >
                <el-option
                  v-for="artist in allArtists"
                  :key="artist.id"
                  :label="artist.name"
                  :value="artist"
                />
              </el-select>
              <el-text size="small" type="info">
                {{ $t('page.mediums.edit.manualEntryHint.artist') }}
              </el-text>
            </div>
          </el-form-item>

          <el-form-item :label="$t('page.mediums.edit.tags')">
            <div class="flex w-full flex-col gap-1">
              <el-select
                v-model="mediumState.tags"
                multiple
                filterable
                allow-create
                default-first-option
                :placeholder="$t('page.mediums.edit.tagsPlaceholder')"
                class="w-full"
                value-key="id"
                @change="handleTagsChange"
              >
                <el-option
                  v-for="tag in allTags"
                  :key="tag.id"
                  :label="tag.name"
                  :value="tag"
                />
              </el-select>
              <el-text size="small" type="info">
                {{ $t('page.mediums.edit.manualEntryHint.tag') }}
              </el-text>
            </div>
          </el-form-item>
        </el-form>

        <!-- 选择封面图片（Comic） -->
        <div v-if="mediumState.mediumType === MediumType.Comic">
          <div class="mb-2 flex items-center justify-between">
            <el-text size="small" type="info">
              {{ $t('page.mediums.edit.coverSelectTip') }}
            </el-text>
          </div>
          <div v-if="comicImagesLoading" class="grid grid-cols-6 gap-2">
            <div
              v-for="n in 6"
              :key="n"
              class="bg-muted h-20 animate-pulse rounded"
            ></div>
          </div>
          <div
            v-else
            class="grid max-h-64 grid-cols-6 gap-2 overflow-auto pr-1"
          >
            <div
              v-for="img in comicImages"
              :key="img.id"
              class="group relative cursor-pointer overflow-hidden rounded border"
              :class="{
                'border-primary ring-primary ring-2': isSelectedComicImage(
                  img.id,
                ),
              }"
              @click="setCoverFromComicImage(img)"
            >
              <img
                loading="lazy"
                class="h-20 w-full object-cover transition-transform group-hover:scale-105"
                :src="`${apiURL}${comicApi.url.getComicImage.format({ comicImageId: img.id, maxWidth: 300 })}`"
                :alt="img.name || img.id"
              />
              <div
                v-if="selectingCoverImageId === img.id"
                class="absolute inset-0 flex items-center justify-center bg-black/40 text-xs text-white"
              >
                {{ $t('common.loading') }}
              </div>
            </div>
          </div>
        </div>

        <div class="mt-4 flex justify-end gap-3">
          <el-button :disabled="loading" @click="dialog.close()">
            {{ $t('common.cancel') }}
          </el-button>
          <el-button type="primary" :loading="loading" @click="handleSave">
            {{ $t('common.save') }}
          </el-button>
        </div>
      </div>
    </div>
  </div>
</template>

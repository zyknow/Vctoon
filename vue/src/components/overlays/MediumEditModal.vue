<script setup lang="ts">
import { artistApi } from '@/api/http/artist'
import type { Artist } from '@/api/http/artist/typing'
import { MediumType } from '@/api/http/library/typing'
import { mediumApi } from '@/api/http/medium'
import type { ComicImage, Medium } from '@/api/http/medium/typing'
import { tagApi } from '@/api/http/tag'
import type { Tag } from '@/api/http/tag/typing'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores'

defineOptions({
  name: 'MediumEditModal',
})


interface Props {
  mediumId: string
  registerDirtyChecker?: (fn: () => boolean) => void
  markClean?: () => void
}

const props = defineProps<Props>()

const emit = defineEmits<{
  close: [value: Medium | undefined]
  updated: [medium: Medium]
}>()

const toast = useToast()
// const { apiURL } = useEnvConfig()
const { isMobile } = useIsMobile()

const mediumState = ref<Medium | null>(null)
let initialSnapshot = ''
const isDirty = ref(false)
const loading = ref(false)
const initialLoading = ref(true)
const comicImages = ref<ComicImage[]>([])
const comicImagesLoading = ref(false)
const selectedCoverComicImageId = ref<null | string>(null)
// const selectingCoverImageId = ref<null | string>(null)

const currentCoverUrl = computed(() => {
  if (!mediumState.value?.cover) return ''
  return mediumApi.getCoverUrl(mediumState.value.cover)
})

// function isSelectedComicImage(id: string) {
//   return selectedCoverComicImageId.value === id
// }

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
    toast.add({
      title: $t('page.mediums.selection.dialog.createSuccess.artist', {
        name: created.name ?? trimmed,
      }),
      color: 'success',
    })
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
    toast.add({
      title: $t('page.mediums.selection.dialog.createSuccess.tag', {
        name: created.name ?? trimmed,
      }),
      color: 'success',
    })
    return created
  } catch (error) {
    console.error('创建标签失败:', error)
    return null
  } finally {
    creatingTagNames.delete(normalized)
  }
}

// 转换 Artist 和 Tag 为 SelectMenu 选项格式
const artistOptions = computed(() => {
  return allArtists.value.map((artist) => ({
    label: artist.name || '',
    value: artist.id,
  }))
})

const tagOptions = computed(() => {
  return allTags.value.map((tag) => ({
    label: tag.name || '',
    value: tag.id,
  }))
})

// SelectMenu 的选中值（ID 数组）
const artistSelection = ref<string[]>([])
const tagSelection = ref<string[]>([])

// 监听 mediumState 初始化选中的 artists 和 tags
watch(
  () => mediumState.value,
  (val) => {
    if (val) {
      artistSelection.value = val.artists?.map((a) => a.id) || []
      tagSelection.value = val.tags?.map((t) => t.id) || []
    }
  },
  { immediate: true },
)

const handleArtistsChange = async (selected: string[]) => {
  if (!mediumState.value) return
  const resolved: Artist[] = []

  for (const value of selected) {
    // 检查是否是已存在的 artist ID
    const existing = allArtists.value.find((a) => a.id === value)
    if (existing) {
      resolved.push(existing)
    } else {
      // 新创建的 artist (value 是名称)
      const artist = await ensureArtistEntry(value)
      if (artist && !resolved.some((entry) => entry.id === artist.id)) {
        resolved.push(artist)
      }
    }
  }

  mediumState.value.artists = resolved
  artistSelection.value = resolved.map((a) => a.id)
}

const handleTagsChange = async (selected: string[]) => {
  if (!mediumState.value) return
  const resolved: Tag[] = []

  for (const value of selected) {
    // 检查是否是已存在的 tag ID
    const existing = allTags.value.find((t) => t.id === value)
    if (existing) {
      resolved.push(existing)
    } else {
      // 新创建的 tag (value 是名称)
      const tag = await ensureTagEntry(value)
      if (tag && !resolved.some((entry) => entry.id === tag.id)) {
        resolved.push(tag)
      }
    }
  }

  mediumState.value.tags = resolved
  tagSelection.value = resolved.map((t) => t.id)
}

// 8. 方法
const loadMedium = async () => {
  if (!props.mediumId) return
  mediumState.value = await mediumApi.getById(props.mediumId)
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
      comicImages.value = await mediumApi.getComicImageList(
        mediumState.value.id,
      )
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
      await mediumApi.update(mediumState.value as any, mediumState.value.id)

      // 更新作者
      const artistIds = mediumState.value.artists?.map((a: Artist) => a.id) || []
      await mediumApi.updateArtists(mediumState.value.id, artistIds)

      // 更新标签
      const tagIds = mediumState.value.tags?.map((t: Tag) => t.id) || []
      await mediumApi.updateTags(mediumState.value.id, tagIds)

      // 成功提示交给全局拦截器，这里仅刷新数据
      // 重新获取更新后的数据
      const updatedMedium = await mediumApi.getById(mediumState.value.id)
      props.markClean?.()
      initialSnapshot = JSON.stringify(updatedMedium)
      isDirty.value = false
      emit('updated', updatedMedium as Medium)
      emit('close', updatedMedium as Medium)
    } catch (error) {
      console.error('更新失败:', error)
      // 全局拦截器处理错误提示
    } finally {
      loading.value = false
    }
  }

const handleCoverUploadChange = async (files: File[]) => {
  if (!mediumState.value || !files.length) return
  const file = files[0]

  // 验证文件
  const isImage = file.type.startsWith('image/')
  if (!isImage) {
    toast.add({
      title: $t('page.mediums.edit.uploadErrorTitle'),
      description: $t('page.mediums.edit.uploadErrorType'),
      color: 'error',
    })
    return
  }

  loading.value = true
  try {
    const updatedMedium = await mediumApi.updateCover(
      mediumState.value.id,
      file,
    )
    // 更新封面
    mediumState.value.cover = updatedMedium.cover
    // 清空选中图片状态，因为现在是用户上传的封面
    selectedCoverComicImageId.value = null
    emit('updated', updatedMedium as Medium)
  } catch (error) {
    console.error('上传封面失败:', error)
    // 全局拦截器处理错误提示
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <UModal :fullscreen="isMobile" :dismissible="false">
    <template #body>
      <div v-if="initialLoading" class="text-muted-foreground py-8 text-center">
        {{ $t('common.loading') }}
      </div>
      <div
        v-else-if="mediumState"
        :class="isMobile ? 'flex flex-col gap-6' : 'flex flex-row gap-8'"
      >
        <!-- 封面预览区 -->
        <div
          class="flex flex-col items-start gap-4"
          :class="isMobile ? 'w-full' : 'w-44'"
        >
          <div v-if="currentCoverUrl" class="relative w-44">
            <img
              :src="currentCoverUrl"
              alt="cover"
              class="h-64 w-44 rounded border object-cover shadow-sm"
            />
          </div>
          <div
            v-else
            class="bg-muted flex h-64 w-44 items-center justify-center rounded border"
          >
            <UIcon
              name="i-heroicons-photo"
              class="text-muted-foreground size-12"
            />
          </div>

          <UFileUpload
            accept="image/*"
            :max-size="10 * 1024 * 1024"
            @change="handleCoverUploadChange!"
          >
            <UButton size="sm">
              {{ $t('page.mediums.edit.uploadCover') }}
            </UButton>
          </UFileUpload>

          <p class="text-muted-foreground max-w-44 text-xs leading-snug">
            {{ $t('page.mediums.edit.coverTip') }}
          </p>
        </div>

        <!-- 表单 + 图片选择区 -->
        <div class="flex-1 space-y-4">
          <UForm :state="mediumState" class="space-y-4">
            <UFormField
              :label="$t('page.mediums.edit.titleLabel')"
              name="title"
            >
              <UInput
                v-model="mediumState.title"
                :placeholder="$t('page.mediums.edit.titlePlaceholder')"
                maxlength="200"
                class="w-full"
              />
            </UFormField>

            <UFormField
              :label="$t('page.mediums.edit.description')"
              name="description"
            >
              <UTextarea
                v-model="mediumState.description"
                :placeholder="$t('page.mediums.edit.descriptionPlaceholder')"
                :rows="3"
                maxlength="1000"
                class="w-full"
              />
            </UFormField>

            <UFormField :label="$t('page.mediums.edit.artists')" name="artists">
              <USelectMenu
                v-model="artistSelection"
                :items="artistOptions"
                multiple
                searchable
                creatable
                :placeholder="$t('page.mediums.edit.artistsPlaceholder')"
                class="w-full"
                value-key="value"
                @update:model-value="handleArtistsChange"
              />
              <template #hint>
                <span class="text-muted-foreground text-xs">
                  {{ $t('page.mediums.edit.manualEntryHint.artist') }}
                </span>
              </template>
            </UFormField>

            <UFormField :label="$t('page.mediums.edit.tags')" name="tags">
              <USelectMenu
                v-model="tagSelection"
                :items="tagOptions"
                multiple
                searchable
                creatable
                :placeholder="$t('page.mediums.edit.tagsPlaceholder')"
                class="w-full"
                value-key="value"
                @update:model-value="handleTagsChange"
              />
              <template #hint>
                <span class="text-muted-foreground text-xs">
                  {{ $t('page.mediums.edit.manualEntryHint.tag') }}
                </span>
              </template>
            </UFormField>
          </UForm>

          <!-- 选择封面图片（Comic） -->
        </div>
      </div>
    </template>

    <template #footer>
      <UButton
        variant="ghost"
        :disabled="loading"
        @click="emit('close', undefined)"
      >
        {{ $t('common.cancel') }}
      </UButton>
      <UButton :loading="loading" @click="handleSave">
        {{ $t('common.save') }}
      </UButton>
    </template>
  </UModal>
</template>

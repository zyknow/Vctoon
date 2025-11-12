import { computed, type ComputedRef } from 'vue'
import type { DropdownMenuItem } from '@nuxt/ui'
import { format } from 'date-fns'
import { useRouter } from 'vue-router'

import { comicApi } from '@/api/http/comic'
import { MediumType } from '@/api/http/library/typing'
import { mediumResourceApi } from '@/api/http/medium-resource'
import type { MediumGetListOutput } from '@/api/http/typing'
import { videoApi } from '@/api/http/video'
import { getMediumAnchorId } from '@/components/mediums/anchor'
import ConfirmModal from '@/components/overlays/ConfirmModal.vue'
import MediumEditModal from '@/components/overlays/MediumEditModal.vue'
import MediumInfoModal from '@/components/overlays/MediumInfoModal.vue'
import { $t } from '@/locales/i18n'

import {
  useInjectedMediumAllItemProvider,
  useInjectedMediumItemProvider,
} from './useMediumProvider'

const toDate = (value: string | Date | undefined | null) => {
  if (!value) return undefined
  const date = value instanceof Date ? value : new Date(value)
  if (Number.isNaN(date.getTime())) return undefined
  return date
}

export function useMediumItem(mediumRef: ComputedRef<MediumGetListOutput>) {
  const router = useRouter()
  const { selectedMediumIds, items } = useInjectedMediumItemProvider()
  const overlay = useOverlay()
  const toast = useToast()
  const allProvider = useInjectedMediumAllItemProvider()

  // 创建 modal 实例
  const infoModal = overlay.create(MediumInfoModal)
  const confirmModal = overlay.create(ConfirmModal)
  const editModal = overlay.create(MediumEditModal)

  const mediumType = computed(() => mediumRef.value.mediumType)
  const mediumAnchorId = computed(() => getMediumAnchorId(mediumRef.value.id))

  const isSelected = computed(() => {
    return selectedMediumIds.value.includes(mediumRef.value?.id || '')
  })

  const isInSelectionMode = computed(() => {
    return selectedMediumIds.value.length > 0
  })

  const handleSingleSelection = (itemId: string) => {
    const currentIndex = selectedMediumIds.value.indexOf(itemId)
    if (currentIndex === -1) {
      selectedMediumIds.value.push(itemId)
    } else {
      selectedMediumIds.value.splice(currentIndex, 1)
    }
  }

  const handleRangeSelection = (currentId: string) => {
    const allIds = items.value.map((item) => item.id)
    const currentIndex = allIds.indexOf(currentId)

    if (selectedMediumIds.value.length === 0) {
      selectedMediumIds.value.push(currentId)
      return
    }

    const selectedIndices = selectedMediumIds.value
      .map((id) => allIds.indexOf(id))
      .filter((index) => index !== -1)
      .sort((a, b) => a - b)

    const lastSelectedIndex = selectedIndices[selectedIndices.length - 1]

    if (lastSelectedIndex !== undefined) {
      const startIndex = Math.min(lastSelectedIndex, currentIndex)
      const endIndex = Math.max(lastSelectedIndex, currentIndex)
      const rangeIds = allIds.slice(startIndex, endIndex + 1)
      const newSelectedIds = [
        ...new Set([...rangeIds, ...selectedMediumIds.value]),
      ]
      selectedMediumIds.value = newSelectedIds
    }
  }

  const toggleSelectionFromIcon = (event: MouseEvent) => {
    const currentId = mediumRef.value?.id
    if (!currentId) return
    event.stopPropagation()
    if (event.shiftKey) {
      handleRangeSelection(currentId)
    } else {
      handleSingleSelection(currentId)
    }
  }

  const buildMediumDropdown = (): DropdownMenuItem[] => {
    return [
      {
        label: $t('page.mediums.actions.info'),
        icon: 'i-heroicons-information-circle',
        onSelect: openInfo,
      },
      isCompleted.value
        ? {
            label: $t('page.mediums.actions.markAsUnwatched'),
            icon: 'i-heroicons-x-circle',
            onSelect: markAsUnwatched as any,
          }
        : {
            label: $t('page.mediums.actions.markAsWatched'),
            icon: 'i-heroicons-check-circle',
            onSelect: markAsWatched,
          },
      { type: 'separator' },
      {
        label: $t('page.mediums.actions.delete'),
        icon: 'i-heroicons-trash',
        color: 'error',
        onSelect: deleteMedium as any,
      },
    ]
  }

  const toggleSelection = (event: MouseEvent) => {
    const currentId = mediumRef.value?.id
    if (!currentId) return
    if (event.shiftKey) {
      handleRangeSelection(currentId)
    } else {
      handleSingleSelection(currentId)
    }
  }

  const navigateToDetail = () => {
    if (isInSelectionMode.value) return
    const id = mediumRef.value?.id
    if (!id) return
    const typeSegment =
      mediumType.value === MediumType.Video ? 'video' : 'comic'
    void router.push({
      name: 'MediumDetail',
      params: { mediumId: id, type: typeSegment },
    })
  }

  const navigateToPrimaryAction = async () => {
    const id = mediumRef.value?.id
    if (!id) return
    if (mediumType.value === MediumType.Comic) {
      let page = 1
      try {
        const progress = mediumRef.value?.readingProgress ?? 0
        if (progress > 0 && progress < 1) {
          // TODO: comicApi.getImagesByComicId 属于不必要的请求，考虑将Page参数放到ComicDetail处理
          const images = await comicApi.getImagesByComicId(id)
          const total = Array.isArray(images) ? images.length : 0
          if (total > 0) {
            const calc = Math.ceil(progress * total)
            page = Math.min(Math.max(calc, 1), total)
          }
        }
      } catch {
        page = 1
      }
      const query: Record<string, string> = {
        mode: 'resume',
        page: String(page),
      }
      void router.push({ name: 'ComicReader', params: { id: id }, query })
      return
    }
    navigateToDetail()
  }

  const handleCardClick = (event: MouseEvent) => {
    if (isInSelectionMode.value) {
      toggleSelection(event)
      return
    }
    navigateToDetail()
  }

  const handleEdit = async (event: MouseEvent) => {
    event.stopPropagation()

    try {
      const updated = await editModal.open({ medium: mediumRef.value })
      if (updated) {
        // 优先通过全量 Provider 更新（同步推荐区与列表区）
        if (allProvider) {
          allProvider.updateItemField(updated)
        } else {
          const idx = items.value.findIndex((i) => i.id === updated.id)
          if (idx !== -1) {
            items.value[idx] = updated
          }
        }
      }
    } catch {
      toast.add({
        title: $t('common.error'),
        description: $t('common.operationFailed'),
        color: 'error',
      })
    }
  }

  const openInfo = async (event: MouseEvent) => {
    event.stopPropagation()
    if (!mediumRef.value?.id) return
    await infoModal.open({
      mediumId: mediumRef.value.id,
      mediumType: mediumRef.value.mediumType,
    })
  }

  const markAsWatched = async (event: MouseEvent) => {
    event.stopPropagation()
    const id = mediumRef.value?.id
    if (!id) return
    try {
      const updateObj = {
        mediumId: id,
        progress: 1,
        readingLastTime: new Date().toISOString(),
        mediumType: mediumRef.value.mediumType,
      }
      await mediumResourceApi.updateReadingProcess([updateObj])
      allProvider?.updateItemField({
        id,
        readingProgress: 1,
        readingLastTime: new Date(),
      })
    } catch {
      // 全局拦截器处理失败提示
    }
  }

  const markAsUnwatched = async (event: MouseEvent) => {
    event.stopPropagation()
    const id = mediumRef.value?.id
    if (!id) return
    try {
      const updateObj = {
        mediumId: id,
        progress: 0,
        readingLastTime: null,
        mediumType: mediumRef.value.mediumType,
      }
      await mediumResourceApi.updateReadingProcess([updateObj])
      allProvider?.updateItemField({
        id,
        readingProgress: 0,
        readingLastTime: undefined,
      })
    } catch {
      // 全局拦截器处理失败提示
    }
  }

  const deleteMedium = async (event: MouseEvent) => {
    event.stopPropagation()
    const id = mediumRef.value?.id
    if (!id) return
    const ok = await confirmModal.open({
      title: $t('page.mediums.actions.deleteConfirmTitle'),
      message: $t('page.mediums.actions.deleteConfirmMessage', {
        title: mediumRef.value?.title || id,
      }),
    })
    if (!ok) return
    try {
      if (mediumRef.value.mediumType === MediumType.Comic) {
        await comicApi.delete(id)
      } else {
        await videoApi.delete(id)
      }
      const idx = items.value.findIndex((m) => m.id === id)
      if (idx !== -1) items.value.splice(idx, 1)
    } catch {
      // 全局拦截器处理失败提示
    }
  }

  const title = computed(() => mediumRef.value?.title ?? '')
  const year = computed(() => {
    const date = toDate(mediumRef.value?.creationTime)
    if (!date) return ''
    try {
      return format(date, 'yyyy')
    } catch {
      return ''
    }
  })
  const timeAgo = computed(() => {
    const date = toDate(
      mediumRef.value?.lastModificationTime || mediumRef.value?.creationTime,
    )
    if (!date) return ''
    try {
      return format(date, 'yyyy-MM-dd')
    } catch {
      return ''
    }
  })

  const readingProgress = computed(() => {
    const progress = mediumRef.value?.readingProgress
    if (typeof progress !== 'number') return 0
    const normalizedProgress = Math.max(0, Math.min(1, progress))
    return Math.round(normalizedProgress * 100)
  })

  const showReadingProgress = computed(() => {
    const progress = mediumRef.value?.readingProgress
    return typeof progress === 'number' && progress > 0 && progress < 1
  })

  const isCompleted = computed(() => {
    const progress = mediumRef.value?.readingProgress
    return typeof progress === 'number' && progress >= 1
  })

  const cover = computed(() => {
    return mediumResourceApi.getCoverUrl(mediumRef.value.cover!)
  })

  return {
    // state
    mediumType,
    mediumAnchorId,
    isSelected,
    isInSelectionMode,
    title,
    year,
    timeAgo,
    readingProgress,
    showReadingProgress,
    isCompleted,
    cover,

    // actions
    buildMediumDropdown,
    toggleSelectionFromIcon,
    toggleSelection,
    handleSingleSelection,
    handleRangeSelection,
    navigateToDetail,
    navigateToPrimaryAction,
    handleCardClick,
    handleEdit,
    openInfo,
    markAsWatched,
    markAsUnwatched,
    deleteMedium,
  }
}

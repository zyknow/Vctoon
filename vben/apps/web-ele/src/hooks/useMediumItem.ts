import type { ComputedRef } from 'vue'

import type { MediumGetListOutput } from '@vben/api'

import { computed } from 'vue'
import { useRouter } from 'vue-router'

import { comicApi, mediumResourceApi, MediumType, videoApi } from '@vben/api'
import { useAppConfig } from '@vben/hooks'
import { formatDate } from '@vben/utils'

import { getMediumAnchorId } from '#/components/mediums/anchor'
import MediumInfoDialog from '#/components/mediums/medium-info-dialog.vue'
import { $t } from '#/locales'

import { useDialogService } from './useDialogService'
import {
  useInjectedMediumAllItemProvider,
  useInjectedMediumItemProvider,
} from './useMediumProvider'

export interface UseMediumItemOptions {
  onEdit?: (medium: MediumGetListOutput) => void
  onUpdated?: (medium: MediumGetListOutput) => void
}

export function useMediumItem(
  mediumRef: ComputedRef<MediumGetListOutput>,
  options: UseMediumItemOptions = {},
) {
  const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)
  const router = useRouter()
  const { selectedMediumIds, items } = useInjectedMediumItemProvider()
  const { open, confirm } = useDialogService()
  const allProvider = useInjectedMediumAllItemProvider()
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
      void router.push({ name: 'ComicReader', params: { comicId: id }, query })
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

  const handleEdit = (event: MouseEvent) => {
    event.stopPropagation()
    options.onEdit?.(mediumRef.value)
  }

  const openInfo = async (event: MouseEvent) => {
    event.stopPropagation()
    if (!mediumRef.value?.id) return
    await open(MediumInfoDialog, {
      props: {
        mediumId: mediumRef.value.id,
        mediumType: mediumRef.value.mediumType,
      },
      title: $t('page.mediums.actions.info'),
      width: 520,
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
      options.onUpdated?.(mediumRef.value)
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
      options.onUpdated?.(mediumRef.value)
    } catch {
      // 全局拦截器处理失败提示
    }
  }

  const deleteMedium = async (event: MouseEvent) => {
    event.stopPropagation()
    const id = mediumRef.value?.id
    if (!id) return
    const ok = await confirm({
      title: $t('page.mediums.actions.deleteConfirmTitle'),
      message: $t('page.mediums.actions.deleteConfirmMessage', {
        title: mediumRef.value?.title || id,
      }),
      danger: true,
    })
    if (!ok) return
    try {
      mediumRef.value.mediumType === MediumType.Comic
        ? await comicApi.delete(id)
        : await videoApi.delete(id)
      const idx = items.value.findIndex((m) => m.id === id)
      if (idx !== -1) items.value.splice(idx, 1)
    } catch {
      // 全局拦截器处理失败提示
    }
  }

  const title = computed(() => mediumRef.value?.title ?? '')
  const year = computed(() => {
    const time = mediumRef.value?.creationTime
    if (!time) return ''
    try {
      return formatDate(time, 'YYYY')
    } catch {
      return ''
    }
  })
  const timeAgo = computed(() => {
    const time =
      mediumRef.value?.lastModificationTime || mediumRef.value?.creationTime
    if (!time) return ''
    try {
      return formatDate(time, 'YYYY-MM-DD')
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
    let mediumUrl
    switch (mediumType.value) {
      case MediumType.Comic: {
        mediumUrl = mediumResourceApi.url.getCover.format({
          cover: mediumRef.value?.cover,
        })
        break
      }
      case MediumType.Video: {
        mediumUrl = mediumResourceApi.url.getCover.format({
          cover: mediumRef.value?.cover,
        })
        break
      }
      default: {
        break
      }
    }
    return `${apiURL}${mediumUrl}`
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

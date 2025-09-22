import type { Reactive, Ref } from 'vue'

import type {
  ComicGetListInput,
  ComicGetListOutput,
  MediumGetListOutput,
  VideoGetListInput,
} from '@vben/api'

import { reactive, ref } from 'vue'

import { comicApi, MediumType, videoApi } from '@vben/api'

// useLibraryMediumProvider.ts

export type UseRecommendMediumProviderOptions = ComicGetListInput &
  VideoGetListInput & {
    autoLoad?: boolean
    mediumTypes: MediumType[]
  }

export type RecommendMediumProvider = {
  hasMore: Ref<boolean>
  items: Ref<MediumGetListOutput[]>
  loading: Ref<boolean>
  loadItems: () => Promise<void>
  loadNext: () => Promise<void>
  mediumPageRequests: Reactive<
    Record<MediumType, ComicGetListOutput | VideoGetListInput>
  >
  title: Ref<string>
  totalCount: Ref<number>
}

/** 基于 Library 的便捷封装 */
export function createRecommendMediumProvider(
  opts: UseRecommendMediumProviderOptions,
): RecommendMediumProvider {
  const mediumTypes = opts.mediumTypes

  const mediumApis = {
    [MediumType.Comic]: comicApi.getPage,
    [MediumType.Video]: videoApi.getPage,
  }

  const mediumPageRequests = reactive<
    Record<MediumType, ComicGetListInput | VideoGetListInput>
  >({} as Record<MediumType, ComicGetListInput | VideoGetListInput>)

  const items = ref<MediumGetListOutput[]>([])
  const title = ref(opts.title ?? '')
  const totalCount = ref(0)
  const loading = ref(false)
  const hasMore = ref(true)

  mediumTypes.forEach((type) => {
    mediumPageRequests[type] = {
      sorting: 'CreationTime DESC',
      maxResultCount: 10,
      ...opts,
    }
  })

  const loadItems = async () => {
    if (loading.value) return

    loading.value = true
    try {
      const allResults = await Promise.all(
        mediumTypes.map((type) => mediumApis[type](mediumPageRequests[type])),
      )
      const combinedItems: MediumGetListOutput[] = []
      let combinedTotalCount = 0
      allResults.forEach((res) => {
        combinedItems.push(...res.items)
        combinedTotalCount += res.totalCount
      })
      items.value = combinedItems
      totalCount.value = combinedTotalCount
      hasMore.value = combinedItems.length < combinedTotalCount
    } finally {
      loading.value = false
    }
  }
  const loadNext = async () => {
    if (loading.value || !hasMore.value) return

    loading.value = true
    try {
      const allResults = await Promise.all(
        mediumTypes.map((type) => {
          const req = mediumPageRequests[type]
          ;(req as any).skipCount =
            (req as any).skipCount + (req.maxResultCount || 20)
          return mediumApis[type](req)
        }),
      )
      const combinedItems: MediumGetListOutput[] = []
      let combinedTotalCount = 0
      allResults.forEach((res) => {
        combinedItems.push(...res.items)
        combinedTotalCount += res.totalCount
      })
      items.value.push(...combinedItems)
      totalCount.value = combinedTotalCount
      hasMore.value = items.value.length < combinedTotalCount
    } finally {
      loading.value = false
    }
  }

  return {
    hasMore,
    items,
    loading,
    loadItems,
    loadNext,
    mediumPageRequests,
    title,
    totalCount,
  }
}

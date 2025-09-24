import type { ComputedRef, Reactive, Ref } from 'vue'

import type {
  ComicGetListInput,
  ComicGetListOutput,
  MediumGetListOutput,
  VideoGetListInput,
} from '@vben/api'

import { computed, reactive, ref } from 'vue'

import { comicApi, MediumType, videoApi } from '@vben/api'

// useLibraryMediumProvider.ts

export type UseRecommendMediumProviderOptions = {
  autoLoad?: boolean
  mediumTypes: MediumType[]
  pageRequest: ComicGetListInput & VideoGetListInput
  title?: string
}

export type RecommendMediumProvider = {
  hasMore: ComputedRef<boolean>
  items: ComputedRef<(MediumGetListOutput & { mediumType: MediumType })[]>
  loading: Ref<boolean>
  loadItems(): Promise<void>
  loadNext(): Promise<void>
  mediumPageRequests: Reactive<
    Record<MediumType, ComicGetListOutput | VideoGetListInput>
  >
  title: Ref<string>
  totalCount: ComputedRef<number>
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

  const mediumPageResults = reactive<
    Record<MediumType, PageResult<MediumGetListOutput>>
  >({} as Record<MediumType, PageResult<MediumGetListOutput>>)

  const mediumItems = reactive<Record<MediumType, MediumGetListOutput[]>>(
    {} as Record<MediumType, MediumGetListOutput[]>,
  )

  const items = computed(() => {
    return mediumTypes.flatMap((type) => mediumItems[type] || [])
  })
  const title = ref(opts.title ?? '')
  const totalCount = computed(() => {
    return mediumTypes.reduce((sum, type) => {
      return sum + (mediumPageResults[type]?.totalCount ?? 0)
    }, 0)
  })
  const loading = ref(false)
  const hasMore = computed(() => {
    return items.value.length < totalCount.value
  })

  mediumTypes.forEach((type) => {
    mediumPageRequests[type] = {
      sorting: 'CreationTime DESC',
      maxResultCount: 10,
      ...opts.pageRequest,
    }
  })

  const loadItems = async (loadMore?: boolean) => {
    if (loading.value) return

    loading.value = true

    try {
      await Promise.all(
        mediumTypes.map(async (type) => {
          const pageApi = mediumApis[type]
          const pageRequest = mediumPageRequests[type]
          const result = await pageApi(
            pageRequest as ComicGetListInput & VideoGetListInput,
          )
          mediumPageResults[type] = result
          pageRequest.skipCount = loadMore
            ? (mediumItems[type]?.length ?? 0)
            : 0

          mediumItems[type] = loadMore
            ? [...(mediumItems[type] || []), ...result.items]
            : result.items || []
        }),
      )
    } catch (error) {
      console.error('加载推荐内容失败', error)
    } finally {
      loading.value = false
    }
  }
  const loadNext = async () => {
    return await loadItems(true)
  }

  if (opts.autoLoad) {
    // 不阻塞 setup

    loadItems()
  }

  return {
    hasMore,
    items: items as any,
    loading,
    loadItems,
    loadNext,
    mediumPageRequests,
    title,
    totalCount,
  }
}

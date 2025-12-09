import { computed, ref } from 'vue'
import type { ComputedRef, Ref } from 'vue'

import { MediumType } from '@/api/http/library'
import { mediumApi } from '@/api/http/medium'
import type {
  MediumGetListInput,
  MediumGetListOutput,
} from '@/api/http/medium/typing'

// useLibraryMediumProvider.ts

export type UseRecommendMediumProviderOptions = {
  autoLoad?: boolean
  pageRequest: MediumGetListInput
  title?: string
}

export type RecommendMediumProvider = {
  hasMore: ComputedRef<boolean>
  items: ComputedRef<(MediumGetListOutput & { mediumType: MediumType })[]>
  loading: Ref<boolean>
  loadItems(): Promise<void>
  loadNext(): Promise<void>
  mediumPageRequests: Ref<MediumGetListInput>
  title: Ref<string>
  totalCount: ComputedRef<number>
}

/** 基于 Library 的便捷封装 */
export function createRecommendMediumProvider(
  opts: UseRecommendMediumProviderOptions,
): RecommendMediumProvider {
  const mediumPageRequests = ref<MediumGetListInput>({
    sorting: 'CreationTime DESC',
    maxResultCount: 10,
    ...opts.pageRequest,
  })

  const mediumPageResults = ref<PageResult<MediumGetListOutput>>({} as any)

  const items = ref<MediumGetListOutput[]>([])
  const title = ref(opts.title ?? '')

  const totalCount = computed(() => mediumPageResults.value.totalCount || 0)
  const loading = ref(false)
  const hasMore = computed(() => {
    return items.value.length < totalCount.value
  })

  const loadItems = async (loadMore?: boolean) => {
    if (loading.value) return

    loading.value = true

    try {
      const pageResult = mediumPageResults.value
      if (
        pageResult?.totalCount !== 0 &&
        pageResult?.totalCount <= (items.value.length ?? 0)
      ) {
        return
      }

      const pageRequest = mediumPageRequests.value
      pageRequest.skipCount = loadMore ? (items.value.length ?? 0) : 0
      const result = await mediumApi.getPage(pageRequest)
      mediumPageResults.value = result

      items.value = loadMore
        ? [...(items.value || []), ...result.items]
        : result.items || []
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

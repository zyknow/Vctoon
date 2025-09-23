import type { Ref } from 'vue'

import type {
  ComicGetListInput,
  MediumGetListOutput,
  VideoGetListInput,
} from '@vben/api'

// useMediumProvider.ts
import { computed, inject, provide, reactive, ref } from 'vue'

import { comicApi, MediumType, videoApi } from '@vben/api'

export const mediumProviderKey = Symbol('mediumProvider')

/** 公共的分页查询类型，避免 union 导致的类型缩小问题 */
export type PageRequest = Partial<ComicGetListInput & VideoGetListInput>
export type MediumViewTab = 'collection' | 'library' | 'recommend'
/** 对外暴露的 Provider 接口（显式使用 Ref 类型） */
export type MediumProvider = {
  currentTab: Ref<MediumViewTab>
  hasMore: Ref<boolean>
  // state（都是 Ref 或 reactive 对象）
  items: Ref<MediumGetListOutput[]>
  loading: Ref<boolean>
  loadItems(): Promise<void>
  loadNext(): Promise<void>
  loadType: Ref<MediumType>
  pageRequest: PageRequest // reactive 对象的静态类型即可
  selectedMediumIds: Ref<string[]>
  title: Ref<string>
  totalCount: Ref<number>
  updateSorting(sorting: string): Promise<void>
}

export interface UseMediumProviderOptions {
  loadType?: MediumType
  title?: string
  pageRequest?: PageRequest
  autoLoad?: boolean
}

/** 工厂：创建响应式 Provider（仅创建，不负责 provide） */
export function createMediumProvider(
  opts: UseMediumProviderOptions = {},
): MediumProvider {
  const items = ref<MediumGetListOutput[]>([])
  const loadType = ref<MediumType>(opts.loadType ?? MediumType.Comic)
  const pageRequest = reactive<PageRequest>({
    sorting: 'CreationTime desc',
    maxResultCount: 50,
    ...opts.pageRequest,
  })
  const selectedMediumIds = ref<string[]>([])
  const title = ref(opts.title ?? '')
  const totalCount = ref(0)
  const loading = ref(false)
  const hasMore = ref(true)
  const currentTab = ref<MediumViewTab>('recommend')
  const pageApi = computed<typeof comicApi.getPage | typeof videoApi.getPage>(
    () => {
      return loadType.value === MediumType.Comic
        ? comicApi.getPage
        : videoApi.getPage
    },
  )

  const loadItems = async (loadMore?: boolean) => {
    if (loading.value) return

    loading.value = true

    try {
      // 初始化加载时重置分页位置
      pageRequest.skipCount = loadMore ? items.value.length : 0
      const result = await pageApi.value(
        pageRequest as ComicGetListInput & VideoGetListInput,
      )
      items.value = loadMore ? [...items.value, ...result.items] : result.items
      totalCount.value = result.totalCount
      hasMore.value = items.value.length < totalCount.value
    } catch (error) {
      console.error('加载媒体项失败', error)
    } finally {
      loading.value = false
    }
  }

  const loadNext = async () => {
    return await loadItems(true)
  }

  const updateSorting = async (sorting: string) => {
    pageRequest.sorting = sorting
    await loadItems()
  }

  const model: MediumProvider = {
    loadNext,
    currentTab,
    loading,
    items,
    loadType,
    pageRequest,
    selectedMediumIds,
    title,
    totalCount,
    loadItems,
    updateSorting,
    hasMore,
  }

  if (opts.autoLoad) {
    // 不阻塞 setup

    loadItems()
  }

  return model
}

/** 在父组件 provide */
export function provideMediumProvider(model: MediumProvider) {
  provide(mediumProviderKey, model)
}

/** 在子组件 inject */
export function useInjectedMediumProvider(): MediumProvider {
  const model = inject<MediumProvider>(mediumProviderKey)
  if (!model) throw new Error('mediumProvider 未提供')
  return model
}

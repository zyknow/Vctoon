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
export interface MediumProvider {
  // state（都是 Ref 或 reactive 对象）
  items: Ref<MediumGetListOutput[]>
  loadType: Ref<MediumType>
  pageRequest: PageRequest // reactive 对象的静态类型即可
  selectedMediumIds: Ref<string[]>
  title: Ref<string>
  currentTab: Ref<MediumViewTab>
  totalCount: Ref<number>
  loading: Ref<boolean>
  // actions
  loadItems: () => Promise<void>
  updateSorting: (sorting: string) => Promise<void>
  loadNext: () => Promise<void>
  hasMore: Ref<boolean>
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

  const loadItems = async () => {
    loading.value = true
    // 初始化加载时重置分页位置
    ;(pageRequest as any).skipCount = 0
    const result = await pageApi.value(
      pageRequest as ComicGetListInput & VideoGetListInput,
    )
    items.value = result.items
    totalCount.value = result.totalCount
    hasMore.value = items.value.length < totalCount.value
    loading.value = false
  }

  const updateSorting = async (sorting: string) => {
    pageRequest.sorting = sorting
    await loadItems()
  }

  const loadNext = async () => {
    if (loading.value) return
    if (!hasMore.value) return
    loading.value = true
    const currentSkip = Number((pageRequest as any).skipCount ?? 0)
    const size = Number((pageRequest as any).maxResultCount ?? 50)
    ;(pageRequest as any).skipCount = currentSkip + size
    const result = await pageApi.value(
      pageRequest as ComicGetListInput & VideoGetListInput,
    )
    // 追加
    items.value = [...items.value, ...result.items]
    totalCount.value = result.totalCount
    hasMore.value = items.value.length < totalCount.value
    loading.value = false
  }

  const model: MediumProvider = {
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
    loadNext,
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

// useMediumProvider.ts
import { computed, inject, provide, reactive, ref } from 'vue'
import type { ComputedRef, Ref } from 'vue'

import type { ComicGetListInput } from '@/api/http/comic'
import { comicApi } from '@/api/http/comic'
import { MediumType } from '@/api/http/library'
import type { MediumGetListOutput } from '@/api/http/typing'
import type { VideoGetListInput } from '@/api/http/video'
import { videoApi } from '@/api/http/video'

// 使用全局符号注册以避免在 HMR 期间因模块热替换导致的 Symbol 实例不一致
// 这将确保 provide 与 inject 使用到的是同一个 key，即使文件被热更新重新评估
export const mediumProviderKey = Symbol.for('mediumProvider')
export const mediumItemProviderKey = Symbol.for('mediumItemProvider')
export const mediumAllItemProviderKey = Symbol.for('mediumAllItemProvider')
export type MediumItemProvider = {
  items: Ref<MediumGetListOutput[]>
  selectedMediumIds: Ref<string[]>
}

export type MediumAllItemProvider = {
  items: ComputedRef<MediumGetListOutput[]>
  itemsMap: Record<string, Ref<MediumGetListOutput[]>>
  updateItemField(medium: Partial<MediumGetListOutput> & { id: string }): void
}

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

  type LoadItemsOptions = {
    append?: boolean
    skipCount?: number
  }

  const loadItems = async (options: LoadItemsOptions = {}) => {
    if (loading.value) return

    loading.value = true

    try {
      const append = options.append ?? false
      const skip = options.skipCount ?? (append ? items.value.length : 0)
      pageRequest.skipCount = skip
      const result = await pageApi.value(
        pageRequest as ComicGetListInput & VideoGetListInput,
      )
      items.value = append ? [...items.value, ...result.items] : result.items
      totalCount.value = result.totalCount
      const loadedCount = append
        ? items.value.length
        : Math.min(result.items.length + skip, totalCount.value)
      hasMore.value = loadedCount < totalCount.value
    } catch (error) {
      console.error('加载媒体项失败', error)
    } finally {
      loading.value = false
    }
  }

  const loadNext = async () => {
    return await loadItems({ append: true })
  }

  const updateSorting = async (sorting: string) => {
    pageRequest.sorting = sorting
    await loadItems({ append: false })
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

export function provideMediumItemProvider(model: MediumItemProvider) {
  provide(mediumItemProviderKey, model)
}

export function provideMediumAllItemProvider(
  model: Omit<MediumAllItemProvider, 'items' | 'updateItemField'>,
) {
  const modelRef = model as MediumAllItemProvider

  modelRef.items = computed(() => {
    return Object.values(model.itemsMap).flatMap((i) => i.value)
  })
  modelRef.updateItemField = (
    medium: Partial<MediumGetListOutput> & { id: string },
  ) => {
    Object.values(model.itemsMap).forEach((item) => {
      const foundItem = item.value.find((item) => item.id === medium.id)
      if (foundItem) {
        Object.assign(foundItem, medium)
      }
    })
  }
  provide(mediumAllItemProviderKey, model)
}

/** 在子组件 inject */
export function useInjectedMediumProvider(): MediumProvider {
  const model = inject<MediumProvider>(mediumProviderKey)
  if (!model) throw new Error('mediumProvider 未提供')
  return model
}

export function useInjectedMediumItemProvider(): MediumItemProvider {
  const model = inject<MediumItemProvider>(mediumItemProviderKey)
  if (!model) throw new Error('mediumItemProvider 未提供')
  return model
}

export function useInjectedMediumAllItemProvider():
  | MediumAllItemProvider
  | undefined {
  const model = inject<MediumAllItemProvider>(mediumAllItemProviderKey)
  return model
}

import type { Library } from '@/api/http/library'
import { MediumType } from '@/api/http/library'

import type { MediumProvider, PageRequest } from './useMediumProvider'
// useLibraryMediumProvider.ts
import { createMediumProvider } from './useMediumProvider'

type LibraryMediumProviderCacheGlobal = typeof globalThis & {
  __libraryMediumProviderCache__?: Map<string, LibraryMediumProvider>
}

const globalWithCache = globalThis as LibraryMediumProviderCacheGlobal

function ensureCache() {
  if (!globalWithCache.__libraryMediumProviderCache__) {
    globalWithCache.__libraryMediumProviderCache__ = new Map()
  }
  return globalWithCache.__libraryMediumProviderCache__
}

export interface UseLibraryMediumProviderOptions {
  autoLoad?: boolean
  loadType?: MediumType
  pageRequest?: PageRequest
}

export type LibraryMediumProvider = MediumProvider & {
  library: Library
}

/** 基于 Library 的便捷封装 */
export function createLibraryMediumProvider(
  library: Library,
  opts: UseLibraryMediumProviderOptions = {},
): LibraryMediumProvider {
  const cache = ensureCache()
  const cacheKey = library.id
  const cached = cache.get(cacheKey)
  if (cached) {
    cached.library = library
    if (opts.pageRequest) {
      Object.assign(cached.pageRequest, opts.pageRequest)
    }
    if (opts.loadType && cached.loadType.value !== opts.loadType) {
      cached.loadType.value = opts.loadType
    }
    const shouldAutoLoad = opts.autoLoad !== false
    if (
      shouldAutoLoad &&
      cached.items.value.length === 0 &&
      !cached.loading.value
    ) {
      void cached.loadItems()
    }
    return cached
  }

  const model = createMediumProvider({
    loadType: opts.loadType ?? library.mediumType,
    title: library.name,
    pageRequest: {
      sorting: 'CreationTime DESC',
      libraryId: library.id,
      ...opts.pageRequest,
    },
    autoLoad: opts.autoLoad ?? true,
  }) as LibraryMediumProvider
  model.library = library
  cache.set(cacheKey, model)

  return model
}

export function clearLibraryMediumProviderCache(libraryId?: string) {
  const cache = globalWithCache.__libraryMediumProviderCache__
  if (!cache) return
  if (libraryId) {
    cache.delete(libraryId)
    return
  }
  cache.clear()
}

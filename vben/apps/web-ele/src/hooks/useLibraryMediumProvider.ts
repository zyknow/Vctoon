import type { Library } from '@vben/api'

import type { MediumProvider } from './useMediumProvider'

// useLibraryMediumProvider.ts
import { createMediumProvider } from './useMediumProvider'

export interface UseLibraryMediumProviderOptions {
  autoLoad?: boolean
}

/** 基于 Library 的便捷封装 */
export function createLibraryMediumProvider(
  library: Library,
  opts: UseLibraryMediumProviderOptions = {},
): MediumProvider {
  const model = createMediumProvider({
    loadType: library.mediumType,
    title: library.name,
    pageRequest: {
      sorting: 'CreationTime DESC',
      // 需要的话在此带入 library 过滤条件：
      // libraryId: library.id,
    },
    autoLoad: opts.autoLoad ?? true,
  })

  return model
}

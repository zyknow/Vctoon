import type { ComputedRef } from 'vue'

import type { MediumGetListInputBase } from '@vben/api'

import type { MediumProvider } from './useMediumProvider'

import { computed } from 'vue'

import { useInjectedMediumProvider } from './useMediumProvider'

export type MediumFilterValue = Partial<
  Pick<
    MediumGetListInputBase,
    | 'artists'
    | 'createdInDays'
    | 'hasReadCount'
    | 'readingProgressType'
    | 'tags'
  >
>

interface MediumFilterBindingResult {
  filterValue: ComputedRef<MediumFilterValue>
  clearFilters: () => void
}

const normalizeStringList = (list?: string[]): string[] => {
  if (!list || list.length === 0) {
    return []
  }
  const seen = new Set<string>()
  const result: string[] = []
  for (const item of list) {
    if (!item || seen.has(item)) {
      continue
    }
    seen.add(item)
    result.push(item)
  }
  return result
}

const areSameStringList = (a?: string[], b?: string[]) => {
  const listA = a ?? []
  const listB = b ?? []
  if (listA.length !== listB.length) {
    return false
  }
  return listA.every((item, index) => item === listB[index])
}

const createFilterValueGetter = (provider: MediumProvider) => {
  return (): MediumFilterValue => {
    const value: MediumFilterValue = {}
    const request = provider.pageRequest as MediumFilterValue
    if (request.readingProgressType !== undefined) {
      value.readingProgressType = request.readingProgressType
    }
    if (request.hasReadCount !== undefined) {
      value.hasReadCount = request.hasReadCount
    }
    if (request.createdInDays !== undefined) {
      value.createdInDays = request.createdInDays
    }
    const tags = normalizeStringList(request.tags)
    if (tags.length > 0) {
      value.tags = tags
    }
    const artists = normalizeStringList(request.artists)
    if (artists.length > 0) {
      value.artists = artists
    }
    return value
  }
}

const createFilterValueSetter = (provider: MediumProvider) => {
  return (value: MediumFilterValue) => {
    const request = provider.pageRequest as Record<string, unknown>
    let changed = false

    const updateScalarField = <
      K extends 'createdInDays' | 'hasReadCount' | 'readingProgressType',
    >(
      key: K,
      nextValue: MediumFilterValue[K],
    ) => {
      const currentValue = request[key] as MediumFilterValue[K] | undefined
      if (nextValue === undefined) {
        if (currentValue !== undefined) {
          Reflect.deleteProperty(request, key)
          changed = true
        }
        return
      }
      if (currentValue !== nextValue) {
        request[key] = nextValue as unknown
        changed = true
      }
    }

    const updateArrayField = (key: 'artists' | 'tags', list?: string[]) => {
      const currentValue = request[key] as string[] | undefined
      const normalized = normalizeStringList(list)
      if (normalized.length > 0) {
        if (!areSameStringList(normalized, currentValue)) {
          request[key] = [...normalized]
          changed = true
        }
        return
      }
      if (currentValue !== undefined && currentValue.length > 0) {
        Reflect.deleteProperty(request, key)
        changed = true
      }
    }

    updateScalarField('readingProgressType', value.readingProgressType)
    updateScalarField('hasReadCount', value.hasReadCount)
    updateScalarField('createdInDays', value.createdInDays)
    updateArrayField('tags', value.tags)
    updateArrayField('artists', value.artists)

    if (!changed) {
      return
    }
    provider.pageRequest.skipCount = 0
    provider.selectedMediumIds.value = []
    void provider.loadItems()
  }
}

export const useMediumFilterBinding = (
  provider?: MediumProvider,
): MediumFilterBindingResult => {
  const mediumProvider = provider ?? useInjectedMediumProvider()
  const filterValue = computed<MediumFilterValue>({
    get: createFilterValueGetter(mediumProvider),
    set: createFilterValueSetter(mediumProvider),
  })

  const clearFilters = () => {
    filterValue.value = {}
  }

  return {
    filterValue,
    clearFilters,
  }
}

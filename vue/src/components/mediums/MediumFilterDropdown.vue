<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import type { TabsItem } from '@nuxt/ui'

import type { MediumGetListInputBase } from '@/api/http/base/medium-base'
import { ReadingProgressType } from '@/api/http/base/medium-base'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores/user'

defineOptions({
  name: 'MediumFilterDropdown',
  inheritAttrs: false,
})

const props = withDefaults(defineProps<MediumFilterDropdownProps>(), {
  size: 'default',
  disabled: false,
  modelValue: () => ({}),
})

const emit = defineEmits<MediumFilterDropdownEmits>()

type MediumFilterValue = Partial<
  Pick<
    MediumGetListInputBase,
    | 'artists'
    | 'createdInDays'
    | 'hasReadCount'
    | 'readingProgressType'
    | 'tags'
  >
>

interface MediumFilterDropdownProps {
  modelValue?: MediumFilterValue
  size?: 'default' | 'large' | 'small'
  disabled?: boolean
}

type MediumFilterDropdownEmits = {
  change: [value: MediumFilterValue]
  'update:modelValue': [value: MediumFilterValue]
}

interface MediumFilterState {
  readingProgressType?: ReadingProgressType
  hasReadCount?: boolean
  createdInDays?: number
  tags?: string[]
  artists?: string[]
}

type FilterPatch = Partial<MediumFilterState>
type FilterKey = keyof MediumFilterState
type MultiGroupKey = 'artists' | 'tags'
type SingleGroupKey = 'recent' | 'status'
type FilterGroupKey = MultiGroupKey | SingleGroupKey

interface SingleFilterOption {
  key: string
  label: string
  active: boolean
  onSelect(): void
}

interface MultiFilterOption {
  value: string
  label: string
}

interface SingleFilterGroup {
  key: SingleGroupKey
  label: string
  type: 'single'
  options: SingleFilterOption[]
}

interface MultiFilterGroup {
  key: MultiGroupKey
  label: string
  type: 'multi'
  options: MultiFilterOption[]
  searchable?: boolean
  placeholder?: string
  emptyText?: string
}

type FilterGroup = MultiFilterGroup | SingleFilterGroup

const userStore = useUserStore()

const createEmptyState = (): MediumFilterState => ({
  tags: [],
  artists: [],
})

const normalizeStringList = (list?: string[]): string[] => {
  if (!list || list.length === 0) return []
  const seen = new Set<string>()
  const result: string[] = []
  for (const item of list) {
    if (!item) continue
    if (seen.has(item)) continue
    seen.add(item)
    result.push(item)
  }
  return result
}

const toState = (value?: MediumFilterValue): MediumFilterState => {
  const state = createEmptyState()
  if (value?.readingProgressType !== undefined) {
    state.readingProgressType = value.readingProgressType
  }
  if (value?.hasReadCount !== undefined) {
    state.hasReadCount = value.hasReadCount
  }
  if (value?.createdInDays !== undefined) {
    state.createdInDays = value.createdInDays
  }
  state.tags = normalizeStringList(value?.tags)
  state.artists = normalizeStringList(value?.artists)
  return state
}

const normalizeState = (state: MediumFilterState): MediumFilterState => ({
  readingProgressType: state.readingProgressType,
  hasReadCount: state.hasReadCount,
  createdInDays: state.createdInDays,
  tags: normalizeStringList(state.tags),
  artists: normalizeStringList(state.artists),
})

const isSameList = (
  a: string[] | undefined,
  b: string[] | undefined,
): boolean => {
  const listA = a ?? []
  const listB = b ?? []
  if (listA.length !== listB.length) return false
  return listA.every((item, index) => item === listB[index])
}

const isSameState = (a: MediumFilterState, b: MediumFilterState): boolean => {
  return (
    a.readingProgressType === b.readingProgressType &&
    a.hasReadCount === b.hasReadCount &&
    a.createdInDays === b.createdInDays &&
    isSameList(a.tags, b.tags) &&
    isSameList(a.artists, b.artists)
  )
}

const toFilterValue = (state: MediumFilterState): MediumFilterValue => {
  const value: MediumFilterValue = {}
  if (state.readingProgressType !== undefined) {
    value.readingProgressType = state.readingProgressType
  }
  if (state.hasReadCount !== undefined) {
    value.hasReadCount = state.hasReadCount
  }
  if (state.createdInDays !== undefined) {
    value.createdInDays = state.createdInDays
  }
  if (state.tags && state.tags.length > 0) {
    value.tags = [...state.tags]
  }
  if (state.artists && state.artists.length > 0) {
    value.artists = [...state.artists]
  }
  return value
}

const localFilters = ref<MediumFilterState>(toState(props.modelValue))

watch(
  () => props.modelValue,
  (value) => {
    const nextState = toState(value)
    if (isSameState(nextState, localFilters.value)) return
    localFilters.value = nextState
  },
  { deep: true },
)

const applyFilter = (patch: FilterPatch, resetKeys: FilterKey[] = []) => {
  const next: MediumFilterState = {
    ...localFilters.value,
    ...patch,
  }
  for (const key of resetKeys) {
    switch (key) {
      case 'artists': {
        next.artists = []
        break
      }
      case 'createdInDays': {
        next.createdInDays = undefined
        break
      }
      case 'hasReadCount': {
        next.hasReadCount = undefined
        break
      }
      case 'readingProgressType': {
        next.readingProgressType = undefined
        break
      }
      case 'tags': {
        next.tags = []
        break
      }
    }
  }
  const normalized = normalizeState(next)
  if (isSameState(normalized, localFilters.value)) return
  localFilters.value = normalized
  const emitted = toFilterValue(normalized)
  emit('update:modelValue', emitted)
  emit('change', emitted)
}

// 已不再使用 Element Plus 尺寸映射，统一由样式控制尺寸

const activeGroupKey = ref<FilterGroupKey>('status')
const searchQueries = reactive<Record<MultiGroupKey, string>>({
  tags: '',
  artists: '',
})

const referenceLoading = ref(false)

const tags = computed(() => userStore.tags ?? [])
const artists = computed(() => userStore.artists ?? [])

const ensureReferenceData = async () => {
  if (referenceLoading.value) return
  referenceLoading.value = true
  try {
    await Promise.all([userStore.reloadTags(), userStore.reloadArtists()])
  } catch (error) {
    console.error('加载筛选引用数据失败', error)
  } finally {
    referenceLoading.value = false
  }
}

const statusGroup = computed<SingleFilterGroup>(() => {
  const { readingProgressType, hasReadCount } = localFilters.value
  return {
    key: 'status',
    label: $t('page.mediums.filter.groups.status'),
    type: 'single',
    options: [
      {
        key: 'status-all',
        label: $t('page.mediums.filter.status.all'),
        active: readingProgressType === undefined && hasReadCount === undefined,
        onSelect: () =>
          applyFilter({}, ['readingProgressType', 'hasReadCount']),
      },
      {
        key: 'status-not-started',
        label: $t('page.mediums.filter.status.notStarted'),
        active: readingProgressType === ReadingProgressType.NotStarted,
        onSelect: () =>
          applyFilter({ readingProgressType: ReadingProgressType.NotStarted }, [
            'hasReadCount',
          ]),
      },
      {
        key: 'status-in-progress',
        label: $t('page.mediums.filter.status.inProgress'),
        active: readingProgressType === ReadingProgressType.InProgress,
        onSelect: () =>
          applyFilter({ readingProgressType: ReadingProgressType.InProgress }, [
            'hasReadCount',
          ]),
      },
      {
        key: 'status-completed',
        label: $t('page.mediums.filter.status.completed'),
        active: readingProgressType === ReadingProgressType.Completed,
        onSelect: () =>
          applyFilter({ readingProgressType: ReadingProgressType.Completed }, [
            'hasReadCount',
          ]),
      },
      {
        key: 'status-has-read',
        label: $t('page.mediums.filter.status.hasReadCount'),
        active: hasReadCount === true,
        onSelect: () =>
          applyFilter({ hasReadCount: true }, ['readingProgressType']),
      },
      {
        key: 'status-no-read',
        label: $t('page.mediums.filter.status.noReadCount'),
        active: hasReadCount === false,
        onSelect: () =>
          applyFilter({ hasReadCount: false }, ['readingProgressType']),
      },
    ],
  }
})

const recentGroup = computed<SingleFilterGroup>(() => {
  const createdInDays = localFilters.value.createdInDays
  return {
    key: 'recent',
    label: $t('page.mediums.filter.groups.recent'),
    type: 'single',
    options: [
      {
        key: 'recent-all',
        label: $t('page.mediums.filter.recent.all'),
        active: createdInDays === undefined,
        onSelect: () => applyFilter({}, ['createdInDays']),
      },
      {
        key: 'recent-7',
        label: $t('page.mediums.filter.recent.last7'),
        active: createdInDays === 7,
        onSelect: () => applyFilter({ createdInDays: 7 }, []),
      },
      {
        key: 'recent-30',
        label: $t('page.mediums.filter.recent.last30'),
        active: createdInDays === 30,
        onSelect: () => applyFilter({ createdInDays: 30 }, []),
      },
      {
        key: 'recent-365',
        label: $t('page.mediums.filter.recent.last365'),
        active: createdInDays === 365,
        onSelect: () => applyFilter({ createdInDays: 365 }, []),
      },
    ],
  }
})

const tagOptions = computed<MultiFilterOption[]>(() =>
  tags.value.map((tag) => ({
    value: tag.id,
    label: tag.name || tag.id,
  })),
)

const artistOptions = computed<MultiFilterOption[]>(() =>
  artists.value.map((artist) => ({
    value: artist.id,
    label: artist.name || artist.id,
  })),
)

const tagsGroup = computed<MultiFilterGroup>(() => ({
  key: 'tags',
  label: $t('page.mediums.filter.groups.tags'),
  type: 'multi',
  searchable: true,
  placeholder: $t('page.mediums.filter.search.tags'),
  emptyText: $t('page.mediums.filter.empty.tags'),
  options: tagOptions.value,
}))

const artistsGroup = computed<MultiFilterGroup>(() => ({
  key: 'artists',
  label: $t('page.mediums.filter.groups.artists'),
  type: 'multi',
  searchable: true,
  placeholder: $t('page.mediums.filter.search.artists'),
  emptyText: $t('page.mediums.filter.empty.artists'),
  options: artistOptions.value,
}))

const groups = computed<FilterGroup[]>(() => [
  statusGroup.value,
  recentGroup.value,
  tagsGroup.value,
  artistsGroup.value,
])

watch(
  groups,
  (groupList) => {
    if (groupList.length === 0) return
    const exists = groupList.some((group) => group.key === activeGroupKey.value)
    if (!exists) {
      const [first] = groupList
      if (first) {
        activeGroupKey.value = first.key
      }
    }
  },
  { immediate: true },
)

const isMultiGroupKey = (key: FilterGroupKey): key is MultiGroupKey => {
  return key === 'tags' || key === 'artists'
}

watch(activeGroupKey, (key, previous) => {
  if (previous && isMultiGroupKey(previous) && previous !== key) {
    searchQueries[previous] = ''
  }
})

const activeGroup = computed<FilterGroup | null>(() => {
  const list = groups.value
  if (list.length === 0) return null
  const found = list.find((group) => group.key === activeGroupKey.value)
  return found ?? list[0]!
})

const activeMultiGroup = computed(() =>
  activeGroup.value?.type === 'multi' ? activeGroup.value : null,
)

const filteredMultiOptions = computed(() => {
  const group = activeMultiGroup.value
  if (!group) return []
  const query = searchQueries[group.key].trim().toLowerCase()
  if (!query) return group.options
  return group.options.filter((option) =>
    option.label.toLowerCase().includes(query),
  )
})

const dropdownLabel = computed(() => $t('page.mediums.filter.label'))

// Group selection handled by UTabs v-model

// Popover 打开时通过 content.onOpenAutoFocus 触发 ensureReferenceData()

const getSelectedList = (key: MultiGroupKey): string[] =>
  localFilters.value[key] ?? []

const isMultiOptionSelected = (key: MultiGroupKey, value: string) =>
  getSelectedList(key).includes(value)

const setMultiOptionSelected = (
  key: MultiGroupKey,
  value: string,
  checked: boolean,
) => {
  const current = new Set(getSelectedList(key))
  if (checked) {
    current.add(value)
  } else {
    current.delete(value)
  }
  applyFilter({ [key]: [...current] } as FilterPatch)
}

const clearMultiGroup = (key: MultiGroupKey) => {
  if (searchQueries[key]) {
    searchQueries[key] = ''
  }
  applyFilter({ [key]: [] } as FilterPatch)
}

const getGroupSelectionCount = (key: FilterGroupKey): number => {
  switch (key) {
    case 'artists': {
      return localFilters.value.artists?.length ?? 0
    }
    case 'recent': {
      return localFilters.value.createdInDays === undefined ? 0 : 1
    }
    case 'status': {
      return localFilters.value.readingProgressType !== undefined ||
        localFilters.value.hasReadCount !== undefined
        ? 1
        : 0
    }
    case 'tags': {
      return localFilters.value.tags?.length ?? 0
    }
  }
}

const activeFilterCount = computed(() => {
  return (
    getGroupSelectionCount('status') +
    getGroupSelectionCount('recent') +
    getGroupSelectionCount('tags') +
    getGroupSelectionCount('artists')
  )
})

const clearAllFilters = () => {
  applyFilter({}, [
    'readingProgressType',
    'hasReadCount',
    'createdInDays',
    'tags',
    'artists',
  ])
  searchQueries.tags = ''
  searchQueries.artists = ''
}

// Tabs items derived from groups, keep counts/check badges
const tabItems = computed<TabsItem[]>(() =>
  groups.value.map((group) => {
    const count = getGroupSelectionCount(group.key)
    return {
      label: group.label,
      value: group.key,
      badge: count > 0 ? (isMultiGroupKey(group.key) ? count : '✓') : undefined,
    }
  }),
)
</script>

<template>
  <UPopover
    :content="{
      side: 'bottom',
      align: 'start',
      collisionPadding: 8,
      onOpenAutoFocus: () => ensureReferenceData(),
    }"
    :disabled="disabled"
  >
    <div
      class="text-muted-foreground hover:bg-muted inline-flex cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-sm"
      :class="{ 'cursor-not-allowed opacity-50': disabled }"
    >
      <UIcon name="i-heroicons-funnel" class="text-base" />
      <span>{{ dropdownLabel }}</span>
      <UBadge v-if="activeFilterCount > 0" size="sm" color="primary">
        {{ activeFilterCount }}
      </UBadge>
    </div>

    <template #content>
      <div class="bg-card flex w-[560px] max-w-[90vw] rounded-lg shadow-sm">
        <UTabs
          v-model="activeGroupKey"
          :items="tabItems"
          orientation="vertical"
          variant="link"
          :ui="{
            root: 'flex items-start gap-0',
            list: 'w-40 p-2 border-0',
            indicator: 'hidden',
          }"
          class="w-full"
        >
          <template #content="{ item }">
            <div class="flex-1 p-3">
              <div class="mb-2 flex items-center justify-between">
                <span class="font-medium">
                  {{ item.label }}
                </span>
                <div class="flex items-center">
                  <UButton
                    v-if="
                      getGroupSelectionCount(item.value as FilterGroupKey) > 0
                    "
                    variant="ghost"
                    size="sm"
                    @click="
                      (item.value as FilterGroupKey) === 'tags' ||
                      (item.value as FilterGroupKey) === 'artists'
                        ? clearMultiGroup(item.value as MultiGroupKey)
                        : (item.value as FilterGroupKey) === 'status'
                          ? applyFilter({}, [
                              'readingProgressType',
                              'hasReadCount',
                            ])
                          : applyFilter({}, ['createdInDays'])
                    "
                  >
                    {{ $t('common.reset') }}
                  </UButton>
                  <UButton variant="ghost" size="sm" @click="clearAllFilters">
                    {{ $t('page.mediums.filter.actions.clearAll') }}
                  </UButton>
                </div>
              </div>

              <div
                v-if="
                  (item.value as FilterGroupKey) === 'status' ||
                  (item.value as FilterGroupKey) === 'recent'
                "
                class="space-y-1"
              >
                <button
                  v-for="option in item.value === 'status'
                    ? statusGroup.options
                    : recentGroup.options"
                  :key="option.key"
                  type="button"
                  class="text-muted-foreground hover:bg-muted flex w-full items-center justify-between rounded-md px-2 py-2 text-sm"
                  :class="{
                    'bg-muted text-foreground font-medium': option.active,
                  }"
                  @click="option.onSelect()"
                >
                  <span>{{ option.label }}</span>
                </button>
              </div>

              <div v-else class="space-y-1">
                <div class="flex items-center pb-2">
                  <UInput
                    v-if="activeMultiGroup?.searchable"
                    v-model="searchQueries[activeMultiGroup!.key]"
                    :placeholder="activeMultiGroup?.placeholder"
                  >
                    <template #leading>
                      <UIcon name="i-heroicons-magnifying-glass" />
                    </template>
                  </UInput>
                  <UButton
                    v-if="
                      activeMultiGroup &&
                      getGroupSelectionCount(activeMultiGroup.key) > 0
                    "
                    variant="ghost"
                    size="sm"
                    @click="clearMultiGroup(activeMultiGroup.key)"
                  >
                    {{ $t('common.reset') }}
                  </UButton>
                </div>
                <UScrollbar class="mt-2" :height="256">
                  <template
                    v-if="
                      referenceLoading && activeMultiGroup?.options.length === 0
                    "
                  >
                    <div class="text-muted-foreground py-8 text-center text-sm">
                      {{ $t('common.loading') }}
                    </div>
                  </template>
                  <template v-else-if="filteredMultiOptions.length > 0">
                    <UCheckbox
                      v-for="option in filteredMultiOptions"
                      :key="option.value"
                      :model-value="
                        activeMultiGroup! &&
                        isMultiOptionSelected(
                          activeMultiGroup.key,
                          option.value,
                        )
                      "
                      :label="option.label"
                      :ui="{ label: 'truncate' }"
                      class="w-full py-1"
                      @update:model-value="
                        (value: boolean | 'indeterminate') =>
                          activeMultiGroup &&
                          setMultiOptionSelected(
                            activeMultiGroup.key,
                            option.value,
                            value === true,
                          )
                      "
                    />
                  </template>
                  <UEmpty v-else :description="activeMultiGroup?.emptyText" />
                </UScrollbar>
              </div>
            </div>
          </template>
        </UTabs>
      </div>
    </template>
  </UPopover>
</template>

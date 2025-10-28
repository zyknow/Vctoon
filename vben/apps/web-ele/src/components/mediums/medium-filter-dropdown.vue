<script setup lang="ts">
import type { ComponentSize } from 'element-plus'

import type { MediumGetListInputBase } from '@vben/api'

import { computed, reactive, ref, watch } from 'vue'

import { ReadingProgressType } from '@vben/api'
import { CiSearch, MdiCheck, MdiChevronDown, MdiFilter } from '@vben/icons'
import { useUserStore } from '@vben/stores'

import { $t } from '#/locales'

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

const elementSize = computed<ComponentSize>(() => {
  const map: Record<string, ComponentSize> = {
    small: 'small',
    large: 'large',
    default: 'default',
  }
  return map[props.size] || 'default'
})

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

const activeSingleGroup = computed(() =>
  activeGroup.value?.type === 'single' ? activeGroup.value : null,
)

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

const handleGroupSelect = (key: FilterGroupKey) => {
  activeGroupKey.value = key
}

const handleDropdownVisibleChange = (visible: boolean) => {
  if (visible) {
    void ensureReferenceData()
  }
}

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
</script>

<template>
  <el-dropdown
    v-bind="$attrs"
    :size="elementSize"
    :disabled="disabled"
    trigger="click"
    placement="bottom-start"
    :hide-on-click="false"
    @visible-change="handleDropdownVisibleChange"
  >
    <div>
      <div
        class="medium-filter-dropdown__trigger"
        :class="{ 'is-disabled': disabled }"
      >
        <component :is="MdiFilter" class="text-base" />
        <span>{{ dropdownLabel }}</span>
        <el-tag
          v-if="activeFilterCount > 0"
          size="small"
          type="primary"
          effect="light"
          round
        >
          {{ activeFilterCount }}
        </el-tag>
        <component :is="MdiChevronDown" class="text-base" />
      </div>
    </div>
    <template #dropdown>
      <div class="medium-filter-dropdown__panel">
        <div class="medium-filter-dropdown__sidebar">
          <button
            v-for="group in groups"
            :key="group.key"
            type="button"
            class="medium-filter-dropdown__group-item"
            :class="{ 'is-active': activeGroup?.key === group.key }"
            @click="handleGroupSelect(group.key)"
          >
            <span>{{ group.label }}</span>
            <template v-if="group.key === 'status'">
              <component
                v-if="getGroupSelectionCount('status') > 0"
                :is="MdiCheck"
                class="text-primary text-sm"
              />
            </template>
            <template v-else-if="group.key === 'recent'">
              <component
                v-if="getGroupSelectionCount('recent') > 0"
                :is="MdiCheck"
                class="text-primary text-sm"
              />
            </template>
            <el-tag
              v-else-if="
                isMultiGroupKey(group.key) &&
                getGroupSelectionCount(group.key) > 0
              "
              size="small"
              type="info"
              effect="plain"
              round
            >
              {{ getGroupSelectionCount(group.key) }}
            </el-tag>
          </button>
        </div>
        <div class="medium-filter-dropdown__content">
          <div class="medium-filter-dropdown__content-header">
            <span class="font-medium">
              {{ activeGroup?.label }}
            </span>
            <div class="flex items-center gap-2">
              <el-button
                v-if="
                  activeGroup && getGroupSelectionCount(activeGroup.key) > 0
                "
                text
                size="small"
                @click="
                  activeGroup?.type === 'multi'
                    ? clearMultiGroup(activeGroup.key)
                    : activeGroup.key === 'status'
                      ? applyFilter({}, ['readingProgressType', 'hasReadCount'])
                      : applyFilter({}, ['createdInDays'])
                "
              >
                {{ $t('common.reset') }}
              </el-button>
              <el-button text size="small" @click="clearAllFilters">
                {{ $t('page.mediums.filter.actions.clearAll') }}
              </el-button>
            </div>
          </div>
          <div v-if="activeSingleGroup" class="medium-filter-dropdown__options">
            <button
              v-for="option in activeSingleGroup.options"
              :key="option.key"
              type="button"
              class="medium-filter-dropdown__option"
              :class="{ 'is-active': option.active }"
              @click="option.onSelect()"
            >
              <span>{{ option.label }}</span>
            </button>
          </div>
          <div
            v-else-if="activeMultiGroup"
            class="medium-filter-dropdown__options"
          >
            <div class="flex items-center gap-2 pb-2">
              <el-input
                v-if="activeMultiGroup.searchable"
                v-model="searchQueries[activeMultiGroup.key]"
                :placeholder="activeMultiGroup.placeholder"
                :prefix-icon="CiSearch"
                clearable
                size="small"
              />
              <el-button
                v-if="
                  isMultiGroupKey(activeMultiGroup.key) &&
                  getGroupSelectionCount(activeMultiGroup.key) > 0
                "
                text
                size="small"
                @click="clearMultiGroup(activeMultiGroup.key)"
              >
                {{ $t('common.reset') }}
              </el-button>
            </div>
            <el-scrollbar class="medium-filter-dropdown__scroll">
              <template
                v-if="referenceLoading && activeMultiGroup.options.length === 0"
              >
                <div class="text-muted-foreground py-8 text-center text-sm">
                  {{ $t('common.loading') }}
                </div>
              </template>
              <template v-else-if="filteredMultiOptions.length > 0">
                <el-checkbox
                  v-for="option in filteredMultiOptions"
                  :key="option.value"
                  :label="option.value"
                  :model-value="
                    activeMultiGroup &&
                    isMultiOptionSelected(activeMultiGroup.key, option.value)
                  "
                  class="medium-filter-dropdown__checkbox"
                  @change="
                    (checked: boolean | string | number) =>
                      activeMultiGroup &&
                      setMultiOptionSelected(
                        activeMultiGroup.key,
                        option.value,
                        Boolean(checked),
                      )
                  "
                >
                  <span class="truncate">{{ option.label }}</span>
                </el-checkbox>
              </template>
              <el-empty
                v-else
                :description="activeMultiGroup.emptyText"
                :image-size="80"
              />
            </el-scrollbar>
          </div>
        </div>
      </div>
    </template>
  </el-dropdown>
</template>

<style scoped>
.medium-filter-dropdown__trigger {
  @apply flex items-center gap-1;
}

.medium-filter-dropdown__panel {
  @apply bg-card flex w-[520px] gap-4 rounded-lg p-4 shadow-lg;
}

.medium-filter-dropdown__sidebar {
  @apply flex w-40 flex-col gap-1;
}

.medium-filter-dropdown__group-item {
  @apply flex cursor-pointer items-center justify-between rounded-md px-3 py-2 text-left text-sm transition-colors;
}

.medium-filter-dropdown__group-item:hover {
  @apply bg-muted text-foreground;
}

.medium-filter-dropdown__group-item.is-active {
  @apply bg-primary/10 text-primary;
}

.medium-filter-dropdown__content {
  @apply flex min-h-[232px] flex-1 flex-col;
}

.medium-filter-dropdown__content-header {
  @apply mb-2 flex items-center justify-between;
}

.medium-filter-dropdown__options {
  @apply flex-1;
}

.medium-filter-dropdown__option {
  @apply flex w-full items-center justify-between rounded-md px-3 py-2 text-left text-sm transition-colors;
}

.medium-filter-dropdown__option:hover {
  @apply bg-muted;
}

.medium-filter-dropdown__option.is-active {
  @apply bg-primary/10 text-primary;
}

.medium-filter-dropdown__scroll {
  @apply max-h-64 pr-2;
}

.medium-filter-dropdown__checkbox {
  @apply flex w-full items-center gap-2 py-1;
}
</style>

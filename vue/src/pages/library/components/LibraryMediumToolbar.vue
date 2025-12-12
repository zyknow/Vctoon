<script setup lang="ts">
import type { MediumGetListInput } from '@/api/http/medium/typing'
import type { SortField } from '@/components/mediums/types'
import type { MediumProvider, MediumViewTab } from '@/hooks/useMediumProvider'

type MediumFilterValue = Partial<
  Pick<
    MediumGetListInput,
    | 'artists'
    | 'createdInDays'
    | 'hasReadCount'
    | 'readingProgressType'
    | 'tags'
  >
>

type TabsItem = {
  label: string
  value: MediumViewTab
}

const props = defineProps<{
  isMobile: boolean
  title: string
  tabs: TabsItem[]
  modelValue: MediumViewTab
  hasActiveSelection: number
  libraryState: MediumProvider
  seriesState: MediumProvider
  mediumFilterValue: MediumFilterValue
  seriesFilterValue: MediumFilterValue
  librarySorting: string
  seriesSorting: string
  additionalSorts: SortField[]
}>()

const emit = defineEmits<{
  'update:modelValue': [value: MediumViewTab]
  'update:mediumFilterValue': [value: MediumFilterValue]
  'update:seriesFilterValue': [value: MediumFilterValue]
  'update:librarySorting': [value: string]
  'update:seriesSorting': [value: string]
  librarySortChange: [value: string]
  seriesSortChange: [value: string]
}>()

const currentTab = computed<MediumViewTab>({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value),
})
</script>

<template>
  <div class="medium-toolbar" :class="isMobile ? 'mb-2' : 'gap-4'">
    <medium-toolbar-first :title="title">
      <template #center>
        <UTabs
          v-model="currentTab"
          size="xs"
          class="min-w-80"
          :class="isMobile ? 'w-full' : ''"
          :items="tabs"
        />
      </template>
    </medium-toolbar-first>

    <div class="w-full">
      <medium-toolbar-second
        v-if="currentTab === 'library' && hasActiveSelection === 0"
      >
        <template #left>
          <div class="flex flex-row items-center gap-4">
            <medium-filter-dropdown
              :model-value="mediumFilterValue"
              :disabled="libraryState.loading.value"
              @update:model-value="emit('update:mediumFilterValue', $event)"
            />

            <medium-sort-dropdown
              :model-value="librarySorting"
              :additional-sort-field-list="additionalSorts"
              @update:model-value="emit('update:librarySorting', $event)"
              @change="emit('librarySortChange', $event)"
            />
            <UBadge variant="subtle">{{ libraryState.totalCount || 0 }}</UBadge>
          </div>
        </template>
      </medium-toolbar-second>

      <medium-toolbar-second
        v-else-if="currentTab === 'series' && hasActiveSelection === 0"
      >
        <template #left>
          <div class="flex flex-row items-center gap-4">
            <medium-filter-dropdown
              :model-value="seriesFilterValue"
              :disabled="seriesState.loading.value"
              @update:model-value="emit('update:seriesFilterValue', $event)"
            />

            <medium-sort-dropdown
              :model-value="seriesSorting"
              :additional-sort-field-list="additionalSorts"
              @update:model-value="emit('update:seriesSorting', $event)"
              @change="emit('seriesSortChange', $event)"
            />
            <UBadge variant="subtle">{{ seriesState.totalCount || 0 }}</UBadge>
          </div>
        </template>
      </medium-toolbar-second>

      <medium-toolbar-second-select
        v-else-if="hasActiveSelection > 0"
        :show-selected-all-btn="true"
      />
    </div>
  </div>
</template>

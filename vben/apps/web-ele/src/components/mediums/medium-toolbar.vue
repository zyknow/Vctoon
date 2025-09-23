<script setup lang="ts">
import {
  useInjectedMediumItemProvider,
  useInjectedMediumProvider,
} from '#/hooks/useMediumProvider'

defineProps<{
  // 这里可以定义其他需要传递的 props
  hiddenSecondToolbar?: boolean
}>()

const { currentTab } = useInjectedMediumProvider()
const { selectedMediumIds } = useInjectedMediumItemProvider()
</script>

<template>
  <div class="medium-toolbar">
    <medium-toolbar-first>
      <template #left>
        <slot name="left"></slot>
      </template>
      <template #center>
        <slot name="center"></slot>
      </template>
    </medium-toolbar-first>
    <div class="w-full" v-if="!hiddenSecondToolbar">
      <medium-toolbar-second
        v-if="currentTab === 'library' && selectedMediumIds?.length === 0"
      >
        <template #filter-left>
          <slot name="filter-left"></slot>
        </template>
        <template #filter-center>
          <slot name="filter-center"></slot>
        </template>
        <template #filter-right>
          <slot name="filter-right"></slot>
        </template>
      </medium-toolbar-second>
      <medium-toolbar-second-select v-else-if="selectedMediumIds?.length > 0">
        <template #filter-left>
          <slot name="filter-left-select"></slot>
        </template>
        <template #filter-center>
          <slot name="filter-center-select"></slot>
        </template>
        <template #filter-right>
          <slot name="filter-right-select"></slot>
        </template>
      </medium-toolbar-second-select>
    </div>
  </div>
</template>
<style lang="scss" scoped></style>

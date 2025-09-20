<script setup lang="ts">
import { useInjectedMediumProvider } from '#/hooks/useMediumProvider'

const { selectedMediumIds } = useInjectedMediumProvider()
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
    <medium-toolbar-second v-if="selectedMediumIds?.length === 0">
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
    <medium-toolbar-second-select v-else>
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
</template>
<style lang="scss" scoped></style>

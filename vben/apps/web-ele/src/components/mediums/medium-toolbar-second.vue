<script setup lang="ts">
import { useInjectedMediumProvider } from '#/hooks/useMediumProvider'

const { pageRequest, updateSorting, totalCount } = useInjectedMediumProvider()
</script>

<template>
  <div class="flex w-full flex-row items-center justify-between">
    <slot name="left">
      <div class="flex flex-row items-center gap-2">
        <medium-sort-dropdown
          v-model:model-value="pageRequest.sorting"
          @change="updateSorting"
        />
        <el-tag type="primary">{{ totalCount || 0 }}</el-tag>
      </div>
    </slot>
    <slot name="center">
      <div></div>
    </slot>
    <slot name="right">
      <div></div>
    </slot>
  </div>
</template>
<style lang="scss" scoped></style>

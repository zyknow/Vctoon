<script setup lang="ts">
import { useInjectedMediumProvider } from '#/hooks/useMediumProvider'
import { useMediumStore } from '#/store'

const { title, currentTab } = useInjectedMediumProvider()

const mediumStore = useMediumStore()
</script>

<template>
  <div class="flex w-full flex-row items-center justify-between">
    <slot name="left">
      <div class="text-xl font-bold">{{ title }}</div>
    </slot>
    <slot name="center">
      <el-tabs v-model="currentTab">
        <el-tab-pane label="Recommend" name="recommend" />
        <el-tab-pane label="Library" name="library" />
        <el-tab-pane label="Collection" name="collection" />
      </el-tabs>
    </slot>
    <div name="right">
      <div class="flex w-40 flex-row items-center">
        <el-slider
          size="small"
          class="scale-75"
          :min="0.8"
          :max="1.6"
          :step="0.05"
          v-model="mediumStore.itemZoom"
        />
        <medium-layout-dropdown-select v-model="mediumStore.itemDisplayMode" />
      </div>
    </div>
  </div>
</template>
<style lang="scss" scoped></style>

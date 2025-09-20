<script setup lang="ts">
import { computed, ref } from 'vue'

import { MdiViewGrid, MdiViewList } from '@vben/icons'

const props = defineProps<{
  modelValue?: 'grid' | 'list'
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: 'grid' | 'list'): void
}>()

const layouts = {
  grid: MdiViewGrid,
  list: MdiViewList,
}
const selectedLayout = ref<'grid' | 'list'>('grid')
const selectLayoutList = computed(
  () =>
    Object.keys(layouts).filter(
      (layout) => layout !== selectedLayout.value,
    ) as ('grid' | 'list')[],
)

const updateLayout = (layout: 'grid' | 'list') => {
  selectedLayout.value = layout
  emit('update:modelValue', layout)
}

const selectedLayoutComponent = computed(() => layouts[selectedLayout.value])
</script>

<template>
  <el-dropdown>
    <component :is="selectedLayoutComponent" class="cursor-pointer text-3xl" />
    <template #dropdown>
      <el-dropdown-menu>
        <el-dropdown-item
          :icon="layouts[item]"
          @click="updateLayout(item)"
          v-for="item in selectLayoutList"
          :key="item"
        >
          <span>{{ item }}</span>
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>
<style lang="scss" scoped></style>

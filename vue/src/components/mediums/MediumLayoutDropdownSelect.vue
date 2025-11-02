<script setup lang="ts">
const props = defineProps<{
  modelValue?: 'grid' | 'list'
}>()

const emit = defineEmits<{
  'update:modelValue': [value: 'grid' | 'list']
}>()

const items = computed(() => {
  const allLayouts: Array<{
    value: 'grid' | 'list'
    label: string
    icon: string
  }> = [
    { value: 'grid', label: 'Grid', icon: 'i-heroicons-squares-2x2' },
    { value: 'list', label: 'List', icon: 'i-heroicons-list-bullet' },
  ]

  return allLayouts
    .filter((layout) => layout.value !== props.modelValue)
    .map((layout) => ({
      label: layout.label,
      icon: layout.icon,
      onSelect: () => emit('update:modelValue', layout.value),
    }))
})

const currentIcon = computed(() => {
  return props.modelValue === 'grid'
    ? 'i-heroicons-squares-2x2'
    : 'i-heroicons-list-bullet'
})
</script>

<template>
  <UDropdownMenu :items="items">
    <UIcon :name="currentIcon" class="size-6 cursor-pointer" />
  </UDropdownMenu>
</template>

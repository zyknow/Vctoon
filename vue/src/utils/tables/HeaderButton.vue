<script setup lang="ts">
import { ButtonProps } from '@nuxt/ui'

const props = defineProps<ButtonProps>()
</script>

<template>
  <UButton v-bind="props"> </UButton>
</template>
<style lang="scss" scoped></style>

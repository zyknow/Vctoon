<script setup lang="ts"></script>

<template>
  <div class="flex w-full flex-row items-center justify-between">
    <slot name="left"> </slot>
    <slot name="center">
      <div></div>
    </slot>
    <slot name="right">
      <div></div>
    </slot>
  </div>
</template>
<style lang="scss" scoped></style>

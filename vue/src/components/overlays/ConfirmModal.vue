<script setup lang="ts">
import { computed } from 'vue'

interface Props {
  cancelText?: string
  confirmText?: string
  danger?: boolean
  title?: string | undefined
  message?: string
}

const props = withDefaults(defineProps<Props>(), {
  danger: false,
  cancelText: undefined,
  confirmText: undefined,
  title: undefined,
  message: undefined,
})

const emit = defineEmits<{
  close: [value: boolean]
}>()

const buttonColor = computed(() => (props.danger ? 'error' : 'primary'))

function handleCancel() {
  emit('close', false)
}

function handleConfirm() {
  emit('close', true)
}
</script>

<template>
  <UModal :title="title || $t('common.confirm')">
    <template #body>
      <p v-if="message">{{ message }}</p>
    </template>

    <template #footer>
      <UButton color="neutral" variant="ghost" @click="handleCancel">
        {{ cancelText || $t('common.cancel') }}
      </UButton>
      <UButton :color="buttonColor" variant="solid" @click="handleConfirm">
        {{ confirmText || $t('common.confirm') }}
      </UButton>
    </template>
  </UModal>
</template>

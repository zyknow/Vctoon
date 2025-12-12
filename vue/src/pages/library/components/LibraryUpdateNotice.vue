<script setup lang="ts">
import { $t } from '@/locales/i18n'

defineProps<{
  visible: boolean
}>()

const emit = defineEmits<{
  refresh: []
  dismiss: []
}>()
</script>

<template>
  <div v-if="visible" class="fixed right-4 bottom-4 z-50">
    <UCard>
      <div class="flex items-start gap-3">
        <UIcon name="i-lucide-refresh-cw" class="text-info text-lg" />
        <div class="flex-1">
          <div class="text-sm font-medium">
            {{ $t('page.library.messages.updatedTitle') }}
          </div>
          <div class="text-muted-foreground text-sm">
            {{ $t('page.library.messages.updatedDescription') }}
          </div>
        </div>
        <div class="flex gap-2">
          <UButton variant="ghost" size="sm" @click="emit('refresh')">
            <template #leading>
              <UIcon name="i-lucide-refresh-cw" />
            </template>
            {{ $t('common.refresh') }}
          </UButton>
          <UButton variant="ghost" size="sm" @click="emit('dismiss')">
            <template #leading>
              <UIcon name="i-lucide-x" />
            </template>
            {{ $t('common.cancel') }}
          </UButton>
        </div>
      </div>
    </UCard>
  </div>
</template>

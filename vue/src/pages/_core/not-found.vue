<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

function goHome() {
  router.push('/').catch(() => {})
}
</script>

<template>
  <div class="flex h-screen w-screen flex-col items-center justify-center">
    <div class="flex flex-col items-center gap-6 text-center">
      <div class="bg-muted/50 rounded-full p-6">
        <UIcon name="i-lucide-search-x" class="text-primary size-12" />
      </div>

      <div class="space-y-2">
        <h1 class="text-3xl font-semibold">
          {{ $t('ui.fallback.pageNotFound') }}
        </h1>
        <p class="text-muted-foreground mx-auto max-w-md">
          {{ $t('ui.fallback.pageNotFoundDesc') }}
        </p>
      </div>

      <UButton size="lg" icon="i-lucide-arrow-left" @click="goHome">
        {{ $t('ui.fallback.pageNotFoundAction') }}
      </UButton>
    </div>
  </div>
</template>

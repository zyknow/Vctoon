<script setup lang="ts">
import { useIsMobile } from '@/hooks/useIsMobile'
import type { MediumProvider } from '@/hooks/useMediumProvider'
import {
  provideMediumItemProvider,
  provideMediumProvider,
} from '@/hooks/useMediumProvider'

const props = defineProps<{
  state: MediumProvider
}>()

const { isMobile } = useIsMobile()

provideMediumProvider(props.state)
provideMediumItemProvider({
  items: props.state.items,
  selectedMediumIds: props.state.selectedMediumIds,
})

const contentRef = ref<{
  onEndReached: (direction: 'top' | 'bottom' | 'left' | 'right') => void
} | null>(null)

const onEndReached = (direction: 'top' | 'bottom' | 'left' | 'right') => {
  contentRef.value?.onEndReached(direction)
}

defineExpose({
  onEndReached,
})
</script>

<template>
  <MediumContent ref="contentRef" :use-scrollbar="!isMobile" />
</template>

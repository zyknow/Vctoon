<script setup lang="ts">
import {
  computed,
  nextTick,
  onActivated,
  onDeactivated,
  onMounted,
  onUnmounted,
  ref,
  useSlots,
  watch,
} from 'vue'

import { useIsMobile } from '@/hooks/useIsMobile'
import { useLayoutStore } from '@/stores/layout'

const props = withDefaults(
  defineProps<{
    mobileOnly?: boolean
  }>(),
  {
    mobileOnly: false,
  },
)

const layoutStore = useLayoutStore()
const slots = useSlots()
const { isMobile } = useIsMobile()

const isReady = ref(false)

const isActive = computed(() => {
  if (props.mobileOnly) {
    return isMobile.value
  }
  return true
})

// Track registration state
const registered = {
  header: false,
  headerLeft: false,
  headerRight: false,
  footer: false,
}

const updateRegistration = (register: boolean) => {
  // Header
  if (slots.header) {
    if (register && !registered.header) {
      layoutStore.registerCustomHeader()
      registered.header = true
    } else if (!register && registered.header) {
      layoutStore.unregisterCustomHeader()
      registered.header = false
    }
  }

  // Header Left
  if (slots['header-left']) {
    if (register && !registered.headerLeft) {
      layoutStore.registerCustomHeaderLeft()
      registered.headerLeft = true
    } else if (!register && registered.headerLeft) {
      layoutStore.unregisterCustomHeaderLeft()
      registered.headerLeft = false
    }
  }

  // Header Right
  if (slots['header-right']) {
    if (register && !registered.headerRight) {
      layoutStore.registerCustomHeaderRight()
      registered.headerRight = true
    } else if (!register && registered.headerRight) {
      layoutStore.unregisterCustomHeaderRight()
      registered.headerRight = false
    }
  }

  // Footer
  if (slots.footer) {
    if (register && !registered.footer) {
      layoutStore.registerCustomFooter()
      registered.footer = true
    } else if (!register && registered.footer) {
      layoutStore.unregisterCustomFooter()
      registered.footer = false
    }
  }
}

const register = () => {
  if (!isActive.value) return
  updateRegistration(true)
}
const unregister = () => updateRegistration(false)

const checkTargets = () => {
  let ready = true
  if (slots.header && !document.querySelector('#layout-header')) ready = false
  if (slots['header-left'] && !document.querySelector('#layout-header-left'))
    ready = false
  if (slots['header-right'] && !document.querySelector('#layout-header-right'))
    ready = false
  if (slots.footer && !document.querySelector('#layout-footer')) ready = false
  return ready
}

const ensureReady = () => {
  if (checkTargets()) {
    isReady.value = true
  } else {
    setTimeout(() => {
      if (checkTargets()) {
        isReady.value = true
      } else {
        setTimeout(() => {
          isReady.value = true
        }, 200)
      }
    }, 50)
  }
}

watch(isActive, async (active) => {
  if (active) {
    register()
    await nextTick()
    ensureReady()
  } else {
    unregister()
    isReady.value = false
  }
})

onMounted(async () => {
  if (isActive.value) {
    register()
    await nextTick()
    ensureReady()
  }
})

onActivated(register)
onUnmounted(unregister)
onDeactivated(unregister)
</script>

<template>
  <div v-if="isReady && isActive">
    <Teleport v-if="$slots.header" to="#layout-header">
      <slot name="header" :is-mobile="isMobile" />
    </Teleport>

    <Teleport v-if="$slots['header-left']" to="#layout-header-left">
      <slot name="header-left" :is-mobile="isMobile" />
    </Teleport>

    <Teleport v-if="$slots['header-right']" to="#layout-header-right">
      <slot name="header-right" :is-mobile="isMobile" />
    </Teleport>

    <Teleport v-if="$slots.footer" to="#layout-footer">
      <slot name="footer" :is-mobile="isMobile" />
    </Teleport>
  </div>
</template>

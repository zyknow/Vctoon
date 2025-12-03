import { ref } from 'vue'
import { defineStore } from 'pinia'

export const useLayoutStore = defineStore('layout', () => {
  const customHeaderLeftCount = ref(0)
  const customHeaderRightCount = ref(0)
  const customHeaderCount = ref(0)
  const customFooterCount = ref(0)

  function registerCustomHeaderLeft() {
    customHeaderLeftCount.value++
  }

  function unregisterCustomHeaderLeft() {
    customHeaderLeftCount.value--
  }

  function registerCustomHeaderRight() {
    customHeaderRightCount.value++
  }

  function unregisterCustomHeaderRight() {
    customHeaderRightCount.value--
  }

  function registerCustomHeader() {
    customHeaderCount.value++
  }

  function unregisterCustomHeader() {
    customHeaderCount.value--
  }

  function registerCustomFooter() {
    customFooterCount.value++
  }

  function unregisterCustomFooter() {
    customFooterCount.value--
  }

  return {
    customHeaderLeftCount,
    registerCustomHeaderLeft,
    unregisterCustomHeaderLeft,
    customHeaderRightCount,
    registerCustomHeaderRight,
    unregisterCustomHeaderRight,
    customHeaderCount,
    registerCustomHeader,
    unregisterCustomHeader,
    customFooterCount,
    registerCustomFooter,
    unregisterCustomFooter,
  }
})

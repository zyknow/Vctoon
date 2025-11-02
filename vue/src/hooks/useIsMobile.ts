import { computed, readonly } from 'vue'
import { useBreakpoints, useWindowSize } from '@vueuse/core'

const tailwindBreakpoints = {
  sm: 640,
  md: 768,
  lg: 1024,
  xl: 1280,
  '2xl': 1536,
}

export type TailwindBreakpointKey = keyof typeof tailwindBreakpoints

export interface UseIsMobileOptions {
  breakpoint?: TailwindBreakpointKey
  maxWidth?: number
  ssrWidth?: number
}

export function useIsMobile(options: UseIsMobileOptions = {}) {
  const { breakpoint = 'lg', maxWidth, ssrWidth } = options

  const breakpoints = useBreakpoints(tailwindBreakpoints)
  const { width } = useWindowSize({
    initialWidth:
      typeof ssrWidth === 'number' ? ssrWidth : tailwindBreakpoints.lg,
    initialHeight: 0,
  })

  const isBelowBreakpoint = breakpoints.smaller(breakpoint)

  const isMobile = computed(() => {
    if (typeof maxWidth === 'number') {
      return width.value <= maxWidth
    }

    return isBelowBreakpoint.value
  })

  const isDesktop = computed(() => !isMobile.value)
  const currentBreakpoint = breakpoints.current()

  return {
    width,
    currentBreakpoint,
    isMobile: readonly(isMobile),
    isDesktop: readonly(isDesktop),
    breakpoints: tailwindBreakpoints,
  }
}

export type UseIsMobileReturn = ReturnType<typeof useIsMobile>

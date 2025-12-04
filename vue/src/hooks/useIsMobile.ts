import { computed, readonly } from 'vue'
import {
  createSharedComposable,
  useBreakpoints,
  useWindowSize,
} from '@vueuse/core'

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

const useSharedBreakpoints = createSharedComposable(() =>
  useBreakpoints(tailwindBreakpoints),
)

const useSharedWindowSize = createSharedComposable(() =>
  useWindowSize({
    initialWidth: tailwindBreakpoints.lg,
    initialHeight: 0,
  }),
)

export function useIsMobile(options: UseIsMobileOptions = {}) {
  const { breakpoint = 'lg', maxWidth, ssrWidth } = options

  const breakpoints = useSharedBreakpoints()

  let width
  if (typeof ssrWidth === 'number') {
    const windowSize = useWindowSize({
      initialWidth: ssrWidth,
      initialHeight: 0,
    })
    width = windowSize.width
  } else {
    const windowSize = useSharedWindowSize()
    width = windowSize.width
  }

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

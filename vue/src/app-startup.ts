import { computed, watch } from 'vue'
import * as locales from '@nuxt/ui/locale'
import { useHead } from '@nuxt/ui/runtime/vue/stubs.js'
import { useColorMode } from '@vueuse/core'
import colors from 'tailwindcss/colors'
import { useI18n } from 'vue-i18n'

export const appThemeStartup = () => {
  const appConfig = useAppConfig()

  const getStoreUi = () => {
    const saveUi = localStorage.getItem('nuxt-ui')
    if (saveUi) {
      return JSON.parse(saveUi)
    }
    return null
  }

  const storeUi = getStoreUi()

  appConfig.ui = { ...appConfig.ui, ...storeUi }

  watch(
    appConfig.ui,
    (newVal) => {
      const saveUi = {
        colors: newVal.colors,
        theme: newVal.theme,
      }

      appConfig.ui.theme.blackAsPrimary = newVal.colors.primary === 'black'

      console.log(appConfig.ui.theme.blackAsPrimary)

      localStorage.setItem('nuxt-ui', JSON.stringify(saveUi))
    },
    { deep: true },
  )

  const colorMode = useColorMode()

  const color = computed(() =>
    colorMode.value === 'dark'
      ? (colors as any)[appConfig.ui.colors.neutral][900]
      : 'white',
  )
  const radius = computed(
    () => `:root { --ui-radius: ${appConfig.ui.theme.radius}rem; }`,
  )
  const blackAsPrimary = computed(() =>
    appConfig.ui.theme.blackAsPrimary
      ? `:root { --ui-primary: black; } .dark { --ui-primary: white; }`
      : ':root {}',
  )

  useHead({
    meta: [
      { name: 'viewport', content: 'width=device-width, initial-scale=1' },
      { key: 'theme-color', name: 'theme-color', content: color },
    ],
    style: [
      { innerHTML: radius, id: 'nuxt-ui-radius', tagPriority: -2 },
      {
        innerHTML: blackAsPrimary,
        id: 'nuxt-ui-black-as-primary',
        tagPriority: -2,
      },
    ],
  })
}

export const appI18nStartup = () => {
  const { locale } = useI18n()

  const localeMap: Record<string, keyof typeof locales> = {
    'en-US': 'en',
    'zh-CN': 'zh_cn',
  }

  const lang = computed(() => locales[localeMap[locale.value]]?.code)
  const dir = computed(() => locales[localeMap[locale.value]]?.dir)

  useHead({
    htmlAttrs: {
      lang,
      dir,
    },
  })

  return computed(() => locales[localeMap[locale.value]])
}

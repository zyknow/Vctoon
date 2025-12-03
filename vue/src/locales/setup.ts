import { watch } from 'vue'
import { storeToRefs } from 'pinia'
import type { App } from 'vue'

import { useUserStore } from '@/stores'
import { usePreferenceStore } from '@/stores/preferences'

import { i18n, type SupportedLocale } from './i18n'

type MessageDictionary = Record<string, unknown>

// 懒加载并合并同一语言目录下的多个 JSON：./langs/<locale>/*.json
const LOCALE_MODULE_LOADERS = import.meta.glob('./langs/*/*.json') as Record<
  string,
  () => Promise<{ default: MessageDictionary }>
>

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

async function loadLocaleMessages(
  locale: SupportedLocale,
): Promise<MessageDictionary> {
  const entries = Object.entries(LOCALE_MODULE_LOADERS).filter(([path]) =>
    path.includes(`/langs/${locale}/`),
  )

  if (entries.length === 0) {
    return {}
  }

  function getNamespaceFromPath(path: string): string {
    const filename = path.split('/').pop() ?? ''
    return filename.endsWith('.json') ? filename.slice(0, -5) : filename
  }

  function deepMerge(
    target: MessageDictionary,
    source: MessageDictionary,
  ): void {
    for (const [key, value] of Object.entries(source)) {
      const current = (target as Record<string, unknown>)[key]
      if (isRecord(current) && isRecord(value)) {
        deepMerge(current as MessageDictionary, value as MessageDictionary)
      } else {
        ;(target as Record<string, unknown>)[key] = value
      }
    }
  }

  const loaded = await Promise.all(
    entries.map(async ([path, loader]) => {
      const ns = getNamespaceFromPath(path)
      try {
        const mod = await loader()
        const data = isRecord(mod?.default)
          ? (mod.default as MessageDictionary)
          : {}
        return [ns, data] as const
      } catch {
        return [ns, {} as MessageDictionary] as const
      }
    }),
  )

  const merged: MessageDictionary = {}
  for (const [ns, part] of loaded) {
    if (!isRecord((merged as Record<string, unknown>)[ns])) {
      ;(merged as Record<string, unknown>)[ns] = {}
    }
    const bucket = (merged as Record<string, unknown>)[ns]
    if (isRecord(bucket) && isRecord(part)) {
      deepMerge(bucket as MessageDictionary, part)
    } else if (isRecord(part)) {
      ;(merged as Record<string, unknown>)[ns] = part
    }
  }
  return merged
}

export function setupI18n(app: App) {
  app.use(i18n)

  const preferenceStore = usePreferenceStore()
  const { locale } = storeToRefs(preferenceStore)

  watch(
    locale,
    async (currentLocale) => {
      const target =
        (currentLocale as SupportedLocale) ?? ('en-US' as SupportedLocale)
      try {
        const messages = await loadLocaleMessages(target)
        i18n.global.setLocaleMessage(target, messages as MessageDictionary)
        i18n.global.locale.value = target
        if (typeof document !== 'undefined') {
          document.documentElement.setAttribute('lang', target)
        }

        const userStore = useUserStore()
        await userStore.refreshAccessMenuLocale()
      } catch {
        // 兜底回退到 en-US
        const fallback = 'en-US' as SupportedLocale
        const messages = await loadLocaleMessages(fallback)
        i18n.global.setLocaleMessage(fallback, messages as MessageDictionary)
        i18n.global.locale.value = fallback
        if (typeof document !== 'undefined') {
          document.documentElement.setAttribute('lang', fallback)
        }
      }
    },
    { immediate: true },
  )

  if (import.meta.hot) {
    import.meta.hot.on('vite:afterUpdate', async (payload) => {
      const hasLocaleUpdate = payload.updates.some((u) =>
        u.path.includes('/src/locales/langs/'),
      )

      if (hasLocaleUpdate) {
        const currentLocale =
          (locale.value as SupportedLocale) ?? ('en-US' as SupportedLocale)
        try {
          const messages = await loadLocaleMessages(currentLocale)
          i18n.global.setLocaleMessage(
            currentLocale,
            messages as MessageDictionary,
          )
        } catch (error) {
          console.error('Failed to reload locale messages:', error)
        }
      }
    })
  }

  

  return i18n
}

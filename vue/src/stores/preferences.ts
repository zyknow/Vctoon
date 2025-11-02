import { defineStore } from 'pinia'

import { SUPPORTED_LOCALES } from '@/locales/i18n'

export type LocaleCode = (typeof SUPPORTED_LOCALES)[number]['code']
export type SupportedLocale = (typeof SUPPORTED_LOCALES)[number]
export type ColorScheme = 'light' | 'dark' | 'system'

/** Preference Store 状态接口 */
export interface PreferenceState {
  /** 当前语言 */
  locale: LocaleCode
  /** 侧边栏是否折叠 */
  collapsed: boolean
}

export const usePreferenceStore = defineStore('preferences', {
  state: (): PreferenceState => ({
    locale: 'en-US',
    collapsed: false,
  }),

  getters: {},

  actions: {
    setLocale(code: LocaleCode): void {
      if (
        SUPPORTED_LOCALES.some((localeOption) => localeOption.code === code)
      ) {
        this.locale = code
      }
    },
    setCollapsed(value: boolean): void {
      this.collapsed = value
    },
  },

  // 使用 persist 插件自动持久化
  persist: {
    key: 'app-preferences',
    storage: typeof window !== 'undefined' ? localStorage : undefined,
    pick: ['locale', 'collapsed'],
  },
})

import { createI18n } from 'vue-i18n'

export const SUPPORTED_LOCALES = [
  { code: 'en-US', label: 'English' },
  { code: 'zh-CN', label: '简体中文' },
]

export type SupportedLocale = 'en-US' | 'zh-CN'

export const i18n = createI18n({
  legacy: false,
  locale: 'en-US',
  globalInjection: true,
  messages: {},
  // 日期时间格式定义：支持 long 与 short，两种语言
  datetimeFormats: {
    'en-US': {
      // 默认建议使用 long：完整日期+时间，12 小时制
      long: {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false,
      },
      // short：常用于列表场景，较紧凑
      short: {
        year: '2-digit',
        month: 'numeric',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true,
      },
    },
    'zh-CN': {
      // long：完整日期+时间，24 小时制
      long: {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false,
      },
      // short：紧凑显示
      short: {
        year: '2-digit',
        month: 'numeric',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false,
      },
    },
  },
  // 关闭缺失键与回退告警（动态加载期间会暂时缺失）
  missingWarn: false,
  fallbackWarn: false,
})

export const $t = i18n.global.t
export const $te = i18n.global.te
export const $d = i18n.global.d

export type DateTimeStyle = 'long' | 'short'

/**
 * 使用 i18n 的日期时间格式化，默认采用 long 样式。
 * - 入参支持 Date 或时间戳（number）。如需支持字符串，请先在上层完成解析。
 * - 当入参无效时返回空字符串，避免抛错影响 UI。
 */
/** 字符串是否为纯数字 */
function isNumericString(s: string): boolean {
  return /^\d+$/.test(s)
}

/** 是否为有效日期对象 */
function isValidDate(d: Date): boolean {
  return d instanceof Date && !Number.isNaN(d.getTime())
}

/**
 * 尝试将多种输入规范化为 vue-i18n 可接受的日期值（Date 或 number 毫秒时间戳）
 * - 支持：Date、number（毫秒）、string（ISO 字符串、纯数字时间戳：10位秒/13位毫秒）
 */
function parseDateInput(
  input: Date | number | string | null | undefined,
): Date | number | null {
  if (input === null || input === undefined) return null
  if (input instanceof Date) return isValidDate(input) ? input : null
  if (typeof input === 'number') return Number.isFinite(input) ? input : null
  if (typeof input === 'string') {
    const s = input.trim()
    if (!s) return null
    // 纯数字：按 10 位秒、13 位毫秒处理；其他长度按毫秒尝试
    if (isNumericString(s)) {
      const n = Number(s)
      if (!Number.isFinite(n)) return null
      if (s.length === 10) return n * 1000 // 秒 -> 毫秒
      return n // 默认视为毫秒
    }
    // 非纯数字：尝试按 ISO 日期解析
    const d = new Date(s)
    return isValidDate(d) ? d : null
  }
  return null
}

export function formatDateTime(
  value: Date | number | string | null | undefined,
  style: DateTimeStyle = 'long',
): string {
  try {
    const parsed = parseDateInput(value)
    if (parsed === null) return ''
    return i18n.global.d(parsed, style)
  } catch {
    return ''
  }
}

export type AppI18n = typeof i18n

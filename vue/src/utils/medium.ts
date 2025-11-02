/**
 * Medium 相关的工具函数
 */

import { $d } from '@/locales/i18n'

/**
 * 格式化日期时间
 */
export function formatMediumDateTime(
  value: Date | string | null | undefined,
): string {
  if (!value) return '-'

  return $d(value, 'long')
}

/**
 * 格式化阅读进度（0-1 -> 百分比字符串）
 */
export function formatMediumProgress(
  progress: number | null | undefined,
): string {
  if (progress == null) return '-'

  const clamped = Math.max(0, Math.min(1, progress))
  const percent = Math.round(clamped * 100)

  return `${percent}%`
}

/**
 * 格式化文件大小
 */
export function formatFileSize(bytes: number | null | undefined): string {
  if (bytes == null || bytes <= 0) return '-'

  const units = ['B', 'KB', 'MB', 'GB', 'TB']
  let size = bytes
  let unitIndex = 0

  while (size >= 1024 && unitIndex < units.length - 1) {
    size /= 1024
    unitIndex++
  }

  return `${size.toFixed(2)} ${units[unitIndex]}`
}

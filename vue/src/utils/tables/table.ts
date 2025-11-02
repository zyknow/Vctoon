import { h, type VNode } from 'vue'
import type { Column } from '@tanstack/vue-table'

import { $t } from '@/locales/i18n'

import HeaderButton from './HeaderButton.vue'
/**
 * 创建可排序的表头按钮（通用）
 * - 使用 Nuxt UI 的 UButton 组件
 * - 点击切换排序方向，自动显示当前排序图标
 * - 使用 i18n key，确保语言切换时响应式
 */
export function createSortableHeader<T extends object>(
  labelKey: string,
): (ctx: { column: Column<T, unknown> }) => VNode {
  return ({ column }) => {
    const isSorted = column.getIsSorted() as false | 'asc' | 'desc'
    const UButton = HeaderButton
    return h(UButton, {
      color: 'neutral',
      variant: 'ghost',
      label: $t(labelKey),
      icon: isSorted
        ? isSorted === 'asc'
          ? 'i-lucide-arrow-up-narrow-wide'
          : 'i-lucide-arrow-down-wide-narrow'
        : '',
      class: '-mx-2.5',
      onClick: () => column.toggleSorting(isSorted === 'asc'),
    })
  }
}

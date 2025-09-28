import { $t } from '#/locales'

import { useDialogService } from '../hooks/useDialogService'

/**
 * 脏检查守卫创建器。
 * 返回 registerDirtyChecker 与 beforeClose，可减少各服务重复代码。
 */
export function createDirtyGuard(options?: {
  /** 关闭确认文案 */
  message?: string
  /** 标题 */
  title?: string
}) {
  const dialog = useDialogService()
  let dirtyCheck: () => boolean = () => false
  const registerDirtyChecker = (fn: () => boolean) => (dirtyCheck = fn)
  // 防止 close -> beforeClose -> confirm -> done -> onClosed -> resolve -> close 再次触发 beforeClose 的循环
  let closing = false
  const markClean = () => (dirtyCheck = () => false)
  const beforeClose = (done: () => void) => {
    if (closing) {
      return done()
    }
    if (!dirtyCheck()) {
      return done()
    }
    dialog
      .confirm({
        message:
          options?.message ||
          $t('common.unsavedLeave') ||
          '有未保存的更改，确定关闭？',
        title: options?.title || $t('common.confirm'),
      })
      .then((ok) => {
        if (!ok) return
        closing = true
        markClean() // 避免 onClosed -> resolve -> close 再次触发确认
        done()
      })
      .catch(() => {})
  }
  return { registerDirtyChecker, beforeClose, markClean }
}

/**
 * 创建编辑去重器，避免同一实体被重复打开多个编辑弹窗。
 * 通过 key 做缓存直到 promise 结束。
 */
export function createEditDeduper<K, V>() {
  const map = new Map<K, Promise<V>>()
  function run(key: K, factory: () => Promise<V>): Promise<V> {
    if (map.has(key)) return map.get(key) as Promise<V>
    const p = factory()
    map.set(key, p)
    p.finally(() => map.delete(key))
    return p
  }
  return { run }
}

/** Medium 说明：
 * Medium 仅支持编辑（openEdit），没有创建弹窗需求：
 * - createDirtyGuard 与 createEditDeduper 在使用时直接套用
 * - 若未来新增 create，可直接复用现有 helper
 */
export const MEDIUM_DIALOG_NOTE = 'Medium 仅有编辑弹窗，无创建流程'

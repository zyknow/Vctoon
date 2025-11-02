import type { RouteLocationNormalizedLoaded } from 'vue-router'

/**
 * 生成 KeepAlive 缓存键：
 * - 默认使用路由名
 * - 如 meta.keepAliveByParams 指定了参数名，则按这些参数拼接分片键
 */
export function getKeepAliveKey(route: RouteLocationNormalizedLoaded): string {
  const name = String(route.name ?? 'unknown')
  const byParams = route.meta?.keepAliveByParams as string[] | undefined
  if (byParams && byParams.length > 0) {
    const values = byParams
      .map((key) => {
        const v = route.params[key]
        return Array.isArray(v) ? v.join(',') : String(v ?? '')
      })
      .join('|')
    return `${name}:${values}`
  }
  return name
}
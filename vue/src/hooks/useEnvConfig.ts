import type { ApplicationConfig } from '../types/env-config'

/**
 * 获取应用配置
 *
 * @returns 应用配置对象
 *
 * @description
 * 在生产环境下，从全局变量 window._APP_CONFIG_ 读取配置（由后端注入）
 * 在开发环境下，从 import.meta.env 读取 .env 文件中的配置
 *
 * @example
 * ```ts
 * // 在 main.ts 中初始化
 * const appConfig = useAppConfig()
 *
 * // 使用配置
 * console.log(appConfig.apiURL)
 * console.log(appConfig.auth.authority)
 * ```
 */
export function useEnvConfig(): ApplicationConfig {
  const isProduction = import.meta.env.PROD

  // 生产环境下，直接使用 window._APP_CONFIG_ 全局变量
  // 这允许在不重新构建的情况下修改配置
  if (isProduction) {
    const prodConfig = window._APP_CONFIG_
    if (!prodConfig) {
      throw new Error(
        '生产环境未找到全局配置 window._APP_CONFIG_，请检查 index.html',
      )
    }
    // 生产环境已经是处理好的配置对象，直接返回
    return prodConfig
  }

  // 开发环境：从环境变量读取并转换
  const rawConfig = import.meta.env

  const {
    VITE_APP_TITLE,
    VITE_GLOB_API_URL,
    VITE_GLOB_AUTH_AUTHORITY,
    VITE_GLOB_AUTH_CLIENT_ID,
    VITE_GLOB_AUTH_CLIENT_SECRET,
    VITE_GLOB_AUTH_SCOPE,
  } = rawConfig

  // 必填配置验证
  if (!VITE_GLOB_API_URL) {
    throw new Error('VITE_GLOB_API_URL 配置不能为空')
  }

  const applicationConfig: ApplicationConfig = {
    title: VITE_APP_TITLE,
    apiURL: VITE_GLOB_API_URL,
    auth: {},
  }

  // 可选的认证配置
  if (VITE_GLOB_AUTH_AUTHORITY && VITE_GLOB_AUTH_CLIENT_ID) {
    applicationConfig.auth = {
      authority: VITE_GLOB_AUTH_AUTHORITY,
      clientId: VITE_GLOB_AUTH_CLIENT_ID,
      clientSecret: VITE_GLOB_AUTH_CLIENT_SECRET,
      scope: VITE_GLOB_AUTH_SCOPE,
    }
  }

  return applicationConfig
}

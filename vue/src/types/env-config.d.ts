/**
 * 应用配置原始类型（从环境变量读取）
 */
export interface AppConfigRaw {
  VITE_APP_TITLE?: string
  VITE_GLOB_API_URL: string
  VITE_GLOB_AUTH_AUTHORITY?: string
  VITE_GLOB_AUTH_CLIENT_ID?: string
  VITE_GLOB_AUTH_CLIENT_SECRET?: string
  VITE_GLOB_AUTH_SCOPE?: string
}

/**
 * 认证配置
 */
export interface AuthConfig {
  authority?: string
  clientId?: string
  clientSecret?: string
  scope?: string
}

/**
 * 应用配置（处理后的配置对象）
 */
export interface ApplicationConfig {
  /** 应用标题 */
  title?: string
  /** API 基础地址 */
  apiURL: string
  /** 认证配置 */
  auth: AuthConfig
}

// 扩展全局 Window 类型
declare global {
  interface Window {
    /** 生产环境全局配置 */
    _APP_CONFIG_?: ApplicationConfig
  }
}

// 确保此文件被视为模块
export {}

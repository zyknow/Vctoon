import type { CreateAxiosDefaults } from 'axios'
import axios from 'axios'

import { useEnvConfig } from '@/hooks/useEnvConfig'
// use helper from OIDC to avoid direct router dependency here
import { usePreferenceStore } from '@/stores/preferences'

import { useOidcManager } from './oidc'

const { apiURL } = useEnvConfig()

function createRequestClient(
  baseURL: string,
  options?: CreateAxiosDefaults,
): CustomAxiosInstance {
  const client = axios.create({
    baseURL,
    ...options,
  })

  // 请求拦截器
  client.interceptors.request.use(async (config) => {
    const oidc = useOidcManager()

    const oidcUser = await oidc.manager.getUser()
    const preferences = usePreferenceStore()

    config.headers.Authorization = `Bearer ${oidcUser?.access_token}`
    config.headers['Accept-Language'] = preferences.locale ?? 'en'
    return config
  })

  // 响应拦截器
  client.interceptors.response.use(
    async (response) => {
      // 检查是否是Blob数据（流式数据，如文件下载）
      if (response.data instanceof Blob) {
        return response
      }

      // 检查响应类型是否为blob（通过responseType配置）
      if (response.config.responseType === 'blob') {
        return response
      }

      // 普通JSON响应，解构data节点
      const { data, status } = response

      // HTTP状态码检查
      if (status < 200 || status >= 300) {
        return Promise.reject(
          new Error(data?.message || `请求失败，状态码：${status}`),
        )
      }

      // ABP框架通常直接返回数据，不需要额外解构
      // 如果后端有自定义的code字段，可以在这里检查
      if (data && typeof data === 'object' && 'code' in data) {
        if (data.code !== 200 && data.code !== 0) {
          return Promise.reject(new Error(data.message || '请求失败'))
        }
        // 返回data节点数据
        return data.data !== undefined ? data.data : data
      }

      // 直接返回响应数据
      return data
    },
    async (error) => {
      const { status } = error.response

      // 错误处理
      const message =
        error.response?.data?.message ||
        error.response?.data?.error?.message ||
        error.message ||
        '请求失败'

      // 检查是否没有权限
      if (status === 403) {
        return Promise.reject(new Error('没有权限'))
      }

      // 检查是否未授权
      if (status === 401) {
        const oidc = useOidcManager()
        await oidc.signinRedirectWithState()
        return
      }

      return Promise.reject(new Error(message))
    },
  )

  return client
}

export const requestClient: CustomAxiosInstance = createRequestClient(apiURL, {
  timeout: 10000,
})

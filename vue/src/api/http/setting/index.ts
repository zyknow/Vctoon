import { requestClient } from '../../request'
import type {
  ResetSettingValuesInput,
  SettingGroup,
  SettingValuesInput,
} from './typing'

/** Setting UI 根路径 */
const baseUrl = '/api/setting-ui'

const url = {
  /** 获取所有设置分组与信息 */
  get: `${baseUrl}`,
  /** 设置设置值（PUT，body: Record<string,string>） */
  setValues: `${baseUrl}/set-setting-values`,
  /** 重置设置值（PUT，body: string[]） */
  resetValues: `${baseUrl}/reset-setting-values`,
}

export const settingUiApi = {
  url,

  /** 获取设置分组列表 */
  get() {
    return requestClient.get<SettingGroup[]>(url.get)
  },

  /** 设置设置值（键值对） */
  setValues(input: SettingValuesInput) {
    return requestClient.put(url.setValues, input)
  },

  /** 重置设置值（按名称列表） */
  resetValues(input: ResetSettingValuesInput) {
    return requestClient.put(url.resetValues, input)
  },
}

export * from './typing'

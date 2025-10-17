/** Setting UI 类型（对齐 EasyAbp.Abp.SettingUi.Dto） */

export type SettingInfo = {
  description?: null | string
  displayName?: null | string
  name?: null | string
  permission?: null | string
  /** 任意属性（输入组件渲染的辅助信息） */
  properties?: null | Record<string, any>
  value?: null | string
}

export type SettingGroup = {
  groupDisplayName?: null | string
  groupName?: null | string
  permission?: null | string
  settingInfos?: null | SettingInfo[]
}

/** 设置值输入：键值对（string -> string） */
export type SettingValuesInput = Record<string, string>

/** 重置设置值输入：数组（每项是设置名） */
export type ResetSettingValuesInput = string[]

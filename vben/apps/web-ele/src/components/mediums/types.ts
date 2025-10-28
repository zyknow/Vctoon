export interface SortField {
  label: string
  value: string
  listSort?: number
}

export interface SortOption {
  field: string
  order: 'asc' | 'desc'
}

export interface MediumSortDropdownProps {
  /**
   * 当前排序选项，格式为 "field order"，如 "creationTime desc"
   */
  modelValue?: string
  /**
   * 额外的排序字段列表
   */
  additionalSortFieldList?: SortField[]
  /**
   * 组件大小
   */
  size?: 'default' | 'large' | 'small'
  /**
   * 是否禁用
   */
  disabled?: boolean
}

export interface MediumSortDropdownEmits {
  'update:modelValue': [value: string]
  change: [value: string]
}

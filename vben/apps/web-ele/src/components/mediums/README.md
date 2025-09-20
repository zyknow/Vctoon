# MediumSortDropdown 排序下拉组件

一个用于媒体内容排序的下拉组件，符合 Vben Admin 5.5.9 项目开发规范。

## 功能特性

- ✅ 使用 `el-dropdown` 提供更好的用户体验
- ✅ 支持多种排序字段（标题、创建时间、修改时间、大小等）
- ✅ 智能排序逻辑：点击同一字段时在 `升序` ↔ `降序` 之间切换
- ✅ 默认按创建时间降序排序，确保总是有排序状态
- ✅ 只有当前排序的字段才显示状态图标
- ✅ 支持自定义额外排序字段
- ✅ 支持多种尺寸（small、default、large）
- ✅ 支持禁用状态
- ✅ 完整的类型定义和国际化支持
- ✅ 遵循项目图标使用规范
- ✅ 响应式设计，移动端友好

## 使用方法

### 基础用法

```vue
<script setup lang="ts">
import { ref } from 'vue'
import type { SortOption } from '@/components/mediums/types'
import MediumSortDropdown from '@/components/mediums/medium-sort-dropdown.vue'

// 默认按创建时间降序排序
const sortOption = ref<SortOption>({
  field: 'dateAdded',
  order: 'desc'
})

const handleSortChange = (value: SortOption) => {
  // 处理排序变化
  console.log('排序已变更:', value)
}
</script>

<template>
  <MediumSortDropdown
    v-model="sortOption"
    @change="handleSortChange"
  />
</template>
```

### 完整配置

```vue
<template>
  <MediumSortDropdown
    v-model="sortOption"
    :additional-sort-field-list="additionalFields"
    size="default"
    :disabled="false"
    @change="handleSortChange"
  >
    <!-- 可选的插槽内容 -->
    <el-button size="small">刷新</el-button>
  </MediumSortDropdown>
</template>
```

## 排序逻辑

组件采用简化的二状态排序逻辑：

1. **默认状态**：按创建时间降序排序（最新的在前）
2. **点击字段**：
   - 如果是不同字段：设置为该字段的升序排序
   - 如果是当前字段且为升序：切换为降序排序
   - 如果是当前字段且为降序：切换为升序排序

注意：组件不允许清空排序状态，始终保持有序状态。

## Props

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `modelValue` | `SortOption` | `{ field: 'dateAdded', order: 'desc' }` | 当前排序选项，默认按创建时间降序 |
| `additionalSortFieldList` | `SortField[]` | `[]` | 额外的排序字段列表 |
| `size` | `'default' \| 'large' \| 'small'` | `'default'` | 组件大小 |
| `disabled` | `boolean` | `false` | 是否禁用 |

## Events

| 事件名 | 参数 | 说明 |
|--------|------|------|
| `update:modelValue` | `SortOption` | 当排序选项发生变化时触发 |
| `change` | `SortOption` | 当排序选项发生变化时触发 |

## 类型定义

```typescript
interface SortField {
  label: string
  value: string
}

interface SortOption {
  field: string
  order: 'asc' | 'desc'
}
```

## 默认排序字段

- `title` - 标题
- `dateAdded` - 创建时间
- `dateModified` - 最近修改时间
- `size` - 大小

## 国际化

组件支持中英文国际化，相关翻译位于：

- `apps/web-ele/src/locales/langs/zh-CN/page.json`
- `apps/web-ele/src/locales/langs/en-US/page.json`

在 `mediums.sort` 命名空间下定义。

## 图标使用

组件使用的图标已在 `packages/icons/src/iconify/index.ts` 中统一导出：

- `MdiSort` - 默认排序图标
- `MdiSortAscending` - 升序图标  
- `MdiSortDescending` - 降序图标
- `MdiChevronDown` - 下拉箭头图标

## 样式规范

- 使用 TailwindCSS 进行样式编写
- 遵循项目颜色系统，优先使用主题色
- 响应式设计，在移动端隐藏排序顺序文本，仅显示图标

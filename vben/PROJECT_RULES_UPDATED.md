# Vben Admin 5.5.9 项目开发规则

## 项目概览

本项目基于 **Vben Admin 5.5.9** 构建，是一个现代化的企业级前端管理系统，使用 **Element Plus** 作为 UI 组件库，后端对接 **ABP Framework**。

### 核心技术栈

- **框架**: Vue 3.5.17 + TypeScript 5.9.2
- **构建工具**: Vite 7.1.2
- **包管理**: pnpm 10.15.1 (Monorepo)
- **UI 组件库**: Element Plus 2.10.5
- **样式方案**: TailwindCSS 3.4.17 + PostCSS
- **状态管理**: Pinia 3.0.3
- **路由**: Vue Router 4.5.1
- **国际化**: Vue I18n 11.1.7
- **认证**: OIDC Client TS 2.4.0
- **后端框架**: ABP Framework

## 架构规范

### 1. 项目结构规范

```
vben/
├── apps/
│   └── web-ele/                    # 主应用（Element Plus版本）
├── packages/
│   ├── @core/                      # 核心包
│   │   ├── base/                   # 基础包
│   │   │   ├── design/             # 设计系统
│   │   │   ├── icons/              # 图标组件
│   │   │   ├── shared/             # 共享工具
│   │   │   └── typings/            # 类型定义
│   │   ├── ui-kit/                 # UI组件套件
│   │   │   ├── form-ui/            # 表单组件 (VbenForm)
│   │   │   ├── layout-ui/          # 布局组件 (VbenAdminLayout)
│   │   │   ├── menu-ui/            # 菜单组件 (Menu, MenuBadge)
│   │   │   ├── popup-ui/           # 弹窗组件 (Modal, Drawer, Alert)
│   │   │   ├── shadcn-ui/          # Shadcn UI组件
│   │   │   └── tabs-ui/            # 标签页组件 (TabsView)
│   │   ├── composables/            # 组合式函数
│   │   │   ├── use-is-mobile       # 移动端检测
│   │   │   ├── use-layout-style    # 布局样式
│   │   │   ├── use-namespace       # 命名空间
│   │   │   ├── use-sortable        # 拖拽排序
│   │   │   └── ...                 # 其他composables
│   │   └── preferences/            # 偏好设置管理
│   ├── effects/                    # 效果包
│   │   ├── access/                 # 权限控制
│   │   │   ├── AccessControl.vue   # 权限控制组件
│   │   │   ├── accessible          # 权限判断
│   │   │   ├── directive           # 权限指令
│   │   │   └── use-access          # 权限hooks
│   │   ├── common-ui/              # 通用UI组件
│   │   │   ├── captcha/            # 验证码组件
│   │   │   ├── count-to/           # 数字动画
│   │   │   ├── ellipsis-text/      # 文本省略
│   │   │   ├── icon-picker/        # 图标选择器
│   │   │   ├── json-viewer/        # JSON查看器
│   │   │   ├── loading/            # 加载组件
│   │   │   ├── page/               # 页面组件
│   │   │   └── resize/             # 尺寸调整
│   │   ├── hooks/                  # 钩子函数
│   │   │   ├── watermark          # 水印功能
│   │   │   └── element-plus-tokens # Element Plus主题
│   │   ├── layouts/                # 布局组件
│   │   │   ├── authentication/    # 认证布局
│   │   │   ├── basic/             # 基础布局
│   │   │   ├── iframe/            # iframe布局
│   │   │   └── widgets/           # 布局小部件
│   │   └── request/                # 请求封装 (axios)
│   ├── api/                        # API定义
│   │   ├── oidc/                  # OIDC认证
│   │   ├── request/               # 请求配置
│   │   ├── vctoon-hub/           # SignalR Hub
│   │   └── vctoon/               # 业务API
│   │       ├── abp-application-configuration/  # ABP配置
│   │       ├── library/          # 图书馆API
│   │       ├── user/             # 用户API
│   │       └── ...               # 其他业务API
│   ├── stores/                     # 状态管理
│   │   ├── modules/
│   │   │   ├── access.ts         # 权限状态
│   │   │   ├── user.ts           # 用户状态
│   │   │   └── tabbar.ts         # 标签页状态
│   │   └── setup.ts              # Store设置
│   ├── utils/                      # 工具函数
│   │   └── helpers/
│   │       ├── generate-menus    # 菜单生成
│   │       ├── generate-routes   # 路由生成
│   │       ├── find-menu-by-path # 菜单查找
│   │       └── reset-routes      # 路由重置
│   ├── constants/                  # 常量定义
│   ├── icons/                      # 图标库
│   ├── locales/                    # 国际化
│   ├── preferences/                # 偏好设置
│   ├── styles/                     # 样式文件
│   └── types/                      # 类型定义
├── internal/                       # 内部工具
│   ├── lint-configs/               # 代码规范配置
│   ├── tailwind-config/            # TailwindCSS配置
│   ├── vite-config/                # Vite配置
│   ├── tsconfig/                   # TypeScript配置
│   └── node-utils/                 # Node工具
└── scripts/                        # 脚本工具
    ├── turbo-run/                  # Turbo运行脚本
    └── vsh/                        # Shell工具
```

### 2. 包功能详细说明

#### 2.1 @core 核心包
- **@vben-core/form-ui**: 提供 `VbenForm` 表单组件，支持动态表单生成
- **@vben-core/layout-ui**: 提供 `VbenAdminLayout` 管理后台布局组件
- **@vben-core/menu-ui**: 提供 `Menu`、`MenuBadge` 菜单相关组件
- **@vben-core/popup-ui**: 提供 `Modal`、`Drawer`、`Alert` 弹窗组件
- **@vben-core/shadcn-ui**: 基于 Radix Vue 的 UI 组件库
- **@vben-core/tabs-ui**: 提供 `TabsView` 标签页组件
- **@vben-core/composables**: 提供常用组合式函数
  - `useIsMobile`: 移动端检测
  - `useLayoutStyle`: 布局样式管理
  - `useNamespace`: 命名空间管理
  - `useSortable`: 拖拽排序功能
- **@vben-core/preferences**: 用户偏好设置管理

#### 2.2 effects 效果包
- **@vben/access**: 权限控制系统
  - `AccessControl.vue`: 权限控制组件
  - `accessible`: 权限判断工具
  - `directive`: v-access 权限指令
  - `use-access`: 权限相关 hooks
- **@vben/common-ui**: 通用UI组件库
  - `captcha`: 验证码组件（滑块、点选等）
  - `count-to`: 数字动画组件
  - `ellipsis-text`: 文本省略组件
  - `icon-picker`: 图标选择器
  - `json-viewer`: JSON查看器
  - `loading`: 加载状态组件
  - `page`: 页面容器组件
  - `resize`: 尺寸调整组件
- **@vben/hooks**: 业务钩子函数
  - `useElementPlusDesignTokens`: Element Plus 主题令牌
  - 水印功能相关 hooks
- **@vben/layouts**: 布局组件系统
  - `authentication`: 认证页面布局
  - `basic`: 基础管理后台布局
  - `iframe`: iframe 嵌入布局
  - `widgets`: 布局小部件
- **@vben/request**: HTTP 请求封装（基于 axios）

#### 2.3 业务包
- **@vben/api**: API接口定义
  - `oidc`: OIDC 认证相关接口
  - `vctoon-hub`: SignalR 实时通信
  - `vctoon/*`: 业务 API（图书馆、用户、标签等）
- **@vben/stores**: 状态管理
  - `access`: 权限状态（权限码、菜单、路由、锁屏等）
  - `user`: 用户状态（用户信息、图书馆列表等）
  - `tabbar`: 标签页状态
- **@vben/utils**: 工具函数
  - `generate-menus`: 菜单生成器
  - `generate-routes`: 路由生成器
  - `find-menu-by-path`: 菜单查找
  - `reset-routes`: 路由重置

### 3. 命名规范

#### 3.1 文件和目录命名
- **组件文件**: 使用 PascalCase，如 `UserProfile.vue`
- **页面文件**: 使用 kebab-case，如 `user-management/index.vue`
- **工具文件**: 使用 kebab-case，如 `format-utils.ts`
- **常量文件**: 使用 SCREAMING_SNAKE_CASE，如 `API_CONSTANTS.ts`

#### 3.2 变量和函数命名
- **变量**: 使用 camelCase，如 `userInfo`, `isLoading`
- **函数**: 使用 camelCase，如 `getUserInfo()`, `handleSubmit()`
- **常量**: 使用 SCREAMING_SNAKE_CASE，如 `API_BASE_URL`
- **类型/接口**: 使用 PascalCase，如 `UserInfo`, `ApiResponse`

### 4. 包依赖规范

#### 4.1 workspace依赖使用
```json
{
  "dependencies": {
    "@vben/api": "workspace:*",
    "@vben/stores": "workspace:*",
    "@vben/utils": "workspace:*"
  }
}
```

#### 4.2 catalog依赖使用
```json
{
  "dependencies": {
    "vue": "catalog:",
    "element-plus": "catalog:",
    "@vueuse/core": "catalog:"
  }
}
```

## 开发规范

### 1. 组件开发规范

#### 1.1 Vue组件结构
```vue
<script lang="ts" setup>
// 1. 导入类型
import type { ComponentProps } from './types'

// 2. 导入外部依赖
import { ref, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'

// 3. 导入内部依赖
import { useUserStore } from '@vben/stores'
import { formatDate } from '@vben/utils'

// 4. 定义组件选项
defineOptions({ 
  name: 'ComponentName',
  inheritAttrs: false 
})

// 5. 定义Props和Emits
const props = withDefaults(defineProps<ComponentProps>(), {
  size: 'default'
})

const emit = defineEmits<{
  change: [value: string]
  submit: [data: FormData]
}>()

// 6. 响应式数据
const loading = ref(false)
const userStore = useUserStore()

// 7. 计算属性
const computedValue = computed(() => {
  return props.value?.toUpperCase()
})

// 8. 方法
const handleSubmit = async () => {
  loading.value = true
  try {
    // 处理逻辑
    emit('submit', formData)
  } catch (error) {
    ElMessage.error('操作失败')
  } finally {
    loading.value = false
  }
}

// 9. 生命周期
onMounted(() => {
  // 初始化逻辑
})
</script>

<template>
  <div class="component-container">
    <!-- 模板内容 -->
  </div>
</template>

<style scoped>
/* 组件样式 */
</style>
```

#### 1.2 Element Plus组件使用规范
```vue
<script setup>
// 自动导入，无需手动import
// AutoImport配置已处理Element Plus组件导入
</script>

<template>
  <!-- 直接使用Element Plus组件，使用el-前缀 -->
  <el-button type="primary" @click="handleClick">
    点击按钮
  </el-button>
  
  <el-form :model="form" :rules="rules">
    <el-form-item label="用户名" prop="username">
      <el-input v-model="form.username" />
    </el-form-item>
  </el-form>
</template>
```

#### 1.3 图标使用规范（集中导出 + 统一引用）

为便于统一管理、按需收敛和重命名，所有业务中使用到的图标，必须先在 `packages/icons/src/iconify/index.ts` 中添加并导出为组件，再在项目中引用使用；禁止在业务代码中直接拼接 `iconify` 名称或使用临时类名。

##### 添加新图标（在统一注册处）

在 `packages/icons/src/iconify/index.ts` 中新增并导出图标组件：

```ts
// packages/icons/src/iconify/index.ts
import { createIconifyIcon } from '@vben-core/icons'

// 命名规范：图标库前缀首字母大写 + PascalCase 名称
// 例如：mdi:plus  -> MdiPlus
export const MdiPlus = createIconifyIcon('mdi:plus')

// 例如：ci:add-plus -> CiAddPlus
export const CiAddPlus = createIconifyIcon('ci:add-plus')

// 已有示例（仓库中已存在）：
// export const MdiVideo = createIconifyIcon('mdi:filmstrip')
// export const MdiComic = createIconifyIcon('mdi:book')
```

命名要求：

- 使用 PascalCase；图标库前缀首字母大写（如 Mdi、Ci、Ri）。
- 尽量采用语义清晰的英文名；避免缩写不明的命名。

##### 在项目中使用（仅引用已导出的组件）

```vue
<script setup lang="ts">
import { MdiPlus, MdiVideo, MdiComic } from '@vben/icons'
</script>

<template>
  <!-- 有 icon 属性的组件，直接传组件引用 -->
  <el-button type="primary" :icon="MdiPlus">添加</el-button>

  <!-- 也可直接渲染为组件使用 -->
  <MdiVideo class="text-xl" />
  <MdiComic class="ml-2 text-lg" />
</template>
```

##### 禁用与例外

- 禁止：在业务代码中直接使用 `createIcon('mdi:xxx')` 或 `icon-[mdi:xxx]` 类名。
- 例外：原型验证、一次性 Demo 可临时使用，但合入前必须改回集中导出的组件方式。

##### 支持的图标库

项目通过 Iconify 使用多个图标库，常见前缀：

| 图标库 | 前缀 | 示例 | 特点 |
|--------|------|------|------|
| Circum Icons | `ci` | `ci:add-plus` | 现代简约风格，适合 UI 界面 |
| Material Design Icons | `mdi` | `mdi:account` | Material Design 风格 |
| Remix Icon | `ri` | `ri:search-line` | 线性图标，简洁清晰 |

注意：实际可用范围以 `@vben/icons` 包集中导出的组件为准；如需新增，请先在集中注册处补充再使用。

#### 1.4 Vben组件使用规范

```vue
<script setup>
import { useVbenForm } from '@vben-core/form-ui'
import { VbenAdminLayout } from '@vben-core/layout-ui'
import { AccessControl } from '@vben/access'
import { CountTo } from '@vben/common-ui'
</script>

<template>
  <!-- 使用Vben布局组件 -->
  <VbenAdminLayout>
    <!-- 权限控制组件 -->
    <AccessControl :permissions="['User.View']">
      <div class="user-content">
        <!-- 数字动画组件 -->
        <CountTo :end-value="1000" />
      </div>
    </AccessControl>
  </VbenAdminLayout>
</template>
```

### 2. 样式开发规范

#### 2.1 TailwindCSS使用规范

```vue
<template>
  <!-- 优先使用TailwindCSS类 -->
  <div class="flex items-center justify-between p-4 bg-white rounded-lg shadow-sm">
    <h1 class="text-2xl font-bold text-gray-900">标题</h1>
    <el-button class="ml-4">操作</el-button>
  </div>
  
  <!-- 使用自定义主题色 -->
  <div class="bg-primary text-primary-foreground p-4">
    主题色背景
  </div>
</template>
```

#### 2.2 颜色使用规范

**核心原则：优先使用主题色系统，避免硬编码颜色值**

##### 颜色优先级顺序

1. **优先使用主题色**: 使用项目定义的设计系统颜色
2. **其次使用语义色**: 使用有明确语义的颜色变量
3. **最后使用具体颜色**: 只在确实不适合时使用具体颜色值

```vue
<template>
  <!-- ✅ 推荐：使用主题色 -->
  <div class="bg-primary text-primary-foreground">主题色</div>
  <div class="text-primary">主题色文本</div>
  <div class="text-muted-foreground">次要文本</div>
  
  <!-- ✅ 推荐：使用语义色 -->
  <div class="text-destructive">错误信息</div>
  <div class="bg-success text-success-foreground">成功状态</div>
  
  <!-- ⚠️ 谨慎使用：具体颜色值 -->
  <div class="text-red-500">仅在确实需要时使用</div>
  
  <!-- ❌ 避免：随意使用颜色 -->
  <div class="text-purple-400">不符合设计系统</div>
</template>
```

##### 可用的主题颜色类

| 类别 | TailwindCSS 类 | 使用场景 |
|------|----------------|----------|
| **主题色** | `bg-primary`, `text-primary` | 品牌色、重要按钮、链接 |
| **前景色** | `text-foreground`, `text-primary-foreground` | 主要文本内容 |
| **次要色** | `text-muted-foreground`, `bg-muted` | 次要信息、辅助文本 |
| **边框色** | `border`, `border-input` | 边框、分割线 |
| **背景色** | `bg-background`, `bg-card` | 页面背景、卡片背景 |
| **语义色** | `text-destructive`, `bg-success` | 状态指示、反馈信息 |

##### 颜色使用示例

```vue
<!-- 按钮颜色使用 -->
<el-button class="bg-primary text-primary-foreground">主要按钮</el-button>
<el-button class="bg-muted text-muted-foreground">次要按钮</el-button>

<!-- 状态指示颜色 -->
<div class="text-primary">✅ 扫描进行中</div>
<div class="text-destructive">❌ 扫描失败</div>
<div class="text-success">✅ 扫描成功</div>

<!-- 图标颜色 -->
<MdiLoading class="text-primary animate-spin" />
<MdiSuccess class="text-success" />
<MdiError class="text-destructive" />
```

##### 禁止事项

- ❌ 不要随意使用 `text-blue-500`, `text-red-400` 等具体颜色值
- ❌ 不要使用与设计系统不符的颜色搭配
- ❌ 不要在同一功能中混用不同的颜色方案

##### 例外情况

只有在以下情况下才可以使用具体颜色值：
- 特定的品牌色要求（如第三方平台图标）
- 数据可视化图表中的特定颜色需求
- 临时原型或演示代码（需在正式开发时改为主题色）

#### 2.3 CSS变量使用

```css
/* 使用项目定义的CSS变量 */
.custom-component {
  background-color: hsl(var(--background));
  color: hsl(var(--foreground));
  border: 1px solid hsl(var(--border));
}

/* 主题色变量 */
.primary-button {
  background-color: hsl(var(--primary));
  color: hsl(var(--primary-foreground));
}
```

### 3. API开发规范

#### 3.1 API定义规范

```typescript
// packages/api/src/vctoon/user.ts
import { requestClient } from '../request'
import type { UserInfo, CreateUserDto } from './types'

export namespace userApi {
  /**
   * 获取用户信息
   */
  export const getUserInfo = (id: string): Promise<UserInfo> => {
    return requestClient.get(`/api/users/${id}`)
  }
  
  /**
   * 创建用户
   */
  export const createUser = (data: CreateUserDto): Promise<UserInfo> => {
    return requestClient.post('/api/users', data)
  }
  
  /**
   * 获取当前用户信息
   */
  export const getCurrentUser = (): Promise<UserInfo> => {
    return requestClient.get('/api/users/current')
  }
}
```

#### 3.2 ABP集成规范

```typescript
// 使用ABP应用配置
import { useAbpStore } from '#/store/abp'

export const useAbpAuth = () => {
  const abpStore = useAbpStore()
  
  const checkPermission = (permission: string): boolean => {
    return abpStore.application?.auth?.grantedPolicies?.[permission] === true
  }
  
  const getCurrentUser = () => {
    return abpStore.application?.currentUser
  }
  
  return {
    checkPermission,
    getCurrentUser
  }
}
```

### 4. 状态管理规范

#### 4.1 使用现有Store

```typescript
// 使用现有的用户状态
import { useUserStore } from '@vben/stores'

export const useUserInfo = () => {
  const userStore = useUserStore()
  
  // 获取用户信息
  const userInfo = computed(() => userStore.userInfo)
  const libraries = computed(() => userStore.libraries)
  
  // 更新用户信息
  const updateUserInfo = (info: UserInfo) => {
    userStore.setUserInfo(info)
  }
  
  return {
    userInfo,
    libraries,
    updateUserInfo
  }
}
```

#### 4.2 权限状态使用

```typescript
// 使用权限状态
import { useAccessStore } from '@vben/stores'

export const usePermission = () => {
  const accessStore = useAccessStore()
  
  // 检查权限
  const hasPermission = (permission: string): boolean => {
    return accessStore.accessCodes.includes(permission)
  }
  
  // 获取可访问菜单
  const accessMenus = computed(() => accessStore.accessMenus)
  
  return {
    hasPermission,
    accessMenus
  }
}
```

### 5. 路由开发规范

#### 5.1 路由定义规范

```typescript
// router/routes/modules/user.ts
import type { RouteRecordRaw } from 'vue-router'
import { $t } from '#/locales'

const routes: RouteRecordRaw[] = [
  {
    meta: {
      icon: 'mdi:account-group',
      keepAlive: true,
      order: 100,
      title: $t('routes.user.title'),
      // 权限控制
      permissions: ['User.Management']
    },
    name: 'UserManagement',
    path: '/user-management',
    component: () => import('#/views/user-management/index.vue'),
    children: [
      {
        meta: {
          title: $t('routes.user.list'),
          permissions: ['User.List']
        },
        name: 'UserList',
        path: 'list',
        component: () => import('#/views/user-management/list.vue')
      }
    ]
  }
]

export default routes
```

### 6. 表单开发规范

#### 6.1 使用VbenForm

```vue
<script setup>
import { useVbenForm } from '@vben-core/form-ui'

const [Form, formApi] = useVbenForm({
  schema: [
    {
      component: 'Input',
      fieldName: 'username',
      label: '用户名',
      rules: 'required'
    },
    {
      component: 'Input',
      componentProps: {
        type: 'password'
      },
      fieldName: 'password',
      label: '密码',
      rules: 'required'
    }
  ]
})

const handleSubmit = async () => {
  const values = await formApi.submitForm()
  console.log('表单值:', values)
}
</script>

<template>
  <div class="form-container">
    <Form />
    <el-button @click="handleSubmit">提交</el-button>
  </div>
</template>
```

### 7. 国际化规范

#### 7.1 多语言文件组织

```
locales/
├── index.ts
└── langs/
    ├── en-US/
    │   ├── page.json
    │   ├── common.json
    │   └── validation.json
    └── zh-CN/
        ├── page.json
        ├── common.json
        └── validation.json
```

#### 7.2 国际化使用规范

```typescript
// 在组件中使用
import { $t } from '#/locales'

// 模板中使用
<template>
  <h1>{{ $t('page.user.title') }}</h1>
  <el-button>{{ $t('common.save') }}</el-button>
</template>

// JavaScript中使用
const title = $t('page.user.title')
ElMessage.success($t('common.success'))
```

## 代码质量规范

### 1. TypeScript规范
- 严格启用TypeScript严格模式
- 优先使用类型推断，避免过度类型标注
- 使用接口定义复杂数据结构
- 为API响应数据定义完整的类型

### 2. 组件使用优先级
1. **优先使用现有Vben组件**: 如 `VbenForm`、`VbenAdminLayout`、`AccessControl` 等
2. **其次使用通用UI组件**: 如 `CountTo`、`JsonViewer`、`IconPicker` 等
3. **最后使用Element Plus组件**: 基础UI组件使用 `el-` 前缀

### 3. 状态管理优先级
1. **优先使用现有Store**: `useUserStore`、`useAccessStore` 等
2. **复用现有状态逻辑**: 避免重复实现相同功能
3. **必要时扩展现有Store**: 而不是创建新的Store

### 4. API使用规范
1. **使用现有API定义**: 优先使用 `@vben/api` 中已定义的接口
2. **遵循ABP规范**: 与后端ABP Framework保持一致
3. **统一错误处理**: 使用项目统一的错误处理机制

## 性能优化规范

### 1. 组件懒加载
```typescript
// 路由懒加载
component: () => import('#/views/user-management/index.vue')

// 组件懒加载
const AsyncComponent = defineAsyncComponent(() => import('./HeavyComponent.vue'))
```

### 2. 图片资源优化
- 使用WebP格式图片
- 大图片使用懒加载
- 合理使用图片压缩

### 3. 包体积优化
- 按需导入第三方库
- 使用Tree Shaking
- 避免重复打包

## 安全规范

### 1. 权限控制
- 使用`@vben/access`包进行权限控制
- 路由级别权限验证
- 组件级别权限控制

### 2. 数据安全
- 敏感数据加密存储
- API请求使用HTTPS
- 防止XSS和CSRF攻击

## 部署规范

### 1. 环境配置
- 开发环境：`pnpm dev:ele`
- 构建生产：`pnpm build:ele`
- 预览构建：`pnpm preview`

### 2. Docker部署
```bash
# 构建Docker镜像
pnpm build:docker

# 运行容器
docker run -p 80:80 vben-admin
```

## 故障排查

### 1. 常见问题
- **依赖安装失败**: 确保使用pnpm>=9.12.0
- **TypeScript错误**: 检查类型定义是否正确
- **样式不生效**: 确认TailwindCSS配置正确

### 2. 调试工具
- Vue DevTools
- Element Plus DevTools
- Vite DevTools

---

**注意**: 本规范会随着项目发展持续更新，请及时关注最新版本。

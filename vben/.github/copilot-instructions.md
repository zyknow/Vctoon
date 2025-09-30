# Copilot Instructions for this Repository

你是一个专业的软件开发助手，必须严格遵循以下要求：
1. 代码必须完整可运行，包含必要的导入、类型声明、注释。
2. 使用简洁、规范的命名，遵循常见的代码风格最佳实践。
3. 避免冗余代码，提取公共逻辑为函数或常量。
4. 所有输入输出必须有明确的类型定义，禁止使用 any。
5. 保证代码健壮性，必须包含错误处理。
6. 注释仅用于关键逻辑，避免无意义注释。

### 代码生成与格式化提示
> 使用本仓库开发时，你只需要专注“生成正确的代码”。多余的导入、未使用变量、格式风格（缩进/分号/引号/排序等）无需手动反复调整，但多余无用的方法需要你手动删除：直接保存文件即可，保存会触发统一的 Lint & Format （包括 ESLint + Prettier + Stylelint + 自动移除未使用的 import）。

规范补充：
- 不必手写 import 顺序优化；保存自动整理。
- 看到未使用的变量/导入报错，若暂时不用请删除；若将来会用可以先加前缀 `_` 以通过校验。
- 若仅为通过类型而存在的占位，可使用 `// TODO:` 标记，避免影响可读性。
- 不要在提交中刻意加入无意义空格/换行；持续保存即可获得最终格式。
- 代码批量生成后，一次性保存触发格式化，再做针对性微调，避免多次无效 diff。

# 聊天  
1. 使用中文（简体）回复  
2. 保持回答简洁，只聚焦核心信息


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
import { Page } from '@vben/common-ui'
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
  
  <!-- 页面容器组件使用规范 -->
  <!-- ✅ 正确：使用 content-class 属性 -->
  <Page content-class="flex flex-col gap-4">
    <div>页面内容</div>
  </Page>
  
  <!-- ❌ 错误：不要使用 class 属性 -->
  <Page class="flex flex-col gap-4">
    <div>页面内容</div>
  </Page>
</template>
```

##### Page组件使用要点

**重要**: `Page` 组件必须使用 `content-class` 属性而不是 `class` 属性来设置内容样式。

- **正确用法**: `<Page content-class="flex flex-col gap-4">`
- **错误用法**: `<Page class="flex flex-col gap-4">`

`content-class` 属性会将样式应用到页面内容容器上，确保正确的布局和样式隔离。

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

#### 4.2 UserStore 数据使用规范

##### 重要规则：禁止直接调用 API 获取 tags、artists、libraries 数据

项目中的 `tags`、`artists`、`libraries` 数据必须统一通过 `useUserStore` 获取，不得直接调用对应的 API。

##### 4.2.1 数据获取规范

```typescript
import { useUserStore } from '@vben/stores'

// ✅ 正确：使用 userStore 中的数据
const userStore = useUserStore()

// 使用计算属性获取数据
const tags = computed(() => userStore.tags || [])
const artists = computed(() => userStore.artists || [])
const libraries = computed(() => userStore.libraries || [])

// ❌ 错误：直接调用 API
// const tags = await tagApi.getAllTags()
// const artists = await artistApi.getAllArtist()
// const libraries = await libraryApi.getCurrentUserLibraryList()
```

##### 4.2.2 数据加载/刷新规范

```typescript
// ✅ 正确：使用 userStore 的方法刷新数据
const loadData = async () => {
  loading.value = true
  try {
    await userStore.reloadTags()      // 刷新标签数据
    await userStore.reloadArtists()   // 刷新艺术家数据
    await userStore.reloadLibraries() // 刷新图书馆数据
    
    // 或者一次性加载所有数据
    await userStore.reloadAllUserData()
  } catch (error) {
    console.error('加载数据失败:', error)
  } finally {
    loading.value = false
  }
}

// ❌ 错误：直接调用 API 并设置本地状态
// const loadTags = async () => {
//   const result = await tagApi.getAllTags()
//   tags.value = result
// }
```

##### 4.2.3 实时更新优势

UserStore 已集成 SignalR 实时更新机制，当数据发生变化时会自动同步：

- **OnCreated**: 新增数据时自动添加到对应数组
- **OnUpdated**: 数据更新时自动替换对应项
- **OnDeleted**: 数据删除时自动从数组中移除

```typescript
// 无需手动处理实时更新，userStore 会自动处理
// dataChangedHub 已在 userStore 中配置完成
```

##### 4.2.4 使用场景示例

```typescript
// 在标签管理页面
import { useUserStore } from '@vben/stores'

const userStore = useUserStore()
const tags = computed(() => userStore.tags || [])

// 初始化时加载数据
onMounted(async () => {
  await userStore.reloadTags()
})

// 在艺术家选择器组件
const artists = computed(() => userStore.artists || [])

// 在图书馆选择组件
const libraries = computed(() => userStore.libraries || [])
```

##### 4.2.5 数据一致性保障

- **单一数据源**: 避免多处维护相同数据导致的不一致
- **缓存机制**: `loadedKeys` 确保数据只加载一次，避免重复请求
- **实时同步**: SignalR 确保多端数据实时同步
- **类型安全**: 完整的 TypeScript 类型定义

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
4. **禁止在请求中直接使用 ElMessage**: 所有网络请求的成功/错误提示已在 Axios 拦截器内统一处理，业务代码中不要再在请求的 then/catch/finally 中直接调用 `ElMessage.*`。只有在拦截器无法覆盖的额外业务语义（例如：局部二次确认、组合多请求后的汇总提示）时，才能补充自定义消息，且需先判断不与全局提示重复。

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

## 8. Dialog 弹窗开发规范

### 8.1 设计目标
统一弹窗创建/关闭逻辑，消除各业务页面中散落的 `<el-dialog v-model>` 与重复样板代码，使弹窗：
- Promise 化：结果/取消可直接 `await`；
- 结构清晰：内容组件（纯业务） + DialogService（壳与生命周期） + 领域 Service（语义封装）；
- 可复用 / 可拓展：跨页面调用、集中配置、移动端自适应 fullscreen；
- 易测试：业务逻辑脱离 UI 容器。

### 8.2 核心概念
- `useDialogService`（基础服务）
  - 位置：`apps/web-ele/src/hooks/useDialogService.ts`
  - 能力：`open(component, options)` / `confirm(options)` / `closeAll()`。
  - 行为：自动挂载 + 提供上下文 + 关闭即销毁；未显式 `resolve` 时在关闭时返回 `undefined`。
- `useDialogContext()`（内容组件内部使用）
  - 提供：`resolve(value)`、`reject(error)`、`close()`、`id`。
  - 不再暴露 `visible`，内容组件不直接控制外壳显隐。
- 领域服务（如 `useLibraryDialogService`）
  - 二次封装常用弹窗，加入语义方法：`openCreate` / `openEdit` / `openPermission`。
  - 负责：标题 i18n、宽度/移动端 fullscreen、业务参数打包。
- 内容组件（Headless Dialog Content）
  - 不包含 `<el-dialog>`；只写表单 / 权限 / 校验逻辑；
  - 通过 `useDialogContext().resolve(data)` 返回成功结果；
  - 取消 => `close()`；不需要 emit `success` / `update:modelValue`。

### 8.3 基础用法示例
```ts
// 调用方（页面 / 组件）
import { useLibraryDialogService } from '#/hooks/useLibraryDialogService'

const dialog = useLibraryDialogService()

async function handleCreate() {
  const lib = await dialog.openCreate()
  if (lib) {
    // 成功创建，刷新数据
    await userStore.reloadLibraries()
  }
}
```

```vue
<!-- 内容组件（示例） -->
<script setup lang="ts">
import { useDialogContext } from '#/hooks/useDialogService'
import { libraryApi } from '@vben/api'
import { ElMessage } from 'element-plus'
import { $t } from '#/locales'

const { resolve, close } = useDialogContext<any>()
// ... 业务表单逻辑
async function submit() {
  const created = await libraryApi.create(form)
  ElMessage.success($t('page.library.create.actions.success'))
  resolve(created) // 返回给调用方
}
</script>
```

### 8.4 结果约定
| 使用场景 | resolve 的值 | 调用方含义 |
|----------|--------------|------------|
| 创建成功 | 新建实体对象 | 需要刷新列表或插入本地缓存 |
| 编辑成功 | 更新后的实体 | 需要局部更新或全量刷新 |
| 权限保存 | `true` | 可选刷新权限数据 |
| 用户取消 / 关闭 | `undefined` | 不做任何数据操作 |

### 8.5 规范与约束（Do / Don't）
Do：
1. 新增所有业务弹窗统一基于 `useDialogService`。
2. 始终使用 i18n：标题通过领域服务传入，按钮文案在内容组件内部 `$t()`。
3. 表单验证通过后再调用后端 API → 成功后 `resolve(data)`；失败不关闭弹窗。
4. 需要刷新全局数据优先调用相关 store（如 `userStore.reloadLibraries()`），不要直接重新调列表 API（遵守单一数据源原则）。
5. 移动端自适配：通过领域服务内 `fullscreen: isMobile` 控制。

Don't：
1. 不在页面再写 `<el-dialog v-model="xxx">`。
2. 不在内容组件内引用或操作外部可见状态（如 `props.modelValue` / `emit('update:modelValue')`）。
3. 不再新增使用旧的 `openDialog` / `openConfirm`（它们仅保留兼容，后续会移除）。
4. 不在业务组件直接 `import { ElDialog } from 'element-plus'` 实现弹窗外壳。
5. 不在对话中使用全局事件总线广播成功事件（Promise 返回即可）。

### 8.6 目录与命名建议
- 领域服务：`apps/web-ele/src/hooks/use<Entity>DialogService.ts`。
- 内容组件：放在对应业务视图目录下：`views/<entity>/<action>-<entity>-dialog.vue`（保留历史命名但去除外壳）。
- 复杂弹窗（>300 行）可按逻辑拆分子组件（保持纯展示/交互），仍由内容组件聚合并 `resolve`。

### 8.7 迁移步骤参考
1. 复制旧弹窗内部 `<el-dialog>` 的表单内容为独立根 `<div>`；删除 `<el-dialog>`。
2. 移除 `modelValue / emit('update:modelValue')`，加上 `const { resolve, close } = useDialogContext<ReturnType>()`。
3. 将 `@success` / `emit('success', data)` 改为 `resolve(data)`。
4. 原调用处删除本地 `ref(false)`，改为 `await service.openXxx()`。
5. 根据需要刷新 store 或保持实时推送。

### 8.8 错误处理模式
- 接口异常：`try/catch` + `ElMessage.error()`，保持弹窗不关闭。
- 校验失败：Element Plus `formRef.validate()`；不调用接口。
- 用户取消：调用 `close()` 或点击关闭按钮 → 返回 `undefined`。
- 需要阻止误关闭（表单已修改但未提交）：可在领域 service 传入 `dialog: { beforeClose }` 自定义拦截。

### 8.9 统一确认弹窗
简单二次确认使用 `useDialogService().confirm({ message, danger })`，不要再使用 `ElMessageBox.confirm`（可逐步替换）。

### 8.10 升级与清理
- 新增弹窗一律使用新模式；旧模式文件在迁移完成后集中删除遗留 API。
- 后续计划：移除 `openDialog/openConfirm` 兼容层；加入全局“阻止重复打开同类弹窗”的去重策略（可通过 key/缓存实现）。

> 如需新增通用交互类型（Drawer / 全屏表单），保持同样 headless 思路，在 `useDialogService` 增量扩展或并行创建 `useDrawerService`。

## 9. 移动端判断统一规范

所有组件 / 页面在需要根据是否为移动端调整布局或交互时，必须统一使用组合式函数 `useIsMobile`，避免自行编写 `window.innerWidth` / `matchMedia` 等散落逻辑。

示例（推荐解构写法，防止误把 Ref 对象当作 truthy 导致条件恒成立）：

```ts
import { useIsMobile } from '@vben/hooks'

// 推荐：直接解构，模板语法会自动解包 Ref
const { isMobile } = useIsMobile()

// 模板中： :class="isMobile ? 'flex-col' : 'flex-row'"

// 旧写法（可用但需注意）：
// const mobile = useIsMobile()
// 若直接写  mobile.isMobile ? ...  其实依然会被模板自动解包；
// 但在 JS 逻辑里 if (mobile.isMobile) {...} 会因为 mobile.isMobile 是一个 Ref 对象而恒为真。
```

使用约定：
1. 命名：若需要整体对象可命名为 `mobile`；仅使用布尔值推荐直接解构 `const { isMobile }`。
2. 不手动修改 `isMobile`；它是响应式只读状态。
3. 模板中条件：`isMobile ? '...' : '...'`；JS 逻辑中务必使用 `isMobile.value`（如果未解构）或直接解构后的 `isMobile`。
4. 若需尺寸联动逻辑，集中到 `useIsMobile` 内处理，业务层不再重复添加 window 事件。
5. 禁止再实现自定义窗口宽度判断（如再写一次 `window.innerWidth < 768`）。
6. 避免在脚本里写 `if (mobile.isMobile) {...}`（Ref 对象恒 truthy）；应写 `if (mobile.isMobile.value)` 或解构后 `if (isMobile)`。

典型应用场景：
- 弹窗在移动端使用 `:fullscreen="mobile.isMobile"`。
- 详情/编辑对话框：桌面端左右两栏（封面 + 表单），移动端上下布局。
- 表格操作列：移动端合并为折叠面板或下拉菜单。

错误示例（禁止）：
```ts
const isMobile = window.innerWidth < 768 // ❌ 直接写死
```

> 如需调整移动端判定阈值，请集中在 `useIsMobile` 内修改，而不是在业务代码中分散替换。


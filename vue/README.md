# Vctoon

基于 Vite + Vue 3 + TypeScript 构建的企业级控制台模板，整合 `@nuxt/ui` 组件体系、Pinia 状态管理、`vue-i18n` 国际化与 `@unovis/vue` 数据可视化能力，开箱即用地提供主题定制、深浅色切换与多语言支持。

## 快速开始

1. 安装依赖：`pnpm install`
2. 启动开发服务：`pnpm dev`
3. 生产构建：`pnpm build`
4. 本地预览产物：`pnpm preview`
5. 代码质量：`pnpm lint`
6. 类型校验：`pnpm typecheck`

> 项目默认为 pnpm 工作区，请使用 PNPM 10+ 与 Node.js 18+。

## 目录结构

- `src/main.ts`：应用入口，注册 UI、路由、Pinia、国际化。
- `src/layouts`：页面布局，默认主布局 `MainLayout.vue`。
- `src/pages`：路由页面，`home/index.vue` 为示例首页。
- `src/router`：路由定义与模块拆分.
- `src/stores`：Pinia store，包括全局导航与用户偏好。
- `src/hooks`：组合式函数，`useIsMobile` 用于统一移动端断点判断。
- `src/locales`：中英文文案，新增页面需同步维护 `en.json` 与 `zh-CN.json`。
- `src/assets/css/main.css`：主题变量与 Tailwind 配置入口。

## 开发约定

- 使用 `<script setup lang="ts">` 与组合式 API，禁止 `any`/隐式 `any`。
- 业务 UI 优先采用 `@nuxt/ui` 组件，样式遵循项目提供的 Tailwind 主题色规范。
- 复用逻辑抽离至 `src/utils` 或 `src/hooks`；全局状态通过 Pinia `defineStore`。
- 网络/异步交互需包含加载态与错误兜底，路由变更需保障导航与标题同步。
- 保存后通过 `pnpm exec eslint --fix --ext .ts,.vue <paths>` 进行格式化；提交前运行 `pnpm lint` + `pnpm typecheck`。

更多编码规范详见 `.cursor/rules/main-rules.mdc`。

## 主题与响应式

- 样式颜色遵循主题优先、语义优先、具体颜色兜底的顺序，详细规范见 `main-rules.mdc` 中的「样式开发规范」。
- 响应式布局统一通过 `@hooks/useIsMobile`：

```ts
import { useIsMobile } from '@src/hooks/useIsMobile'

const { isMobile } = useIsMobile()
```

支持通过 `useIsMobile({ breakpoint: 'lg' })` 或 `useIsMobile({ maxWidth: 1024 })` 自定义判断阈值。

## 常见问题

- **如何新增 Hook？** 直接放置于 `src/hooks`，并通过 `@hooks/<hook>` 引入；无需额外的集中导出文件，保持按需加载即可。
- **如何新增语言？** 在 `src/stores/preferences.ts` 的 `SUPPORTED_LOCALES` 与 `src/locales` 中补充对应文件，同时更新 UI 文案。
- **如何定制主题色？** 修改 `src/assets/css/main.css` 或使用 `UserMenu` 下的主题配置入口。

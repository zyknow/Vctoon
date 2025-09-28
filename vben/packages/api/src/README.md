# API 开发规范与生成流程

本文档记录在 `src/vctoon` 目录下为 ABP 后端生成接口模块与类型定义的流程与约定，以便后续按统一规范扩展。

## 目标

- 以 ABP AppService 为基准，快速生成前端 API 模块（CRUD + 自定义接口）。
- 类型完备：与后端 DTO 对齐，复用全局基础类型。
- 注释统一使用 JSDoc（/** ... */），便于 IDE 智能提示与阅读。

## 目录结构

- 每个服务一个子目录：`src/vctoon/<service>/`
  - `typing.ts`：该服务的前端类型定义（与后端 DTO 对齐）。
  - `index.ts`：该服务的 API 封装，复用通用 CRUD 工具；自定义接口额外声明。

示例：

```text
src/vctoon/
  artist/
    index.ts
    typing.ts
  comic/
    index.ts
    typing.ts
  base/
    base-curd-api-definition.ts
  request.ts
```

## 基础依赖

- 请求客户端：`src/request.ts`（封装了拦截器等逻辑）
- 通用 CRUD 工具：`src/vctoon/base/base-curd-api-definition.ts`
- 全局类型（ambient declarations）：`@vben/types`
  - HTTP：`BasePageRequest`、`FilterPageRequest`、`ItemsResult<T>`、`PageResult<T>`
  - ABP DTO：`FullAuditedEntityDto` 等
  - String 扩展：`String.prototype.format`

> 说明：由于 `String.prototype.format` 为全局扩展，TS 在某些模块下可能需要 `// @ts-expect-error` 进行抑制，项目已有同类用法可参考。

## 工作区与路径定位

- 后端源码位置以仓库根目录为基准：`<repo-root>/aspnetcore/src`。
- 如果当前 VS Code 只打开了 `vben` 子仓库：
  - 推荐使用"多根工作区"（File > Add Folder to Workspace）把 `<repo-root>/aspnetcore` 一并加入，以便跨端检索 DTO 与服务；或
  - 直接在文件系统中浏览 `<repo-root>/aspnetcore/src`；或
  - 无法访问后端源码时，可使用后端 Swagger `https://localhost:44364/swagger/v1/swagger.json` 来确认路由与 DTO 结构。

## 生成流程

1. 确认后端 AppService 与 DTO
   - 打开后端仓库（仓库根目录的 `<repo-root>/aspnetcore/src`；若只打开了 `vben` 工作区，可按上节说明将后端加入工作区或通过 Swagger 获取信息）查找对应服务与 DTO，例如：
     - `Vctoon.Application.Contracts/.../Dtos/*Dto.cs`
     - `...AppService.cs`/`I...AppService.cs`
   - 记录：
     - 基础实体 DTO 结构（列表输出、详情、创建/更新输入）
     - 列表查询输入（分页/排序/筛选字段）
     - 额外自定义接口（路径、方法、参数与返回类型）

2. 约定前端 `baseUrl`
   - 基于 ABP 默认：`/api/app/{entity}`（一般使用单数，例如 `artist`、`comic`）。
   - 若后端实际为复数或自定义路由，请以后端为准。

3. 编写 `typing.ts`
   - 复用全局基础类型，保持字段名与后端一致：
     - 列表项（GetList 输出）类型：如 `XxxGetListOutput`
     - 详情类型：如 `Xxx`
     - 创建/更新输入：如 `XxxCreateUpdate`
     - 列表查询输入：如 `XxxGetListInput`（扩展 `BasePageRequest`）
   - 使用 JSDoc 注释说明用途。

4. 编写 `index.ts`
   - 导入 `requestClient` 与 `createBaseCurdApi`/`createBaseCurdUrl`：
     - `const baseUrl = '/api/app/xxx'`
     - `const url = { ...createBaseCurdUrl(baseUrl), ...自定义路径 }`
   - 导出 `xxxApi`：
     - 展开 `createBaseCurdApi<PK, Detail, ListItem, CreateInput, UpdateInput, ListQuery, PageResult<ListItem>>(baseUrl)`
     - 为自定义接口单独编写方法，并补充 JSDoc 注释（参数、返回值）。

5. 处理 String.format 类型告警
   - 模板路径常使用 `{id}` 占位，项目扩展了 `String.prototype.format`：
     - 用法：`url.detail.format({ id })`
     - 若出现类型告警，参考现有代码加入：`// @ts-expect-error String.prototype.format is globally extended`

6. 本地校验
   - 运行类型检查或构建，确保无 TS 错误。

## 模板片段

- `typing.ts`

```ts
/** 模块类型定义 */
export type XxxGetListOutput = FullAuditedEntityDto & {
  // ...fields
}

/** 详情 */
export type Xxx = XxxGetListOutput & {
  // ...extra fields
}

/** 新建/更新输入 */
export type XxxCreateUpdate = {
  // ...fields
}

/** 分页查询输入 */
export type XxxGetListInput = BasePageRequest & {
  // sorting?: string
}
```

- `index.ts`

```ts
import type { Xxx, XxxCreateUpdate, XxxGetListInput, XxxGetListOutput } from './typing'
import { requestClient } from '../../request'
import { createBaseCurdApi, createBaseCurdUrl } from '../base/base-curd-api-definition'

/** 根路径 */
const baseUrl = '/api/app/xxx'

const url = {
  ...createBaseCurdUrl(baseUrl),
  /** 自定义接口示例 */
  custom: `${baseUrl}/{id}/custom`,
}

/** API 封装 */
export const xxxApi = {
  url,
  ...createBaseCurdApi<string, Xxx, XxxGetListOutput, XxxCreateUpdate, XxxCreateUpdate, XxxGetListInput, PageResult<XxxGetListOutput>>(baseUrl),

  /** 自定义方法示例 */
  custom(id: string) {
    // @ts-expect-error String.prototype.format is globally extended
    return requestClient.get(url.custom.format({ id }))
  },
}
```

## 已实现示例

- `artist`：纯 CRUD 模块。
- `comic`：CRUD + 自定义媒体相关接口（封面、单图获取、按漫画列图、删除图片）。
- `audit-logs`：审计日志查询模块（分页列表、详情查看、实体变更记录）。
- `security-logs`：安全日志查询模块（分页列表、当前用户日志）。

## 注意事项

- 路由单复数以后端为准，如出现 404，请对照后端服务路由修正 `baseUrl` 与自定义路径。
- 字段名与类型需严格对齐后端 DTO，必要时参考 swagger 或后端源码。
- JSDoc 注释有助于团队协作与 IDE 提示，请保持完整。

# API 模块开发速览

聚焦如何在 `packages/api/src/vctoon` 下快速、统一、低心智成本地新增或维护前端 API 模块。

---

## 核心原则

1. 以后端 AppService + DTO 为唯一真源（Swagger / 源码二选一）。
2. CRUD 复用通用工厂；自定义接口最小封装 + 清晰注释。
3. 流式 / 二进制资源：前端只暴露 URL（不再额外封装获取方法，除非需要动态参数组合）。
4. 类型严格对齐；不要“想当然”新增本地字段。
5. 只在必要时使用 `// @ts-expect-error`（String.prototype.format 扩展）。

---

## 必备文件结构（约定）

```text
vctoon/
  <service>/
    typing.ts   # 类型定义（与后端 DTO 同步）
    index.ts    # API 封装（CRUD + 自定义）
  base/         # CRUD / Medium 公共构造工具
  request.ts    # axios 实例（拦截器、全局提示）
```

---

## 获取后端契约（Swagger 推荐）

本地运行后端后访问：

```text
https://localhost:44364/swagger/v1/swagger.json
```

筛选关键字（如 `"/api/app/comic"`、`Dto`）确认：

- 基础路由（`/api/app/{entity}`）
- 分页查询参数（大小写要与后端一致：如 `Title`, `Tags`, `SkipCount`）
- 自定义子路径（如 `/comic-image`, `/by-comic-id/{comicId}`）
- 对应 DTO 结构（字段、可空性、枚举）

无法使用 Swagger 时改为直接查看后端： `Vctoon.Application.Contracts/*/Dtos/*.cs` 与对应 `AppService`。

---

## CRUD 快速生成步骤

1. 确定 `baseUrl`: `/api/app/<entity>`（与后端一致，单复数不要自作修改）。
1. 写 `typing.ts`：

- `XxxGetListOutput`（列表或轻量 DTO）
- `Xxx`（详情，可 = GetListOutput & 关系）
- `XxxCreateUpdate`（创建 / 更新输入）
- `XxxGetListInput`（分页查询，继承 `BasePageRequest`）

1. 写 `index.ts`：

- `const baseUrl = '/api/app/xxx'`
- `const url = { ...createBaseCurdUrl(baseUrl), /* 自定义扩展 */ }`
- `export const xxxApi = { url, ...createBaseCurdApi<...>(baseUrl) , /* 自定义方法 */}`

1. 需要模板路径：`url.detail.format({ id })`。
1. 运行类型检查（或依赖主项目构建脚本）。

---

## Medium 系列专用（Comic / Video 等）

使用 `createMediumBaseCurdApi` / `createMediumBaseCurdUrl`，自动提供：

- `create / update / delete / getById / getPage`
- `updateCover(id, file)`（FormData）
- `updateArtists(id, string[])`
- `updateTags(id, string[])`

调用后会为返回实体补齐 `mediumType`（前端依赖逻辑统一）。

---

## Comic 模块附加说明

基础根：`/api/app/comic`

当前自定义 URL（维护于 `comic/index.ts` 的 `url` 对象）：

| 说明 | URL 模板 | 备注 |
| --- | --- | --- |
| 获取单张图片流 | `/api/app/comic/comic-image?comicImageId={comicImageId}&maxWidth={maxWidth}` | 直接使用 URL，前端若仅展示可不再写方法（已保留 url） |
| 删除图片 | `/api/app/comic/comic-image/{comicImageId}` | query: `deleteFile`（可选） |
| 根据漫画 Id 获取图片元数据 | `/api/app/comic/by-comic-id/{comicId}` | 返回 `ComicImage[]` |

封装方法（存在于 `comicApi`）:

- `deleteComicImage(comicImageId, deleteFile?)`
- `getImagesByComicId(comicId)`

为什么 `getComicImage` 没再提供函数：可以直接拼接 URL 放入 `<img :src="...">`、`fetch`、`URL.createObjectURL` 等，无额外逻辑。

---

## 流式 / 二进制资源规范

场景：图片、封面、文件流。

策略：

1. 只在 `url` 暴露模板字符串：`xxx: baseUrl + '...?id={id}&...'`。
1. 业务层使用时通过 `url.xxx.format({ id, ... })` 得到最终地址。
1. 只有当需要：自动附带 headers、鉴权签名、动态参数裁剪、错误兜底，才新增封装方法。

好处：减少 axios responseType=blob 包装次数，避免滥用内存（直接给原生 `<img>` 由浏览器缓存）。

---

## String.prototype.format 使用

模板：`'/api/app/comic/{id}'.format({ id })`

未被 TS 识别时再使用：

```ts
// @ts-expect-error String.prototype.format is globally extended
url.detail.format({ id })
```

若编辑器已正确声明（通常在本项目里会生效），不要保留多余的 expect-error。

---

## 代码示例（精简版）

```ts
// typing.ts
export type ArtistGetListOutput = FullAuditedEntityDto & { name?: string }
export type Artist = ArtistGetListOutput
export type ArtistCreateUpdate = { name: string }
export type ArtistGetListInput = BasePageRequest & { filter?: string }

// index.ts
import type {
  Artist,
  ArtistCreateUpdate,
  ArtistGetListInput,
  ArtistGetListOutput,
} from './typing'
import { createBaseCurdApi, createBaseCurdUrl, requestClient } from '../..'

const baseUrl = '/api/app/artist'
export const artistApi = {
  url: { ...createBaseCurdUrl(baseUrl) },
  ...createBaseCurdApi<
    string,
    Artist,
    ArtistGetListOutput,
    ArtistCreateUpdate,
    ArtistCreateUpdate,
    ArtistGetListInput,
    PageResult<ArtistGetListOutput>
  >(baseUrl),
}
```

---

## 校验清单（提交前 Quick Check）

| 项                  | 通过条件                                  |
| ------------------- | ----------------------------------------- |
| baseUrl 正确        | 与 swagger 路径完全一致（大小写、单复数） |
| 类型同步            | 所有可空/枚举/数组字段与后端匹配          |
| 自定义 url          | 仅保留真实后端存在的路径                  |
| format 使用         | 仅在需要占位替换的路径上使用              |
| 无冗余 expect-error | 未触发类型错误就移除                      |
| 流式资源            | 仅暴露 URL，无多余包装（除非确需）        |

---

## 已实现模块速览

| 模块            | 特点                                        |
| --------------- | ------------------------------------------- |
| artist          | 纯 CRUD                                     |
| comic           | Medium 派生 + 图片相关自定义接口            |
| video           | Medium 派生（封面/作者/标签操作）           |
| audit-logs      | 分页 + 详情 + 实体变更记录                  |
| security-logs   | 分页 + 当前用户日志                         |
| medium-resource | 封面流、批量阅读进度、批量作者/标签关联维护 |

---

## 常见问题

| 问题         | 解决                                                  |
| ------------ | ----------------------------------------------------- |
| 404          | 检查 baseUrl / 子路径是否与 swagger 一致              |
| 字段缺失     | Swagger 更新后未同步 typing.ts，及时补齐              |
| format 报错  | 确认全局 d.ts 是否被包含，必要时加 `@ts-expect-error` |
| 图片加载失败 | 直接访问构造后的 URL 手测；留意权限/登录态            |

---

保持 README 精准、短、可执行；新增模块按此流程即可无脑复制改造。

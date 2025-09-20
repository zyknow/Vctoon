# 📺 Vctoon - 漫画视频管理系统

> 一个现代化的漫画视频管理软件

[English](./README.md) | **中文**

![License](https://img.shields.io/github/license/zyknow/Vctoon)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Vue](https://img.shields.io/badge/Vue-3.x-green)
![ABP](https://img.shields.io/badge/ABP-9.x-red)
![Vben](https://img.shields.io/badge/Vben-5.x-blue)

## 📖 项目简介

Vctoon 是一个基于现代化技术栈开发的漫画视频管理系统，提供了完整的漫画、视频内容管理解决方案。项目采用前后端分离架构，后端基于 ABP Framework 9.x 构建，前端使用 Vue Vben Admin 5.x 框架开发。

### ✨ 主要特性

- 🎨 **现代化 UI**: 基于 Vue 3 + TypeScript + Vite 构建的响应式前端界面
- 🏗️ **企业级架构**: 采用 ABP Framework 领域驱动设计，提供完整的企业级应用解决方案
- 📱 **多端适配**: 支持桌面端和移动端访问
- 🔐 **权限管理**: 完整的 RBAC 权限控制系统
- 🌍 **国际化**: 内置多语言支持
- 📊 **数据管理**: 完善的漫画和视频内容管理功能
- 🚀 **高性能**: 优化的数据库查询和缓存策略

## 🛠️ 技术栈

### 后端技术

- **框架**: ABP Framework 9.x
- **语言**: C# / .NET 9.0
- **数据库**: Entity Framework Core
- **身份认证**: OpenIddict
- **API**: RESTful API / Swagger

### 前端技术

- **框架**: Vue 3 + TypeScript
- **构建工具**: Vite
- **UI 框架**: Vue Vben Admin 5.x
- **状态管理**: Pinia
- **路由**: Vue Router
- **HTTP**: Axios

## 📁 项目结构

```text
Vctoon/
├── aspnetcore/                    # 后端项目
│   ├── src/
│   │   ├── Vctoon.Domain/         # 领域层
│   │   ├── Vctoon.Domain.Shared/  # 领域共享层
│   │   ├── Vctoon.Application/    # 应用层
│   │   ├── Vctoon.Application.Contracts/ # 应用契约层
│   │   ├── Vctoon.EntityFrameworkCore/   # 数据访问层
│   │   ├── Vctoon.HttpApi/        # HTTP API层
│   │   ├── Vctoon.HttpApi.Client/ # HTTP API客户端
│   │   ├── Vctoon.Web/            # Web表示层
│   │   └── Vctoon.DbMigrator/     # 数据库迁移工具
│   └── test/                      # 测试项目
├── vben/                          # 前端项目
│   ├── apps/web-ele/              # 主应用
│   ├── packages/                  # 公共包
│   └── internal/                  # 内部工具
└── README.md
```

## 🚀 快速开始

### 环境要求

#### 后端环境

- [.NET 9.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
- [SQL Server](https://www.microsoft.com/sql-server) / MySQL / PostgreSQL (任选其一)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) 或 [JetBrains Rider](https://www.jetbrains.com/rider/) (推荐)

#### 前端环境

- [Node.js 20+](https://nodejs.org/)
- [pnpm 9.12.0+](https://pnpm.io/)

### 安装步骤

#### 1. 克隆项目

```bash
git clone https://github.com/zyknow/Vctoon.git
cd Vctoon
```

#### 2. 后端配置

```bash
cd aspnetcore
```

##### 配置数据库连接字符串

编辑 `src/Vctoon.Web/appsettings.json` 和 `src/Vctoon.DbMigrator/appsettings.json` 文件中的数据库连接字符串：

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=Vctoon;Trusted_Connection=True;TrustServerCertificate=true"
  }
}
```

##### 安装客户端依赖包

```bash
abp install-libs
```

##### 运行数据库迁移

```bash
cd src/Vctoon.DbMigrator
dotnet run
```

##### 生成开发证书

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p d12daf0f-8bed-486b-b70d-a7e1e2ce70db
```

##### 启动后端服务

```bash
cd ../Vctoon.Web
dotnet run
```

#### 3. 前端配置

```bash
cd vben
```

##### 安装依赖

```bash
pnpm install
```

##### 启动开发服务器

```bash
pnpm dev
```

## 📋 功能模块

- 📚 **内容管理**: 漫画、视频内容的上传、编辑、分类管理
- 👥 **用户管理**: 用户注册、登录、权限分配
- 🏷️ **分类管理**: 内容分类、标签管理
- 📊 **统计分析**: 内容访问统计、用户行为分析
- ⚙️ **系统设置**: 系统配置、参数管理
- 🔔 **通知中心**: 消息推送、通知管理

## 🌐 API 文档

启动后端服务后，可通过以下地址访问 API 文档：

- Swagger UI: `https://localhost:44346/swagger`
- API 端点: `https://localhost:44346/api`

## 📱 前端访问

前端开发服务器默认运行在：`http://localhost:5173`

默认管理员账户：

- 用户名: `admin`
- 密码: `1q2w3E*`

## 🏗️ 构建部署

### 后端构建

```bash
cd aspnetcore
dotnet publish -c Release -o ./publish
```

### 前端构建

```bash
cd vben
pnpm build
```

构建完成后，将生成的静态文件部署到 Web 服务器即可。

## 🤝 贡献指南

欢迎提交 Pull Request 或创建 Issue！

1. Fork 本仓库
2. 创建你的特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交你的修改 (`git commit -m 'Add some amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 打开一个 Pull Request

## 📄 开源协议

本项目基于 [MIT](LICENSE) 协议开源。

## 👨‍💻 作者

- **zyknow** - *项目创建者* - [zyknow](https://github.com/zyknow)

## 🙏 致谢

- [ABP Framework](https://abp.io/) - 提供了强大的应用程序框架
- [Vue Vben Admin](https://github.com/vbenjs/vue-vben-admin) - 优秀的 Vue 3 管理后台模板
- [Vue.js](https://vuejs.org/) - 渐进式 JavaScript 框架
- [.NET](https://dotnet.microsoft.com/) - 跨平台开发框架

## 📞 联系方式

如果您有任何问题或建议，请通过以下方式联系：

- 📧 Email: [your-email@example.com]
- 🐛 Issues: [GitHub Issues](https://github.com/zyknow/Vctoon/issues)

---

Made with ❤️ by zyknow

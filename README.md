# 📺 Vctoon - Comic & Video Management System

> A modern comic and video management software

**English** | [中文](./README.zh-CN.md)

![License](https://img.shields.io/github/license/zyknow/Vctoon)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Vue](https://img.shields.io/badge/Vue-3.x-green)
![ABP](https://img.shields.io/badge/ABP-9.x-red)
![Vben](https://img.shields.io/badge/Vben-5.x-blue)

## 📖 Introduction

Vctoon is a modern comic and video management system built with cutting-edge technology stack. It provides a complete solution for managing comic and video content. The project adopts a front-end and back-end separation architecture, with the backend built on ABP Framework 9.x and the frontend developed using Vue Vben Admin 5.x framework.

### ✨ Key Features

- 🎨 **Modern UI**: Responsive frontend interface built with Vue 3 + TypeScript + Vite
- 🏗️ **Enterprise Architecture**: Domain-driven design using ABP Framework for complete enterprise-level solutions
- 📱 **Multi-platform**: Support for desktop and mobile access
- 🔐 **Permission Management**: Complete RBAC permission control system
- 🌍 **Internationalization**: Built-in multi-language support
- 📊 **Data Management**: Comprehensive comic and video content management features
- 🚀 **High Performance**: Optimized database queries and caching strategies

## 🛠️ Technology Stack

### Backend Technologies

- **Framework**: ABP Framework 9.x
- **Language**: C# / .NET 9.0
- **Database**: Entity Framework Core
- **Authentication**: OpenIddict
- **API**: RESTful API / Swagger

### Frontend Technologies

- **Framework**: Vue 3 + TypeScript
- **Build Tool**: Vite
- **UI Framework**: Vue Vben Admin 5.x
- **State Management**: Pinia
- **Routing**: Vue Router
- **HTTP**: Axios

## 📁 Project Structure

```text
Vctoon/
├── aspnetcore/                    # Backend project
│   ├── src/
│   │   ├── Vctoon.Domain/         # Domain layer
│   │   ├── Vctoon.Domain.Shared/  # Domain shared layer
│   │   ├── Vctoon.Application/    # Application layer
│   │   ├── Vctoon.Application.Contracts/ # Application contracts
│   │   ├── Vctoon.EntityFrameworkCore/   # Data access layer
│   │   ├── Vctoon.HttpApi/        # HTTP API layer
│   │   ├── Vctoon.HttpApi.Client/ # HTTP API client
│   │   ├── Vctoon.Web/            # Web presentation layer
│   │   └── Vctoon.DbMigrator/     # Database migration tool
│   └── test/                      # Test projects
├── vben/                          # Frontend project
│   ├── apps/web-ele/              # Main application
│   ├── packages/                  # Shared packages
│   └── internal/                  # Internal tools
└── README.md
```

## 🚀 Getting Started

### Prerequisites

#### Backend Requirements

- [.NET 9.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
- [SQL Server](https://www.microsoft.com/sql-server) / MySQL / PostgreSQL (choose one)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [JetBrains Rider](https://www.jetbrains.com/rider/) (recommended)

#### Frontend Requirements

- [Node.js 20+](https://nodejs.org/)
- [pnpm 9.12.0+](https://pnpm.io/)

### Installation

#### 1. Clone the Repository

```bash
git clone https://github.com/zyknow/Vctoon.git
cd Vctoon
```

#### 2. Backend Setup

```bash
cd aspnetcore
```

##### Configure Database Connection String

Edit the database connection string in `src/Vctoon.Web/appsettings.json` and `src/Vctoon.DbMigrator/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=Vctoon;Trusted_Connection=True;TrustServerCertificate=true"
  }
}
```

##### Install Client-side Packages

```bash
abp install-libs
```

##### Run Database Migration

```bash
cd src/Vctoon.DbMigrator
dotnet run
```

##### Generate Development Certificate

```bash
dotnet dev-certs https -v -ep openiddict.pfx -p d12daf0f-8bed-486b-b70d-a7e1e2ce70db
```

##### Start Backend Service

```bash
cd ../Vctoon.Web
dotnet run
```

#### 3. Frontend Setup

```bash
cd vben
```

##### Install Dependencies

```bash
pnpm install
```

##### Start Development Server

```bash
pnpm dev
```

## 📋 Features

- 📚 **Content Management**: Upload, edit, and categorize comics and videos
- 👥 **User Management**: User registration, login, and permission assignment
- 🏷️ **Category Management**: Content categorization and tag management
- 📊 **Analytics**: Content access statistics and user behavior analysis
- ⚙️ **System Settings**: System configuration and parameter management
- 🔔 **Notification Center**: Message push and notification management

## 🌐 API Documentation

After starting the backend service, you can access the API documentation at:

- Swagger UI: `https://localhost:44346/swagger`
- API Endpoints: `https://localhost:44346/api`

## 📱 Frontend Access

The frontend development server runs at: `http://localhost:5173`

Default administrator account:

- Username: `admin`
- Password: `1q2w3E*`

## 🏗️ Build & Deployment

### Backend Build

```bash
cd aspnetcore
dotnet publish -c Release -o ./publish
```

### Frontend Build

```bash
cd vben
pnpm build
```

After building, deploy the generated static files to your web server.

## 🤝 Contributing

Pull requests and issues are welcome!

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the [MIT](LICENSE) License.

## 👨‍💻 Author

- **zyknow** - *Project Creator* - [zyknow](https://github.com/zyknow)

## 🙏 Acknowledgments

- [ABP Framework](https://abp.io/) - Powerful application framework
- [Vue Vben Admin](https://github.com/vbenjs/vue-vben-admin) - Excellent Vue 3 admin template
- [Vue.js](https://vuejs.org/) - Progressive JavaScript framework
- [.NET](https://dotnet.microsoft.com/) - Cross-platform development framework

## 📞 Contact

If you have any questions or suggestions, please contact us through:

- 📧 Email: [your-email@example.com]
- 🐛 Issues: [GitHub Issues](https://github.com/zyknow/Vctoon/issues)

---

Made with ❤️ by zyknow

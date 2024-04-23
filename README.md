# Vctoon Comic Management Software

## Project Overview

Vctoon is a comprehensive comic management software developed using the ABP framework version 8.1.x. It leverages the
capabilities of Fluent Blazor from Microsoft (https://github.com/microsoft/fluentui-blazor) and UnoCSS for a sleek and
responsive user interface. The backend is powered by SharpCompress and SixLabors.ImageSharp, facilitating efficient
comic file management and image processing.

### Key Features

- **Comic Management:** Organize, browse, and manage your comic collection with ease.
- **Future Extensions:** Plans to incorporate video management capabilities, expanding the utility of the software.

### System Requirements

- [.NET 8.0+ SDK](https://dotnet.microsoft.com/download/dotnet)
- [Node v18 or v20](https://nodejs.org/en)

### Initial Configuration

The software is designed to work with minimal initial setup. However, consider modifying the following configurations to
tailor the application to your needs:

#### Security Configuration

In production environments, a robust signing certificate setup is crucial. By default, the `openiddict.pfx` certificate
is managed by ABP CLI. If necessary, regenerate this certificate with:

\`\`\`bash
dotnet dev-certs https -v -ep openiddict.pfx -p your-password-here
\`\`\`

Refer to the official OpenIddict documentation for detailed guidance on certificate
management: https://documentation.openiddict.com/configuration/encryption-and-signing-credentials.html

#### Client-Side Libraries Installation

Install necessary client libraries if not already handled by the ABP CLI:

\`\`\`bash
abp install-libs
\`\`\`

#### Database Setup

To set up and initialize the database, run:

\`\`\`bash
Vctoon.DbMigrator
\`\`\`

### Project Structure

This layered monolithic application is structured as follows:

- **Vctoon.DbMigrator:** A console application responsible for database migrations and initial data seeding, useful in
  both development and production environments.

### Deployment

Deploying the Vctoon application follows standard procedures for .NET and ASP.NET Core applications. Consult the ABP
deployment documentation for best practices: https://docs.abp.io/en/abp/latest/Deployment/Index

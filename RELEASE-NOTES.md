# Blazor Admin Template release notes

## 2024-12-02 - v1.2.0

### Added/Refactoring

- Feature: Login with external account - Microsoft.
- Refactor template structure - added new project `Libs`.

### Fixed/Improvement

- Fix(CodeQL): Nested if statements.
- Fix(others): Minor bug fixes and improvements.

## 2024-11-21 - v1.1.1

### Fixed/Improvement

- Fix(deps): Prevent dependencies from jumping to .NET 9.
- Improvement: Notify stage changed when AppInfo is fetched.
- Fix(dev/demo): Login/Profile info are automatically filled on Development env.

## 2024-10-31 - v1.1.0

### Added/Refactoring

- Refactor: control seeding data via appsettings.json
- Feature: add landing page

### Fixed/Improvement

- Improvement: integrate user avatars from Gravatar.com
- Fix(CodeQL): CodeQL warnings and recommendations

## 2024-10-29 - v1.0.0

### Added/Refactoring

- Feature: .NET solution structure: `Shared`, `Shared.EF`, `Api` and Blazor Server/Client/Shared projects.
- Feature: Admin template built on [CoreUI Free Bootstrap Admin Template](https://coreui.io/product/free-bootstrap-admin-template/).
- Feature: Authentication and authorization using JWT and [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity).
- Feature: Sample database access implementation with [Entity Framework](https://learn.microsoft.com/en-us/ef/core/).
- Feature: Sample GitHub Actions workflows and Dockerfiles.

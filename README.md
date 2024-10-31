# Blazor Admin Template

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![Actions Status](https://github.com/DDTH/blazor-admin-template/workflows/ci/badge.svg)](https://github.com/DDTH/blazor-admin-template/actions)
[![Release](https://img.shields.io/github/release/DDTH/blazor-admin-template.svg?style=flat-square)](RELEASE-NOTES.md)

Template to quickly create admin control panel projects with .NET Blazor.

## Features

- Template to rapidly generate a .NET solution, including:
  - A RESTful API project that can function independently to build an API server.
  - Blazor Server and Blazor WebAssembly (WASM) projects configured with [interactive auto-rendering mode](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes).
  - Shared projects for common code and resources.
- UI components and layouts built on [CoreUI Free Bootstrap Admin Template](https://coreui.io/product/free-bootstrap-admin-template/).
- Authentication and authorization using JWT and [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity).
- Sample database access implementation using [Entity Framework](https://learn.microsoft.com/en-us/ef/core/).
- GitHub Actions workflows:
  - `dependabot.yaml`, `automerge-dependabot.yaml`: Automatically update dependencies and merge PRs from Dependabot.
  - `ci.yaml`: Automate builds, tests, and code coverage reporting.
  - `release.yaml`: Automatically create new releases.
  - `codeql.yaml`: Perform automated CodeQL security analysis.
- Sample [Dockerfiles](https://docs.docker.com/get-started/overview/) to build Docker images for both Linux and Windows environments.
- Sample files included: README, LICENSE, RELEASE-NOTES and .gitignore.

### 👉 LIVE DEMO: https://demo-bat.gpvcloud.com/

## Usage

Install (or update) the package from NuGet to make the template available:

```sh
$ dotnet new install Ddth.Templates.Blazor
```

After the package is installed, create a new solution using the template:

```sh
$ dotnet new bat -n MyApp
```

The above command will create a new solution named `MyApp` in the current directory.

**Happy coding!**

🌟 If you find this project useful, please star it. 🌟

## Resources and Documentation

- [Project Wiki](https://github.com/DDTH/blazor-admin-template/wiki)
- [CoreUI Bootstrap Demo](https://coreui.io/demos/bootstrap/5.0/free/)
- [CoreUI Bootstrap Docs](https://coreui.io/bootstrap/docs/getting-started/introduction/)
- [CoreUI Icons](https://coreui.io/icons/)
- [Bootstrap Icons](https://icons.getbootstrap.com/)

## License

This template is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Contributing & Support

Feel free to create [pull requests](https://github.com/DDTH/blazor-admin-template/compare/contribution_queue...) or [issues](https://github.com/DDTH/blazor-admin-template/issues) to report bugs or suggest new features.

> Please create PRs against the `contribution_queue` branch.

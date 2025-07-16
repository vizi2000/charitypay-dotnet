# Coding Standards

This document summarizes key conventions for the CharityPay .NET codebase.  Developers should follow these guidelines to keep the project consistent and maintainable.

## General
- **Target Framework**: .NET 8.0
- **Language Version**: C# 12
- **Nullable Reference Types**: enabled
- **Implicit Usings**: enabled
- **File-Scoped Namespaces** are preferred.

## Naming
- Interfaces are prefixed with `I`.
- Classes, methods and properties use **PascalCase**.
- Private fields are `camelCase` with a leading underscore.
- Constants are `UPPER_CASE`.

## Async methods
- Use the `Async` suffix and include a `CancellationToken` parameter.
- Avoid `async void` except for event handlers.

## Commits and Pull Requests
- Use [Conventional Commits](https://www.conventionalcommits.org/).
- Reference task numbers from `TASK.md` where possible.
- Keep pull request descriptions concise and list completed tasks and testing steps.

## Documentation
Update the `README.md` and relevant docs whenever features or configuration change.

# AGENTS.md

This repository contains the ContosoInventory sample app for the GitHub Copilot customization training module. The app is a .NET 10 solution with a Blazor WebAssembly client, ASP.NET Core server, and a shared library.

## Project at a glance

- Primary docs: [README.md](README.md), [stakeholders-starter-app-specification.md](stakeholders-starter-app-specification.md)
- Solution: [ContosoInventory/ContosoInventory.sln](ContosoInventory/ContosoInventory.sln)
- Server app: [ContosoInventory/ContosoInventory.Server](ContosoInventory/ContosoInventory.Server)
- Client app: [ContosoInventory/ContosoInventory.Client](ContosoInventory/ContosoInventory.Client)
- Shared library: [ContosoInventory/ContosoInventory.Shared](ContosoInventory/ContosoInventory.Shared)

## Required environment

- Use the .NET 10 SDK. The repo includes [global.json](global.json) and targets `net10.0`.
- Do not downgrade the project to .NET 8 or change the target framework without a clear reason.

## Build and run

Run from the repository root:

```bash
dotnet build ContosoInventory/ContosoInventory.sln
```

Run the server:

```bash
dotnet run --project ContosoInventory/ContosoInventory.Server
```

The app listens on `http://localhost:5240` and exposes Swagger at `/swagger`.

## Architecture conventions

- Server: ASP.NET Core Web API, EF Core, ASP.NET Core Identity, SQLite.
- Client: Blazor WebAssembly front end.
- Shared: DTOs and shared contracts used by both projects.
- Prefer the existing service/repository pattern and dependency injection instead of introducing new architectural patterns.
- Keep API contracts in the shared project when the client and server need to agree on DTOs.

## Common project patterns

- Use `async`/`await` for I/O-bound work.
- Keep database and identity work in the server project.
- Use SQLite-friendly paths and cross-platform conventions already described in the specification.
- When making changes, preserve the existing app structure and naming patterns used in the current codebase.

## Before making changes

- Check the relevant README/spec section for requirements and constraints.
- Prefer the smallest targeted edit that matches the existing project structure.
- Validate with the relevant `dotnet` build command after edits.

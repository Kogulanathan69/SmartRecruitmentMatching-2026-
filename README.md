# NexHire

Smart hiring & candidate matching platform.

## Solution Layout
- `src/NexHire.API` – ASP.NET Core Web API (Controllers, Middleware, DI setup)
- `src/NexHire.Application` – Application layer (DTOs, service interfaces/implementations, validators, mappings)
- `src/NexHire.Domain` – Domain entities, enums, value objects
- `src/NexHire.Infrastructure` – EF Core data access, authentication, matching engine, file storage, notifications
- `tests/` – Unit and integration test projects
- `database/` – SQL seed scripts and ER diagram
- `docs/` – BRD, SRS, API docs, DB docs, test docs, viva material

## Getting Started
1. Open `NexHire.sln` in Visual Studio 2022+.
2. Update the connection string in `src/NexHire.API/appsettings.json`.
3. Set `NexHire.API` as the startup project.
4. Run EF Core migrations: `dotnet ef database update --project src/NexHire.Infrastructure --startup-project src/NexHire.API`.
5. Press F5 to run.

# Push checklist

Run these commands from the repository root before committing:

```cmd
dotnet clean NexHire.sln
dotnet restore NexHire.sln
dotnet build NexHire.sln --no-restore
dotnet test NexHire.sln --no-build
dotnet ef database update --project src\NexHire.Infrastructure --startup-project src\NexHire.API
git status --short
```

Only push after build, tests, and the database migration complete successfully.

The API launch URLs are:

- Swagger: `http://localhost:5000/swagger/index.html` when launched without a profile.
- Swagger: `http://localhost:5119/swagger/index.html` with the `http` launch profile.
- Login UI: `/auth/login.html`.

Do not commit `.vs`, `bin`, `obj`, `.env`, or local production settings.

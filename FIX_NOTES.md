# NexHire complete repair notes

## Build errors repaired

- Removed the obsolete `Microsoft.AspNetCore.Http.Abstractions 2.2.0` package.
- Added `FrameworkReference Include="Microsoft.AspNetCore.App"` to `NexHire.Infrastructure`.
- Removed `AutoMapper.Extensions.Microsoft.DependencyInjection 12.0.1`.
- Aligned both API and Application projects to `AutoMapper 15.1.3`.
- Updated AutoMapper registration to the configuration-first overload.
- Updated .NET 8 Microsoft packages to the 8.0.29 servicing release.
- Removed all old `bin` and `obj` build output.

## Runtime stability repairs

- Added JSON cycle protection for EF Core navigation properties.
- Included the `User` navigation when loading detailed job-seeker profiles.
- Made database initialization fall back to `EnsureCreated()` when migrations are absent.
- Added a LocalDB development connection string.

## First run

1. Extract the ZIP.
2. Open `NexHire.sln` in Visual Studio 2022.
3. Right-click `NexHire.API` and choose **Set as Startup Project**.
4. Use **Build > Rebuild Solution**.
5. Run the HTTPS profile.

Command-line equivalent:

```powershell
dotnet clean
dotnet restore
dotnet build
```

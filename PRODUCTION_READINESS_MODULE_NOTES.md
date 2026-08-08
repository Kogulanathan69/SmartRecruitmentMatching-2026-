# NexHire Production Readiness Module

## Added

- Correlation ID middleware (`X-Correlation-ID`)
- Security response headers
- Global fixed-window API rate limiting
- Liveness health endpoint
- Database readiness health endpoint
- Trace ID in exception responses
- Production configuration template

## Endpoints

- `GET /health/live` — confirms that the API process is running.
- `GET /health/ready` — confirms that SQL Server can be reached.

## Rate limit

The default policy allows 100 requests per minute for each API endpoint pipeline. A rejected request returns HTTP `429 Too Many Requests`.

## Production secrets

Do not commit real secrets into `appsettings.Production.json`. Configure these using environment variables or a secret store:

```text
ConnectionStrings__DefaultConnection
Jwt__Key
Jwt__Issuer
Jwt__Audience
```

## Build

```powershell
dotnet clean NexHire.sln
dotnet restore NexHire.sln
dotnet build NexHire.sln
```

This module does not change the database schema, so no migration is required.

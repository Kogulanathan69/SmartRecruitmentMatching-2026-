# NexHire Docker Deployment

## Requirements

- Docker Desktop
- Docker Compose v2

## Start the system

From the solution root:

```powershell
Copy-Item .env.example .env
docker compose up --build -d
```

Open:

- Swagger: `http://localhost:8080/swagger`
- Live health: `http://localhost:8080/health/live`
- Database health: `http://localhost:8080/health/ready`

## View logs

```powershell
docker compose logs -f api
docker compose logs -f sqlserver
```

## Stop containers

```powershell
docker compose down
```

## Remove database volume

This permanently deletes Docker SQL data:

```powershell
docker compose down -v
```

## Production notes

- Do not use the example SA password or JWT key in production.
- Store secrets in Azure App Service settings, GitHub Secrets, or another secret manager.
- Use `ASPNETCORE_ENVIRONMENT=Production` in production.
- Apply migrations through a controlled deployment step before starting the production API.
- Restrict CORS to the real frontend URL.

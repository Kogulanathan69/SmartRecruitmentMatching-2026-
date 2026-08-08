# Docker & CI/CD Module

## Added

- Multi-stage .NET 8 Dockerfile
- SQL Server + API Docker Compose setup
- Persistent SQL volume
- API and SQL health checks
- Environment-variable configuration
- `.env.example`
- `.dockerignore`
- GitHub Actions restore/build/test workflow
- Docker image build verification in CI
- Deployment guide

## Local run

```powershell
Copy-Item .env.example .env
docker compose up --build -d
```

Then open `http://localhost:8080/swagger`.

## Important

The Compose configuration uses Development mode so the current project initializer can apply schema/seed data and Swagger is available for the project demo. Before production deployment, move database migration into a controlled release step and use Production mode.

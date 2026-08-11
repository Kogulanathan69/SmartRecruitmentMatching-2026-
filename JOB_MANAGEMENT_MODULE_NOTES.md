# NexHire – Job Management Module

## Added or enhanced
- Job responsibilities, education requirement, vacancy count, hybrid mode and update timestamps.
- Verified active companies only can publish jobs.
- Required skills are mandatory; preferred skills exclude duplicates of required skills.
- Company ownership is checked for create, update, publish, pause, reopen and close actions.
- Published jobs can be paused and reopened.
- Overdue published jobs can be marked Expired by an admin endpoint.
- Public search automatically excludes closed, suspended and overdue jobs.
- Advanced filters: company, keyword, city, country, employment type, remote, hybrid, candidate experience, salary and skills.
- Sorting: Newest, SalaryHigh, SalaryLow and ClosingSoon.
- Response includes verified-company state and application count.

## Endpoints
- POST `/api/jobs`
- GET `/api/jobs/{jobId}`
- PUT `/api/jobs/{jobId}`
- POST `/api/jobs/{jobId}/publish`
- POST `/api/jobs/{jobId}/pause`
- POST `/api/jobs/{jobId}/reopen`
- POST `/api/jobs/{jobId}/close`
- POST `/api/jobs/expire-overdue` (Admin)
- GET `/api/jobs/search`
- GET `/api/jobs/company/{companyId}`

## Migration
```cmd
dotnet ef migrations add JobManagementModule --project src\NexHire.Infrastructure --startup-project src\NexHire.API
dotnet ef database update --project src\NexHire.Infrastructure --startup-project src\NexHire.API
```

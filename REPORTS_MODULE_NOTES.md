# NexHire Reports Module

## Included reports

- Admin system report
- Company hiring report
- Job seeker activity report
- Date-range filtering
- Application status breakdown
- Monthly trend series
- Offer acceptance rate
- Application-to-hire conversion
- Job-level performance
- CSV export for company and job seeker reports

## Endpoints

```http
GET /api/reports/admin?from=2026-01-01&to=2026-08-04
GET /api/reports/company?from=2026-01-01&to=2026-08-04
GET /api/reports/jobseeker?from=2026-01-01&to=2026-08-04
GET /api/reports/company/export.csv?from=2026-01-01&to=2026-08-04
GET /api/reports/jobseeker/export.csv?from=2026-01-01&to=2026-08-04
```

## Authorization

- Admin report: Admin only
- Company report: Employer only; returns only the authenticated employer's company data
- Job seeker report: JobSeeker only; returns only the authenticated candidate's data

## Database

This module uses existing tables and does not require a migration.

## Build

```powershell
dotnet clean NexHire.sln
dotnet restore NexHire.sln
dotnet build NexHire.sln
```

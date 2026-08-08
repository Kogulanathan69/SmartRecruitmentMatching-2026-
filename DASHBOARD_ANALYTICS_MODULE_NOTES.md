# NexHire Dashboard & Analytics Module

## New endpoints

- `GET /api/dashboard/admin` — Admin only
- `GET /api/dashboard/company` — Employer only
- `GET /api/dashboard/jobseeker` — Job Seeker only

All endpoints require a valid JWT bearer token.

## Admin dashboard

- Total users, job seekers, employers and companies
- Verified and pending companies
- Active jobs and total applications
- Scheduled interviews, sent offers and hires
- Six-month user and job growth
- Recent audit activity

## Company dashboard

- Company trust score and trust level
- Active and draft jobs
- Applications, shortlisted candidates and hires
- Interviews today and upcoming interviews
- Pending offers and offer acceptance rate
- Hiring funnel
- Six-month application trend
- Recent applications

## Job Seeker dashboard

- Rule-based profile completion score
- Primary resume completeness and quality
- Application status summary
- Upcoming interviews and active offers
- Unread notifications
- Six-month application trend
- Recent applications

## Database migration

No entity or table schema was changed. A migration is not required.

## Build commands

```powershell
dotnet clean NexHire.sln
dotnet restore NexHire.sln
dotnet build NexHire.sln
```

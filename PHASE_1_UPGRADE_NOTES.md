# NexHire Backend Upgrade – Phase 1

This upgraded package adds the first production-oriented business-logic layer discussed in the project blueprint.

## Added

- Expanded company, application, job, and verification statuses
- Evidence-based company data fields
- Duplicate company registration/email checks
- Company trust score and trust level
- Website/email-domain consistency check
- Verified-company-only job publishing
- Required-skill validation before job publishing
- Job-expiry validation before applying
- Candidate profile-completion rule
- Mandatory-skill eligibility rule
- Explainable matching notes
- Matched and missing skill lists
- Eligibility result and mandatory-rule failures
- Fair competition ranking with shared ranks for ties
- Application status-transition rules
- Matching-rule total must equal 100%

## Still Planned

- Resume builder and PDF generation
- CV file validation and evidence verification
- Interview scorecard weights
- Waiting-list fallback offer flow
- Automatic talent-pool re-matching
- Real file storage and notifications
- Database migration regeneration
- Full unit/integration tests

## Run on Windows

```cmd
dotnet clean NexHire.sln
dotnet restore NexHire.sln
dotnet build NexHire.sln
```

After a successful build, regenerate the EF Core migration because entity fields and enums changed.

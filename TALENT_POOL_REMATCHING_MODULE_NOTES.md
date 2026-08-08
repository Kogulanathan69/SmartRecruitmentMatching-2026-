# NexHire Talent Pool Auto Re-Matching Module

## Purpose

This module protects qualified candidates from being discarded after one recruitment decision. Employers can keep candidates in a company talent pool and re-run the transparent matching engine when a new job is published.

## New endpoints

- `POST /api/talentpool/{companyId}/rematch/{jobId}`
- `POST /api/talentpool/{companyId}/add-qualified-from-job/{jobId}`

## Re-match request example

```json
{
  "minimumScore": 75,
  "eligibleOnly": true,
  "notifyCandidates": true,
  "take": 50
}
```

## Add qualified applicants example

```json
{
  "minimumScore": 65,
  "includeRejected": true,
  "includeWaitingList": true,
  "tag": "Strong Backend Candidate"
}
```

## Business rules

1. The job must belong to the selected company.
2. Only published and non-expired jobs can run re-matching.
3. Private profiles and candidates not open to work are skipped.
4. Eligibility and transparent matching use the existing matching engine.
5. The employer controls the minimum score.
6. Candidate notifications are optional.
7. Duplicate talent-pool entries are prevented.
8. Re-matching is decision support; it does not automatically hire or submit an application.

## Database migration

No migration is required. Existing TalentPoolEntries, MatchResults and Notifications tables are used.

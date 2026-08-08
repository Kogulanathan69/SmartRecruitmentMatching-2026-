# NexHire Matching Engine – Phase 5.3

## Added

- Competition ranking with shared ranks: `1, 2, 2, 4`
- Tie detection
- Top-candidate endpoint
- Ranking summary endpoint
- Candidate recommendation labels
- Score bands
- Selected-candidate comparison for 2–4 candidates
- Shortlist and reject actions
- Rejected and withdrawn candidates excluded from ranking
- Transparent score breakdown retained in each ranking item

## New endpoints

- `GET /api/Matching/ranking/{jobId}?take=20`
- `GET /api/Matching/top/{jobId}?count=10`
- `POST /api/Matching/compare`
- `POST /api/Matching/{applicationId}/shortlist`
- `POST /api/Matching/{applicationId}/reject`

## Compare request example

```json
{
  "jobId": "00000000-0000-0000-0000-000000000000",
  "jobSeekerProfileIds": [
    "00000000-0000-0000-0000-000000000001",
    "00000000-0000-0000-0000-000000000002"
  ]
}
```

## Recommendation rules

- 90–100: Highly Recommended
- 75–89.99: Recommended for Review
- 60–74.99: Consider with Caution
- Below 60: Not Recommended
- Mandatory rule failure: Not Eligible

The recommendation is decision support only. The employer makes the final hiring decision.

# NexHire Sprint 6 – Interview Management

## Implemented

- Schedule interview only for shortlisted candidates
- Employer ownership validation
- Future date and duration validation
- Candidate overlap detection
- Candidate confirmation
- Employer/candidate reschedule workflow
- Interview cancellation
- Flexible criterion-based scorecard
- Completion only after score submission
- Application status synchronization
- Audit logs for schedule, update, confirm, reschedule, score, cancel and complete
- Employer and candidate interview-list endpoints

## New endpoints

- `POST /api/interviews`
- `GET /api/interviews/{interviewId}`
- `PUT /api/interviews/{interviewId}`
- `GET /api/interviews/application/{applicationId}`
- `GET /api/interviews/company/my`
- `GET /api/interviews/candidate/my`
- `POST /api/interviews/{interviewId}/confirm`
- `POST /api/interviews/{interviewId}/reschedule`
- `POST /api/interviews/{interviewId}/cancel`
- `POST /api/interviews/{interviewId}/score`
- `POST /api/interviews/{interviewId}/complete`

## Migration required

The Interview entity gained RoundName, reason fields and audit timestamps. Create a migration:

```powershell
dotnet ef migrations add InterviewManagementModule --project src/NexHire.Infrastructure --startup-project src/NexHire.API
dotnet ef database update --project src/NexHire.Infrastructure --startup-project src/NexHire.API
```

## Scorecard example

```json
{
  "scores": [
    { "criterion": "Technical Knowledge", "score": 88, "comments": "Strong .NET knowledge" },
    { "criterion": "Problem Solving", "score": 82 },
    { "criterion": "Communication", "score": 75 },
    { "criterion": "Project Knowledge", "score": 90 },
    { "criterion": "Teamwork", "score": 80 },
    { "criterion": "Professional Attitude", "score": 85 }
  ],
  "overallFeedback": "Recommended for the next stage."
}
```

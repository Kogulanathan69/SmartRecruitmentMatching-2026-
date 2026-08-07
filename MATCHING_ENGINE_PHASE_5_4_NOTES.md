# Sprint 5.4 – Explainable Matching and HR Decisions

## Added
- Explainable match endpoint by application ID
- Strengths, improvement areas, matched/missing skills and score breakdown
- Strict application status transitions
- Under Review, Shortlist, Waiting List and Reject actions
- Closed/expired/suspended job protection
- Audit log creation for every HR status decision
- Decision responses showing previous/current status

## Endpoints
- `GET /api/matching/explanation/{applicationId}`
- `POST /api/matching/{applicationId}/under-review`
- `POST /api/matching/{applicationId}/shortlist`
- `POST /api/matching/{applicationId}/waiting-list`
- `POST /api/matching/{applicationId}/reject`

## Allowed transitions
- Submitted / ReMatched -> UnderReview
- Submitted / UnderReview / WaitingList -> Shortlisted
- UnderReview / Shortlisted / InterviewCompleted -> WaitingList
- Submitted / UnderReview / Shortlisted / InterviewCompleted / WaitingList -> Rejected

No database migration is required because the existing AuditLogs table is reused.

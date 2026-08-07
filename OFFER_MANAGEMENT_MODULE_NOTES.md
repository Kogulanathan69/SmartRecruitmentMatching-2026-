# NexHire Sprint 7 – Offer Management

Includes draft creation, employer ownership checks, interview-completed/waiting-list eligibility, future joining and expiry validation, one offer per application, send, candidate view tracking, accept, reject, clarification, cancel, audit logs, and company/candidate lists.

## Status flow
Draft → Sent → Accepted / Declined / ClarificationRequested / Cancelled / Expired

Accepted updates application to Hired. Declined updates application to WaitingList for manual company review. The system never automatically sends an offer to the next candidate.

## Migration
`dotnet ef migrations add OfferManagementModule --project src/NexHire.Infrastructure --startup-project src/NexHire.API`

`dotnet ef database update --project src/NexHire.Infrastructure --startup-project src/NexHire.API`

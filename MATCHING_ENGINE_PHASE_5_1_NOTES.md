# NexHire Matching Engine – Phase 5.1

## Added

- `IEligibilityEngine`
- `EligibilityEngine`
- Mandatory job-status and closing-date validation
- Mandatory skill existence validation
- Mandatory skill proficiency validation
- Minimum experience validation
- Rule-based education-level validation
- Clear eligibility failure reasons
- Matched and missing mandatory-skill lists
- Dependency injection registration
- Integration with the existing transparent matching service

## Eligibility order

```text
Job availability
  ↓
Mandatory skills
  ↓
Minimum experience
  ↓
Education requirement
  ↓
Eligible / Not Eligible
  ↓
Weighted score is still calculated for explanation
```

## Fairness rule

The engine does not automatically select or reject a candidate for the employer. It records eligibility evidence and transparent score details. Final recruitment decisions remain with the company.

## Build

```powershell
dotnet clean NexHire.sln
dotnet restore NexHire.sln
dotnet build NexHire.sln
```

No database migration is required for Phase 5.1 because no entity schema was changed.

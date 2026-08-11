# NexHire Matching Engine – Phase 5.2

## Transparent Weighted Score Engine

This phase upgrades the existing matching response without changing the database schema.

### Default scoring categories

- Skills
- Experience
- Education
- Certification
- Location
- Projects
- Profile completion

The active weights are read from `MatchingRules`. Active weights must total 100%.

### New transparent output

Each category now returns:

- Raw score out of 100
- Configured weight
- Weighted points earned
- Maximum weighted points
- Status: Strong, Good, Partial, or Needs Improvement
- Explanation note

The overall response now includes:

- Earned points out of 100
- Strengths
- Improvement areas
- Matched mandatory skills
- Missing mandatory skills
- Eligibility failures
- Plain-language summary

### Improved skill scoring

When both mandatory and preferred skills exist:

- Mandatory skills contribute 80% of the skill score
- Preferred skills contribute 20% of the skill score

Mandatory eligibility is still evaluated separately by `EligibilityEngine`.

### Important fairness rule

The system does not automatically hire a candidate. It provides explainable decision support only.

## Build

```powershell
dotnet clean NexHire.sln
dotnet restore NexHire.sln
dotnet build NexHire.sln
```

No migration is required for this phase.

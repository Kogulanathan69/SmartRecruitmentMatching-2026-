# NexHire Rule-Based CV Builder Module

## Included
- Multiple named resumes per job seeker
- One primary resume rule
- Five seeded templates
- Resume completeness score (0–100)
- Missing-section detection and recommendations
- Rule-based HTML CV generation without AI
- Preview and downloadable printable HTML
- Unique resume name validation
- Minimum education, 3 skills and 50-character objective validation
- Fresher-friendly experience rule

## Endpoints
- GET `/api/resumes`
- GET `/api/resumes/templates`
- POST `/api/resumes`
- GET/PUT/DELETE `/api/resumes/{id}`
- GET `/api/resumes/completeness?resumeId=`
- POST `/api/resumes/{id}/generate`
- GET `/api/resumes/{id}/preview`
- GET `/api/resumes/{id}/download`

## Important
The MVP exports printable HTML. Browser Print → Save as PDF can produce a PDF. A server-side PDF library can be added later.

## Migration
`dotnet ef migrations add CvBuilderModule --project src/NexHire.Infrastructure --startup-project src/NexHire.API`

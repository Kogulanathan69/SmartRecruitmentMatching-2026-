# NexHire – Company Verification Module

## Added endpoints

### Employer
- `POST /api/companies`
- `GET /api/companies/mine`
- `PUT /api/companies/{companyId}`
- `POST /api/companies/{companyId}/documents`
- `POST /api/companies/{companyId}/verify-email`
- `POST /api/companies/{companyId}/verify-phone`
- `POST /api/companies/{companyId}/submit-verification`
- `GET /api/companies/{companyId}/verification-status`

### Admin
- `GET /api/admin/companies/pending`
- `GET /api/admin/companies/{companyId}/verification`
- `POST /api/admin/companies/{companyId}/review`

## Important rules
- Payment never creates verification.
- Registration number and official email must be unique.
- Business-registration document is mandatory.
- Supported files: PDF, JPG, JPEG, PNG; maximum 5 MB.
- Email and phone ownership must be verified before submission.
- Admin can approve, reject, suspend, or request more information.
- Only an active evidence-verified company can publish jobs.
- Trust score is visible but does not automatically ban a company.

## Migration

```cmd
dotnet ef migrations add CompanyVerificationModule --project src\NexHire.Infrastructure --startup-project src\NexHire.API
dotnet ef database update --project src\NexHire.Infrastructure --startup-project src\NexHire.API
```

The email/phone verification endpoints are MVP simulation endpoints. Replace them later with OTP provider integrations.

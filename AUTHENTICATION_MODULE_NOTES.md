# NexHire Authentication Module

Implemented endpoints:

- POST `/api/auth/register`
- POST `/api/auth/login`
- POST `/api/auth/refresh`
- POST `/api/auth/revoke` (authorized)
- GET `/api/auth/me` (authorized)
- POST `/api/auth/change-password` (authorized)

## Register sample

```json
{
  "email": "candidate@example.com",
  "password": "Strong@123",
  "confirmPassword": "Strong@123",
  "firstName": "Kogul",
  "lastName": "Nathan",
  "phoneNumber": "+94771234567",
  "role": "JobSeeker"
}
```

Allowed self-registration roles: `JobSeeker`, `Employer`.
Admin accounts must be seeded or created by an existing administrator.

## Login sample

```json
{
  "email": "candidate@example.com",
  "password": "Strong@123"
}
```

Use returned access token in Swagger Authorize:

```text
Bearer YOUR_ACCESS_TOKEN
```

## Change password sample

```json
{
  "currentPassword": "Strong@123",
  "newPassword": "NewStrong@456",
  "confirmNewPassword": "NewStrong@456"
}
```

## Security behavior

- BCrypt work factor 12
- JWT role and user-id claims
- Refresh-token rotation
- Inactive/suspended account login blocked
- Duplicate email blocked
- Admin self-registration blocked
- Strong password validation

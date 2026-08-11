# Admin Member Management

Protected by JWT role `Admin`. API base: `/api/Admin/users`.

- GET `/` and `/{id}`
- POST `/`
- PUT `/{id}`
- PATCH `/{id}/status`
- POST `/{id}/reset-password`
- DELETE `/{id}` performs safe disable and refresh-token revocation

Frontend: `/admin/members.html`

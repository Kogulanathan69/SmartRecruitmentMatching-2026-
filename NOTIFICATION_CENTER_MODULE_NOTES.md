# NexHire Notification Center Module

## Included
- Authenticated user notification list
- Unread-only filter
- Unread count
- Mark one notification as read
- Mark all notifications as read
- Delete own notification
- Admin manual notification creation
- Reusable `INotificationService.CreateAsync(...)` for Interview, Offer, Application and Company workflows

## Endpoints
- `GET /api/notifications`
- `GET /api/notifications?unreadOnly=true&take=20`
- `GET /api/notifications/unread-count`
- `POST /api/notifications` (Admin)
- `POST /api/notifications/{id}/read`
- `POST /api/notifications/read-all`
- `DELETE /api/notifications/{id}`

## Database
Uses the existing `Notifications` table and therefore does not require a migration.

## Next integration
Inject `INotificationService` into InterviewService and OfferService and call `CreateAsync` after schedule, confirm, complete, send, accept and reject events.

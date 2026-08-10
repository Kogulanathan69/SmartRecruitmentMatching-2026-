NexHire Professional Prototype-Style Frontend

This version keeps the 99-endpoint API console and upgrades the user-facing role pages into a polished prototype-style UI.

Main role workspaces:
- Job Seeker: dashboard, jobs, applications, CV Studio, skill passport, match insights, notifications, privacy.
- Employer: dashboard, company + verification, vacancies, candidates, matching, reports, notifications.
- Admin: dashboard, company verification, users, matching rules, audit, reports, privacy.
- shared/api-console.html remains the complete advanced UI for all 99 implemented backend endpoints.

Run:
1. Start NexHire.API (default https://localhost:7119).
2. Trust the localhost HTTPS certificate.
3. Serve this frontend using VS Code Live Server; do not open with file:///.
4. Register -> OTP verify -> Login -> Role dashboard.

Empty backend controllers (Interviews, Offers, TalentPool, Complaints) still cannot have live working actions until backend endpoints are implemented.

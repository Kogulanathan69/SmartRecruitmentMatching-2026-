NEXHIRE FINAL PROTOTYPE-STYLE FRONTEND
=====================================

Verified backend endpoint UI count: 99

THIS VERSION CONTAINS
---------------------
1. Prototype-style Job Seeker workspace
   - Overview dashboard
   - Find Jobs
   - Applications
   - CV Studio / Resume
   - Skill Passport / Profile
   - Match Insights
   - Notifications
   - Privacy Center

2. Prototype-style Employer workspace
   - Overview dashboard
   - Company Profile
   - Company Verification
   - Vacancies / Job Management
   - Candidate Applications / Ranking
   - Matching
   - Reports
   - Notifications

3. Prototype-style Administrator workspace
   - Overview dashboard
   - Company Verification
   - User Accounts
   - Vacancy Control
   - Matching Rules
   - Audit Log
   - Reports
   - Privacy / deletion requests

4. Authentication flow
   Register -> OTP -> Verify -> Login -> Role Dashboard

5. ALL 99 IMPLEMENTED BACKEND ENDPOINTS
   See ENDPOINT_INVENTORY.txt and shared/api-console.html.
   The role pages expose customer-friendly workflows; the API console keeps every implemented endpoint testable.

6. Visual reference
   prototype-reference.html contains the validated enterprise prototype used as the UI/UX reference.

IMPORTANT BACKEND LIMITATION
----------------------------
ComplaintsController, InterviewsController, OffersController and TalentPoolController have no implemented HTTP endpoints in the supplied 99-endpoint backend inventory, so the frontend must not pretend those actions are live backend features.

RUN
---
1. Start NexHire ASP.NET backend first.
2. Confirm Swagger works at the backend HTTPS address.
3. Open this frontend with VS Code Live Server (do NOT use file:/// directly).
4. If the backend port changes, update assets/js/api.js.
5. Register -> verify OTP -> login -> use the role workspace.

FILES TO CHECK FIRST
--------------------
index.html
jobseeker/dashboard.html
employer/dashboard.html
admin/dashboard.html
shared/api-console.html
prototype-reference.html
ENDPOINT_INVENTORY.txt

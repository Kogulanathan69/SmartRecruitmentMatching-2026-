NexHire Complete Pure HTML/CSS/JavaScript Frontend

SOURCE
- Generated from the backend ZIP supplied in this conversation.
- Implemented HTTP endpoints discovered: 99
- Empty backend controllers: ComplaintsController, InterviewsController, OffersController, TalentPoolController

RUN
1. Start NexHire.API.
2. Confirm Swagger opens at https://localhost:7119/swagger/index.html
3. Trust the localhost HTTPS certificate.
4. Serve this frontend folder with VS Code Live Server (recommended).
5. Open index.html through Live Server.
6. API base URL is in assets/js/api.js and defaults to https://localhost:7119.

IMPORTANT
- shared/api-console.html gives a frontend execution UI for EVERY implemented HTTP endpoint extracted from the backend.
- Role folders provide separate Job Seeker, Employer and Admin navigation/screens.
- Controllers that are literally empty in the backend cannot have working endpoint integrations until backend endpoints are implemented.
- For endpoints with GUID route placeholders, replace the placeholder in the path field before Execute.
- For POST/PUT/PATCH endpoints, enter the backend DTO JSON in the request-body box.

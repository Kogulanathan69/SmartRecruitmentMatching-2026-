using NexHire.Infrastructure.Privacy;
using NexHire.API.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NexHire.API.Extensions;
using NexHire.API.Services;

using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Mappings;
using NexHire.Application.Matching;
using NexHire.Application.Services;

using NexHire.Infrastructure.Data;
using NexHire.Infrastructure.Matching;
using NexHire.Infrastructure.Repositories;
using NexHire.Infrastructure.Reports;
using NexHire.Infrastructure.Notifications;
using NexHire.Infrastructure.Email;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1. CONTROLLERS
// ----------------------------------------------------
builder.Services.AddControllers();

// ----------------------------------------------------
// 2. SWAGGER
// ----------------------------------------------------
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddJwtSwagger();
});

// ----------------------------------------------------
// 3. DATABASE
// ----------------------------------------------------
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// ----------------------------------------------------
// 4. AUTHENTICATION
// ----------------------------------------------------
builder.Services.AddNexHireAuthentication(
    builder.Configuration);

// ----------------------------------------------------
// 5. HTTP CONTEXT / CURRENT USER
// ----------------------------------------------------
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

// ----------------------------------------------------
// 6. USER / AUTH REPOSITORY
// ----------------------------------------------------
builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

// ----------------------------------------------------
// 7. EMAIL / OTP SERVICE
// ----------------------------------------------------
builder.Services.AddScoped<
    IEmailSender,
    SmtpEmailSender>();

// ----------------------------------------------------
// 8. JOB APPLICATION REPOSITORY
// ----------------------------------------------------
builder.Services.AddScoped<
    IJobApplicationRepository,
    JobApplicationRepository>();

// ----------------------------------------------------
// 9. APPLICATION STATUS HISTORY
// ----------------------------------------------------
builder.Services.AddScoped<
    IApplicationStatusHistoryRepository,
    ApplicationStatusHistoryRepository>();

// ----------------------------------------------------
// 10. CONTACT REQUEST REPOSITORY
// ----------------------------------------------------
builder.Services.AddScoped<
    IContactRequestRepository,
    ContactRequestRepository>();

// ----------------------------------------------------
// 11. APPLICATION STATUS SERVICE
// ----------------------------------------------------
builder.Services.AddScoped<
    IApplicationStatusService,
    ApplicationStatusService>();

// ----------------------------------------------------
// 12. CONSENT CONTACT SERVICE
// ----------------------------------------------------
builder.Services.AddScoped<
    IConsentContactService,
    ConsentContactService>();

// ----------------------------------------------------
// 13. JOB SEEKER + RESUME MODULE
// ----------------------------------------------------
builder.Services.AddScoped<
    IJobSeekerRepository,
    JobSeekerRepository>();

builder.Services.AddScoped<
    IResumeRepository,
    ResumeRepository>();

builder.Services.AddScoped<
    IJobSeekerService,
    JobSeekerService>();

builder.Services.AddScoped<
    IResumeService,
    ResumeService>();

builder.Services.AddAutoMapper(
    cfg => { },
    typeof(JobSeekerMappingProfile).Assembly);

// ----------------------------------------------------
// 14. MATCHING ENGINE
// ----------------------------------------------------

// Eligibility
builder.Services.AddScoped<
    IEligibilityEngine,
    EligibilityEngine>();

// Individual calculators
builder.Services.AddScoped<
    SkillMatchCalculator>();

builder.Services.AddScoped<
    ExperienceMatchCalculator>();

builder.Services.AddScoped<
    EducationMatchCalculator>();

builder.Services.AddScoped<
    CertificationMatchCalculator>();

builder.Services.AddScoped<
    LocationMatchCalculator>();

builder.Services.AddScoped<
    ProjectMatchCalculator>();

builder.Services.AddScoped<
    ProfileCompletionMatchCalculator>();

// Final score
builder.Services.AddScoped<
    MatchScoreCalculator>();

// Recommendation / ranking / comparison
builder.Services.AddScoped<
    IRecommendationEngine,
    RecommendationEngine>();

builder.Services.AddScoped<
    CandidateRankingEngine>();

builder.Services.AddScoped<
    CandidateComparisonEngine>();

// Main matching facade
builder.Services.AddScoped<
    IMatchingEngine,
    MatchingEngine>();

// Matching service
builder.Services.AddScoped<
    IMatchingService,
    MatchingService>();

// ----------------------------------------------------
// 15. ADMIN MODULE
// ----------------------------------------------------
builder.Services.AddScoped<
    IAdminService,
    AdminService>();

// ----------------------------------------------------
// 16. COMPANY MODULE
// ----------------------------------------------------
builder.Services.AddScoped<
    ICompanyRepository,
    CompanyRepository>();

builder.Services.AddScoped<
    IUnitOfWork,
    UnitOfWork>();

builder.Services.AddScoped<
    ICompanyService,
    CompanyService>();

// ----------------------------------------------------
// 17. CORE COMPLETION MODULES
// ----------------------------------------------------

// Jobs
builder.Services.AddScoped<
    IJobRepository,
    JobRepository>();

builder.Services.AddScoped<
    IJobService,
    JobService>();

// Applications
builder.Services.AddScoped<
    IApplicationRepository,
    ApplicationRepository>();

builder.Services.AddScoped<
    IApplicationService,
    ApplicationService>();

// Core Matching
builder.Services.AddScoped<
    ICoreMatchingService,
    CoreMatchingService>();

// Notifications
builder.Services.AddScoped<
    INotificationWriter,
    NotificationWriter>();

builder.Services.AddScoped<
    INotificationService,
    NexHire.Infrastructure.Notifications.NotificationService>();

builder.Services.AddScoped<
    INotificationRepository,
    NotificationRepository>();

builder.Services.AddScoped<
    IDashboardRepository,
    DashboardRepository>();

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();

builder.Services.AddHostedService<
    OutboxWorker>();

// Reports
builder.Services.AddScoped<
    IReportService,
    ReportService>();

// Privacy
builder.Services.AddScoped<
    IPrivacyService,
    PrivacyService>();

// Audit
builder.Services.AddScoped<
    IAuditLogRepository,
    AuditLogRepository>();

// ----------------------------------------------------
// 18. AUTHORIZATION
// ----------------------------------------------------
builder.Services.AddAuthorization();

// ----------------------------------------------------
// 19. CORS
// ----------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// ----------------------------------------------------
// BUILD APP
// ----------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------
// 20. CUSTOM MIDDLEWARE
// ----------------------------------------------------
app.UseMiddleware<
    CorrelationIdMiddleware>();

app.UseMiddleware<
    SecurityHeadersMiddleware>();

app.UseMiddleware<
    ExceptionMiddleware>();

// ----------------------------------------------------
// 21. SWAGGER
// ----------------------------------------------------
app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "NexHire API v1");

    options.RoutePrefix = "swagger";
});

// ----------------------------------------------------
// 22. HTTPS
// ----------------------------------------------------
app.UseHttpsRedirection();

// ----------------------------------------------------
// 23. CORS
// ----------------------------------------------------
app.UseCors(
    "AllowFrontend");

// ----------------------------------------------------
// 24. FRONTEND STATIC FILES
// ----------------------------------------------------
var frontendPath =
    Path.GetFullPath(
        Path.Combine(
            app.Environment.ContentRootPath,
            "..",
            "..",
            "frontend"));

if (Directory.Exists(frontendPath))
{
    app.UseStaticFiles(
        new StaticFileOptions
        {
            FileProvider =
                new PhysicalFileProvider(
                    frontendPath)
        });
}

// ----------------------------------------------------
// 25. AUTHENTICATION + AUTHORIZATION
// ----------------------------------------------------
app.UseAuthentication();

app.UseAuthorization();

// ----------------------------------------------------
// 26. CONTROLLERS
// ----------------------------------------------------
app.MapControllers();

// ----------------------------------------------------
// 27. DEFAULT PAGE
// ----------------------------------------------------
app.MapGet(
    "/",
    () =>
        Results.Redirect(
            "/auth/login.html"));

// ----------------------------------------------------
// RUN
// ----------------------------------------------------
app.Run();
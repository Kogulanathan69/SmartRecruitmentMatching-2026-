using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NexHire.API.Extensions;
using NexHire.API.Middleware;
using NexHire.API.Services;

using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Mappings;
using NexHire.Application.Matching;
using NexHire.Application.Services;
using NexHire.Application.Validators;

using NexHire.Infrastructure.Data;
using NexHire.Infrastructure.Matching;
using NexHire.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1. CONTROLLERS
// ----------------------------------------------------
builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssemblyContaining<
    UpdateApplicationStatusRequestValidator>();

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
// NEXHIRE PRODUCTION CONFIGURATION GUARD
// ----------------------------------------------------
if (!builder.Environment.IsDevelopment())
{
    var jwtKey = builder.Configuration["Jwt:Key"];

    if (string.IsNullOrWhiteSpace(jwtKey) ||
        jwtKey.StartsWith(
            "CHANGE_ME",
            StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Production requires a secure Jwt:Key from environment variables or a secret store.");
    }

    if (connectionString.Contains(
        "(localdb)",
        StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException(
            "Production cannot use the LocalDB development connection string.");
    }
}

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
// 7. JOB APPLICATION REPOSITORY
// ----------------------------------------------------
builder.Services.AddScoped<
    IJobApplicationRepository,
    JobApplicationRepository>();

// ----------------------------------------------------
// 8. APPLICATION STATUS HISTORY
// ----------------------------------------------------
builder.Services.AddScoped<
    IApplicationStatusHistoryRepository,
    ApplicationStatusHistoryRepository>();

// ----------------------------------------------------
// 9. CONTACT REQUEST REPOSITORY
// ----------------------------------------------------
builder.Services.AddScoped<
    IContactRequestRepository,
    ContactRequestRepository>();

// ----------------------------------------------------
// 10. APPLICATION STATUS SERVICE
// ----------------------------------------------------
builder.Services.AddScoped<
    IApplicationStatusService,
    ApplicationStatusService>();

// ----------------------------------------------------
// 11. CONSENT CONTACT SERVICE
// ----------------------------------------------------
builder.Services.AddScoped<
    IConsentContactService,
    ConsentContactService>();

// ----------------------------------------------------
// 12. JOB SEEKER + RESUME MODULE
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

builder.Services.AddScoped<
    IResumeFileStorage,
    LocalResumeFileStorage>();

builder.Services.AddAutoMapper(
    cfg => { },
    typeof(JobSeekerMappingProfile).Assembly);

// ----------------------------------------------------
// 13. MATCHING ENGINE
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

// Matching repository
builder.Services.AddScoped<
    IMatchingRepository,
    MatchingRepository>();

// Matching service
builder.Services.AddScoped<
    IMatchingService,
    MatchingService>();

// ----------------------------------------------------
// 14. ADMIN MODULE
// ----------------------------------------------------
builder.Services.AddScoped<
    IAdminService,
    AdminService>();

// ----------------------------------------------------
// 15. COMPANY MODULE
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
// CORE JOB / APPLICATION MODULE
// ----------------------------------------------------
builder.Services.AddScoped<
    IJobRepository,
    JobRepository>();

builder.Services.AddScoped<
    IJobService,
    JobService>();

builder.Services.AddScoped<
    IApplicationRepository,
    ApplicationRepository>();

builder.Services.AddScoped<
    IApplicationService,
    ApplicationService>();

builder.Services.AddScoped<
    INotificationRepository,
    NotificationRepository>();

builder.Services.AddScoped<
    INotificationService,
    NotificationService>();

builder.Services.AddScoped<
    IDashboardRepository,
    DashboardRepository>();

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();
// ----------------------------------------------------
// 16. AUTHORIZATION
// ----------------------------------------------------
builder.Services.AddAuthorization();

// ----------------------------------------------------
// 17. CORS
// ----------------------------------------------------
var productionAllowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .GetChildren()
        .Select(item => item.Value)
        .Where(value => !string.IsNullOrWhiteSpace(value))
        .Cast<string>()
        .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();

                return;
            }

            if (productionAllowedOrigins.Length > 0)
            {
                policy
                    .WithOrigins(productionAllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();

                return;
            }

            // The production frontend is normally served from the
            // same origin as the API, so no cross-origin access is
            // granted unless Cors:AllowedOrigins is configured.
            policy
                .SetIsOriginAllowed(_ => false)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// ----------------------------------------------------
// BUILD APP
// ----------------------------------------------------

// ----------------------------------------------------
// NEXHIRE FINAL HARDENING SERVICES
// ----------------------------------------------------

// Standard safe error responses for unexpected failures.
builder.Services.AddProblemDetails();

// Protect authentication endpoints from excessive requests.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        "auth",
        httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey:
                    httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown",

                factory: _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
});
var app = builder.Build();

// ----------------------------------------------------
// 18. ERROR HANDLING + DEVELOPMENT SWAGGER
// ----------------------------------------------------
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "NexHire API v1");

        options.RoutePrefix = "swagger";
    });
}

// ----------------------------------------------------
// 19. HTTPS
// ----------------------------------------------------

// ----------------------------------------------------
// CORRELATION ID + SECURITY HEADERS
// ----------------------------------------------------
app.Use(async (context, next) =>
{
    var incomingCorrelationId =
        context.Request.Headers["X-Correlation-ID"]
            .FirstOrDefault();

    var correlationId =
        string.IsNullOrWhiteSpace(incomingCorrelationId)
            ? Guid.NewGuid().ToString("N")
            : incomingCorrelationId;

    context.Items["CorrelationId"] =
        correlationId;

    context.Response.Headers["X-Correlation-ID"] =
        correlationId;

    context.Response.Headers["X-Content-Type-Options"] =
        "nosniff";

    context.Response.Headers["X-Frame-Options"] =
        "DENY";

    context.Response.Headers["Referrer-Policy"] =
        "strict-origin-when-cross-origin";

    context.Response.Headers["Permissions-Policy"] =
        "camera=(), microphone=(), geolocation=()";

    await next();
});
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// ----------------------------------------------------
// 20. CORS
// ----------------------------------------------------
app.UseCors("AllowFrontend");

// ----------------------------------------------------
// 21. FRONTEND STATIC FILES
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
// 22. AUTHENTICATION + AUTHORIZATION
// ----------------------------------------------------
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------------------------
// 23. CONTROLLERS
// ----------------------------------------------------
app.MapControllers();
// ----------------------------------------------------
// HEALTH CHECK ENDPOINTS
// ----------------------------------------------------

app.MapGet(
    "/health/live",
    () =>
        Results.Ok(
            new
            {
                status = "live",
                service = "NexHire.API",
                utc = DateTime.UtcNow
            }))
    .AllowAnonymous();

app.MapGet(
    "/health/ready",
    async (
        AppDbContext db,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var connected =
                await db.Database.CanConnectAsync(
                    cancellationToken);

            if (!connected)
            {
                return Results.Json(
                    new
                    {
                        status = "not-ready",
                        database = "unreachable"
                    },
                    statusCode:
                        StatusCodes.Status503ServiceUnavailable);
            }

            return Results.Ok(
                new
                {
                    status = "ready",
                    database = "reachable"
                });
        }
        catch
        {
            return Results.Json(
                new
                {
                    status = "not-ready",
                    database = "unreachable"
                },
                statusCode:
                    StatusCodes.Status503ServiceUnavailable);
        }
    })
    .AllowAnonymous();


// ----------------------------------------------------
// 24. DEFAULT PAGE
// ----------------------------------------------------
app.MapGet(
    "/",
    () => Results.Redirect(
        "/auth/login.html"));

// ----------------------------------------------------
// RUN
// ----------------------------------------------------
app.Run();

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NexHire.API.Extensions;
using NexHire.API.Services;

using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Mappings;
using NexHire.Application.Services;
using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using NexHire.Infrastructure.Data;
using NexHire.Infrastructure.Repositories;

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
builder.Services.AddNexHireAuthentication(builder.Configuration);

// ----------------------------------------------------
// 5. HTTP CONTEXT
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
// 9. CONTACT REQUEST
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
//
// ----------------------------------------------------
// MATCHING ENGINE
// ----------------------------------------------------
//

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

builder.Services.AddAutoMapper(
    cfg => { },
    typeof(JobSeekerMappingProfile).Assembly);
// Eligibility rules.
builder.Services.AddScoped<
    IEligibilityEngine,
    EligibilityEngine>();

// Individual score calculators.
builder.Services.AddScoped<SkillMatchCalculator>();
builder.Services.AddScoped<ExperienceMatchCalculator>();
builder.Services.AddScoped<EducationMatchCalculator>();
builder.Services.AddScoped<CertificationMatchCalculator>();
builder.Services.AddScoped<LocationMatchCalculator>();
builder.Services.AddScoped<ProjectMatchCalculator>();
builder.Services.AddScoped<ProfileCompletionMatchCalculator>();

// Final weighted score calculator.
builder.Services.AddScoped<MatchScoreCalculator>();

// Recommendation, ranking and comparison.
builder.Services.AddScoped<
    IRecommendationEngine,
    RecommendationEngine>();

builder.Services.AddScoped<CandidateRankingEngine>();
builder.Services.AddScoped<CandidateComparisonEngine>();

// Main facade used by MatchingService/API.
builder.Services.AddScoped<
    IMatchingEngine,
    MatchingEngine>();

// Application-level matching orchestration service.
builder.Services.AddScoped<
    IMatchingService,
    MatchingService>();

// ----------------------------------------------------
// 13. ADMIN MODULE
// ----------------------------------------------------
builder.Services.AddScoped<
    IAdminService,
    AdminService>();

// ----------------------------------------------------
// 14. AUTHORIZATION
// ----------------------------------------------------
builder.Services.AddAuthorization();

// ----------------------------------------------------
// 15. CORS
// ----------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ----------------------------------------------------
// BUILD
// ----------------------------------------------------
var app = builder.Build();

// ----------------------------------------------------
// 16. SWAGGER
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
// 17. HTTPS
// ----------------------------------------------------
app.UseHttpsRedirection();

// ----------------------------------------------------
// 18. CORS
// ----------------------------------------------------
app.UseCors("AllowFrontend");

// ----------------------------------------------------
// 19. FRONTEND STATIC FILES
// ----------------------------------------------------
var frontendPath = Path.GetFullPath(
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
                new PhysicalFileProvider(frontendPath)
        });
}

// ----------------------------------------------------
// 20. AUTHENTICATION + AUTHORIZATION
// ----------------------------------------------------
app.UseAuthentication();
app.UseAuthorization();

// ----------------------------------------------------
// 21. CONTROLLERS
// ----------------------------------------------------
app.MapControllers();

// ----------------------------------------------------
// 22. DEFAULT PAGE
// ----------------------------------------------------
app.MapGet(
    "/",
    () => Results.Redirect("/auth/login.html"));

// ----------------------------------------------------
// RUN
// ----------------------------------------------------
app.Run();

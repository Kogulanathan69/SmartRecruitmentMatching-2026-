using Microsoft.EntityFrameworkCore;

using NexHire.API.Services;

using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Services;
using NexHire.Application.Matching;
using NexHire.Infrastructure.Matching;
using NexHire.Infrastructure.Data;
using NexHire.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

//
// ----------------------------------------------------
// 1. CONTROLLERS
// ----------------------------------------------------
//
builder.Services.AddControllers();


//
// ----------------------------------------------------
// 2. SWAGGER
// ----------------------------------------------------
//
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//
// ----------------------------------------------------
// 3. DATABASE
// ----------------------------------------------------
//
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});


//
// ----------------------------------------------------
// 4. HTTP CONTEXT ACCESSOR
// ----------------------------------------------------
//
builder.Services.AddHttpContextAccessor();


//
// ----------------------------------------------------
// 5. CURRENT USER SERVICE
// ----------------------------------------------------
//
builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();


//
// ----------------------------------------------------
// 6. JOB APPLICATION REPOSITORY
// ----------------------------------------------------
//
builder.Services.AddScoped<
    IJobApplicationRepository,
    JobApplicationRepository>();


//
// ----------------------------------------------------
// 7. APPLICATION STATUS HISTORY REPOSITORY
// ----------------------------------------------------
//
builder.Services.AddScoped<
    IApplicationStatusHistoryRepository,
    ApplicationStatusHistoryRepository>();


//
// ----------------------------------------------------
// 8. CONTACT REQUEST REPOSITORY
// ----------------------------------------------------
//
builder.Services.AddScoped<
    IContactRequestRepository,
    ContactRequestRepository>();


//
// ----------------------------------------------------
// 9. APPLICATION STATUS SERVICE
// ----------------------------------------------------
//
builder.Services.AddScoped<
    IApplicationStatusService,
    ApplicationStatusService>();


//
// ----------------------------------------------------
// 10. CONSENT CONTACT SERVICE
// ----------------------------------------------------
//
builder.Services.AddScoped<
    IConsentContactService,
    ConsentContactService>();
//
// ----------------------------------------------------
// MATCHING ENGINE
// ----------------------------------------------------
//

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

//
// ----------------------------------------------------
// 11. AUTHORIZATION
// ----------------------------------------------------
//
builder.Services.AddAuthorization();


//
// ----------------------------------------------------
// 12. CORS
// ----------------------------------------------------
//
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


//
// ----------------------------------------------------
// BUILD APPLICATION
// ----------------------------------------------------
//
var app = builder.Build();


//
// ----------------------------------------------------
// 13. SWAGGER MIDDLEWARE
// ----------------------------------------------------
//
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//
// ----------------------------------------------------
// 14. HTTPS REDIRECTION
// ----------------------------------------------------
//
app.UseHttpsRedirection();


//
// ----------------------------------------------------
// 15. CORS
// ----------------------------------------------------
//
app.UseCors("AllowFrontend");


//
// ----------------------------------------------------
// 16. AUTHORIZATION
// ----------------------------------------------------
//
app.UseAuthorization();


//
// ----------------------------------------------------
// 17. MAP CONTROLLERS
// ----------------------------------------------------
//
app.MapControllers();


//
// ----------------------------------------------------
// RUN APPLICATION
// ----------------------------------------------------
//
app.Run();
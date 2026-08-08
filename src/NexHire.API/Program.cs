using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NexHire.API.Extensions;

using NexHire.API.Services;

using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Services;

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
builder.Services.AddSwaggerGen(options => options.AddJwtSwagger());


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

builder.Services.AddNexHireAuthentication(builder.Configuration);


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

var frontendPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "frontend"));
if (Directory.Exists(frontendPath))
{
    app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(frontendPath) });
}


//
// ----------------------------------------------------
// 16. AUTHORIZATION
// ----------------------------------------------------
//
app.UseAuthentication();
app.UseAuthorization();


//
// ----------------------------------------------------
// 17. MAP CONTROLLERS
// ----------------------------------------------------
//
app.MapControllers();

app.MapGet("/", () => Results.Redirect("/auth/login.html"));


//
// ----------------------------------------------------
// RUN APPLICATION
// ----------------------------------------------------
//
app.Run();

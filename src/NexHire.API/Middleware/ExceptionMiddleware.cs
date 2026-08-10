using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common.Exceptions;

namespace NexHire.API.Middleware;

public sealed class ExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            await WriteProblemAsync(context, exception);
        }
    }

    private async Task WriteProblemAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException or KeyNotFoundException or FileNotFoundException =>
                (StatusCodes.Status404NotFound, "Resource not found"),
            ValidationException or ArgumentException =>
                (StatusCodes.Status400BadRequest, "Validation failed"),
            BusinessRuleException =>
                (StatusCodes.Status409Conflict, "Business rule conflict"),
            UnauthorizedException =>
                (StatusCodes.Status401Unauthorized, "Authentication required"),
            UnauthorizedAccessException =>
                (StatusCodes.Status403Forbidden, "Access forbidden"),
            _ =>
                (StatusCodes.Status500InternalServerError, "Unexpected server error")
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled API exception for {Method} {Path}",
                context.Request.Method,
                context.Request.Path);
        }
        else
        {
            _logger.LogInformation(
                "Handled API exception {ExceptionType} as HTTP {StatusCode} for {Method} {Path}",
                exception.GetType().Name,
                statusCode,
                context.Request.Method,
                context.Request.Path);
        }

        var correlationId =
            context.Items.TryGetValue("CorrelationId", out var value) &&
            value is not null
                ? value.ToString()!
                : context.TraceIdentifier;

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        context.Response.Headers["X-Correlation-ID"] = correlationId;
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] =
            "camera=(), microphone=(), geolocation=()";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode >= StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred. Use the trace identifier when contacting support."
                : exception.Message,
            Instance = context.Request.Path.Value ?? "/"
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;
        problem.Extensions["correlationId"] = correlationId;

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            problem,
            JsonOptions,
            cancellationToken: context.RequestAborted);
    }
}

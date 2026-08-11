using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NexHire.API.Middleware;
using NexHire.Application.Common.Exceptions;
using Xunit;

namespace NexHire.IntegrationTests;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task NotFoundException_Returns404ProblemDetails()
    {
        var result = await ExecuteAsync(
            new NotFoundException("Profile was not found."));

        Assert.Equal(StatusCodes.Status404NotFound, result.Context.Response.StatusCode);
        Assert.Equal("application/problem+json", result.Context.Response.ContentType);
        Assert.Equal(
            "test-correlation-id",
            result.Context.Response.Headers["X-Correlation-ID"].ToString());

        using var json = JsonDocument.Parse(result.Body);
        Assert.Equal(
            StatusCodes.Status404NotFound,
            json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(
            "Profile was not found.",
            json.RootElement.GetProperty("detail").GetString());
        Assert.Equal(
            "test-correlation-id",
            json.RootElement.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task BusinessRuleException_Returns409()
    {
        var result = await ExecuteAsync(
            new BusinessRuleException("Profile already exists."));

        Assert.Equal(StatusCodes.Status409Conflict, result.Context.Response.StatusCode);
    }

    [Fact]
    public async Task ValidationException_Returns400()
    {
        var result = await ExecuteAsync(
            new NexHire.Application.Common.Exceptions.ValidationException(
                "A required value is missing."));

        Assert.Equal(StatusCodes.Status400BadRequest, result.Context.Response.StatusCode);
    }

    [Fact]
    public async Task UnexpectedException_ReturnsSafe500WithoutLeakingMessage()
    {
        var result = await ExecuteAsync(
            new Exception("secret database password"));

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            result.Context.Response.StatusCode);
        Assert.DoesNotContain("secret database password", result.Body);
        Assert.Contains("unexpected error", result.Body.ToLowerInvariant());
    }

    private static async Task<MiddlewareResult> ExecuteAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/test";
        context.Items["CorrelationId"] = "test-correlation-id";
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionMiddleware(
            _ => Task.FromException(exception),
            NullLogger<ExceptionMiddleware>.Instance);

        await middleware.InvokeAsync(context);
        context.Response.Body.Position = 0;

        using var reader = new StreamReader(
            context.Response.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        return new MiddlewareResult(context, body);
    }

    private sealed record MiddlewareResult(
        DefaultHttpContext Context,
        string Body);
}

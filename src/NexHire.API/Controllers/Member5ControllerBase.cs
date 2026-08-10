using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;

namespace NexHire.API.Controllers;

public abstract class Member5ControllerBase : ControllerBase
{
    private readonly ILogger _logger;

    protected Member5ControllerBase(ILogger logger)
    {
        _logger = logger;
    }

    protected async Task<ActionResult<T>> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return Ok(await action());
        }
        catch (Member5ValidationException exception)
        {
            return ProblemResult(400, "Validation failed", exception);
        }
        catch (Member5ForbiddenException exception)
        {
            return ProblemResult(403, "Forbidden", exception);
        }
        catch (Member5NotFoundException exception)
        {
            return ProblemResult(404, "Not found", exception);
        }
        catch (Member5ConflictException exception)
        {
            return ProblemResult(409, "Conflict", exception);
        }
        catch (Member5DependencyException exception)
        {
            _logger.LogError(exception, "Member 5 dependency is not configured.");
            return ProblemResult(503, "Dependency unavailable", exception);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled Member 5 endpoint failure. TraceId: {TraceId}", HttpContext.TraceIdentifier);
            var problem = new ProblemDetails
            {
                Status = 500,
                Title = "Unexpected server error",
                Detail = "The request could not be completed.",
                Instance = HttpContext.Request.Path
            };
            problem.Extensions["code"] = "member5.unexpected_error";
            problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
            return StatusCode(500, problem);
        }
    }

    private ObjectResult ProblemResult(int status, string title, Member5Exception exception)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception.Message,
            Instance = HttpContext.Request.Path
        };
        problem.Extensions["code"] = exception.Code;
        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return StatusCode(status, problem);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.DTOs.Interview;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Authorize]
[Route("api/Interviews")]
public sealed class InterviewScoresController : Member5ControllerBase
{
    private readonly IInterviewService _service;

    public InterviewScoresController(IInterviewService service, ILogger<InterviewScoresController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpPost("{id:guid}/score")]
    public Task<ActionResult<InterviewScoreResponse>> RecordScore(
        Guid id,
        [FromBody] RecordInterviewScoreRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RecordScoreAsync(id, request, cancellationToken));
}

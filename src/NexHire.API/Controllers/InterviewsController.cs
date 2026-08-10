using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.DTOs.Interview;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Authorize]
[Route("api/Interviews")]
public sealed class InterviewsController : Member5ControllerBase
{
    private readonly IInterviewService _service;

    public InterviewsController(IInterviewService service, ILogger<InterviewsController> logger) : base(logger)
    {
        _service = service;
    }

    [HttpPost]
    public Task<ActionResult<InterviewResponse>> Schedule(
        [FromBody] ScheduleInterviewRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.ScheduleAsync(request, cancellationToken));

    [HttpGet("company")]
    public Task<ActionResult<PagedResponse<InterviewResponse>>> CompanyList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => _service.GetCompanyPageAsync(page, pageSize, cancellationToken));

    [HttpGet("candidate")]
    public Task<ActionResult<PagedResponse<InterviewResponse>>> CandidateList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ExecuteAsync(() => _service.GetCandidatePageAsync(page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    public Task<ActionResult<InterviewResponse>> Detail(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}/reschedule")]
    public Task<ActionResult<InterviewResponse>> Reschedule(
        Guid id,
        [FromBody] RescheduleInterviewRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.RescheduleAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public Task<ActionResult<InterviewResponse>> Cancel(
        Guid id,
        [FromBody] CancelInterviewRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CancelAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/complete")]
    public Task<ActionResult<InterviewResponse>> Complete(Guid id, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CompleteAsync(id, cancellationToken));
}

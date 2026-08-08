using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.DTOs.Resume;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/resumes")]
[Authorize(Roles = RoleNames.JobSeeker)]
public class ResumesController : ControllerBase
{
    private readonly IResumeService _service;
    private readonly ICurrentUserService _currentUser;

    public ResumesController(IResumeService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine() =>
        Ok(await _service.GetMyResumesAsync(_currentUser.UserId));

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates() =>
        Ok(await _service.GetTemplatesAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id) =>
        Ok(await _service.GetByIdAsync(_currentUser.UserId, id));

    [HttpPost]
    public async Task<IActionResult> Create(CreateResumeDto dto)
    {
        var result = await _service.CreateAsync(_currentUser.UserId, dto);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateResumeDto dto) =>
        Ok(await _service.UpdateAsync(_currentUser.UserId, id, dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(_currentUser.UserId, id);
        return NoContent();
    }

    [HttpGet("completeness")]
    public async Task<IActionResult> Completeness([FromQuery] Guid? resumeId) =>
        Ok(await _service.GetCompletenessAsync(_currentUser.UserId, resumeId));

    [HttpPost("{id:guid}/generate")]
    public async Task<IActionResult> Generate(Guid id) =>
        Content(await _service.GenerateHtmlAsync(_currentUser.UserId, id), "text/html");

    [HttpGet("{id:guid}/preview")]
    public async Task<IActionResult> Preview(Guid id) =>
        Content(await _service.GetPreviewHtmlAsync(_currentUser.UserId, id), "text/html");

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var html = await _service.GetPreviewHtmlAsync(_currentUser.UserId, id);
        return File(System.Text.Encoding.UTF8.GetBytes(html), "text/html", $"resume-{id}.html");
    }
}

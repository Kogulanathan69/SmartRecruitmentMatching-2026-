using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.DTOs.Resume;
using NexHire.Application.Interfaces.Services;
using NexHire.Application.Services;

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

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile? file,
        [FromForm] string? resumeName,
        [FromForm] bool isPrimary = false,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Select a PDF, DOC or DOCX CV file." });

        if (file.Length > ResumeFileValidator.MaxFileSizeBytes)
            return BadRequest(new { message = "CV file cannot exceed 5 MB." });

        await using var memory = new MemoryStream();
        await file.CopyToAsync(memory, cancellationToken);

        try
        {
            var result = await _service.UploadAsync(
                _currentUser.UserId,
                file.FileName,
                memory.ToArray(),
                resumeName,
                isPrimary,
                cancellationToken);

            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        catch (NexHire.Application.Common.Exceptions.BusinessRuleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}/file")]
    public async Task<IActionResult> DownloadUploaded(Guid id, CancellationToken cancellationToken)
    {
        var file = await _service.DownloadUploadedAsync(_currentUser.UserId, id, cancellationToken);
        return File(file.Content, file.ContentType, file.DownloadName);
    }

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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Common;
using NexHire.Application.DTOs.JobSeeker;
using NexHire.Application.Interfaces.Services;

namespace NexHire.API.Controllers;

[ApiController]
[Route("api/jobseekers")]
[Authorize]
public class JobSeekersController : ControllerBase
{
    private readonly IJobSeekerService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IMatchingService _matching;

    public JobSeekersController(
        IJobSeekerService service,
        ICurrentUserService currentUser,
        IMatchingService matching)
    {
        _service = service;
        _currentUser = currentUser;
        _matching = matching;
    }

    [HttpGet("me")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> GetMyProfile() =>
        Ok(await _service.GetMyProfileAsync(_currentUser.UserId));

    [HttpGet("{profileId:guid}")]
    public async Task<IActionResult> GetPublicProfile(Guid profileId) =>
        Ok(await _service.GetPublicProfileAsync(profileId));

    [HttpGet("me/matches/{jobId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> GetMyJobMatch(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _matching
                    .CalculateJobSeekerPreviewAsync(
                        jobId,
                        _currentUser.UserId,
                        cancellationToken);

            if (result is null)
            {
                return NotFound(
                    new
                    {
                        message =
                            "Published job or job seeker profile was not found."
                    });
            }

            return Ok(
                NexHire.Application.Mappings
                    .MatchingDtoMapper
                    .ToMatchScoreResponseDto(result));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new { message = exception.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("me")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> CreateMyProfile(CreateJobSeekerProfileDto dto)
    {
        var result = await _service.CreateProfileAsync(_currentUser.UserId, dto);
        return CreatedAtAction(nameof(GetMyProfile), routeValues: null, value: result);
    }

    [HttpPut("me")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> UpdateMyProfile(UpdateJobSeekerProfileDto dto) =>
        Ok(await _service.UpdateProfileAsync(_currentUser.UserId, dto));

    [HttpPost("me/education")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> AddEducation(AddEducationDto dto) =>
        Ok(await _service.AddEducationAsync(_currentUser.UserId, dto));

    [HttpPut("me/education/{educationId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> UpdateEducation(Guid educationId, AddEducationDto dto) =>
        Ok(await _service.UpdateEducationAsync(_currentUser.UserId, educationId, dto));

    [HttpDelete("me/education/{educationId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> DeleteEducation(Guid educationId)
    {
        await _service.DeleteEducationAsync(_currentUser.UserId, educationId);
        return NoContent();
    }

    [HttpPost("me/experience")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> AddExperience(AddExperienceDto dto) =>
        Ok(await _service.AddExperienceAsync(_currentUser.UserId, dto));

    [HttpPut("me/experience/{experienceId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> UpdateExperience(Guid experienceId, AddExperienceDto dto) =>
        Ok(await _service.UpdateExperienceAsync(_currentUser.UserId, experienceId, dto));

    [HttpDelete("me/experience/{experienceId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> DeleteExperience(Guid experienceId)
    {
        await _service.DeleteExperienceAsync(_currentUser.UserId, experienceId);
        return NoContent();
    }

    [HttpPost("me/skills")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> AddSkill(AddSkillDto dto) =>
        Ok(await _service.AddSkillAsync(_currentUser.UserId, dto));

    [HttpPut("me/skills/{candidateSkillId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> UpdateSkill(Guid candidateSkillId, AddSkillDto dto) =>
        Ok(await _service.UpdateSkillAsync(_currentUser.UserId, candidateSkillId, dto));

    [HttpDelete("me/skills/{candidateSkillId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> DeleteSkill(Guid candidateSkillId)
    {
        await _service.DeleteSkillAsync(_currentUser.UserId, candidateSkillId);
        return NoContent();
    }

    [HttpPost("me/projects")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> AddProject(AddProjectDto dto) =>
        Ok(await _service.AddProjectAsync(_currentUser.UserId, dto));

    [HttpPut("me/projects/{projectId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> UpdateProject(Guid projectId, AddProjectDto dto) =>
        Ok(await _service.UpdateProjectAsync(_currentUser.UserId, projectId, dto));

    [HttpDelete("me/projects/{projectId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> DeleteProject(Guid projectId)
    {
        await _service.DeleteProjectAsync(_currentUser.UserId, projectId);
        return NoContent();
    }

    [HttpPost("me/certifications")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> AddCertification(AddCertificationDto dto) =>
        Ok(await _service.AddCertificationAsync(_currentUser.UserId, dto));

    [HttpPut("me/certifications/{certificationId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> UpdateCertification(Guid certificationId, AddCertificationDto dto) =>
        Ok(await _service.UpdateCertificationAsync(_currentUser.UserId, certificationId, dto));

    [HttpDelete("me/certifications/{certificationId:guid}")]
    [Authorize(Roles = RoleNames.JobSeeker)]
    public async Task<IActionResult> DeleteCertification(Guid certificationId)
    {
        await _service.DeleteCertificationAsync(_currentUser.UserId, certificationId);
        return NoContent();
    }
}

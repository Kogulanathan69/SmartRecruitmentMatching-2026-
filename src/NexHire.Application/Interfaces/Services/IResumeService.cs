using NexHire.Application.DTOs.Resume;

namespace NexHire.Application.Interfaces.Services;

public interface IResumeService
{
    Task<IReadOnlyList<ResumeResponseDto>> GetMyResumesAsync(Guid userId);
    Task<ResumeResponseDto> GetByIdAsync(Guid userId, Guid resumeId);
    Task<ResumeResponseDto> CreateAsync(Guid userId, CreateResumeDto dto);
    Task<ResumeResponseDto> UpdateAsync(Guid userId, Guid resumeId, UpdateResumeDto dto);
    Task DeleteAsync(Guid userId, Guid resumeId);
    Task<ResumeCompletenessDto> GetCompletenessAsync(Guid userId, Guid? resumeId = null);
    Task<string> GenerateHtmlAsync(Guid userId, Guid resumeId);
    Task<string> GetPreviewHtmlAsync(Guid userId, Guid resumeId);
    Task<IReadOnlyList<ResumeTemplateResponseDto>> GetTemplatesAsync();
}

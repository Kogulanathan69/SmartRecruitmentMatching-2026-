using NexHire.Application.DTOs.Company;

namespace NexHire.Application.Interfaces.Services;

public interface ICompanyService
{
    Task<CompanyResponseDto> CreateCompanyAsync(Guid userId, CreateCompanyDto dto);
    Task<CompanyResponseDto?> GetByIdAsync(Guid companyId);
    Task<IReadOnlyList<CompanyResponseDto>> GetByOwnerAsync(Guid userId);
    Task<CompanyResponseDto> UpdateCompanyAsync(Guid companyId, Guid userId, UpdateCompanyDto dto);
    Task<CompanyDocumentResponseDto> UploadDocumentAsync(Guid companyId, Guid userId, UploadCompanyDocumentDto dto);
    Task<CompanyVerificationStatusDto> SubmitVerificationAsync(Guid companyId, Guid userId, SubmitCompanyVerificationDto dto);
    Task<CompanyVerificationStatusDto> GetVerificationStatusAsync(Guid companyId, Guid userId, bool isAdmin = false);
    Task<CompanyResponseDto> VerifyCompanyAsync(Guid companyId, Guid adminUserId, VerifyCompanyDto dto);
    Task MarkEmailVerifiedAsync(Guid companyId, Guid userId);
    Task MarkPhoneVerifiedAsync(Guid companyId, Guid userId);
}

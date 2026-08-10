using AutoMapper;
using NexHire.Application.Common.Exceptions;
using NexHire.Application.DTOs.Company;
using NexHire.Application.Interfaces.Repositories;
using NexHire.Application.Interfaces.Services;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;


namespace NexHire.Application.Services;

public class CompanyService : ICompanyService
{
    private static readonly HashSet<string> AllowedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "BusinessRegistration", "TaxCertificate", "AddressProof", "HRIdentity"
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAuditLogRepository _audit;
    private readonly INotificationWriter _notifications;

    public CompanyService(IUnitOfWork unitOfWork, IMapper mapper, IAuditLogRepository audit, INotificationWriter notifications)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _audit = audit;
        _notifications = notifications;
    }

    public async Task<CompanyResponseDto> CreateCompanyAsync(Guid userId, CreateCompanyDto dto)
    {
        var registration = dto.RegistrationNumber.Trim().ToUpperInvariant();
        var email = dto.OfficialEmail.Trim().ToLowerInvariant();

        if (await _unitOfWork.Companies.GetByRegistrationNumberAsync(registration) is not null)
            throw new BusinessRuleException("This business registration number is already registered.");
        if (await _unitOfWork.Companies.GetByOfficialEmailAsync(email) is not null)
            throw new BusinessRuleException("This official company email is already registered.");

        var company = _mapper.Map<Company>(dto);
        company.Id = Guid.NewGuid();
        company.CreatedByUserId = userId;
        company.RegistrationNumber = registration;
        company.OfficialEmail = email;
        company.Status = CompanyStatus.Draft;
        company.CreatedAt = DateTime.UtcNow;
        company.IsDomainMatched = IsWebsiteEmailDomainMatched(company.Website, company.OfficialEmail);
        RecalculateTrust(company);

        await _unitOfWork.Companies.AddAsync(company);
        await _unitOfWork.SaveChangesAsync();
        return MapCompany(company);
    }

    public async Task<CompanyResponseDto?> GetByIdAsync(Guid companyId)
    {
        var company = await _unitOfWork.Companies.GetByIdWithDetailsAsync(companyId);
        return company is null ? null : MapCompany(company);
    }

    public async Task<IReadOnlyList<CompanyResponseDto>> GetByOwnerAsync(Guid userId)
    {
        var companies = await _unitOfWork.Companies.GetByOwnerUserIdAsync(userId);
        return companies.Select(MapCompany).ToList();
    }

    public async Task<IReadOnlyList<CompanyResponseDto>> GetPendingVerificationAsync()
    {
        return await GetPendingAsync();
    }

    public async Task<CompanyResponseDto> UpdateCompanyAsync(
    Guid companyId,
    Guid userId,
    UpdateCompanyDto dto)
    {
        var company =
            await GetOwnedCompanyAsync(
                companyId,
                userId,
                includeDetails: true);

        if (company.Status is
            CompanyStatus.Active or
            CompanyStatus.Suspended)
        {
            throw new BusinessRuleException(
                "An active or suspended company cannot change legal verification fields. Contact an administrator.");
        }

        var verificationDataChanged = false;

        // -------------------------------------------------
        // NAME
        // -------------------------------------------------

        if (dto.Name is not null)
        {
            dto.Name = dto.Name.Trim();
        }

        // -------------------------------------------------
        // LEGAL NAME
        // -------------------------------------------------

        if (dto.LegalName is not null)
        {
            var legalName = dto.LegalName.Trim();

            if (!string.Equals(
                    legalName,
                    company.LegalName,
                    StringComparison.Ordinal))
            {
                verificationDataChanged = true;
            }

            dto.LegalName = legalName;
        }

        // -------------------------------------------------
        // OFFICIAL EMAIL
        // -------------------------------------------------

        if (dto.OfficialEmail is not null)
        {
            var email =
                dto.OfficialEmail
                    .Trim()
                    .ToLowerInvariant();

            var duplicate =
                await _unitOfWork.Companies
                    .GetByOfficialEmailAsync(email);

            if (duplicate is not null &&
                duplicate.Id != company.Id)
            {
                throw new BusinessRuleException(
                    "This official company email is already registered.");
            }

            if (!string.Equals(
                    email,
                    company.OfficialEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                company.IsEmailVerified = false;
                verificationDataChanged = true;
            }

            dto.OfficialEmail = email;
        }

        // -------------------------------------------------
        // PHONE
        // -------------------------------------------------

        if (dto.PhoneNumber is not null)
        {
            var phone = dto.PhoneNumber.Trim();

            if (!string.Equals(
                    phone,
                    company.PhoneNumber,
                    StringComparison.Ordinal))
            {
                company.IsPhoneVerified = false;
                verificationDataChanged = true;
            }

            dto.PhoneNumber = phone;
        }

        // -------------------------------------------------
        // REGISTERED ADDRESS
        // -------------------------------------------------

        if (dto.RegisteredAddress is not null)
        {
            var address =
                dto.RegisteredAddress.Trim();

            if (!string.Equals(
                    address,
                    company.RegisteredAddress,
                    StringComparison.Ordinal))
            {
                company.IsAddressVerified = false;
                verificationDataChanged = true;
            }

            dto.RegisteredAddress = address;
        }

        // -------------------------------------------------
        // OTHER OPTIONAL VALUES
        // -------------------------------------------------

        if (dto.Website is not null)
            dto.Website = dto.Website.Trim();

        if (dto.Industry is not null)
            dto.Industry = dto.Industry.Trim();

        if (dto.CompanySize is not null)
            dto.CompanySize = dto.CompanySize.Trim();

        if (dto.City is not null)
            dto.City = dto.City.Trim();

        if (dto.Country is not null)
            dto.Country = dto.Country.Trim();

        if (dto.LogoUrl is not null)
            dto.LogoUrl = dto.LogoUrl.Trim();

        // -------------------------------------------------
        // MAP
        // -------------------------------------------------

        _mapper.Map(dto, company);

        // Important verification data changed:
        // old verification should no longer remain valid.
        if (verificationDataChanged)
        {
            company.Status = CompanyStatus.Draft;
            company.Verification = null;
        }

        company.UpdatedAt = DateTime.UtcNow;

        company.IsDomainMatched =
            IsWebsiteEmailDomainMatched(
                company.Website,
                company.OfficialEmail);

        RecalculateTrust(company);

        _unitOfWork.Companies.Update(company);

        await _unitOfWork.SaveChangesAsync();

        return MapCompany(company);
    }

    public async Task<CompanyDocumentResponseDto> UploadDocumentAsync(Guid companyId, Guid userId, UploadCompanyDocumentDto dto)
    {
        var company = await GetOwnedCompanyAsync(companyId, userId, includeDetails: true);
        if (company.Status is CompanyStatus.Active or CompanyStatus.Suspended)
            throw new BusinessRuleException("Documents cannot be changed while the company is active or suspended.");
        if (!AllowedDocumentTypes.Contains(dto.DocumentType))
            throw new ValidationException($"Document type must be one of: {string.Join(", ", AllowedDocumentTypes)}.");
        if (string.IsNullOrWhiteSpace(dto.FileUrl) || string.IsNullOrWhiteSpace(dto.FileName))
            throw new ValidationException("File name and URL are required.");
        if (dto.FileSizeBytes <= 0 || dto.FileSizeBytes > 5 * 1024 * 1024)
            throw new ValidationException("Document size must be between 1 byte and 5 MB.");
        if (!IsAllowedFile(dto.FileName, dto.ContentType))
            throw new ValidationException("Only PDF, JPG and PNG documents are allowed.");

        var existing = company.Documents.FirstOrDefault(d => d.DocumentType.Equals(dto.DocumentType, StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            existing.FileName = dto.FileName.Trim();
            existing.FileUrl = dto.FileUrl.Trim();
            existing.ContentType = dto.ContentType;
            existing.FileSizeBytes = dto.FileSizeBytes;
            existing.UploadedAt = DateTime.UtcNow;
            existing.IsReviewed = false;
            existing.IsAccepted = false;
            existing.ReviewNotes = null;
        }
        else
        {
            existing = new CompanyDocument
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                DocumentType = dto.DocumentType.Trim(),
                FileName = dto.FileName.Trim(),
                FileUrl = dto.FileUrl.Trim(),
                ContentType = dto.ContentType,
                FileSizeBytes = dto.FileSizeBytes,
                UploadedAt = DateTime.UtcNow
            };
            company.Documents.Add(existing);
        }

        company.Status = CompanyStatus.Draft;
        company.Verification = null;
        company.UpdatedAt = DateTime.UtcNow;
        RecalculateTrust(company);
        await _unitOfWork.SaveChangesAsync();
        return MapDocument(existing);
    }

    public async Task<CompanyVerificationStatusDto> SubmitVerificationAsync(Guid companyId, Guid userId, SubmitCompanyVerificationDto dto)
    {
        var company = await GetOwnedCompanyAsync(companyId, userId, includeDetails: true);
        if (!dto.InformationIsAccurate)
            throw new ValidationException("The accuracy declaration must be accepted.");
        if (string.IsNullOrWhiteSpace(dto.DeclarationName) || string.IsNullOrWhiteSpace(dto.DeclarationDesignation))
            throw new ValidationException("Declaration name and designation are required.");

        var missing = GetMissingRequirements(company);
        if (missing.Count > 0)
            throw new BusinessRuleException($"Verification cannot be submitted. Missing: {string.Join(", ", missing)}.");

        company.Verification ??= new CompanyVerification { Id = Guid.NewGuid(), CompanyId = company.Id };
        company.Verification.Status = VerificationStatus.Pending;
        company.Verification.DeclarationName = dto.DeclarationName.Trim();
        company.Verification.DeclarationDesignation = dto.DeclarationDesignation.Trim();
        company.Verification.SubmittedAt = DateTime.UtcNow;
        company.Verification.VerifiedAt = null;
        company.Verification.VerifiedByUserId = null;
        company.Verification.Remarks = null;
        company.Status = CompanyStatus.Pending;
        company.UpdatedAt = DateTime.UtcNow;
        RecalculateTrust(company);
        await _audit.AddAsync(new AuditLog { ActorUserId = userId, Action = "CompanyVerificationSubmitted", EntityType = "Company", EntityId = company.Id });
        await _unitOfWork.SaveChangesAsync();
        return MapStatus(company);
    }

    public async Task<CompanyVerificationStatusDto> GetVerificationStatusAsync(Guid companyId, Guid userId, bool isAdmin = false)
    {
        var company = await _unitOfWork.Companies.GetByIdWithDetailsAsync(companyId)
            ?? throw new NotFoundException("Company not found.");
        if (!isAdmin && company.CreatedByUserId != userId)
            throw new UnauthorizedException("You do not have permission to view this verification.");
        return MapStatus(company);
    }

    public async Task MarkEmailVerifiedAsync(Guid companyId, Guid userId)
    {
        var company = await GetOwnedCompanyAsync(companyId, userId, includeDetails: true);
        company.IsEmailVerified = true;
        company.IsDomainMatched = IsWebsiteEmailDomainMatched(company.Website, company.OfficialEmail);
        company.UpdatedAt = DateTime.UtcNow;
        RecalculateTrust(company);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkPhoneVerifiedAsync(Guid companyId, Guid userId)
    {
        var company = await GetOwnedCompanyAsync(companyId, userId, includeDetails: true);
        company.IsPhoneVerified = true;
        company.UpdatedAt = DateTime.UtcNow;
        RecalculateTrust(company);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<CompanyResponseDto> VerifyCompanyAsync(Guid companyId, Guid adminUserId, VerifyCompanyDto dto)
    {
        var company = await _unitOfWork.Companies.GetByIdWithDetailsAsync(companyId)
            ?? throw new NotFoundException("Company not found.");
        if (!Enum.TryParse<VerificationStatus>(dto.Status, true, out var status))
            throw new ValidationException("Status must be Verified, Rejected, MoreInformationRequired, or Suspended.");
        if (company.Verification?.SubmittedAt is null && status == VerificationStatus.Verified)
            throw new BusinessRuleException("The company must submit a verification request first.");

        var registrationDocument = company.Documents.FirstOrDefault(d => d.DocumentType.Equals("BusinessRegistration", StringComparison.OrdinalIgnoreCase));
        if (status == VerificationStatus.Verified && registrationDocument is null)
            throw new BusinessRuleException("A business registration document is required.");
        if (status == VerificationStatus.Verified && !dto.RegistrationDocumentVerified)
            throw new BusinessRuleException("The registration document must be verified before approval.");

        company.Verification ??= new CompanyVerification { Id = Guid.NewGuid(), CompanyId = companyId };
        company.Verification.Status = status;
        company.Verification.RegistrationDocumentVerified = dto.RegistrationDocumentVerified;
        company.Verification.VerifiedByUserId = adminUserId;
        company.Verification.VerifiedAt = DateTime.UtcNow;
        company.Verification.Remarks = dto.Remarks;

        if (registrationDocument is not null)
        {
            registrationDocument.IsReviewed = true;
            registrationDocument.IsAccepted = dto.RegistrationDocumentVerified;
            registrationDocument.ReviewNotes = dto.Remarks;
        }

        company.IsEmailVerified = dto.OfficialEmailVerified;
        company.IsPhoneVerified = dto.PhoneVerified;
        company.IsDomainMatched = dto.WebsiteDomainMatched || IsWebsiteEmailDomainMatched(company.Website, company.OfficialEmail);
        company.IsAddressVerified = dto.RegisteredAddressVerified;
        company.Status = status switch
        {
            VerificationStatus.Verified => CompanyStatus.Active,
            VerificationStatus.MoreInformationRequired => CompanyStatus.MoreInformationRequired,
            VerificationStatus.Rejected => CompanyStatus.Rejected,
            VerificationStatus.Suspended => CompanyStatus.Suspended,
            _ => CompanyStatus.Pending
        };
        company.UpdatedAt = DateTime.UtcNow;
        RecalculateTrust(company);
        _unitOfWork.Companies.Update(company);
        await _audit.AddAsync(new AuditLog { ActorUserId = adminUserId, Action = "CompanyVerificationDecision", EntityType = "Company", EntityId = company.Id, Details = status.ToString() });
        await _notifications.QueueAsync(company.CreatedByUserId, "CompanyVerification", "Company verification updated", $"Your company verification status is {status}.", "Company", company.Id);
        await _unitOfWork.SaveChangesAsync();
        return MapCompany(company);
        
        if (status == VerificationStatus.Verified &&
            !dto.OfficialEmailVerified)
        {
            throw new BusinessRuleException(
                "The official company email must be verified before approval.");
        }

        if (status == VerificationStatus.Verified &&
            !dto.PhoneVerified)
        {
            throw new BusinessRuleException(
                "The company phone number must be verified before approval.");
        }
    }

    private async Task<Company> GetOwnedCompanyAsync(Guid companyId, Guid userId, bool includeDetails)
    {
        var company = includeDetails
            ? await _unitOfWork.Companies.GetByIdWithDetailsAsync(companyId)
            : await _unitOfWork.Companies.GetByIdAsync(companyId);
        if (company is null) throw new NotFoundException("Company not found.");
        if (company.CreatedByUserId != userId)
            throw new UnauthorizedException("You do not have permission to manage this company.");
        return company;
    }

    private static List<string> GetMissingRequirements(Company company)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(company.LegalName)) missing.Add("legal company name");
        if (string.IsNullOrWhiteSpace(company.RegistrationNumber)) missing.Add("registration number");
        if (string.IsNullOrWhiteSpace(company.OfficialEmail)) missing.Add("official email");
        if (string.IsNullOrWhiteSpace(company.PhoneNumber)) missing.Add("phone number");
        if (string.IsNullOrWhiteSpace(company.RegisteredAddress)) missing.Add("registered address");
        if (!company.IsEmailVerified) missing.Add("email verification");
        if (!company.IsPhoneVerified) missing.Add("phone verification");
        if (!company.Documents.Any(d => d.DocumentType.Equals("BusinessRegistration", StringComparison.OrdinalIgnoreCase)))
            missing.Add("business registration document");
        return missing;
    }

    private static bool IsAllowedFile(string fileName, string? contentType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var extensionAllowed = extension is ".pdf" or ".jpg" or ".jpeg" or ".png";
        var typeAllowed = string.IsNullOrWhiteSpace(contentType) || contentType.ToLowerInvariant() is
            "application/pdf" or "image/jpeg" or "image/png";
        return extensionAllowed && typeAllowed;
    }

    private static bool IsWebsiteEmailDomainMatched(string? website, string? email)
    {
        if (string.IsNullOrWhiteSpace(website) || string.IsNullOrWhiteSpace(email) || !email.Contains('@')) return false;
        var normalized = website.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? website : $"https://{website}";
        if (!Uri.TryCreate(normalized, UriKind.Absolute, out var uri)) return false;
        var webDomain = uri.Host.Replace("www.", "", StringComparison.OrdinalIgnoreCase);
        var emailDomain = email[(email.IndexOf('@') + 1)..].Trim();
        return string.Equals(webDomain, emailDomain, StringComparison.OrdinalIgnoreCase);
    }

    private static void RecalculateTrust(Company company)
    {
        var score = 0;
        if (company.Verification?.RegistrationDocumentVerified == true) score += 30;
        if (company.IsEmailVerified) score += 15;
        if (company.IsPhoneVerified) score += 10;
        if (company.IsDomainMatched) score += 15;
        if (company.IsAddressVerified) score += 10;
        if (!string.IsNullOrWhiteSpace(company.Description) && !string.IsNullOrWhiteSpace(company.Industry)) score += 5;
        if (!string.IsNullOrWhiteSpace(company.Website) && !string.IsNullOrWhiteSpace(company.LogoUrl)) score += 5;
        score += 10; // MVP assumes no confirmed active complaints; complaint penalty will replace this in a later sprint.
        company.TrustScore = Math.Clamp(score, 0, 100);
        company.TrustLevel = score switch
        {
            >= 85 => "High Trust",
            >= 65 => "Medium Trust",
            >= 40 => "Review Required",
            _ => "High Risk"
        };
    }

    private static CompanyResponseDto MapCompany(Company c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        LegalName = c.LegalName,
        RegistrationNumber = c.RegistrationNumber,
        OfficialEmail = c.OfficialEmail,
        PhoneNumber = c.PhoneNumber,
        RegisteredAddress = c.RegisteredAddress,
        Description = c.Description,
        Website = c.Website,
        Industry = c.Industry,
        CompanySize = c.CompanySize,
        LogoUrl = c.LogoUrl,
        City = c.City,
        Country = c.Country,
        Status = c.Status.ToString(),
        TrustScore = c.TrustScore,
        TrustLevel = c.TrustLevel,
        IsEmailVerified = c.IsEmailVerified,
        IsPhoneVerified = c.IsPhoneVerified,
        IsDomainMatched = c.IsDomainMatched,
        IsAddressVerified = c.IsAddressVerified,
        VerificationStatus = c.Verification?.Status.ToString() ?? "NotSubmitted",
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
        Documents = c.Documents.Select(MapDocument).ToList()
    };

    private static CompanyDocumentResponseDto MapDocument(CompanyDocument d) => new()
    {
        Id = d.Id,
        DocumentType = d.DocumentType,
        FileName = d.FileName,
        FileUrl = d.FileUrl,
        ContentType = d.ContentType,
        FileSizeBytes = d.FileSizeBytes,
        IsReviewed = d.IsReviewed,
        IsAccepted = d.IsAccepted,
        ReviewNotes = d.ReviewNotes,
        UploadedAt = d.UploadedAt
    };

    private static CompanyVerificationStatusDto MapStatus(Company c)
    {
        var missing = GetMissingRequirements(c);
        return new CompanyVerificationStatusDto
        {
            CompanyId = c.Id,
            CompanyStatus = c.Status.ToString(),
            VerificationStatus = c.Verification?.Status.ToString() ?? "NotSubmitted",
            TrustScore = c.TrustScore,
            TrustLevel = c.TrustLevel,
            RegistrationDocumentUploaded = c.Documents.Any(d => d.DocumentType.Equals("BusinessRegistration", StringComparison.OrdinalIgnoreCase)),
            RegistrationDocumentVerified = c.Verification?.RegistrationDocumentVerified == true,
            OfficialEmailVerified = c.IsEmailVerified,
            PhoneVerified = c.IsPhoneVerified,
            WebsiteDomainMatched = c.IsDomainMatched,
            RegisteredAddressVerified = c.IsAddressVerified,
            SubmittedAt = c.Verification?.SubmittedAt,
            ReviewedAt = c.Verification?.VerifiedAt,
            Remarks = c.Verification?.Remarks,
            MissingRequirements = missing
        };
    }
    public async Task<IReadOnlyList<CompanyResponseDto>> GetPendingAsync()
    {
        var companies =
            await _unitOfWork.Companies.GetByStatusAsync(
                CompanyStatus.Pending);

        return companies
            .Select(MapCompany)
            .ToList();
    }
}

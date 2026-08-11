using FluentValidation;
using NexHire.Application.DTOs.JobSeeker;

namespace NexHire.Application.Validators.JobSeeker;

public class AddCertificationValidator : AbstractValidator<AddCertificationDto>
{
    public AddCertificationValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IssuingOrganization).MaximumLength(200);
        RuleFor(x => x.CredentialUrl).MaximumLength(500);
        RuleFor(x => x)
            .Must(x => !x.IssueDate.HasValue || !x.ExpiryDate.HasValue || x.ExpiryDate.Value.Date >= x.IssueDate.Value.Date)
            .WithMessage("ExpiryDate cannot be earlier than IssueDate.");
    }
}

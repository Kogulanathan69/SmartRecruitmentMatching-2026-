using FluentValidation;
using NexHire.Application.DTOs.Application;

namespace NexHire.Application.Validators;

public class ApplyJobValidator : AbstractValidator<ApplyJobDto>
{
    public ApplyJobValidator()
    {
        // Add validation rules only for properties
        // that actually exist in ApplyJobDto.
    }
}
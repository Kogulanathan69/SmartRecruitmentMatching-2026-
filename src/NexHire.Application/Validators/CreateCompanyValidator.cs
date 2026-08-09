using FluentValidation;
using NexHire.Application.DTOs.Company;
namespace NexHire.Application.Validators;
public class CreateCompanyValidator : AbstractValidator<CreateCompanyDto> { public CreateCompanyValidator() { RuleFor(x => x.Name).NotEmpty().MaximumLength(200); RuleFor(x => x.RegistrationNumber).NotEmpty().MaximumLength(100); RuleFor(x => x.OfficialEmail).NotEmpty().EmailAddress().MaximumLength(250); RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(30); RuleFor(x => x.RegisteredAddress).NotEmpty().MaximumLength(500); } }

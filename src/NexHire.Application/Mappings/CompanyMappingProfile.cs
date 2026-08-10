using AutoMapper;
using NexHire.Application.DTOs.Company;
using NexHire.Domain.Entities;

namespace NexHire.Application.Mappings;

public sealed class CompanyMappingProfile : Profile
{
    public CompanyMappingProfile()
    {
        CreateMap<CreateCompanyDto, Company>(MemberList.Source);

        CreateMap<UpdateCompanyDto, Company>(MemberList.Source)
            .ForAllMembers(options =>
                options.Condition(
                    (source, destination, sourceMember) =>
                        sourceMember is not null));
    }
}
using AutoMapper;
using NexHire.Application.DTOs.Company;
using NexHire.Domain.Entities;

namespace NexHire.Application.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateCompanyDto, Company>();

        CreateMap<UpdateCompanyDto, Company>()
            .ForAllMembers(options =>
                options.Condition(
                    (source, destination, sourceMember) =>
                        sourceMember is not null));
    }
}

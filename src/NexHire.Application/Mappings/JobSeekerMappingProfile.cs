using AutoMapper;
using NexHire.Application.DTOs.JobSeeker;
using NexHire.Domain.Entities;

namespace NexHire.Application.Mappings;

public class JobSeekerMappingProfile : Profile
{
    public JobSeekerMappingProfile()
    {
        CreateMap<CreateJobSeekerProfileDto, JobSeekerProfile>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Educations, o => o.Ignore())
            .ForMember(d => d.Experiences, o => o.Ignore())
            .ForMember(d => d.CandidateSkills, o => o.Ignore())
            .ForMember(d => d.Projects, o => o.Ignore())
            .ForMember(d => d.Certifications, o => o.Ignore())
            .ForMember(d => d.Resumes, o => o.Ignore())
            .ForMember(d => d.Applications, o => o.Ignore())
            .ForMember(d => d.TalentPoolEntries, o => o.Ignore());

        CreateMap<UpdateJobSeekerProfileDto, JobSeekerProfile>()
            .ForAllMembers(o => o.Condition((src, dest, value) => value is not null));

        CreateMap<AddEducationDto, Education>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.JobSeekerProfileId, o => o.Ignore())
            .ForMember(d => d.JobSeekerProfile, o => o.Ignore());

        CreateMap<AddExperienceDto, Experience>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.JobSeekerProfileId, o => o.Ignore())
            .ForMember(d => d.JobSeekerProfile, o => o.Ignore());

        CreateMap<AddProjectDto, Project>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.JobSeekerProfileId, o => o.Ignore())
            .ForMember(d => d.JobSeekerProfile, o => o.Ignore());

        CreateMap<AddCertificationDto, Certification>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.JobSeekerProfileId, o => o.Ignore())
            .ForMember(d => d.JobSeekerProfile, o => o.Ignore());

        CreateMap<Education, EducationResponseDto>();
        CreateMap<Experience, ExperienceResponseDto>();
        CreateMap<Project, ProjectResponseDto>();
        CreateMap<Certification, CertificationResponseDto>();

        CreateMap<CandidateSkill, CandidateSkillResponseDto>()
            .ForMember(d => d.SkillName, o => o.MapFrom(s => s.Skill.Name))
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Skill.Category));

        CreateMap<JobSeekerProfile, JobSeekerProfileResponseDto>()
            .ForMember(d => d.Skills, o => o.MapFrom(s => s.CandidateSkills));

        CreateMap<JobSeekerProfile, PublicJobSeekerProfileResponseDto>()
            .ForMember(d => d.Skills, o => o.MapFrom(s => s.CandidateSkills));
    }
}

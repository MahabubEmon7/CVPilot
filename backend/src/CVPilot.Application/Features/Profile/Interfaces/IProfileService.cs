using CVPilot.Application.Common.Models;
using CVPilot.Application.Features.Profile.DTOs;

namespace CVPilot.Application.Features.Profile.Interfaces;

public interface IProfileService
{
    Task<Result<UserProfileDto>> GetProfileByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Result<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request, CancellationToken ct = default);

    Task<Result<WorkExperienceDto>> AddWorkExperienceAsync(Guid userId, CreateWorkExperienceDto request, CancellationToken ct = default);
    Task<Result<WorkExperienceDto>> UpdateWorkExperienceAsync(Guid userId, Guid experienceId, CreateWorkExperienceDto request, CancellationToken ct = default);
    Task<Result> DeleteWorkExperienceAsync(Guid userId, Guid experienceId, CancellationToken ct = default);

    Task<Result<EducationDto>> AddEducationAsync(Guid userId, CreateEducationDto request, CancellationToken ct = default);
    Task<Result<EducationDto>> UpdateEducationAsync(Guid userId, Guid educationId, CreateEducationDto request, CancellationToken ct = default);
    Task<Result> DeleteEducationAsync(Guid userId, Guid educationId, CancellationToken ct = default);

    Task<Result<UserSkillDto>> AddSkillAsync(Guid userId, CreateUserSkillDto request, CancellationToken ct = default);
    Task<Result> DeleteSkillAsync(Guid userId, Guid skillId, CancellationToken ct = default);

    Task<Result<ProjectDto>> AddProjectAsync(Guid userId, CreateProjectDto request, CancellationToken ct = default);
    Task<Result<ProjectDto>> UpdateProjectAsync(Guid userId, Guid projectId, CreateProjectDto request, CancellationToken ct = default);
    Task<Result> DeleteProjectAsync(Guid userId, Guid projectId, CancellationToken ct = default);
}


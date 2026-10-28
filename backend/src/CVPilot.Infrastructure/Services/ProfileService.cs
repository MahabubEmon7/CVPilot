using System.Text.Json;
using CVPilot.Application.Common.Interfaces;
using CVPilot.Application.Common.Models;
using CVPilot.Application.Features.Profile.DTOs;
using CVPilot.Application.Features.Profile.Interfaces;
using CVPilot.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CVPilot.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly IAppDbContext _dbContext;

    public ProfileService(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserProfileDto>> GetProfileByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var profile = await _dbContext.UserProfiles
            .Include(p => p.User)
            .Include(p => p.Experiences.OrderBy(e => e.DisplayOrder).ThenByDescending(e => e.StartDate))
            .Include(p => p.Educations.OrderBy(e => e.DisplayOrder).ThenByDescending(e => e.StartDate))
            .Include(p => p.Skills.OrderBy(s => s.Category).ThenBy(s => s.Name))
            .Include(p => p.Projects.OrderBy(pr => pr.DisplayOrder))
            .Include(p => p.Certifications)
            .Include(p => p.Languages)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null)
        {
            // Auto-create profile if missing
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user == null)
            {
                return Result<UserProfileDto>.Failure("User not found.");
            }

            profile = new UserProfile
            {
                UserId = userId,
                User = user,
                Headline = "Software Professional",
                LocationCountry = "Bangladesh"
            };
            _dbContext.UserProfiles.Add(profile);
            await _dbContext.SaveChangesAsync(ct);
        }

        var dto = MapToDto(profile);
        return Result<UserProfileDto>.Success(dto);
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request, CancellationToken ct = default)
    {
        var profile = await _dbContext.UserProfiles
            .Include(p => p.User)
            .Include(p => p.Experiences)
            .Include(p => p.Educations)
            .Include(p => p.Skills)
            .Include(p => p.Projects)
            .Include(p => p.Certifications)
            .Include(p => p.Languages)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null)
        {
            return Result<UserProfileDto>.Failure("Profile not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName) && profile.User != null)
        {
            profile.User.FullName = request.FullName.Trim();
        }

        profile.Headline = request.Headline?.Trim();
        profile.Summary = request.Summary?.Trim();
        profile.PhoneNumber = request.PhoneNumber?.Trim();
        profile.LocationCity = request.LocationCity?.Trim();
        profile.LocationCountry = request.LocationCountry?.Trim();
        profile.PostalCode = request.PostalCode?.Trim();
        profile.LinkedInUrl = request.LinkedInUrl?.Trim();
        profile.GitHubUrl = request.GitHubUrl?.Trim();
        profile.PortfolioUrl = request.PortfolioUrl?.Trim();

        await _dbContext.SaveChangesAsync(ct);

        return Result<UserProfileDto>.Success(MapToDto(profile), "Profile updated successfully.");
    }

    public async Task<Result<WorkExperienceDto>> AddWorkExperienceAsync(Guid userId, CreateWorkExperienceDto request, CancellationToken ct = default)
    {
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile == null) return Result<WorkExperienceDto>.Failure("Profile not found.");

        var experience = new WorkExperience
        {
            UserProfileId = profile.Id,
            JobTitle = request.JobTitle.Trim(),
            CompanyName = request.CompanyName.Trim(),
            Location = request.Location?.Trim(),
            StartDate = request.StartDate,
            EndDate = request.IsCurrent ? null : request.EndDate,
            IsCurrent = request.IsCurrent,
            BulletPointsJson = JsonSerializer.Serialize(request.BulletPoints ?? new List<string>()),
            DisplayOrder = request.DisplayOrder
        };

        _dbContext.WorkExperiences.Add(experience);
        await _dbContext.SaveChangesAsync(ct);

        return Result<WorkExperienceDto>.Success(MapExperienceToDto(experience), "Experience added successfully.");
    }

    public async Task<Result<WorkExperienceDto>> UpdateWorkExperienceAsync(Guid userId, Guid experienceId, CreateWorkExperienceDto request, CancellationToken ct = default)
    {
        var experience = await _dbContext.WorkExperiences
            .Include(e => e.UserProfile)
            .FirstOrDefaultAsync(e => e.Id == experienceId && e.UserProfile.UserId == userId, ct);

        if (experience == null) return Result<WorkExperienceDto>.Failure("Experience not found or unauthorized.");

        experience.JobTitle = request.JobTitle.Trim();
        experience.CompanyName = request.CompanyName.Trim();
        experience.Location = request.Location?.Trim();
        experience.StartDate = request.StartDate;
        experience.EndDate = request.IsCurrent ? null : request.EndDate;
        experience.IsCurrent = request.IsCurrent;
        experience.BulletPointsJson = JsonSerializer.Serialize(request.BulletPoints ?? new List<string>());
        experience.DisplayOrder = request.DisplayOrder;

        await _dbContext.SaveChangesAsync(ct);

        return Result<WorkExperienceDto>.Success(MapExperienceToDto(experience), "Experience updated successfully.");
    }

    public async Task<Result> DeleteWorkExperienceAsync(Guid userId, Guid experienceId, CancellationToken ct = default)
    {
        var experience = await _dbContext.WorkExperiences
            .Include(e => e.UserProfile)
            .FirstOrDefaultAsync(e => e.Id == experienceId && e.UserProfile.UserId == userId, ct);

        if (experience == null) return Result.Failure("Experience not found or unauthorized.");

        _dbContext.WorkExperiences.Remove(experience);
        await _dbContext.SaveChangesAsync(ct);

        return Result.Success("Experience deleted successfully.");
    }

    public async Task<Result<EducationDto>> AddEducationAsync(Guid userId, CreateEducationDto request, CancellationToken ct = default)
    {
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile == null) return Result<EducationDto>.Failure("Profile not found.");

        var education = new Education
        {
            UserProfileId = profile.Id,
            Institution = request.Institution.Trim(),
            Degree = request.Degree.Trim(),
            FieldOfStudy = request.FieldOfStudy.Trim(),
            StartDate = request.StartDate,
            EndDate = request.IsCurrent ? null : request.EndDate,
            IsCurrent = request.IsCurrent,
            GradeOrGpa = request.GradeOrGpa?.Trim(),
            Description = request.Description?.Trim(),
            DisplayOrder = request.DisplayOrder
        };

        _dbContext.Educations.Add(education);
        await _dbContext.SaveChangesAsync(ct);

        return Result<EducationDto>.Success(MapEducationToDto(education), "Education added successfully.");
    }

    public async Task<Result<EducationDto>> UpdateEducationAsync(Guid userId, Guid educationId, CreateEducationDto request, CancellationToken ct = default)
    {
        var education = await _dbContext.Educations
            .Include(e => e.UserProfile)
            .FirstOrDefaultAsync(e => e.Id == educationId && e.UserProfile.UserId == userId, ct);

        if (education == null) return Result<EducationDto>.Failure("Education not found or unauthorized.");

        education.Institution = request.Institution.Trim();
        education.Degree = request.Degree.Trim();
        education.FieldOfStudy = request.FieldOfStudy.Trim();
        education.StartDate = request.StartDate;
        education.EndDate = request.IsCurrent ? null : request.EndDate;
        education.IsCurrent = request.IsCurrent;
        education.GradeOrGpa = request.GradeOrGpa?.Trim();
        education.Description = request.Description?.Trim();
        education.DisplayOrder = request.DisplayOrder;

        await _dbContext.SaveChangesAsync(ct);

        return Result<EducationDto>.Success(MapEducationToDto(education), "Education updated successfully.");
    }

    public async Task<Result> DeleteEducationAsync(Guid userId, Guid educationId, CancellationToken ct = default)
    {
        var education = await _dbContext.Educations
            .Include(e => e.UserProfile)
            .FirstOrDefaultAsync(e => e.Id == educationId && e.UserProfile.UserId == userId, ct);

        if (education == null) return Result.Failure("Education not found or unauthorized.");

        _dbContext.Educations.Remove(education);
        await _dbContext.SaveChangesAsync(ct);

        return Result.Success("Education deleted successfully.");
    }

    public async Task<Result<UserSkillDto>> AddSkillAsync(Guid userId, CreateUserSkillDto request, CancellationToken ct = default)
    {
        var profile = await _dbContext.UserProfiles
            .Include(p => p.Skills)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (profile == null) return Result<UserSkillDto>.Failure("Profile not found.");

        var skillName = request.Name.Trim();
        if (profile.Skills.Any(s => s.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<UserSkillDto>.Failure($"Skill '{skillName}' already exists on your profile.");
        }

        var skill = new UserSkill
        {
            UserProfileId = profile.Id,
            Name = skillName,
            Category = request.Category,
            Proficiency = request.Proficiency,
            YearsOfExperience = request.YearsOfExperience
        };

        _dbContext.UserSkills.Add(skill);
        await _dbContext.SaveChangesAsync(ct);

        return Result<UserSkillDto>.Success(new UserSkillDto(skill.Id, skill.Name, skill.Category, skill.Proficiency, skill.YearsOfExperience), "Skill added successfully.");
    }

    public async Task<Result> DeleteSkillAsync(Guid userId, Guid skillId, CancellationToken ct = default)
    {
        var skill = await _dbContext.UserSkills
            .Include(s => s.UserProfile)
            .FirstOrDefaultAsync(s => s.Id == skillId && s.UserProfile.UserId == userId, ct);

        if (skill == null) return Result.Failure("Skill not found or unauthorized.");

        _dbContext.UserSkills.Remove(skill);
        await _dbContext.SaveChangesAsync(ct);

        return Result.Success("Skill removed successfully.");
    }

    public async Task<Result<ProjectDto>> AddProjectAsync(Guid userId, CreateProjectDto request, CancellationToken ct = default)
    {
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile == null) return Result<ProjectDto>.Failure("Profile not found.");

        var project = new Project
        {
            UserProfileId = profile.Id,
            Title = request.Title.Trim(),
            Role = request.Role?.Trim(),
            Description = request.Description.Trim(),
            TechnologiesUsed = request.TechnologiesUsed.Trim(),
            RepoUrl = request.RepoUrl?.Trim(),
            LiveUrl = request.LiveUrl?.Trim(),
            BulletPointsJson = JsonSerializer.Serialize(request.BulletPoints ?? new List<string>()),
            DisplayOrder = request.DisplayOrder
        };

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(ct);

        return Result<ProjectDto>.Success(MapProjectToDto(project), "Project added successfully.");
    }

    public async Task<Result<ProjectDto>> UpdateProjectAsync(Guid userId, Guid projectId, CreateProjectDto request, CancellationToken ct = default)
    {
        var project = await _dbContext.Projects
            .Include(p => p.UserProfile)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserProfile.UserId == userId, ct);

        if (project == null) return Result<ProjectDto>.Failure("Project not found or unauthorized.");

        project.Title = request.Title.Trim();
        project.Role = request.Role?.Trim();
        project.Description = request.Description.Trim();
        project.TechnologiesUsed = request.TechnologiesUsed.Trim();
        project.RepoUrl = request.RepoUrl?.Trim();
        project.LiveUrl = request.LiveUrl?.Trim();
        project.BulletPointsJson = JsonSerializer.Serialize(request.BulletPoints ?? new List<string>());
        project.DisplayOrder = request.DisplayOrder;

        await _dbContext.SaveChangesAsync(ct);

        return Result<ProjectDto>.Success(MapProjectToDto(project), "Project updated successfully.");
    }

    public async Task<Result> DeleteProjectAsync(Guid userId, Guid projectId, CancellationToken ct = default)
    {
        var project = await _dbContext.Projects
            .Include(p => p.UserProfile)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.UserProfile.UserId == userId, ct);

        if (project == null) return Result.Failure("Project not found or unauthorized.");

        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync(ct);

        return Result.Success("Project deleted successfully.");
    }

    private static UserProfileDto MapToDto(UserProfile profile)
    {
        return new UserProfileDto(
            Id: profile.Id,
            UserId: profile.UserId,
            FullName: profile.User?.FullName ?? string.Empty,
            Email: profile.User?.Email ?? string.Empty,
            Headline: profile.Headline,
            Summary: profile.Summary,
            PhoneNumber: profile.PhoneNumber,
            LocationCity: profile.LocationCity,
            LocationCountry: profile.LocationCountry,
            PostalCode: profile.PostalCode,
            LinkedInUrl: profile.LinkedInUrl,
            GitHubUrl: profile.GitHubUrl,
            PortfolioUrl: profile.PortfolioUrl,
            Experiences: profile.Experiences.Select(MapExperienceToDto).ToList(),
            Educations: profile.Educations.Select(MapEducationToDto).ToList(),
            Skills: profile.Skills.Select(s => new UserSkillDto(s.Id, s.Name, s.Category, s.Proficiency, s.YearsOfExperience)).ToList(),
            Projects: profile.Projects.Select(MapProjectToDto).ToList(),
            Certifications: profile.Certifications.Select(c => new CertificationDto(c.Id, c.Name, c.IssuingOrganization, c.IssueDate, c.ExpirationDate, c.CredentialId, c.CredentialUrl)).ToList(),
            Languages: profile.Languages.Select(l => new LanguageDto(l.Id, l.Name, l.ProficiencyLevel)).ToList()
        );
    }

    private static WorkExperienceDto MapExperienceToDto(WorkExperience e)
    {
        List<string> bullets;
        try { bullets = JsonSerializer.Deserialize<List<string>>(e.BulletPointsJson) ?? new List<string>(); }
        catch { bullets = new List<string>(); }

        return new WorkExperienceDto(e.Id, e.JobTitle, e.CompanyName, e.Location, e.StartDate, e.EndDate, e.IsCurrent, bullets, e.DisplayOrder);
    }

    private static EducationDto MapEducationToDto(Education ed)
    {
        return new EducationDto(ed.Id, ed.Institution, ed.Degree, ed.FieldOfStudy, ed.StartDate, ed.EndDate, ed.IsCurrent, ed.GradeOrGpa, ed.Description, ed.DisplayOrder);
    }

    private static ProjectDto MapProjectToDto(Project p)
    {
        List<string> bullets;
        try { bullets = JsonSerializer.Deserialize<List<string>>(p.BulletPointsJson) ?? new List<string>(); }
        catch { bullets = new List<string>(); }

        return new ProjectDto(p.Id, p.Title, p.Role, p.Description, p.TechnologiesUsed, p.RepoUrl, p.LiveUrl, bullets, p.DisplayOrder);
    }
}


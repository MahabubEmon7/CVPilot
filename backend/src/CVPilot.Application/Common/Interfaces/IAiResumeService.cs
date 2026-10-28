using CVPilot.Domain.Entities;

namespace CVPilot.Application.Common.Interfaces;

public record JobAnalysisResultDto(
    string JobTitle,
    string? CompanyName,
    List<string> RequiredSkills,
    List<string> PreferredSkills,
    string? ExperienceRequirement,
    List<string> EducationRequirements,
    List<string> Responsibilities,
    List<string> Keywords
);

public record ProfileMatchResultDto(
    decimal MatchScore,
    List<string> ConfirmedMatches,
    List<string> RelatedExperienceSkills,
    List<string> MissingSkills,
    List<string> Recommendations,
    string SummaryRationale
);

public record TailoredResumeContentDto(
    string TargetRole,
    string ProfessionalSummary,
    List<TailoredExperienceDto> HighlightedExperiences,
    List<string> PrioritizedSkills,
    List<TailoredProjectDto> HighlightedProjects
);

public record TailoredExperienceDto(
    Guid ExperienceId,
    string JobTitle,
    string CompanyName,
    List<string> TailoredBulletPoints
);

public record TailoredProjectDto(
    Guid ProjectId,
    string Title,
    string Description,
    List<string> TailoredBulletPoints
);

public record CoverLetterResultDto(
    string Subject,
    string Salutation,
    string Body,
    string Closing
);

public record SkillGapAnalysisDto(
    List<string> MissingCriticalSkills,
    List<string> RecommendedLearningPath,
    List<string> ActionableTips
);

public interface IAiResumeService
{
    string ProviderName { get; }

    Task<JobAnalysisResultDto> AnalyzeJobDescriptionAsync(string rawJobText, CancellationToken ct = default);
    Task<ProfileMatchResultDto> MatchProfileToJobAsync(UserProfile profile, JobDescription job, CancellationToken ct = default);
    Task<TailoredResumeContentDto> GenerateTailoredResumeAsync(UserProfile profile, JobDescription job, CancellationToken ct = default);
    Task<CoverLetterResultDto> GenerateCoverLetterAsync(UserProfile profile, JobDescription job, CancellationToken ct = default);
    Task<SkillGapAnalysisDto> GenerateSkillGapAnalysisAsync(UserProfile profile, JobDescription job, CancellationToken ct = default);
}


namespace CVPilot.Application.Features.JobDescriptions.DTOs;

public record CreateJobDescriptionRequestDto(
    string RawContent,
    string? JobTitle = null,
    string? CompanyName = null
);

public record UpdateJobDescriptionRequestDto(
    string JobTitle,
    string? CompanyName,
    string? ExperienceRequirement,
    List<string> RequiredSkills,
    List<string> PreferredSkills,
    List<string> Responsibilities,
    List<string> EducationRequirements,
    List<string> Keywords
);

public record JobDescriptionDto(
    Guid Id,
    Guid UserId,
    string JobTitle,
    string? CompanyName,
    string RawContent,
    string? ExperienceRequirement,
    List<string> RequiredSkills,
    List<string> PreferredSkills,
    List<string> Responsibilities,
    List<string> EducationRequirements,
    List<string> Keywords,
    bool IsAnalyzed,
    DateTime? AnalyzedAtUtc,
    DateTime CreatedAtUtc
);

public record JobDescriptionSummaryDto(
    Guid Id,
    string JobTitle,
    string? CompanyName,
    string? ExperienceRequirement,
    int RequiredSkillsCount,
    DateTime CreatedAtUtc
);

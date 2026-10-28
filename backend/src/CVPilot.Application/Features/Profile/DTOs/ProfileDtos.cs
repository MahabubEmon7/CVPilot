using CVPilot.Domain.Enums;

namespace CVPilot.Application.Features.Profile.DTOs;

public record UserProfileDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string? Headline,
    string? Summary,
    string? PhoneNumber,
    string? LocationCity,
    string? LocationCountry,
    string? PostalCode,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? PortfolioUrl,
    List<WorkExperienceDto> Experiences,
    List<EducationDto> Educations,
    List<UserSkillDto> Skills,
    List<ProjectDto> Projects,
    List<CertificationDto> Certifications,
    List<LanguageDto> Languages
);

public record UpdateProfileRequestDto(
    string? FullName,
    string? Headline,
    string? Summary,
    string? PhoneNumber,
    string? LocationCity,
    string? LocationCountry,
    string? PostalCode,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? PortfolioUrl
);

public record WorkExperienceDto(
    Guid Id,
    string JobTitle,
    string CompanyName,
    string? Location,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    List<string> BulletPoints,
    int DisplayOrder
);

public record CreateWorkExperienceDto(
    string JobTitle,
    string CompanyName,
    string? Location,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    List<string> BulletPoints,
    int DisplayOrder = 0
);

public record EducationDto(
    Guid Id,
    string Institution,
    string Degree,
    string FieldOfStudy,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    string? GradeOrGpa,
    string? Description,
    int DisplayOrder
);

public record CreateEducationDto(
    string Institution,
    string Degree,
    string FieldOfStudy,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsCurrent,
    string? GradeOrGpa,
    string? Description,
    int DisplayOrder = 0
);

public record UserSkillDto(
    Guid Id,
    string Name,
    SkillCategory Category,
    SkillProficiency Proficiency,
    int? YearsOfExperience
);

public record CreateUserSkillDto(
    string Name,
    SkillCategory Category,
    SkillProficiency Proficiency,
    int? YearsOfExperience
);

public record ProjectDto(
    Guid Id,
    string Title,
    string? Role,
    string Description,
    string TechnologiesUsed,
    string? RepoUrl,
    string? LiveUrl,
    List<string> BulletPoints,
    int DisplayOrder
);

public record CreateProjectDto(
    string Title,
    string? Role,
    string Description,
    string TechnologiesUsed,
    string? RepoUrl,
    string? LiveUrl,
    List<string> BulletPoints,
    int DisplayOrder = 0
);

public record CertificationDto(
    Guid Id,
    string Name,
    string IssuingOrganization,
    DateTime IssueDate,
    DateTime? ExpirationDate,
    string? CredentialId,
    string? CredentialUrl
);

public record LanguageDto(
    Guid Id,
    string Name,
    string ProficiencyLevel
);


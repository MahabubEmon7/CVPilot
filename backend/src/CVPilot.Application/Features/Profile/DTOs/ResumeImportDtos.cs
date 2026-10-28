namespace CVPilot.Application.Features.Profile.DTOs;

public record ParsedResumeDto(
    string? FullName,
    string? Email,
    string? PhoneNumber,
    string? LocationCity,
    string? LocationCountry,
    string? Headline,
    string? Summary,
    List<CreateWorkExperienceDto> Experiences,
    List<CreateEducationDto> Educations,
    List<CreateUserSkillDto> Skills,
    string RawExtractedText
);

public record ConfirmImportProfileDto(
    string? FullName,
    string? Headline,
    string? Summary,
    string? PhoneNumber,
    string? LocationCity,
    string? LocationCountry,
    string? LinkedInUrl,
    string? GitHubUrl,
    List<CreateWorkExperienceDto> ConfirmedExperiences,
    List<CreateEducationDto> ConfirmedEducations,
    List<CreateUserSkillDto> ConfirmedSkills
);


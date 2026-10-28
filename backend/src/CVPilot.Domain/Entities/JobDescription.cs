using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class JobDescription : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string JobTitle { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string RawContent { get; set; } = string.Empty;

    public string? ExperienceRequirement { get; set; }
    public string RequiredSkillsJson { get; set; } = "[]";
    public string PreferredSkillsJson { get; set; } = "[]";
    public string ResponsibilitiesJson { get; set; } = "[]";
    public string EducationRequirementsJson { get; set; } = "[]";
    public string KeywordsJson { get; set; } = "[]";

    public bool IsAnalyzed { get; set; } = false;
    public DateTime? AnalyzedAtUtc { get; set; }

    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<CoverLetter> CoverLetters { get; set; } = new List<CoverLetter>();
}


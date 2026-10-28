using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class ResumeJobMatch : BaseEntity
{
    public Guid ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;

    public Guid JobDescriptionId { get; set; }
    public JobDescription JobDescription { get; set; } = null!;

    // Score from 0 to 100 representing profile-to-job match
    public decimal MatchPercentage { get; set; }

    public string MatchedSkillsJson { get; set; } = "[]";
    public string PartialSkillsJson { get; set; } = "[]";
    public string MissingSkillsJson { get; set; } = "[]";
    public string RecommendationsJson { get; set; } = "[]";
    public string AnalysisSummary { get; set; } = string.Empty;
}


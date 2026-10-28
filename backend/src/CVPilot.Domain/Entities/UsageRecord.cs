using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class UsageRecord : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string FeatureName { get; set; } = string.Empty; // e.g. "JobAnalysis", "ResumeTailor", "CoverLetter"
    public string Provider { get; set; } = "Mock";
    public int TokensUsed { get; set; } = 0;
    public string Details { get; set; } = string.Empty;
}


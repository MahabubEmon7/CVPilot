using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class Project : BaseEntity
{
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TechnologiesUsed { get; set; } = string.Empty; // e.g. "Angular, .NET, SQL Server"
    public string? RepoUrl { get; set; }
    public string? LiveUrl { get; set; }
    public string BulletPointsJson { get; set; } = "[]";
    public int DisplayOrder { get; set; } = 0;
}


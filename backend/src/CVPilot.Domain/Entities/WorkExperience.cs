using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class WorkExperience : BaseEntity
{
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; } = false;

    // JSON array or newline-delimited bullet points of confirmed achievements
    public string BulletPointsJson { get; set; } = "[]";
    public int DisplayOrder { get; set; } = 0;
}


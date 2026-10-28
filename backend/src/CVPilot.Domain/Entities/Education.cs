using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class Education : BaseEntity
{
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public string FieldOfStudy { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; } = false;
    public string? GradeOrGpa { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } = 0;
}


using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class Language : BaseEntity
{
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string ProficiencyLevel { get; set; } = "Professional Working"; // e.g., Native, Fluent, Professional Working, Basic
}


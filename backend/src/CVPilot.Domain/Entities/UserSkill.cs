using CVPilot.Domain.Common;
using CVPilot.Domain.Enums;

namespace CVPilot.Domain.Entities;

public class UserSkill : BaseEntity
{
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public SkillCategory Category { get; set; } = SkillCategory.Technical;
    public SkillProficiency Proficiency { get; set; } = SkillProficiency.Intermediate;
    public int? YearsOfExperience { get; set; }
}


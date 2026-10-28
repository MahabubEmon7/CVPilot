using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class Certification : BaseEntity
{
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string IssuingOrganization { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
}


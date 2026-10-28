using CVPilot.Domain.Common;
using CVPilot.Domain.Enums;

namespace CVPilot.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryUtc { get; set; }

    public UserProfile? Profile { get; set; }
    public ICollection<JobDescription> JobDescriptions { get; set; } = new List<JobDescription>();
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<CoverLetter> CoverLetters { get; set; } = new List<CoverLetter>();
    public ICollection<UsageRecord> UsageRecords { get; set; } = new List<UsageRecord>();
}


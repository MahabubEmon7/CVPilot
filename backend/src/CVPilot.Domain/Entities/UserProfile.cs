using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string? Headline { get; set; }
    public string? Summary { get; set; }
    public string? PhoneNumber { get; set; }
    public string? LocationCity { get; set; }
    public string? LocationCountry { get; set; }
    public string? PostalCode { get; set; }

    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }

    public ICollection<WorkExperience> Experiences { get; set; } = new List<WorkExperience>();
    public ICollection<Education> Educations { get; set; } = new List<Education>();
    public ICollection<UserSkill> Skills { get; set; } = new List<UserSkill>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Certification> Certifications { get; set; } = new List<Certification>();
    public ICollection<Language> Languages { get; set; } = new List<Language>();
}


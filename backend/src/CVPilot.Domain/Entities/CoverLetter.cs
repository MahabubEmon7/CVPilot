using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class CoverLetter : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid? JobDescriptionId { get; set; }
    public JobDescription? JobDescription { get; set; }

    public string Title { get; set; } = "Untitled Cover Letter";
    public string RecipientName { get; set; } = "Hiring Manager";
    public string CompanyName { get; set; } = string.Empty;
    public string TargetRole { get; set; } = string.Empty;
    public string BodyText { get; set; } = string.Empty;
}


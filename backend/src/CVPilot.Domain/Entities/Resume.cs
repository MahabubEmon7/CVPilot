using CVPilot.Domain.Common;
using CVPilot.Domain.Enums;

namespace CVPilot.Domain.Entities;

public class Resume : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public Guid? JobDescriptionId { get; set; }
    public JobDescription? JobDescription { get; set; }

    public string Title { get; set; } = "Untitled Resume";
    public string TargetRole { get; set; } = string.Empty;
    public ResumeTemplateType TemplateType { get; set; } = ResumeTemplateType.ModernProfessional;

    public int CurrentVersionNumber { get; set; } = 1;

    // Active working content (structured JSON model of full resume sections)
    public string DocumentDataJson { get; set; } = "{}";

    public ICollection<ResumeVersion> Versions { get; set; } = new List<ResumeVersion>();
    public ICollection<ResumeJobMatch> JobMatches { get; set; } = new List<ResumeJobMatch>();
    public ICollection<DocumentExport> Exports { get; set; } = new List<DocumentExport>();
}


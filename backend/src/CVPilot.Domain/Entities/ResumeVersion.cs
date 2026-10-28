using CVPilot.Domain.Common;

namespace CVPilot.Domain.Entities;

public class ResumeVersion : BaseEntity
{
    public Guid ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;

    public int VersionNumber { get; set; }
    public string ChangeSummary { get; set; } = "Snapshot";
    public string DocumentDataJson { get; set; } = "{}";
}


using CVPilot.Domain.Common;
using CVPilot.Domain.Enums;

namespace CVPilot.Domain.Entities;

public class DocumentExport : BaseEntity
{
    public Guid ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;

    public ExportFormat Format { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoragePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int DownloadCount { get; set; } = 0;
}


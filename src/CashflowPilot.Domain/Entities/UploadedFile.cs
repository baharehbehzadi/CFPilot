using CashflowPilot.Domain.Enums;

namespace CashflowPilot.Domain.Entities;

public class UploadedFile
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedByUserId { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public FileStatus Status { get; set; } = FileStatus.Uploaded;
    public string? ErrorMessage { get; set; }
}

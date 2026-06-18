using CashflowPilot.Domain.Enums;

namespace CashflowPilot.Domain.Entities;

public class ValidationIssue
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int UploadedFileId { get; set; }
    public UploadedFile UploadedFile { get; set; } = null!;
    public ValidationSeverity Severity { get; set; }
    public int? RowNumber { get; set; }
    public string? FieldName { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? SuggestedFix { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

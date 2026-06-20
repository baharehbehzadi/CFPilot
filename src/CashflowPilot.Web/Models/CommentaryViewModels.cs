using CashflowPilot.Application.DTOs;
using CashflowPilot.Domain.Entities;

namespace CashflowPilot.Web.Models;

public class ReportPackViewModel
{
    public CommentaryPack Commentary { get; set; } = null!;
    public ReportPackDto Pack { get; set; } = new();
    public string OrganizationName { get; set; } = string.Empty;
}

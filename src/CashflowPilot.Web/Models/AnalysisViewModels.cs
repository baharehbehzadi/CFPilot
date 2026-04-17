using CashflowPilot.Application.DTOs;
using CashflowPilot.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace CashflowPilot.Web.Models;

public class CreateAnalysisViewModel
{
    [Required] public int ForecastRunId { get; set; }
    [Required] public int ActualRunId { get; set; }
    [StringLength(200)] public string? Name { get; set; }
    public List<ForecastRun> ForecastRuns { get; set; } = new();
    public List<ActualRun> ActualRuns { get; set; } = new();
}

public class AnalysisDetailViewModel
{
    public VarianceSummaryDto Summary { get; set; } = new();
    public CommentaryPack? Commentary { get; set; }
}

using CashflowPilot.Application.DTOs;
using CashflowPilot.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CashflowPilot.Web.Models;

public class UploadForecastViewModel
{
    [Required] public IFormFile? File { get; set; }
    [Required, StringLength(200)] public string RunName { get; set; } = string.Empty;
    [Required] public int PortfolioId { get; set; }
    public List<Portfolio> Portfolios { get; set; } = new();
}

public class UploadActualViewModel
{
    [Required] public IFormFile? File { get; set; }
    [Required, StringLength(200)] public string RunName { get; set; } = string.Empty;
    [Required] public int PortfolioId { get; set; }
    [Required] public DateTime ReportingPeriod { get; set; } = DateTime.Today.AddMonths(-1);
    public List<Portfolio> Portfolios { get; set; } = new();
}

public class MappingViewModel
{
    public int UploadedFileId { get; set; }
    public string FileType { get; set; } = string.Empty;
    public int PortfolioId { get; set; }
    public string RunName { get; set; } = string.Empty;
    public DateTime? ReportingPeriod { get; set; }
    public List<string> DetectedHeaders { get; set; } = new();
    public string? FundNameColumn { get; set; }
    public string? StrategyColumn { get; set; }
    public string? PeriodColumn { get; set; }
    public string? CapitalCallsColumn { get; set; }
    public string? DistributionsColumn { get; set; }
    public string? CurrencyColumn { get; set; }
    public string? NotesColumn { get; set; }
    public ParseResultDto? PreviewResult { get; set; }
}

public class ValidationResultViewModel
{
    public int UploadedFileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public ParseResultDto ParseResult { get; set; } = new();
    public int PortfolioId { get; set; }
    public string RunName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime? ReportingPeriod { get; set; }
}

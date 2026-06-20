using CashflowPilot.Application.DTOs;
using CashflowPilot.Infrastructure.Services;
using ClosedXML.Excel;
using FluentAssertions;
using System.Text;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class ReportExportServiceTests
{
    private static VarianceSummaryDto CreateSummary() => new()
    {
        AnalysisId = 1,
        AnalysisName = "H1 2024 Variance Review",
        ForecastRunName = "FY2024 Budget Forecast",
        ActualRunName = "H1 2024 Actuals",
        CreatedAt = new DateTime(2024, 6, 30),
        TotalForecastCalls = 1_000_000,
        TotalActualCalls = 1_200_000,
        TotalCallsVariance = 200_000,
        TotalCallsVariancePct = 20m,
        TotalForecastDistributions = 500_000,
        TotalActualDistributions = 400_000,
        TotalDistributionsVariance = -100_000,
        TotalDistributionsVariancePct = -20m,
        TotalForecastNetCashflow = -500_000,
        TotalActualNetCashflow = -800_000,
        TotalNetCashflowVariance = -300_000,
        TotalNetCashflowVariancePct = null,
        ByPeriod = new List<PeriodVarianceDto>
        {
            new()
            {
                Period = new DateTime(2024, 1, 1),
                ForecastCalls = 1_000_000, ActualCalls = 1_200_000, CallsVariance = 200_000,
                ForecastDistributions = 500_000, ActualDistributions = 400_000, DistributionsVariance = -100_000,
                ForecastNet = -500_000, ActualNet = -800_000, NetVariance = -300_000, CumulativeVariance = -300_000
            }
        },
        ByFund = new List<FundVarianceDto>
        {
            new() { FundName = "Apex Buyout Fund", TotalCallsVariance = 200_000, TotalDistributionsVariance = -100_000, TotalNetVariance = -300_000 }
        }
    };

    [Fact]
    public void ExportVarianceToCsv_IncludesHeaderAndSummaryFigures()
    {
        var bytes = new ReportExportService().ExportVarianceToCsv(CreateSummary());
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("Analysis,H1 2024 Variance Review");
        csv.Should().Contain("Forecast Run,FY2024 Budget Forecast");
        csv.Should().Contain("Capital Calls,1000000.00,1200000.00,200000.00,+20%");
        csv.Should().Contain("Net Cashflow,-500000.00,-800000.00,-300000.00,N/A");
    }

    [Fact]
    public void ExportVarianceToCsv_IncludesByPeriodAndByFundSections()
    {
        var bytes = new ReportExportService().ExportVarianceToCsv(CreateSummary());
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("BY PERIOD");
        csv.Should().Contain("2024-01,1000000.00,1200000.00,200000.00");
        csv.Should().Contain("BY FUND");
        csv.Should().Contain("Apex Buyout Fund,200000.00,-100000.00,-300000.00");
    }

    [Fact]
    public void ExportVarianceToExcel_ProducesWorkbookWithExpectedSheetsAndValues()
    {
        var bytes = new ReportExportService().ExportVarianceToExcel(CreateSummary());

        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);

        workbook.Worksheets.Select(w => w.Name).Should().BeEquivalentTo(new[] { "Summary", "By Period", "By Fund" });

        var summary = workbook.Worksheet("Summary");
        summary.Cell(2, 2).GetString().Should().Be("H1 2024 Variance Review");
        summary.Cell(8, 2).GetDouble().Should().Be(1_000_000d); // Capital Calls forecast

        var byPeriod = workbook.Worksheet("By Period");
        byPeriod.Cell(2, 1).GetString().Should().Be("2024-01");
        byPeriod.Cell(2, 10).GetDouble().Should().Be(-300_000d); // Net Variance

        var byFund = workbook.Worksheet("By Fund");
        byFund.Cell(2, 1).GetString().Should().Be("Apex Buyout Fund");
        byFund.Cell(2, 4).GetDouble().Should().Be(-300_000d); // Net Variance
    }

    [Fact]
    public void ExportVarianceToCsv_EmptyByPeriodAndByFund_StillProducesValidCsv()
    {
        var summary = CreateSummary();
        summary.ByPeriod = new List<PeriodVarianceDto>();
        summary.ByFund = new List<FundVarianceDto>();

        var bytes = new ReportExportService().ExportVarianceToCsv(summary);
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("BY PERIOD");
        csv.Should().Contain("BY FUND");
    }
}

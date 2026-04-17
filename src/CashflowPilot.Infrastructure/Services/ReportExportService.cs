using CashflowPilot.Application.DTOs;
using ClosedXML.Excel;
using System.Text;

namespace CashflowPilot.Infrastructure.Services;

public interface IReportExportService
{
    byte[] ExportVarianceToCsv(VarianceSummaryDto summary);
    byte[] ExportVarianceToExcel(VarianceSummaryDto summary);
}

public class ReportExportService : IReportExportService
{
    public byte[] ExportVarianceToCsv(VarianceSummaryDto summary)
    {
        var sb = new StringBuilder();
        sb.AppendLine("CashflowPilot - Variance Analysis Report");
        sb.AppendLine($"Analysis,{summary.AnalysisName}");
        sb.AppendLine($"Forecast Run,{summary.ForecastRunName}");
        sb.AppendLine($"Actual Run,{summary.ActualRunName}");
        sb.AppendLine($"Generated,{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
        sb.AppendLine();

        sb.AppendLine("SUMMARY");
        sb.AppendLine("Metric,Forecast,Actual,Variance,Variance %");
        sb.AppendLine($"Capital Calls,{summary.TotalForecastCalls:F2},{summary.TotalActualCalls:F2},{summary.TotalCallsVariance:F2},{FormatPct(summary.TotalCallsVariancePct)}");
        sb.AppendLine($"Distributions,{summary.TotalForecastDistributions:F2},{summary.TotalActualDistributions:F2},{summary.TotalDistributionsVariance:F2},{FormatPct(summary.TotalDistributionsVariancePct)}");
        sb.AppendLine($"Net Cashflow,{summary.TotalForecastNetCashflow:F2},{summary.TotalActualNetCashflow:F2},{summary.TotalNetCashflowVariance:F2},{FormatPct(summary.TotalNetCashflowVariancePct)}");
        sb.AppendLine();

        sb.AppendLine("BY PERIOD");
        sb.AppendLine("Period,Forecast Calls,Actual Calls,Calls Variance,Forecast Distributions,Actual Distributions,Dist Variance,Forecast Net,Actual Net,Net Variance,Cumulative Variance");
        foreach (var p in summary.ByPeriod)
            sb.AppendLine($"{p.Period:yyyy-MM},{p.ForecastCalls:F2},{p.ActualCalls:F2},{p.CallsVariance:F2},{p.ForecastDistributions:F2},{p.ActualDistributions:F2},{p.DistributionsVariance:F2},{p.ForecastNet:F2},{p.ActualNet:F2},{p.NetVariance:F2},{p.CumulativeVariance:F2}");
        sb.AppendLine();

        sb.AppendLine("BY FUND");
        sb.AppendLine("Fund,Calls Variance,Distributions Variance,Net Variance");
        foreach (var f in summary.ByFund)
            sb.AppendLine($"{f.FundName},{f.TotalCallsVariance:F2},{f.TotalDistributionsVariance:F2},{f.TotalNetVariance:F2}");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] ExportVarianceToExcel(VarianceSummaryDto summary)
    {
        using var workbook = new XLWorkbook();

        // Summary sheet
        var ws = workbook.Worksheets.Add("Summary");
        ws.Cell(1, 1).Value = "CashflowPilot - Variance Analysis Report";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 6).Merge();

        ws.Cell(2, 1).Value = "Analysis:"; ws.Cell(2, 2).Value = summary.AnalysisName;
        ws.Cell(3, 1).Value = "Forecast Run:"; ws.Cell(3, 2).Value = summary.ForecastRunName;
        ws.Cell(4, 1).Value = "Actual Run:"; ws.Cell(4, 2).Value = summary.ActualRunName;
        ws.Cell(5, 1).Value = "Generated:"; ws.Cell(5, 2).Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") + " UTC";

        int row = 7;
        ws.Cell(row, 1).Value = "Metric"; ws.Cell(row, 2).Value = "Forecast"; ws.Cell(row, 3).Value = "Actual";
        ws.Cell(row, 4).Value = "Variance"; ws.Cell(row, 5).Value = "Variance %";
        ws.Row(row).Style.Font.Bold = true;
        ws.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3a5c");
        ws.Row(row).Style.Font.FontColor = XLColor.White;

        row++;
        AddSummaryRow(ws, row++, "Capital Calls", summary.TotalForecastCalls, summary.TotalActualCalls, summary.TotalCallsVariance, summary.TotalCallsVariancePct);
        AddSummaryRow(ws, row++, "Distributions", summary.TotalForecastDistributions, summary.TotalActualDistributions, summary.TotalDistributionsVariance, summary.TotalDistributionsVariancePct);
        AddSummaryRow(ws, row++, "Net Cashflow", summary.TotalForecastNetCashflow, summary.TotalActualNetCashflow, summary.TotalNetCashflowVariance, summary.TotalNetCashflowVariancePct);
        ws.Columns().AdjustToContents();

        // By Period sheet
        var wsPeriod = workbook.Worksheets.Add("By Period");
        string[] periodHeaders = { "Period", "Fcst Calls", "Act Calls", "Calls Var", "Fcst Dist", "Act Dist", "Dist Var", "Fcst Net", "Act Net", "Net Var", "Cumulative Var" };
        for (int i = 0; i < periodHeaders.Length; i++)
        {
            wsPeriod.Cell(1, i + 1).Value = periodHeaders[i];
            wsPeriod.Cell(1, i + 1).Style.Font.Bold = true;
        }
        wsPeriod.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3a5c");
        wsPeriod.Row(1).Style.Font.FontColor = XLColor.White;

        int pRow = 2;
        foreach (var p in summary.ByPeriod)
        {
            wsPeriod.Cell(pRow, 1).Value = p.Period.ToString("yyyy-MM");
            wsPeriod.Cell(pRow, 2).Value = (double)p.ForecastCalls;
            wsPeriod.Cell(pRow, 3).Value = (double)p.ActualCalls;
            wsPeriod.Cell(pRow, 4).Value = (double)p.CallsVariance;
            wsPeriod.Cell(pRow, 5).Value = (double)p.ForecastDistributions;
            wsPeriod.Cell(pRow, 6).Value = (double)p.ActualDistributions;
            wsPeriod.Cell(pRow, 7).Value = (double)p.DistributionsVariance;
            wsPeriod.Cell(pRow, 8).Value = (double)p.ForecastNet;
            wsPeriod.Cell(pRow, 9).Value = (double)p.ActualNet;
            wsPeriod.Cell(pRow, 10).Value = (double)p.NetVariance;
            wsPeriod.Cell(pRow, 11).Value = (double)p.CumulativeVariance;
            ColorVarianceCell(wsPeriod.Cell(pRow, 10), p.NetVariance);
            pRow++;
        }
        wsPeriod.Columns().AdjustToContents();

        // By Fund sheet
        var wsFund = workbook.Worksheets.Add("By Fund");
        wsFund.Cell(1, 1).Value = "Fund"; wsFund.Cell(1, 2).Value = "Calls Variance"; wsFund.Cell(1, 3).Value = "Dist Variance"; wsFund.Cell(1, 4).Value = "Net Variance";
        wsFund.Row(1).Style.Font.Bold = true;
        wsFund.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#1a3a5c");
        wsFund.Row(1).Style.Font.FontColor = XLColor.White;
        int fRow = 2;
        foreach (var f in summary.ByFund)
        {
            wsFund.Cell(fRow, 1).Value = f.FundName;
            wsFund.Cell(fRow, 2).Value = (double)f.TotalCallsVariance;
            wsFund.Cell(fRow, 3).Value = (double)f.TotalDistributionsVariance;
            wsFund.Cell(fRow, 4).Value = (double)f.TotalNetVariance;
            ColorVarianceCell(wsFund.Cell(fRow, 4), f.TotalNetVariance);
            fRow++;
        }
        wsFund.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    private static void AddSummaryRow(IXLWorksheet ws, int row, string label, decimal forecast, decimal actual, decimal variance, decimal? variancePct)
    {
        ws.Cell(row, 1).Value = label;
        ws.Cell(row, 2).Value = (double)forecast;
        ws.Cell(row, 3).Value = (double)actual;
        ws.Cell(row, 4).Value = (double)variance;
        ws.Cell(row, 5).Value = variancePct.HasValue ? $"{variancePct.Value:+0.##;-0.##;0}%" : "N/A";
        ColorVarianceCell(ws.Cell(row, 4), variance);
    }

    private static void ColorVarianceCell(IXLCell cell, decimal value)
    {
        if (value > 0) cell.Style.Font.FontColor = XLColor.FromHtml("#155724");
        else if (value < 0) cell.Style.Font.FontColor = XLColor.FromHtml("#721c24");
    }

    private static string FormatPct(decimal? value) => value.HasValue ? $"{value.Value:+0.##;-0.##;0}%" : "N/A";
}

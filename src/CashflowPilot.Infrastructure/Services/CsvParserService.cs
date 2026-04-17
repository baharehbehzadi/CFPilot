using CashflowPilot.Application.DTOs;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace CashflowPilot.Infrastructure.Services;

public interface ICsvParserService
{
    ParseResultDto Parse(Stream stream, ColumnMapping? mapping = null);
}

public class CsvParserService : ICsvParserService
{
    public ParseResultDto Parse(Stream stream, ColumnMapping? mapping = null)
    {
        var result = new ParseResultDto();

        try
        {
            using var reader = new StreamReader(stream, leaveOpen: true);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true
            };
            using var csv = new CsvReader(reader, config);

            csv.Read();
            csv.ReadHeader();
            var headers = csv.HeaderRecord ?? Array.Empty<string>();
            result.DetectedHeaders = headers.ToList();

            if (mapping == null || !mapping.IsComplete())
                mapping = ColumnMapping.AutoDetect(headers);

            result.Mapping = mapping;

            if (!mapping.IsComplete())
            {
                result.GlobalErrors.Add("Could not auto-detect required columns (Period, and at least one of CapitalCalls/Distributions). Please map columns manually.");
                return result;
            }

            int rowNum = 1;
            while (csv.Read())
            {
                rowNum++;
                var row = new CashflowRowDto { RowNumber = rowNum };

                row.FundName = mapping.FundNameColumn != null ? csv.GetField(mapping.FundNameColumn) : null;
                row.StrategyName = mapping.StrategyColumn != null ? csv.GetField(mapping.StrategyColumn) : null;
                row.Period = mapping.PeriodColumn != null ? csv.GetField(mapping.PeriodColumn) : null;
                row.CapitalCalls = mapping.CapitalCallsColumn != null ? csv.GetField(mapping.CapitalCallsColumn) : null;
                row.Distributions = mapping.DistributionsColumn != null ? csv.GetField(mapping.DistributionsColumn) : null;
                row.Currency = mapping.CurrencyColumn != null ? csv.GetField(mapping.CurrencyColumn) : "USD";
                row.Notes = mapping.NotesColumn != null ? csv.GetField(mapping.NotesColumn) : null;

                // Skip entirely blank rows
                if (string.IsNullOrWhiteSpace(row.Period) && string.IsNullOrWhiteSpace(row.CapitalCalls) && string.IsNullOrWhiteSpace(row.Distributions))
                    continue;

                result.TotalRows++;

                // Validate Period
                row.PeriodDate = TryParseDate(row.Period);
                if (row.PeriodDate == null)
                    row.ValidationErrors.Add($"Row {rowNum}: Cannot parse period '{row.Period}'. Expected formats: yyyy-MM, MM/yyyy, MMM-yyyy, Q1 yyyy.");

                // Validate amounts
                if (!string.IsNullOrWhiteSpace(row.CapitalCalls))
                {
                    if (TryParseAmount(row.CapitalCalls, out var amount))
                        row.CapitalCallsAmount = amount;
                    else
                        row.ValidationErrors.Add($"Row {rowNum}: Cannot parse capital calls amount '{row.CapitalCalls}'.");
                }
                else
                {
                    row.CapitalCallsAmount = 0;
                }

                if (!string.IsNullOrWhiteSpace(row.Distributions))
                {
                    if (TryParseAmount(row.Distributions, out var amount))
                        row.DistributionsAmount = amount;
                    else
                        row.ValidationErrors.Add($"Row {rowNum}: Cannot parse distributions amount '{row.Distributions}'.");
                }
                else
                {
                    row.DistributionsAmount = 0;
                }

                if (row.IsValid)
                    result.ValidRows++;
                else
                    result.InvalidRows++;

                result.Rows.Add(row);
            }

            result.Success = result.GlobalErrors.Count == 0 && result.ValidRows > 0;
        }
        catch (Exception ex)
        {
            result.GlobalErrors.Add($"Parse error: {ex.Message}");
        }

        return result;
    }

    private static DateTime? TryParseDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        raw = raw.Trim();

        // Quarter notation: Q1 2024, Q2-2023
        var qMatch = System.Text.RegularExpressions.Regex.Match(raw, @"^Q([1-4])[\s\-](\d{4})$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (qMatch.Success)
        {
            int quarter = int.Parse(qMatch.Groups[1].Value);
            int year = int.Parse(qMatch.Groups[2].Value);
            int month = (quarter - 1) * 3 + 1;
            return new DateTime(year, month, 1);
        }

        string[] formats = {
            "yyyy-MM", "yyyy-MM-dd", "MM/yyyy", "M/yyyy",
            "MMM-yyyy", "MMM yyyy", "MMMM yyyy", "MMMM-yyyy",
            "yyyy/MM", "dd/MM/yyyy", "MM/dd/yyyy"
        };
        foreach (var fmt in formats)
        {
            if (DateTime.TryParseExact(raw, fmt, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
                return new DateTime(d.Year, d.Month, 1);
        }

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var fallback))
            return new DateTime(fallback.Year, fallback.Month, 1);

        return null;
    }

    private static bool TryParseAmount(string? raw, out decimal amount)
    {
        amount = 0;
        if (string.IsNullOrWhiteSpace(raw)) return true;
        // Remove currency symbols, commas, spaces, parentheses (accounting negative)
        var cleaned = raw.Trim().Replace(",", "").Replace("$", "").Replace("€", "").Replace("£", "").Replace(" ", "");
        bool negative = cleaned.StartsWith("(") && cleaned.EndsWith(")");
        if (negative) cleaned = "-" + cleaned[1..^1];
        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);
    }
}

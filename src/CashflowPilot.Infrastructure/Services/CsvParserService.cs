using CashflowPilot.Application.DTOs;
using CashflowPilot.Domain.Enums;
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
            var seenFundPeriods = new Dictionary<(string fund, DateTime period), int>();
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
                    row.Issues.Add(new RowValidationIssueDto
                    {
                        Severity = ValidationSeverity.Error, FieldName = "Period",
                        Message = $"Row {rowNum}: Cannot parse period '{row.Period}'. Expected formats: yyyy-MM, MM/yyyy, MMM-yyyy, Q1 yyyy.",
                        SuggestedFix = "Use a format such as 2024-01, Mar-2024, or Q1 2024."
                    });

                // Validate amounts
                if (!string.IsNullOrWhiteSpace(row.CapitalCalls))
                {
                    if (TryParseAmount(row.CapitalCalls, out var amount))
                        row.CapitalCallsAmount = amount;
                    else
                        row.Issues.Add(new RowValidationIssueDto
                        {
                            Severity = ValidationSeverity.Error, FieldName = "CapitalCalls",
                            Message = $"Row {rowNum}: Cannot parse capital calls amount '{row.CapitalCalls}'.",
                            SuggestedFix = "Use a plain number, optionally with commas or accounting parentheses for negatives."
                        });
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
                        row.Issues.Add(new RowValidationIssueDto
                        {
                            Severity = ValidationSeverity.Error, FieldName = "Distributions",
                            Message = $"Row {rowNum}: Cannot parse distributions amount '{row.Distributions}'.",
                            SuggestedFix = "Use a plain number, optionally with commas or accounting parentheses for negatives."
                        });
                }
                else
                {
                    row.DistributionsAmount = 0;
                }

                // Extended validations (non-blocking severity tiers)
                if (string.IsNullOrWhiteSpace(row.FundName))
                {
                    row.Issues.Add(new RowValidationIssueDto
                    {
                        Severity = ValidationSeverity.Warning, FieldName = "FundName",
                        Message = $"Row {rowNum}: Fund name is blank.",
                        SuggestedFix = "Provide a fund name; rows left blank will be grouped under 'Unknown Fund'."
                    });
                }

                if (string.IsNullOrWhiteSpace(row.Currency))
                {
                    row.Issues.Add(new RowValidationIssueDto
                    {
                        Severity = ValidationSeverity.Info, FieldName = "Currency",
                        Message = $"Row {rowNum}: Currency not specified.",
                        SuggestedFix = "Defaults to USD."
                    });
                }
                else if (row.Currency.Trim().Length != 3 || !row.Currency.Trim().All(char.IsLetter))
                {
                    row.Issues.Add(new RowValidationIssueDto
                    {
                        Severity = ValidationSeverity.Warning, FieldName = "Currency",
                        Message = $"Row {rowNum}: Currency '{row.Currency}' does not look like a valid 3-letter ISO code.",
                        SuggestedFix = "Use a standard 3-letter code, e.g. USD, EUR, GBP."
                    });
                }

                if (row.CapitalCallsAmount is < 0)
                {
                    row.Issues.Add(new RowValidationIssueDto
                    {
                        Severity = ValidationSeverity.Warning, FieldName = "CapitalCalls",
                        Message = $"Row {rowNum}: Capital calls amount is negative ({row.CapitalCallsAmount:N0}).",
                        SuggestedFix = "Confirm this is an intentional adjustment rather than a data entry error."
                    });
                }

                if (row.DistributionsAmount is < 0)
                {
                    row.Issues.Add(new RowValidationIssueDto
                    {
                        Severity = ValidationSeverity.Warning, FieldName = "Distributions",
                        Message = $"Row {rowNum}: Distributions amount is negative ({row.DistributionsAmount:N0}).",
                        SuggestedFix = "Confirm this is an intentional clawback rather than a data entry error."
                    });
                }

                if (row.CapitalCallsAmount == 0 && row.DistributionsAmount == 0)
                {
                    row.Issues.Add(new RowValidationIssueDto
                    {
                        Severity = ValidationSeverity.Info, FieldName = null,
                        Message = $"Row {rowNum}: No capital calls or distributions recorded for this period."
                    });
                }

                if (row.PeriodDate.HasValue)
                {
                    var key = (row.FundName?.Trim().ToLowerInvariant() ?? "", row.PeriodDate.Value);
                    if (seenFundPeriods.TryGetValue(key, out var firstRowNum))
                    {
                        row.Issues.Add(new RowValidationIssueDto
                        {
                            Severity = ValidationSeverity.Warning, FieldName = "Period",
                            Message = $"Row {rowNum}: Duplicate entry for fund '{row.FundName}' in period {row.PeriodDate:MMM yyyy} (first seen on row {firstRowNum}).",
                            SuggestedFix = "Verify whether this is an intentional correction or a duplicate row."
                        });
                    }
                    else
                    {
                        seenFundPeriods[key] = rowNum;
                    }
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

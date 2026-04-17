using CashflowPilot.Application.DTOs;
using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using System.Text;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class CsvParserTests
{
    private static CsvParserService CreateParser() => new CsvParserService();

    private static Stream ToStream(string csv) => new MemoryStream(Encoding.UTF8.GetBytes(csv));

    [Fact]
    public void Parse_StandardHeaders_AutoDetectsMapping()
    {
        var csv = "Fund,Period,Capital Calls,Distributions\nApex Buyout,2024-01,1000000,0\n";
        var result = CreateParser().Parse(ToStream(csv));
        result.Mapping.Should().NotBeNull();
        result.Mapping!.FundNameColumn.Should().Be("Fund");
        result.Mapping.PeriodColumn.Should().Be("Period");
        result.Mapping.CapitalCallsColumn.Should().Be("Capital Calls");
        result.Mapping.DistributionsColumn.Should().Be("Distributions");
    }

    [Fact]
    public void Parse_ValidRow_ParsesCorrectly()
    {
        var csv = "Fund,Period,Capital Calls,Distributions,Currency\nApex Buyout,2024-01,1500000,500000,USD\n";
        var result = CreateParser().Parse(ToStream(csv));
        result.ValidRows.Should().Be(1);
        result.Rows[0].CapitalCallsAmount.Should().Be(1500000m);
        result.Rows[0].DistributionsAmount.Should().Be(500000m);
        result.Rows[0].PeriodDate.Should().Be(new DateTime(2024, 1, 1));
        result.Rows[0].FundName.Should().Be("Apex Buyout");
    }

    [Fact]
    public void Parse_QuarterPeriod_ParsesCorrectly()
    {
        var csv = "Fund,Period,Capital Calls,Distributions\nTest Fund,Q1 2024,500000,0\n";
        var result = CreateParser().Parse(ToStream(csv));
        result.ValidRows.Should().Be(1);
        result.Rows[0].PeriodDate.Should().Be(new DateTime(2024, 1, 1));
    }

    [Fact]
    public void Parse_Q4Period_MapsToOctober()
    {
        var csv = "Fund,Period,Capital Calls,Distributions\nTest Fund,Q4-2023,500000,0\n";
        var result = CreateParser().Parse(ToStream(csv));
        result.ValidRows.Should().Be(1);
        result.Rows[0].PeriodDate.Should().Be(new DateTime(2023, 10, 1));
    }

    [Fact]
    public void Parse_MonthYearFormat_ParsesCorrectly()
    {
        var csv = "Fund,Period,Capital Calls,Distributions\nTest Fund,Mar-2024,100000,50000\n";
        var result = CreateParser().Parse(ToStream(csv));
        result.ValidRows.Should().Be(1);
        result.Rows[0].PeriodDate.Should().Be(new DateTime(2024, 3, 1));
    }

    [Fact]
    public void Parse_CommasInAmounts_ParsesCorrectly()
    {
        var csv = "Fund,Period,Capital Calls,Distributions\nTest Fund,2024-01,\"1,500,000\",\"250,000\"\n";
        var result = CreateParser().Parse(ToStream(csv));
        result.ValidRows.Should().Be(1);
        result.Rows[0].CapitalCallsAmount.Should().Be(1500000m);
        result.Rows[0].DistributionsAmount.Should().Be(250000m);
    }

    [Fact]
    public void Parse_AccountingNegative_ParsesAsNegative()
    {
        var csv = "Fund,Period,Capital Calls,Distributions\nTest Fund,2024-01,(500000),0\n";
        var result = CreateParser().Parse(ToStream(csv));
        result.ValidRows.Should().Be(1);
        result.Rows[0].CapitalCallsAmount.Should().Be(-500000m);
    }

    [Fact]
    public void Parse_InvalidPeriod_ReturnsValidationError()
    {
        var csv = "Fund,Period,Capital Calls,Distributions\nTest Fund,not-a-date,1000000,0\n";
        var result = CreateParser().Parse(ToStream(csv));
        result.InvalidRows.Should().Be(1);
        result.Rows[0].ValidationErrors.Should().NotBeEmpty();
    }

    [Fact]
    public void Parse_UnrecognizedHeaders_ReturnsGlobalError()
    {
        var csv = "ColA,ColB,ColC\nvalue1,value2,value3\n";
        var result = CreateParser().Parse(ToStream(csv));
        result.GlobalErrors.Should().NotBeEmpty();
    }

    [Fact]
    public void Parse_EmptyRows_AreSkipped()
    {
        var csv = "Fund,Period,Capital Calls,Distributions\nApex,2024-01,1000000,0\n,,,,\nNordic,2024-02,500000,200000\n";
        var result = CreateParser().Parse(ToStream(csv));
        result.TotalRows.Should().Be(2);
        result.ValidRows.Should().Be(2);
    }

    [Fact]
    public void Parse_ManualMapping_OverridesAutoDetect()
    {
        var csv = "CompanyName,DatePeriod,Contributions,Payouts\nApex,2024-01,1000000,500000\n";
        var mapping = new ColumnMapping
        {
            FundNameColumn = "CompanyName",
            PeriodColumn = "DatePeriod",
            CapitalCallsColumn = "Contributions",
            DistributionsColumn = "Payouts"
        };
        var result = CreateParser().Parse(ToStream(csv), mapping);
        result.ValidRows.Should().Be(1);
        result.Rows[0].FundName.Should().Be("Apex");
        result.Rows[0].CapitalCallsAmount.Should().Be(1000000m);
    }

    [Fact]
    public void AutoDetect_FuzzyHeaders_DetectsMapping()
    {
        var headers = new List<string> { "fund name", "reporting_period", "contributions", "dist" };
        var mapping = ColumnMapping.AutoDetect(headers);
        mapping.FundNameColumn.Should().Be("fund name");
        mapping.PeriodColumn.Should().Be("reporting_period");
        mapping.CapitalCallsColumn.Should().Be("contributions");
        mapping.DistributionsColumn.Should().Be("dist");
    }
}

namespace CashflowPilot.Domain.Enums;

public enum RunStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}

public enum EntryType
{
    Forecast,
    Actual
}

public enum FileType
{
    ForecastCsv,
    ActualCsv
}

public enum FileStatus
{
    Uploaded,
    Processing,
    Processed,
    Failed
}

public enum AnalysisStatus
{
    Pending,
    Completed,
    Failed
}

public enum PeriodFrequency
{
    Monthly,
    Quarterly,
    Annual
}

public enum ValidationSeverity
{
    Info,
    Warning,
    Error
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Analyst = "Analyst";
    public const string Viewer = "Viewer";
}

public enum PlanTier
{
    Trial,
    Starter,
    Professional
}

public enum SubscriptionStatus
{
    Trialing,
    Active,
    PastDue,
    Canceled
}

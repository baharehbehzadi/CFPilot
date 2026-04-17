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

public static class Roles
{
    public const string Admin = "Admin";
    public const string Analyst = "Analyst";
    public const string Viewer = "Viewer";
}

namespace WLMonitored.Shared.Configuration;

public sealed class ScheduleJob
{
    public string Name { get; init; }

    public string Environment { get; init; }

    public bool IsUAT => "UAT" == Environment;

    public int BatchSize { get; init; }

    public int DelayExecutionTimeInSeconds { get; init; }

    public bool IsActive { get; init; }

    public string JobInfo
        => $"{Name}{(string.IsNullOrWhiteSpace(Environment) ? string.Empty : $" - {Environment}")}";
}
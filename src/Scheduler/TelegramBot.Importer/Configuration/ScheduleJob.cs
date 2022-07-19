namespace TelegramBot.Importer.Configuration;

public sealed class ScheduleJob
{
    public string Name { get; init; } = default!;

    public string SubAccPermission { get; init; } = default!;

    public string Environment { get; init; } = default!;

    public bool IsUAT => "UAT" == Environment;

    public bool IsGetTestData { get; init; }

    public string CronExpression { get; init; } = default!;

    public bool IsActive { get; init; }
}
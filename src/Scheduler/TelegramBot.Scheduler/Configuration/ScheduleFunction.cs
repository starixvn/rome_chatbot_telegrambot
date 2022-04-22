namespace TelegramBot.Scheduler.Configuration;

public sealed class ScheduleFunction
{
    public int Id { get; init; }

    public string Name { get; init; } = default!;

    public string GroupName { get; init; } = default!;

    public string CronExpression { get; init; } = default!;

    public bool IsActive { get; init; }
}
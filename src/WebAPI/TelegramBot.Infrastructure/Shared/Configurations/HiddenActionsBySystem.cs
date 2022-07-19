namespace TelegramBot.Infrastructure.Shared.Configurations;

public record HiddenActionsBySystem
{
    public string SiteName { get; init; } = default!;

    public short[] ActionIds { get; init; } = default!;
}

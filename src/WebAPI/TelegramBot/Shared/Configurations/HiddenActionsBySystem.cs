using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.Shared.Configurations;

[ExcludeFromCodeCoverage]
public record HiddenActionsBySystem
{
    public string SiteName { get; init; } = default!;

    public short[] ActionIds { get; init; } = default!;
}

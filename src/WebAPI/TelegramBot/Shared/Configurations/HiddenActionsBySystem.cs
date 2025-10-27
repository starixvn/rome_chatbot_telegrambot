using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.Shared.Configurations;

[ExcludeFromCodeCoverage(Justification = "POCO used for configuration mapping only.")]
public record HiddenActionsBySystem
{
    public string SiteName { get; init; } = default!;

    public short[] ActionIds { get; init; } = default!;
}

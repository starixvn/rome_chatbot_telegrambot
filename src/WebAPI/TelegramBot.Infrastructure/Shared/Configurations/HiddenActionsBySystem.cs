using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.Infrastructure.Shared.Configurations;

[ExcludeFromCodeCoverage]
public record HiddenActionsBySystem
{
    public string SiteName { get; init; } = default!;

    public short[] ActionIds { get; init; } = default!;
}

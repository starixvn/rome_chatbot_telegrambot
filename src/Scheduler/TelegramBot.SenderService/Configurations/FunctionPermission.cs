using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.SenderService.Configurations;

[ExcludeFromCodeCoverage]
public sealed class FunctionPermission
{
    public string FunctionKey { get; init; } = default!;

    public string SubAccPermission { get; init; } = default!;
}
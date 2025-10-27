using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.SenderService.Configurations;

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
public sealed class FunctionPermission
{
    public string FunctionKey { get; init; } = default!;

    public string SubAccPermission { get; init; } = default!;
}
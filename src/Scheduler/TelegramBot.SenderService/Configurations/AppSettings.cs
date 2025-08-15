using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.SenderService.Configurations;

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
public class AppSettings
{
    public IDictionary<string, string> FunctionPermissions { get; init; } = default!;
    public bool IsTest { get; set; }
    public string ProxyAddress { get; set; }
}
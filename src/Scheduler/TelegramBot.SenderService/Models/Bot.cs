using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.SenderService.Models;

[ExcludeFromCodeCoverage(Justification = "This class only contains extension methods for DI registration and is not intended to be covered by tests.")]
public record Bot(
    int BotId,
    string Username,
    string System,
    string AccessToken,
    bool Status,
    bool IsTest,
    string DisplayName
);
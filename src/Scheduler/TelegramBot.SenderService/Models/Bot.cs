using System.Diagnostics.CodeAnalysis;

namespace TelegramBot.SenderService.Models;

[ExcludeFromCodeCoverage]
public record Bot(
    int BotId,
    string Username,
    string System,
    string AccessToken,
    bool Status,
    bool IsTest,
    string DisplayName
);
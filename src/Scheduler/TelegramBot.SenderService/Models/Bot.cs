namespace TelegramBot.SenderService.Models;

public record Bot(
    int BotId,
    string Username,
    string System,
    string AccessToken,
    bool Status,
    bool IsTest,
    string DisplayName
);
namespace TelegramBot.SenderService.Models;

public record Bot(int BotId, string Name, string System, string AccessToken, bool Status, bool IsTest) { }
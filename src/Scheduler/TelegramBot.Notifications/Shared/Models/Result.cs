namespace TelegramBot.Notifications.Shared.Models;

public record Result(int Code, string Message)
{
    public bool IsFail => Code != 0;
}

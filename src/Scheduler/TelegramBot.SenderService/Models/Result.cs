namespace TelegramBot.SenderService.Models;

public record Result(int Code, string Message)
{
    public bool IsFailed => Code != 0;
}

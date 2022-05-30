namespace TelegramBot.SenderService.Models;

public record Result(int Code, string Message)
{
    public bool IsFailed => Code != 0;

    public static Result CreateSuccessfulResult(string message = "")
        => new(0, message);
}

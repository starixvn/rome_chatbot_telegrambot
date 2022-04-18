namespace TelegramBot.Infrastructure.Shared.Models
{
    public class Result
    {
        public Result(int code, string message = "")
        {
            Code = code;
            Message = message;
        }

        public int Code { get; }

        public bool IsOk => Code == 0;

        public bool IsNotOk => !IsOk;

        public string Message { get; }

        public static Result CreateSuccessfulResult(string message = "") => new(0, message);

        public static Result CreateErrorResult(string message = "") => new(1, message);
    }
}
using TelegramBot.Common.Constants;

namespace TelegramBot.Common.Models
{
    public class Result
    {
        public Result(int code, string message = "")
        {
            Code = code;
            Message = message;
        }

        public int Code { get; }

        public bool IsOk => Code == ErrorCodes.Success;

        public bool IsNotOk => !IsOk;

        public string Message { get; }

        public static Result CreateSuccessfulResult(string message = "") => new(ErrorCodes.Success, message);

        public static Result CreateErrorResult(string message = "") => new(ErrorCodes.Generic, message);
    }
}
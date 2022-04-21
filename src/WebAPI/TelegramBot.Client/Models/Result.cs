namespace TelegramBot.Client.Models
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
    }
}
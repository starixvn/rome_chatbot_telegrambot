namespace TelegramBot.Common.Models
{
    public class CustBotMessenger
    {
        public int UserId { get; set; }

        public int BotId { get; set; }

        public string BotName { get; set; }

        public long ChatId { get; set; }

        public bool Status { get; set; }

        public bool IsVerifyOTP { get; set; }

        public string Language { get; set; }
    }
}
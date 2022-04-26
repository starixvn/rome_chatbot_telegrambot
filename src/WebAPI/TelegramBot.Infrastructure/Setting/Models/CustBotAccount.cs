namespace TelegramBot.Infrastructure.Setting.Models
{
    public class CustBotAccount
    {
        public CustBotAccount(int userId, int custId, int subAccId, byte customerLevel, string language)
        {
            UserId = userId;
            CustId = custId;
            SubAccId = subAccId;
            CustomerLevel = customerLevel;
            Language = language;
        }

        public int UserId { get; }

        public int CustId { get; }

        public int SubAccId { get; }

        public byte CustomerLevel { get; }

        public string Language { get; }

        public bool IsSubAccount => SubAccId > 0;
    }
}
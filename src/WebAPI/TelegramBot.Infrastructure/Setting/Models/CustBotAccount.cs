namespace TelegramBot.Infrastructure.Setting.Models
{
    public class CustBotAccount
    {
        public CustBotAccount(int userId, int custId, int subAccId, string language)
        {
            UserId = userId;
            CustId = custId;
            SubAccId = subAccId;
            Language = language;
        }

        public int UserId { get; }

        public int CustId { get; }

        public int SubAccId { get; }

        public string Language { get; }

        public bool IsSubAccount => SubAccId > 0;
    }
}
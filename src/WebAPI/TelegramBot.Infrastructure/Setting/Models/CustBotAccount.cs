namespace TelegramBot.Infrastructure.Setting.Models
{
    public class CustBotAccount
    {
        public CustBotAccount(int userId, int custId, int subAccId)
        {
            UserId = userId;
            CustId = custId;
            SubAccId = subAccId;
        }

        public int UserId { get; }

        public int CustId { get; }

        public int SubAccId { get; }

        public bool IsSubAccount => SubAccId > 0;
    }
}
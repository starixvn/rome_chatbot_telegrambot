using Fanex.Data.Repository;

namespace TelegramBot.Infrastructure.Action.Criteria
{
    public class CustBotActionStatusUpdCriteria : INonQueryCommand
    {
        public CustBotActionStatusUpdCriteria(int userId)
        {
            UserId = userId;
        }

        public int UserId { get; }

        public string GetSettingKey() => "CustBotActionStatusUpd";

        public bool IsValid() => UserId > 0;
    }
}
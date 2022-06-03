using Fanex.Data.Repository;
using TelegramBot.Infrastructure.Function.Models;
using TelegramBot.Infrastructure.Shared;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class CustBotFunctionUpdCriteria : ResultCriteriaBase, INonQueryCommand
    {
        public CustBotFunctionUpdCriteria(int userId, BotFunction function, int creator, string ip)
        {
            UserId = userId;
            Setting = $"<Root><r id='{function.FunctionId}' stt='{function.Enabled}' /></Root>";
            Creator = creator;
            IP = ip;
        }

        public int UserId { get; }

        public string Setting { get; }

        public int Creator { get; }

        public string IP { get; }

        public string GetSettingKey() => "CustBotFunctionUpd";

        public bool IsValid() => UserId > 0;
    }
}
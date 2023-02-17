using Fanex.Data.Repository;
using TelegramBot.Modules.Function.Models;
using TelegramBot.Shared;

namespace TelegramBot.Modules.Setting.Criteria
{
    public class CustBotFunctionUpdCriteria : ResultCriteriaBase, INonQueryCommand
    {
        public CustBotFunctionUpdCriteria(int userId, int botId, BotFunction function, int creator, string ip)
        {
            UserId = userId;
            BotId = botId;
            Setting = $"<Root><r id='{function.FunctionId}' stt='{function.Enabled}' /></Root>";
            Creator = creator;
            IP = ip;
        }

        public int UserId { get; }

        public int BotId { get; }

        public string Setting { get; }

        public int Creator { get; }

        public string IP { get; }

        public string GetSettingKey() => "CustBotFunctionUpd";

        public bool IsValid() => UserId > 0;
    }
}
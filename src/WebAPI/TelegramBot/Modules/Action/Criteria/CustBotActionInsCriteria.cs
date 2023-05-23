using Fanex.Data.Repository;

namespace TelegramBot.Modules.Action.Criteria
{
    public class CustBotActionInsCriteria : INonQueryCommand
    {
        public CustBotActionInsCriteria(int userId, short actionId, string userResponse, string compiledCommand)
        {
            UserId = userId;
            ActionId = actionId;
            UserResponse = userResponse;
            CompiledCommand = compiledCommand;
        }

        public int UserId { get; }

        public short ActionId { get; }

        public string UserResponse { get; }

        public string CompiledCommand { get; }

        public string GetSettingKey() => "CustBotActionIns";

        public bool IsValid() => UserId > 0;
    }
}
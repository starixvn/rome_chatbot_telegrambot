using Fanex.Data.Repository;

namespace TelegramBot.Modules.Action.Criteria
{
    public class CustBotActionCommandUpdCriteria : INonQueryCommand
    {
        public CustBotActionCommandUpdCriteria(long logActionId, string userResponse, string compiledCommand)
        {
            LogActionID = logActionId;
            UserResponse = userResponse;
            CompiledCommand = compiledCommand;
        }

        public long LogActionID { get; }

        public string UserResponse { get; }

        public string CompiledCommand { get; }

        public string GetSettingKey() => "CustBotActionCommandUpd";

        public bool IsValid() => LogActionID > 0;
    }
}
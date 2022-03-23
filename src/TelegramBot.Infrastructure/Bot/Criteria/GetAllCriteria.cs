using Fanex.Data.Repository;

namespace TelegramBot.Infrastructure.Bot.Criteria
{
    public class GetAllCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "GetAllBot";

        public override bool IsValid() => true;
    }
}

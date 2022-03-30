using Fanex.Data.Repository;

namespace TelegramBot.Infrastructure.Bot.Criteria
{
    public class GetAllCriteria : CriteriaBase
    {
        public override string GetSettingKey() => "BotSelAll";

        public override bool IsValid() => true;
    }
}

using Fanex.Data.Repository;

namespace TelegramBot.Infrastructure.Setting.Criteria
{
    public class GetCustBotCriteria : CriteriaBase
    {
        public GetCustBotCriteria(long chatId)
        {
            ChatId = chatId;
        }

        public long ChatId { get; }

        public override string GetSettingKey() => "GetCustBot";

        public override bool IsValid() => ChatId != 0;
    }
}

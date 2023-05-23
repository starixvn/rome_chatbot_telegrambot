using Fanex.Data.Repository;

namespace TelegramBot.Modules.Resource.Criteria
{
    public class BotResourceSelCriteria : CriteriaBase
    {
        public BotResourceSelCriteria(string resourceName, string resourceKey)
        {
            ResourceName = resourceName;
            ResourceKey = resourceKey;
        }

        public string ResourceName { get; }

        public string ResourceKey { get; }

        public override string GetSettingKey() => "BotResourceSel";

        public override bool IsValid() => !string.IsNullOrEmpty(ResourceName) && !string.IsNullOrEmpty(ResourceKey);
    }
}